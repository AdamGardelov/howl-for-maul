import copy
import json
from pathlib import Path
import unittest

from audit_campaigns import audit

DATA = Path(__file__).resolve().parents[1] / 'Docs' / 'Balance'


class CampaignAuditTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.compact = json.loads((DATA / 'HARD-THREE-PLAYER-COMPACT.json').read_text())
        cls.adaptive = json.loads((DATA / 'HARD-THREE-PLAYER-ADAPTIVE.json').read_text())
        cls.refund = json.loads((DATA / 'HARD-THREE-PLAYER-REFUND-CONTROL.json').read_text())[0]

    def test_real_campaigns_include_victory_defeat_unequal_income_and_refunds(self):
        runs = self.compact + self.adaptive
        self.assertEqual(sum(r['Won'] for r in runs), 8)
        self.assertTrue(any(not r['Won'] for r in runs))
        results = [audit(r) for r in runs]
        self.assertEqual(sum(r['walletChecks'] for r in results), 588)
        self.assertEqual(sum(r['incomeChecks'] for r in results), 588)
        self.assertEqual(sum(r['unequalIncomeWaves'] for r in results), 62)
        self.assertGreater(self.refund['Refunded'], 0)
        self.assertEqual(audit(self.refund)['walletChecks'], 20)

    def test_wallet_transfer_is_detected_even_with_same_team_gold(self):
        run = copy.deepcopy(self.compact[0])
        run['Waves'][0]['PlayerGold'][0] += 1
        run['Waves'][0]['PlayerGold'][1] -= 1
        with self.assertRaisesRegex(ValueError, 'player 1 wallet'):
            audit(run)

    def test_income_transfer_is_detected_even_with_same_team_income(self):
        run = copy.deepcopy(self.compact[0])
        run['Waves'][0]['PlayerIncome'][0] += 1
        run['Waves'][0]['PlayerIncome'][1] -= 1
        with self.assertRaisesRegex(ValueError, 'player 1 income'):
            audit(run)

    def test_receipt_cannot_be_reassigned_to_another_owner(self):
        run = copy.deepcopy(self.compact[0])
        receipt = run['Placements'][0]
        receipt['Player'] = receipt['Player'] % 3 + 1
        with self.assertRaisesRegex(ValueError, 'wallet'):
            audit(run)

    def test_defeat_cannot_claim_completion(self):
        run = copy.deepcopy(self.compact[0])
        run['Waves'][-1]['Cleared'] = True
        with self.assertRaisesRegex(ValueError, 'clear/defeat status'):
            audit(run)

    def test_missing_summary_is_rejected(self):
        run = copy.deepcopy(self.compact[0])
        run['Waves'][0]['SummaryPresent'] = False
        with self.assertRaisesRegex(ValueError, 'missing result summary'):
            audit(run)

    def test_refunds_do_not_count_as_income(self):
        run = copy.deepcopy(self.refund)
        sale = run['Sales'][0]
        wave = run['Waves'][sale['BeforeWave'] - 1]
        wave['TeamIncome'] += sale['Refund']
        wave['PlayerIncome'][sale['Player'] - 1] += sale['Refund']
        with self.assertRaisesRegex(ValueError, 'team income'):
            audit(run)

    def test_missing_refund_is_detected(self):
        run = copy.deepcopy(self.refund)
        run['Sales'].pop(0)
        with self.assertRaisesRegex(ValueError, 'wallet'):
            audit(run)


if __name__ == '__main__':
    unittest.main()
