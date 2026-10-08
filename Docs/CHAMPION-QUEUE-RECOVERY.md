# Paid champion queue recovery

The new simulation regression covers every Ironfold faction twice: once when a prerequisite is sold and once when it is destroyed. These sixteen scenarios use the actual supplied Ironfold terrain, unmodified solo 1,200-gold wallet, normal faction prices, builder travel, order completion and refunds. No free towers or extra gold are granted.

Each scenario buys all six regular designs and an existing champion, then queues a second champion followed by a replacement regular design. Removing the first prerequisite before the builder arrives must:

- Relock new champion construction while retaining the already-built champion.
- Skip the queued champion without charging or silently retrying it later.
- Continue to the next queued regular tower, charging its real price and preserving the selected toolbar design.
- Restore the unlock after that replacement is standing.
- Accept a newly issued champion order and debit its full price, ending with both champions standing and an exact nonnegative wallet ledger.

Sale checks use the quoted refund, including Scrap Frontier's distinct base refund. Destruction directly applies lethal grid damage to exercise the removal path and verifies that selling a destroyed tower gives no refund. This is an injected destruction fixture, not a played siege encounter. Existing paid siege and ownership checks remain separate.

All 65 pure simulation cases pass, including the new 16-scenario regression. See Howl-Champion-Recovery-Pure-Tests.txt. Unity compilation succeeded and the focused case passed (1/1), also confirmed from its XML result. See Howl-Champion-Recovery-Unity-Tests.json. This is not a new full Unity-suite pass. No production gameplay code changed; the current Linux/Windows packages remain source 110633b.
