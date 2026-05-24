* **v4.2.2 – Support for Enhanced Version of GTA V**

  * Improved: The script is now compatible with GTA V Enhanced, thanks to [FastBurst](https://www.gta5-mods.com/users/FastBurst) for testing and helping in bringing the support for it

* **v4.2.1 – Open Source Under GPLv2 License**

  * Improved: Released the source code of the script under GPLv2 license on my GitHub account, check it out [Source Code](af)
  * Removed: Embedded Dependencies since it was found to cause issues

* **v4.2.0 – Embedded Dependencies & Improved Icons On-Screen**

  * Added: 36 New blips! Now the script features over 930+ blip icons
  * Added: The ability to switch between vanilla texture sheet and modded texture sheet for the on-screen icons with persistence state between game sessions
  * Improved: Embedded [LemonUI](https://www.gta5-mods.com/tools/lemonui), [INI File Parser](https://github.com/rickyah/ini-parser), and [Newtonsoft JSON](https://github.com/JamesNK/Newtonsoft.Json) directly into the mod — no separate installations required
  * Improved: Updated both fire departments and metro stations category blip icons
  * Fixed: Broken on-screen icons selection positions

* **v4.1.0 – Major Bug Fix & Improvements to Preview Blip**

  * Added: Flash, Flash Interval, & Transparency effects for preview blip. The preview blip will now accurately show exactly what the final blip will look like before you save it!
  * Fixed: Script crashing at start-up

* **v4.0.2 – Patch for Update Checker**

  * Fixed: Update checker function where it couldn't parse current script version

* **v4.0.1 – Patch for Delete Button**

  * Fixed: Delete button in edit blip menu where it caused the game to crash after deleting a blip

* **v4.0.0 – Robust & Optimized for Performance**

  * Added: Automatic handling of the "Access Denied" error
  * Added: Full backward compatibility — existing INI files automatically migrate to JSON on first launch (a backup is saved as Advanced Custom Blips.ini.backup)
  * Added: Transparency field for blip properties, allowing control over how transparent a blip is
  * Added: Group field for blips to make them easier to manage in the "Manage Existing Blips" menu
  * Added: Ability to type icon/color names or IDs directly (not just scroll through lists in the "Add New Blip" and edit blip menus)
  * Added: Automatic and manual update checker
  * Added: Reset to Defaults button in the "Global Settings" menu that restores all options to their default values
  * Improved: Significantly improved the performance and smoothness of the script
  * Improved: Switched from INI to JSON format
  * Improved: Complete naming freedom — both blip and group names can now contain **ANY characters** (underscores, spaces, special symbols, empty names, whitespace — no more restrictions, thanks to JSON)
  * Improved: Clarified all script messages and item descriptions
  * Improved: From version 4.0.0 and beyond, the script follows Semantic Versioning
  * Improved: X, Y, and Z fields in the "Add New Blip" menu now behave as follows: Uses the provided coordinates if all are present and valid. Fills in only missing coordinates (X, Y, or Z) using the player’s current position. Falls back entirely to the player’s position if the provided coordinates are invalid. Stops execution if a valid player cannot be obtained.
  * Improved: Normalized both "." (dot) and "," (comma) as decimal separators — the script no longer differentiates between them; use whichever you prefer
  * Improved: Preview blip now updates its position with the player
  * Improved: Enhanced key assignment visuals in the "Global Settings" menu
  * Improved: Reworked the "Manage Existing Blips" menu — it now lists all groups first; each group contains its blips, and each blip includes its own edit menu
  * Improved: Support for decimal numbers in Add-On blips .txt files
  * Improved: Settings are now stored in a dedicated file called "Advanced Custom Blips Settings.json"
  * Fixed: Flash interval having no effect on blips
  * Fixed: Color mismatch for on-screen icons (Colors 6, 7, and 38)
  * Fixed: "Save Changes" button in the "Global Settings" menu incorrectly saving keys to the INI file, which disabled menu interactions
  * Fixed: Coordinates display system randomly stopping or not appearing at all
  * Fixed: Color field in the "Edit (blip name)" menu
  * Fixed: DirectoryNotFoundException error when loading Add-On blips if the directory does not exist
  * Fixed: 1–2 seconds of freezing when enabling or disabling category blips
  * Fixed: Custom blip names disappearing
  * Fixed: Disappearing vanilla blips when a large number of custom blips exist
  * Removed: Flash and flash interval effects from the preview blip
  * Removed: Saving actual names of blip icons/colors in INI files (not just IDs). JSON is stricter; manual editing will return in future updates
  * Removed: Teleport menu

* **v3.0 – Improved**

  * Added: On-screen icons selection with preview of their color
  * Added: Support for "[Addon Blips](https://www.gta5-mods.com/scripts/addon-blips)" txt files
  * Added: More blips
  * Improved: Upgraded to LemonUI menu
  * Fixed: Flickering of the menu
  * Fixed: Blip name in "Edit (blip name)" menu
  * Fixed: Mismatch between blip icon ID and blip icon name

* **v2.0 – Major Overhaul**

  * Added: Full NativeUI in-game menu (add, edit, copy, delete, and teleport to custom blips)
  * Added: Real-time preview blip in front of player
  * Added: Configurable hotkeys (reload, toggle visibility, open menu, and show coordinates)
  * Added: Predefined locations of gas stations, fire departments, police departments, markets, medical centers, ATMs, and metro stations. Each category can be toggled on or off individually via the in-game NativeUI menu (all categories are toggled off by default)
  * Added: Save actual names of Blip Icons/Colors in INI (not just IDs). Useful for manual editing users
  * Improved: Error handling & coordinate display system

* **v1.0 – Initial Release**

  * Basic INI-based blip creation
