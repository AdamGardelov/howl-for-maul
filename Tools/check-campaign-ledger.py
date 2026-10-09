#!/usr/bin/env python3
"""Independently audit normal paid purchases and per-wave income in a saved campaign."""
import argparse
import json
from pathlib import Path


def audit(path):
    wallets = income_checks = 0
    for run in json.loads(path.read_text()):
        balances = run['StartingWallets'][:]
        assert sum(balances) == run['StartingTeamGold'], 'Starting team budget mismatch'
        for wave in run['Waves']:
            number = wave['Wave']
            for purchase in run['Placements'] + run['Upgrades']:
                if purchase['BeforeWave'] == number:
                    owner = purchase['Player'] - 1
                    balances[owner] -= purchase['Cost']
                    assert balances[owner] >= 0, f"{run['Map']} wave {number}: unpaid purchase"
            expected = wave['Killed'] * run['KillRewards'][number-1]
            if wave['Cleared']:
                expected += run['ClearRewards'][number-1]
            assert expected == wave['TeamIncome'], f"{run['Map']} wave {number}: team income"
            assert sum(wave['PlayerIncome']) == expected, 'Lost shared-income remainder'
            income_checks += 1
            for player, earned in enumerate(wave['PlayerIncome']):
                balances[player] += earned
                assert balances[player] == wave['PlayerGold'][player], f"{run['Map']} wave {number}: P{player+1} wallet"
                wallets += 1
        print(f"PASS {run['Map']}: {len(run['Waves'])} waves; {len(run['Placements'])} purchases; {len(run['Upgrades'])} upgrades; {run['Lives']} lives")
    print(f'PASS {wallets} wallets and {income_checks} team-income calculations')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('ledger', type=Path)
    audit(parser.parse_args().ledger)
