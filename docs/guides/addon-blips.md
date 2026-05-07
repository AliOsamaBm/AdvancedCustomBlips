# Add-On Blips Integration Guide

This guide explains how to load `.txt` blip files from the [Addon Blips](https://www.gta5-mods.com/scripts/addon-blips) mod into Advanced Custom Blips.

---

## 📁 Step 1: Create the AddonBlips Folder

Navigate to your GTA V directory (where `GTA5.exe` is located), then open the `scripts` folder.

Inside `scripts`, make sure a folder named:

```
AddonBlips
```

exists.

If it does not exist, create it **with the exact same name**.

---

## 📄 Step 2: Add Blip Files

Copy the `.txt` blip files from the Addon Blips mod into the `AddonBlips` folder:

```
GTA V/
└── scripts/
    └── AddonBlips/
        ├── example1.txt
        ├── example2.txt
        └── ...
```

> ⚠️ You do **NOT** need to install the full Addon Blips mod.
> Only the `.txt` files are required.

---

## ⚙️ Step 3: Enable Add-On Blips

You can enable Add-On Blips in **two ways**:

---

### Method 1: Using the JSON Settings File

1. Go to:

   ```
   GTA V/scripts/
   ```
2. Open:

   ```
   Advanced Custom Blips Settings.json
   ```
3. Find:

   ```json
   "EnableAddOnBlips": false
   ```
4. Change it to:

   ```json
   "EnableAddOnBlips": true
   ```
5. Save the file

---

### Method 2: Using the In-Game Menu

1. Launch GTA V
2. Press **F10** to open the menu
3. Go to:

   ```
   Global Settings
   ```
4. Enable:

   ```
   Enable Add-On Blips
   ```
5. Select **Save Settings**

---

## 🔄 How It Works

* The script automatically loads all `.txt` files from the `AddonBlips` folder
* Blips are imported into:

  ```
  Advanced Custom Blips.json
  ```
* Disabling the feature removes them from the JSON file
* The original `.txt` files remain untouched, so no need to worry about the script deleting the original txt blips

---

## ⚠️ Important Notes

* Folder name must be exactly: `AddonBlips`
* Only `.txt` files are supported
* Invalid or malformed files will be skipped
* Large numbers of blips will impact menu performance

---

## ❓ Troubleshooting

**Blips not loading?**

Check:

* Folder name is correct (`AddonBlips`)
* Files are `.txt`
* Feature is enabled
* No errors in `Advanced Custom Blips Log.txt`

---
