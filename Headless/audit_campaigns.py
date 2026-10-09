#!/usr/bin/env python3
"""Independently audit paid campaign ledgers produced with wave-income metadata.

Usage: python3 Headless/audit_campaigns.py result.json [more-results.json ...]
This does not run the game or require Unity/.NET. Defeats are valid evidence;
stalls, missing income metadata and inconsistent ledgers are failures.
"""
import argparse
import json
from pathlib import Path


def check(condition, message):
    if not condition:
        raise ValueError(message)


def audit(run):
    players = run['PlayerCount']
    check(1 <= players <= 4, 'player count outside 1–4')
    check(not run['Stalled'], 'campaign stalled')
    grant = run['StartingTeamGold']
    share = lambda total, owner: total // players + int(owner < total % players)
    check(run['StartingWallets'] == [share(grant, p) for p in range(players)], 'starting wallet split')
    waves = run['Waves']
    check(bool(waves), 'no recorded waves')
    receipts = run['Placements'] + run['Upgrades']
    sales = run['Sales']
    for entry in receipts + sales:
        check(1 <= entry['Player'] <= players, 'receipt has invalid owner')
        check(1 <= entry['BeforeWave'] <= len(waves), 'receipt outside recorded campaign')
        amount = entry['Refund'] if 'Refund' in entry else entry['Cost']
        check(amount >= 0, 'negative transaction')
    wallets_checked = incomes_checked = unequal_waves = 0
    for number, wave in enumerate(waves, 1):
        prefix = f'wave {number}: '
        check(wave['Wave'] == number, prefix + 'nonsequential wave')
        check(wave['SummaryPresent'], prefix + 'missing result summary')
        check(wave['Cleared'] == (run['Won'] or number < len(waves)), prefix + 'clear/defeat status')
        check(wave['Killed'] >= 0 and wave['Leaked'] >= 0, prefix + 'negative enemy count')
        before = grant
        reward = wave['Killed'] * run['KillReward'] + (run['WaveReward'] if wave['Cleared'] else 0)
        grant += reward
        check(wave['TeamIncome'] == reward, prefix + 'team income')
        check(len(wave['PlayerGold']) == players and len(wave['PlayerIncome']) == players, prefix + 'wallet/income count')
        for p in range(players):
            spending = sum(t['Cost'] for t in receipts if t['Player'] == p + 1 and t['BeforeWave'] <= number)
            refunds = sum(t['Refund'] for t in sales if t['Player'] == p + 1 and t['BeforeWave'] <= number)
            expected = share(grant, p) - spending + refunds
            check(expected >= 0 and wave['PlayerGold'][p] == expected, prefix + f'player {p + 1} wallet')
            check(wave['PlayerIncome'][p] == share(grant, p) - share(before, p), prefix + f'player {p + 1} income')
            wallets_checked += 1
            incomes_checked += 1
        check(wave['Gold'] == sum(wave['PlayerGold']), prefix + 'team wallet')
        check(sum(wave['PlayerIncome']) == reward, prefix + 'income conservation')
        unequal_waves += len(set(wave['PlayerIncome'])) > 1
    spending = [sum(t['Cost'] for t in receipts if t['Player'] == p + 1) for p in range(players)]
    refunds = [sum(t['Refund'] for t in sales if t['Player'] == p + 1) for p in range(players)]
    check(run['PlayerSpending'] == spending and run['Spent'] == sum(spending), 'total spending')
    check(run['PlayerRefunds'] == refunds and run['Refunded'] == sum(refunds), 'total refunds')
    check(run['FinalWallets'] == waves[-1]['PlayerGold'] and run['Gold'] == sum(run['FinalWallets']), 'final wallets')
    return {'walletChecks': wallets_checked, 'incomeChecks': incomes_checked, 'unequalIncomeWaves': unequal_waves}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('files', nargs='+', type=Path)
    args = parser.parse_args()
    total = {'campaigns': 0, 'wins': 0, 'walletChecks': 0, 'incomeChecks': 0, 'unequalIncomeWaves': 0}
    for path in args.files:
        try:
            runs = json.loads(path.read_text())
            check(isinstance(runs, list) and bool(runs), 'expected a nonempty campaign list')
            for index, run in enumerate(runs):
                try:
                    result = audit(run)
                except (KeyError, TypeError, ValueError) as error:
                    raise ValueError(f'campaign {index + 1}: {error}') from error
                total['campaigns'] += 1
                total['wins'] += int(run['Won'])
                for key, value in result.items():
                    total[key] += value
        except (OSError, KeyError, TypeError, ValueError) as error:
            parser.exit(1, f'AUDIT FAILED {path}: {error}\n')
    print(json.dumps(total, indent=2))


if __name__ == '__main__':
    main()
