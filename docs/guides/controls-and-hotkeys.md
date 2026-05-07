# Controls & Hotkeys

This guide lists all default key bindings used in **Advanced Custom Blips**.  
All keys are fully configurable via the in-game menu or in `Advanced Custom Blips Settings.json` settings file.

---

## Default Key Bindings

| Action | Default Key | Description |
|--------|------------|------------|
| Toggle Coordinates | `F1` | Show/hide the player's current coordinates (X, Y, Z) on screen |
| Reload Script | `F2` | Reloads the script and refreshes all blips |
| Toggle Blip Visibility | `F3` | Show/hide all custom blips loaded from the `Advanced Custom Blips.json` JSON file |
| Toggle Texture Sheet | `F5` | Switch between vanilla and modded texture sheets for on-screen icons |
| Open Menu | `F10` | Opens the main LemonUI menu |

---

## Notes

- All keys can be changed in:
  - **In-game menu → Global Settings**
  - OR via `Advanced Custom Blips Settings.json`
  
- Only keyboards are tested
  
## Disabling a Key

To disable a key, you can use one of the following methods:

### Method 1: JSON Settings File
Set the value to `None` for any of the following keys in:


`Advanced Custom Blips Settings.json` located in the `scripts` folder located in your GTA5 directory (where `GTA5.exe` is located)


- `ToggleCoordinatesKey`
- `ReloadBlipsKey`
- `ToggleBlipsVisibilityKey`
- `ToggleModdedTextureSheetKey`
- `OpenMenuKey`

---

### Method 2: In-Game Menu
1. Open the menu (`F10`)
2. Go to **Global Settings**
3. Select the key you want to disable
4. Scroll through the key list until it shows `None`
5. Select it and press **Save Changes**

---

Once set to `None`, the key will be completely disabled.

- When assigning a new key in-game:
1. Select the key option
2. Press **Enter**
3. Press the desired key

---

## Tips

- If a key does not respond:
- Make sure it is not set to `None`
- Check for conflicts with other mods
- Avoid assigning commonly used gameplay keys to prevent conflicts