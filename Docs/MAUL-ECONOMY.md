# Historical maul economy comparison

Implementation follow-up: see [the new economy and its verification](MAUL-ECONOMY-IMPLEMENTED.md). The Howl values below describe the earlier research checkpoint, not the current balance.

Inspected 2026-10-09. This is research, not a balance change or Warcraft III playtest. Values are tied to identifiable archive files; neither file establishes the economy of every Wintermaul/Mega Man revision, or proves it matches the user's remembered copy.

## Findings

| Reference | Starting gold | Kill rewards | Wave reward |
|---|---:|---|---|
| Wintermaul X5!, file `Wintermaul X5M edited.w3x` | 60 per player | 1 early, rising with enemy type; full table below | Increasing per-player bonus; derived normal sequence 14, 16, 18… after waves 1, 2, 3… |
| Mega Man Maul 3.0 Final, archived reference | 550 per player | 1 → 2 → 3 → 4 → 5 through the main progression; final boss has zero configured bounty | Same increasing per-player bonus structure |
| Howl for Maul, current Rimewatch and Ironfold | 1,200 team total: 1,200 / 600 / 400 / 300 per player at 1 / 2 / 3 / 4 players | Flat 2 gold per kill for the entire team, distributed round-robin | Flat 120 team gold per completed wave, shared |

The 550-gold Mega Man value is genuinely in the inspected file's initialization trigger. Do not silently replace it with an assumed 50/60, but do not call it universal: the archived filename/version label does not certify an unmodified first release. Wintermaul X5 is explicitly an edited derivative. Its description even records a change to wave 6–9 bounty, demonstrating why version labels matter. Sources: [Mega Man archive](https://wc3maps.com/map/2823), [Wintermaul X5 archive](https://wc3maps.com/map/11831).

## Exact kill-bounty progression in the inspected files

Gold below is awarded to the killing player's owner through native bounty. Both scripts enable enemy-player bounty; no shared-kill gold distribution trigger was found. Wave completion rewards are a separate payment to all defenders. This differs from [Wintermaul Redux's explicitly collective bounty system](https://www.hiveworkshop.com/threads/wintermaul-redux-1-08g.121401/), which must not be projected backward onto every older maul.

| Waves | Wintermaul X5 kill gold |
|---|---:|
| 1–4 | 1 |
| 5–13 | 2 |
| 14–18 | 3 |
| 19–23 | 4 |
| 24 | 2 |
| 25–29 | 4 |
| 30 | 100–190 (random bounty dice) |
| 31–32 | 5 |
| 33 | 100 |

The X5 script defines extra rounds beyond its advertised 30. Those rows describe configured units, not a claim that every event/order of the bonus rounds was play-tested. Wave 24's drop to 2 is present in the inherited object values; do not smooth it away when describing this file.

| Waves | Mega Man Maul 3.0 Final kill gold |
|---|---:|
| 1–4 | 1 |
| 5–9 | 2 |
| 10–14 | 3 |
| 15–19 | 4 |
| 20–34 | 5 |
| 35 | 0 |

The first four Mega Man units inherit one bounty die from their base unit. Wintermaul waves 23/24 inherit four/two dice respectively. These defaults were cross-checked in both the English base and Custom_V1 UnitBalance tables of [YDWE](https://github.com/actboy168/YDWE/tree/master/Development/Component/share/en-US/mpq). Both tables agree. This is a reference-data cross-check, not a historical-patch runtime verification. All other table rows have explicit map overrides for the relevant bounty fields. Bounty calculation uses base plus the sum of dice; one-sided dice give an exact fixed amount.

## Other economy rules that affect the comparison

Both scripts initialize the wave-bonus variable to 10, then increase it by 2 whenever the next-level trigger advances. Advancing from level 0 to 1 produces 12 but pays no completion reward; the payment condition requires the next level to be at least 2. Thus the normal nonterminal sequence after clearing wave n is 12 + 2n: 14 after wave 1, 16 after wave 2, 30 after wave 9, and so on. This is derived from trigger order; final/boss transitions and defeat can take other paths, so it is not a promise of a post-victory payout.

The gray/last defender (player slot 9) receives an additional half of the wave bonus when that slot is playing. For example, the normal first reward would be 14 + 7 = 21 for that defender. The no-lives-lost condition controls a message; it is not the condition for this ordinary completion payment.

Wintermaul X5 starts with one lumber and configures another at the transition to wave 15. Mega Man starts with zero and grants one at the transition to wave 25. These gate progression differently from Howl's current twenty-wave, gold-only design. The scripts also have sell/refund actions; a complete per-tower refund audit is outside this pass.

Cheap configured tower costs in X5 include 7 and 10 gold, so a 60-gold opening permits only a small initial maze. Mega Man includes 7/10-gold opening designs as well; 550 buys a much larger opening in that particular file. Comparing raw starting gold without tower costs, spawn counts, player positions, targeting and upgrades would give misleading balance conclusions.

## Implications for Howl for Maul

Our values are provisional original tuning, not numbers verified against the historical maps. At four players our opening is 300 each (five times the inspected X5 opening), but our roster prices and wave counts also differ. Our 2-gold team kill is only 0.5 gold per player on average with four players, while a historical killer received the whole bounty. Our round-robin allocator preserves every integer coin; rewards do not vanish through rounding.

Preserve the user's fixed four-player-equivalent team economy and all-active lanes. Do not blindly copy nine-player, killer-only rewards into a four-player shared-income game. The next balance pass should compare how many useful towers each opening buys, then test a tighter opening and increasing per-wave bounty/rewards as a coherent configuration. It should cover both maps, all factions and 1–4 players before replacing current values. No starting money, kill reward, tower cost or wave reward was changed in this research checkpoint.

Nonblocking question saved for later: does the remembered Mega Man copy have the unusually generous 550-gold opening, or a tighter opening in another revision? We can refine against the exact map if identified. This does not block recording the verified reference values.

## Reproduction and provenance

The archives were inspected as data in chat scratch storage. No map was executed; no extracted scripts, object files, art or audio were added to the game. Only this explanation and numeric research facts are committed. The wave rows in `MAUL-ECONOMY-FACTS.json` are research evidence, not imported game configuration.

- Mega Man archive SHA-256: `305e39b0029636b85005cc15ccdfff7ad7012658c52ea29fe350fbe2662a240b`.
- Wintermaul X5 archive SHA-256: `0b260f0ec0f6d8059d06d35ab63678aaf155a4c77d4286e4e1645e8dd9228c0d`.
- Starting gold: `war3map.j`, initialization trigger `SetPlayerStateBJ`, Mega Man line 525; X5 line 496.
- Wave bonuses: Mega Man globals/next-level/reward code at lines 267, 776, 844–927; X5 at 205, 933, 980–1052.
- Kill tables: join `Trig_Set_Levels` unit assignments to `war3map.w3u` fields `ubba`, `ubdi`, `ubsi`; resolve missing fields from the base unit only. Do not confuse unit purchase price with bounty.
- Current game values: `Assets/Game/Maps/Resources/Rimewatch.asset` and `Ironfold.asset`; distribution and kill/wave payment in `Assets/Game/Simulation/Combat/World.cs`.

No new Unity builds or gameplay tests are warranted for this documentation-only change. Scheduler remains paused and mobile remains on hold.
