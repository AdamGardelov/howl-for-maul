# Solo and LAN menu clarity

Runtime source 1aad187. The main menu now puts map selection first, calls the offline action Play Solo, and calls the network form Multiplayer / LAN. The form explains that LAN uses the host's local address and that internet play needs a reachable host. Selecting two to four offline slots explicitly calls them a same-keyboard/mouse test mode; its action reads Start Local Slot Test. This prevents testing slots from being mistaken for friends joining a lobby. Underlying setup, networking and gameplay behavior is unchanged.

Unity compilation succeeds. The first status query timed out during domain reload; a later status query confirmed successful compilation before packaging. Linux-Menu and Windows-Menu packages both build with zero errors. Linux has one Pipeline-disabled warning; Windows has nineteen Pipeline/ray-tracing warnings, retained in Howl-Menu-Clarity-Packages.json. Attribution and source provenance files are included. Editor target restored to Linux.

Actual isolated Linux input at 1440×900 on llvmpipe verifies the visible solo button, complete LAN Host/Join form, and two-slot test wording. Returning to Solo then completing faction → start → Normal starts Rimewatch with 1,200 gold and all three lanes. Screenshots were inspected. Game-menu Quit exits zero and the player log contains no game exceptions. Both-map data smoke exits zero.

This small UI change does not add tests that merely mirror strings. No full Unity-suite, new network connection or Windows-runtime pass is claimed. Existing player builds are preserved; updated packages use separate Linux-Menu / Windows-Menu directories.

The user's mobile question is answered separately in MOBILE-STATUS.md: Android/iOS remain future scope from the initial brief. Camera input has an interface, but building controls/HUD are desktop-oriented. No phone build or device test has been completed. This checkpoint does not imply mobile support.
