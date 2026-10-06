# Questions for later review

No answer was needed to implement the first slice. These are deferred design decisions, with the assumptions used in the prototype.

1. **How permissive should diagonal gaps feel?** Defaults: tower Fill 0.86, enemy radius 0.20, navigation step 0.50. Touching diagonal clearances do not permit passage. Try smaller footprints/radii and a 0.25 step in the map asset.
2. **What is the final connected-map route?** Assumed one shared rectangular test space with editable ordered ground and air checkpoints. Nothing models player areas as isolated lanes.
3. **Which towers should blocked enemies prefer?** Assumed a weighted route through obstacles, choosing the first obstruction, without considering price, owner, or remaining health. A health-aware breach policy would change gameplay and field invalidation frequency.
4. **Can construction overlap units or route markers?** Assumed no overlap with existing ground-unit discs, spawn, or ground checkpoints. Complete route blockage itself is explicitly allowed.
5. **What should leaks cost?** They increment a debug counter; there is no shared-life or loss condition yet.
6. **How should a builder and economy work?** Direct unlimited construction is the temporary interaction. No builder movement, resources, or purchase delays yet.
7. **How should enemy collision feel at large scale?** Current units are solid discs with basic separation and sliding. Throughput at tight bends is intentionally reduced by geometry; further crowd tuning should use playtest feedback.
8. **How strict must reproducibility become?** Same-runtime fixed-step repeatability is tested. Cross-platform bit-identical lockstep is not promised; an authoritative server remains the intended future direction.
9. **Should waves repeat or keep escalating?** Five explicit example waves stop at completion. The fifth is flying data; future tenth/fifteenth waves should be authored explicitly.
10. **What should sell refunds, tower ownership and cooperative permissions be?** Not implemented for this local mechanics slice.
