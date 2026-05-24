# Frequently Asked Questions (FAQ)

---

## 🐛 I found a bug or mod conflict. How do I report it?

Please open an issue on GitHub or contact me through the mod page.

Include:

* Steps to reproduce the issue
* Your `Advanced Custom Blips Log.txt` file
* What you expected vs what actually happened

Providing clear steps helps identify the problem much faster.

---

## ✍️ Can I add blips manually (edit JSON directly)?

Yes, but it is not recommended unless you know what you are doing.

* The JSON format is strict
* Incorrect values may break loading
* As of v4.0.0, icon and color must use **IDs**, not names

👉 Using the in-game menu is safer and easier.

---

## 🔄 Will updating from older versions delete my blips?

No.

When upgrading from older INI-based versions:

* Your data is automatically migrated to JSON
* A backup file (`.ini.backup`) is created
* Migrated blips are placed in the **"Migrated"** group

Make sure your original INI file is still present during first launch.

---

## 🎮 Does this mod affect performance (FPS)?

No, except in one case:

* Opening **"Manage Existing Blips"** with a large number of blips (100+) may cause a short freeze

This happens due to menu generation and is being optimized.

---

## 🧩 What does the "Group" field do?

The **Group** field is only for organization inside the menu.

* It does NOT affect how blips appear on the main map
* It does NOT create in-game categories

Think of it as a folder or label for easier management.

---

## ⌨️ How do I disable a key?

You have two options (Detailed explaination is in docs -> guides -> controls-and-hotkeys.md):

### 1. In-game

* Open menu (**F10**)
* Go to **Global Settings**
* Set the key to **"None"**
* Save changes

### 2. JSON file

* Open `Advanced Custom Blips Settings.json`
* Replace the key value with `"None"`

---

## 📦 How do I use Add-On Blips?
(Detailed explaination is in docs -> guides -> addon-blips.md)

1. Place the **Addon Blips folder** (with `.txt` files) inside your `scripts` folder
2. Open the menu (**F10**)
3. Go to **Global Settings**
4. Enable **"Add-On Blips"**
5. Save

### Notes:

* You do NOT need the full Add-On Blips mod
* Only the `.txt` files are required
* Disabling the option removes them from your JSON, not from disk

---

## 🧪 Is this compatible with GTA V Enhanced?

Yes, this mod is compatible with both the Legacy and Enhanced versions of GTA V.

---

## 🎯 Why are some icons not clickable?

Some blip icons:

* Do not have known IDs
* Cannot be properly mapped for selection

You can still:

* Manually enter icon IDs
* Scroll through the icon list

---

## 🔥 Why does the camera move when selecting icons?

This happens when using the **vanilla texture sheet**.

* Mouse input affects the camera
* This does NOT occur with modded texture sheets

A fix is being considered.

---

## ⚖️ Can I reuse or modify this project?

Yes.

This project is licensed under the **GNU GPL v2 (or later)**.

That means you are free to:

* Use
* Modify
* Share
* Redistribute

As long as you follow the license terms.

---

## 💡 I have a suggestion. Where should I share it?

Open an issue on GitHub and describe your idea clearly.

Suggestions, improvements, and feedback are always welcome.
