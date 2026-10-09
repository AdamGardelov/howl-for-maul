# Howl for Maul branding

The menu logo is an original generated wordmark: a howling wolf between two towers, warm gold faces, slate depth and small teal insets. The exact title is HOWL FOR MAUL. No reference-game art or logo files were used. It was created with the built-in image_gen tool; initial and refinement prompts are in LOGO-PROMPTS.txt.

Source asset: Assets/Game/UI/Resources/Brand/HowlForMaul.png (1774 × 887, RGBA).
SHA-256: cba9c0f3f2526be8e730773bf0464d7c9f2b4bac8df6ad960748323abf96d9d2

Keep the transparent master intact. PrototypeHud.Brand.cs samples the visible artwork with a small safety margin and scales proportionally; no opaque backdrop is baked into the image. The Unity importer uses source alpha, alpha-edge handling, bilinear filtering, no mipmaps, no NPOT rescale and uncompressed color. The source texture is not CPU-readable in the player.

The start/setup screen gets the large mark. Lobby, faction and difficulty stages use it next to the stage heading. The pause menu includes a smaller mark; its settings scroll when necessary, keeping Return, Leave and Quit accessible. The compact combat HUD retains its short text label for readability and space.

This is presentation only. Game rules, maps, economy, networking and soundtrack attribution are unchanged. The paired Gallery archives are preserved as the preceding friends checkpoint until new builds are explicitly recorded below.

Verification is recorded after the standalone menu check.
