/*
 * Advanced Custom Blips
 * Copyright (C) 2026 Ali Osama
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.IO;
using System.Drawing;
using System.Net.Http;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

using IniParser;
using IniParser.Model;
using LemonUI;
using LemonUI.Menus;
using Newtonsoft.Json;

using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;
using Control = GTA.Control;
using Screen = GTA.UI.Screen;

namespace AdvancedCustomBlips
{
    public sealed class Main : Script
    {
        private readonly FileIniDataParser _iniParser;
        private NativeMenu _manageBlipsMenu;

        private const string CURRENT_SCRIPT_VERSION = "v4.2.0"; 
        private const string MOD_PAGE_URL = "https://www.gta5-mods.com/scripts/advanced-custom-blips-fe7d9d03-1fc1-4088-9fae-005dc2b1a270";
        private const string VERSION_REGEX_PATTERN = @"Advanced Custom Blips\s+v(\d+\.\d+\.\d+)";

        private const string _createGroupLabel = "~y~Create New Group";
        private const int _defaultInterval = 1000;
        private const int _activeInterval = 1;
        private const int TYPE_SPEED = 25; 

        private bool _areBlipsVisible = true;
        private bool _waitingForKeyAssignment = false;
        private bool _showCoordsOnScreen = false;
        private bool _didWeInitializePreviewBlip = false;
        private bool _shouldUseModdedTextureSheet = false;

        private int _alpha = 0;
        private string _fullOverlayText;
        private string _typedOverlayText;
        private int _typingIndex;
        private int _lastTypeTime;

        private NativeListItem<string> _pendingListItem = null;
        private PendingKeyBind _pendingKeyBind = PendingKeyBind.None;
        private readonly string[] _keysNames = Enum.GetNames(typeof(Keys));

        private Blip _previewBlip;

        private readonly string _logFilePath = Path.Combine("scripts", "Advanced Custom Blips Log.txt");
        private readonly string _jsonFilePath = Path.Combine("scripts", "Advanced Custom Blips.json");
        private readonly string _addOnBlipsFilesPath = Path.Combine("scripts", "AddonBlips");
        private readonly string _settingsFilePath = Path.Combine("scripts", "Advanced Custom Blips Settings.json");
        private const string _iniFilePath = "scripts\\Advanced Custom Blips.ini";
        private const string _iniBackupPath = "scripts\\Advanced Custom Blips.ini.backup";
        private bool _migrationCompleted = false;
        private SettingsData _globalSettings = new SettingsData();
        private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
        private StreamWriter _logWriter;

        private readonly Scaleform _scaleform = Scaleform.RequestMovie("instructional_buttons");
        private readonly Sprite _sprite = new Sprite("minimap", "blips_texturesheet_ng_2", new SizeF(640f, 360f), new PointF(Screen.Width * 0.49f, 5));
        private readonly Sprite _sprite1 = new Sprite("minimap", "blips_texturesheet_ng", new SizeF(640f, 350f), new PointF(Screen.Width * 0.49f, Screen.Height * 0.51f));
        private readonly Sprite _sprite2 = new Sprite("minimap", "blips_texturesheet_ng_3", new SizeF(520f, 230f), new PointF(Screen.Width * 0.01f, Screen.Height * 0.56f));
        private CustomSprite _customSprite;
        private CustomSprite _customSprite1;
        private CustomSprite _customSprite2;
        private CustomSprite _mousePointer;

        private readonly ObjectPool _uiMenuPool = new ObjectPool();
        private NativeMenu _mainMenu;
        private NativeMenu _addBlipMenu;
        private NativeMenu _settingsMenu;
        private NativeMenu _categoryBlipMenu;
        private NativeListItem<string> _colorItem;
        private NativeListItem<string> _iconItem;
        private NativeListItem<string> _groupItem;

        private int _lastKnownColorItemIndex;
        private float _lastKnownScreenWidth;
        private float _lastKnownScreenHeight;
        private bool _shouldRefreshGroupItems;
        private string _pendingGroupSelection;
        private Vector3 _lastKnownPreviewPos;

        private string _groupStorageTemp = "Custom";
        private string _nameItemStorageTemp;
        private float? _sizeItemStorageTemp;
        private float? _xItemStorageTemp;
        private float? _yItemStorageTemp;
        private float? _zItemStorageTemp;
        private int? _flashIntervalItemStorageTemp;
        private int? _transparencyItemStorageTemp;

        private readonly ExtendedBlipSprite[] _spriteValues = (ExtendedBlipSprite[])Enum.GetValues(typeof(ExtendedBlipSprite));
        private readonly Dictionary<Type, Array> _enumCache = new Dictionary<Type, Array>();
        private static readonly Dictionary<string, Assembly> _assemblyCache = new Dictionary<string, Assembly>();
        private readonly HashSet<BlipData> _blipDataCache = new HashSet<BlipData>();
        private readonly List<string> _tempFiles = new List<string>();
        private readonly List<Blip> _activeBlips = new List<Blip>();

        private readonly List<string> _availableGroups = new List<string>();
        private readonly HashSet<string> _availableGroupsSet = new HashSet<string>();
        private readonly Dictionary<int, string> _blipToGroupMap = new Dictionary<int, string>();
        private readonly Queue<BlipData> _blipQueue = new Queue<BlipData>();
        private const int _blipsPerTick = 10; 

        private readonly PredefinedBlipData[] _gasStationsCategoryBlips = new PredefinedBlipData[]
                {
new PredefinedBlipData("Gas Station", new Vector3(264.00f, 2609.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(50.00f, 2776.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(1212.00f, 2657.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(2537.00f, 2593.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(2683.00f, 3264.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(1687.00f, 4929.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(1702.00f, 6418.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(180.00f, 6603.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(-90.00f, 6415.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(-2555.00f, 2334.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(-1799.00f, 803.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(-1434.00f, -274.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(-2097.00f, -320.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(-724.00f, -935.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(-526.00f, -1212.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(-71.00f, -1762.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(265.00f, -1261.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(819.00f, -1027.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(1209.00f, -1402.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(1182.00f, -330.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(620.93f, 269.29f, 103.09f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(2581.00f, 362.00f, 30.00f), 361, 0, 1.0f, false, true),
new PredefinedBlipData("Gas Station", new Vector3(2005.00f, 3775.00f, 30.00f), 361, 0, 1.0f, false, true),
                };
        private readonly PredefinedBlipData[] _marketCategoryBlips = new PredefinedBlipData[]
        {
new PredefinedBlipData("Market", new Vector3(-50.80f, -1753.50f, 33.97f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(1697.00f, 4923.00f, 45.63f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(1159.60f, -326.74f, 69.22f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(-1487.00f, -379.00f, 43.78f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(544.00f, 2673.00f, 45.90f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(-3243.00f, 1004.00f, 16.39f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(376.45f, 322.72f, 103.44f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(-705.00f, -913.00f, 23.77f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(1963.00f, 3744.00f, 37.97f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(29.10f, -1349.54f, 35.46f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(1141.88f, -980.81f, 46.20f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(-2973.82f, 390.93f, 20.01f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(-3038.14f, 589.64f, 7.82f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(-1822.00f, 788.09f, 142.70f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(2559.65f, 385.30f, 115.11f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(1166.62f, 2703.37f, 42.75f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(2682.09f, 3282.30f, 60.69f), 59, 0, 1.0f, false, true),
new PredefinedBlipData("Market", new Vector3(1730.93f, 6411.21f, 39.11f), 59, 0, 1.0f, false, true),
        };
        private readonly PredefinedBlipData[] _policeDepartmentCategoryBlips = new PredefinedBlipData[]
        {
new PredefinedBlipData("Police Department", new Vector3(447.30f, -993.10f, 73.69f), 60, 0, 1.0f, false, true),
new PredefinedBlipData("Police Department", new Vector3(371.22f, -1593.35f, 36.95f), 60, 0, 1.0f, false, true),
new PredefinedBlipData("Police Department", new Vector3(826.15f, -1290.16f, 34.37f), 60, 0, 1.0f, false, true),
new PredefinedBlipData("Police Department", new Vector3(-1319.27f, -1521.06f, 10.97f), 60, 0, 1.0f, false, true),
new PredefinedBlipData("Police Department", new Vector3(-1082.88f, -828.94f, 31.25f), 60, 0, 1.0f, false, true),
new PredefinedBlipData("Police Department", new Vector3(-561.56f, -132.08f, 38.22f), 60, 0, 1.0f, false, true),
new PredefinedBlipData("Police Department", new Vector3(585.90f, -5.18f, 101.25f), 60, 0, 1.0f, false, true),
new PredefinedBlipData("Police Department", new Vector3(383.24f, 796.51f, 195.10f), 60, 0, 1.0f, false, true),
new PredefinedBlipData("Police Department", new Vector3(1856.94f, 3680.00f, 33.79f), 60, 0, 1.0f, false, true),
new PredefinedBlipData("Police Department", new Vector3(-440.81f, 6007.08f, 40.26f), 60, 0, 1.0f, false, true),
        };
        private readonly PredefinedBlipData[] _fireDepartmentCategoryBlips = new PredefinedBlipData[]
        {
new PredefinedBlipData("Fire Department", new Vector3(-644.46f, -114.09f, 37.91f), 942, 47, 1.0f, false, true),
new PredefinedBlipData("Fire Department", new Vector3(1200.60f, -1459.13f, 34.77f), 942, 47, 1.0f, false, true),
new PredefinedBlipData("Fire Department", new Vector3(214.89f, -1639.34f, 29.60f), 942, 47, 1.0f, false, true),
new PredefinedBlipData("Fire Department", new Vector3(-2113.49f, 2834.13f, 32.81f), 942, 47, 1.0f, false, true),
new PredefinedBlipData("Fire Department", new Vector3(1697.68f, 3585.90f, 40.33f), 942, 47, 1.0f, false, true),
new PredefinedBlipData("Fire Department", new Vector3(-381.85f, 6121.45f, 31.48f), 942, 47, 1.0f, false, true),
new PredefinedBlipData("Fire Department", new Vector3(-1034.89f, -2383.48f, 14.09f), 942, 47, 1.0f, false, true),
        };
        private readonly PredefinedBlipData[] _ATMCategoryBlips = new PredefinedBlipData[]
        {
new PredefinedBlipData("ATM", new Vector3(-301.82f, -830.02f, 32.41f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-303.43f, -829.76f, 32.42f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(5.19f, -919.80f, 29.56f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(146.04f, -1035.12f, 29.34f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(147.60f, -1035.69f, 29.34f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(114.50f, -776.50f, 31.42f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(111.18f, -775.31f, 31.44f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(112.62f, -819.31f, 31.34f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-28.11f, -724.56f, 44.23f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-30.29f, -723.77f, 44.23f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-254.31f, -692.41f, 33.61f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-256.23f, -716.01f, 33.53f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-258.86f, -723.48f, 33.47f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-203.84f, -861.38f, 30.27f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(119.13f, -883.75f, 31.12f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(295.69f, -896.06f, 29.21f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(527.29f, -160.61f, 57.09f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(380.81f, 323.43f, 103.57f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(89.62f, 2.40f, 68.31f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-165.07f, 234.81f, 94.92f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-165.08f, 232.69f, 94.92f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(1153.75f, -326.80f, 69.21f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(1077.79f, -776.45f, 58.24f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(1166.88f, -456.10f, 66.81f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-57.73f, -92.72f, 57.78f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(357.00f, 173.44f, 103.07f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(238.23f, 215.96f, 106.29f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(237.83f, 216.82f, 106.29f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(237.37f, 217.81f, 106.29f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(236.98f, 218.83f, 106.29f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(236.49f, 219.67f, 106.29f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-821.62f, -1082.00f, 11.13f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(2564.59f, 2584.81f, 38.08f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(1172.46f, 2702.59f, 38.17f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(1171.52f, 2702.59f, 28.18f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(540.33f, 2671.02f, 42.16f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-1091.35f, 2708.54f, 18.96f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(2683.10f, 3286.54f, 55.24f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(1822.68f, 3683.04f, 34.28f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(1968.09f, 3743.65f, 32.34f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(1702.94f, 4933.52f, 42.06f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(1701.26f, 6426.52f, 32.76f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(1735.27f, 6410.50f, 35.04f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-386.89f, 6046.11f, 31.50f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-97.32f, 6455.32f, 31.47f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-95.44f, 6457.18f, 31.46f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(155.78f, 6642.87f, 31.60f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(174.14f, 6637.80f, 31.57f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-3144.29f, 1127.56f, 20.86f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-3240.60f, 1008.61f, 12.83f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-3040.81f, 593.08f, 7.91f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-2975.09f, 380.32f, 15.00f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-1827.19f, 784.91f, 138.30f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(2558.34f, 389.47f, 108.62f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-1410.26f, -98.72f, 52.43f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-1409.61f, -100.58f, 52.38f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-1430.12f, -211.01f, 46.50f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-1415.89f, -211.95f, 46.50f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-1204.93f, -326.28f, 37.83f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-1205.69f, -324.86f, 37.86f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-866.64f, -187.81f, 37.84f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-867.64f, -186.08f, 37.84f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-846.21f, -341.24f, 38.68f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-846.70f, -340.28f, 38.68f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-721.06f, -415.60f, 34.98f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-1315.88f, -834.67f, 16.96f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-1315.00f, -835.84f, 16.96f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-1305.31f, -706.48f, 25.32f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-56.90f, -1752.16f, 29.42f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(33.01f, -1348.16f, 29.50f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(130.08f, -1292.67f, 29.27f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(129.69f, -1292.03f, 29.27f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(129.19f, -1291.15f, 29.27f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-618.32f, -706.95f, 30.05f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-618.25f, -708.78f, 30.05f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-717.61f, -915.82f, 19.22f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-712.98f, -819.00f, 23.73f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-710.18f, -818.95f, 23.73f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-660.57f, -854.07f, 24.49f), 434, 3, 0.7f, false, true),
new PredefinedBlipData("ATM", new Vector3(-537.75f, -854.42f, 29.30f), 434, 3, 0.7f, false, true),
        };
        private readonly PredefinedBlipData[] _metroStationCategoryBlips = new PredefinedBlipData[]
        {
new PredefinedBlipData("Metro Station", new Vector3(-825.84f, -112.67f, 27.96f), 951, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(274.72f, -1204.29f, 38.90f), 951, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-540.97f, -1280.31f, 26.90f), 951, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-947.24f, -2339.23f, 4.51f), 951, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-1040.81f, -2743.34f, 13.45f), 951, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-245.46f, -335.18f, 29.48f), 951, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-1369.87f, -527.97f, 29.82f), 951, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-490.10f, -697.08f, 32.73f), 951, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-215.27f, -1035.11f, 30.14f), 951, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(119.46f, -1730.48f, 30.11f), 951, 1, 0.9f, false, true),
        };
        private readonly PredefinedBlipData[] _medicalCenterCategoryBlips = new PredefinedBlipData[]
        {
new PredefinedBlipData("Medical Center", new Vector3(355.37f, -596.21f, 74.17f), 61, 0, 1.0f, false, true),
new PredefinedBlipData("Medical Center", new Vector3(341.01f, -1396.80f, 32.51f), 61, 0, 1.0f, false, true),
new PredefinedBlipData("Medical Center", new Vector3(-468.68f, -337.11f, 91.01f), 61, 0, 1.0f, false, true),
new PredefinedBlipData("Medical Center", new Vector3(1840.82f, 3670.38f, 33.68f), 61, 0, 1.0f, false, true),
new PredefinedBlipData("Medical Center", new Vector3(-243.96f, 6327.12f, 37.62f), 61, 0, 1.0f, false, true),
        };

        private readonly Color[] _blipColorsArray = new Color[]
        {
            Color.White, 
            ColorTranslator.FromHtml("#e03233"),
            ColorTranslator.FromHtml("#72cc72"),
            ColorTranslator.FromHtml("#5cb6e8"),
            ColorTranslator.FromHtml("#f0f0f0"),
            ColorTranslator.FromHtml("#f0c84f"),
            ColorTranslator.FromHtml("#4e8a5b"),
            ColorTranslator.FromHtml("#97bcff"),
            ColorTranslator.FromHtml("#ff7bc4"),
            ColorTranslator.FromHtml("#f79f7b"),
            ColorTranslator.FromHtml("#b29084"),
            ColorTranslator.FromHtml("#8dcea6"),
            ColorTranslator.FromHtml("#72a9ae"),
            ColorTranslator.FromHtml("#d3d1e7"),
            ColorTranslator.FromHtml("#8f7f99"),
            ColorTranslator.FromHtml("#6ac5c0"),
            ColorTranslator.FromHtml("#d5c498"),
            ColorTranslator.FromHtml("#ea8e4f"),
            ColorTranslator.FromHtml("#98cbea"),
            ColorTranslator.FromHtml("#b26287"),
            ColorTranslator.FromHtml("#908d7a"),
            ColorTranslator.FromHtml("#a5755e"),
            ColorTranslator.FromHtml("#b0a7a8"),
            ColorTranslator.FromHtml("#e88e9a"),
            ColorTranslator.FromHtml("#bcd65b"),
            ColorTranslator.FromHtml("#0d7b56"),
            ColorTranslator.FromHtml("#7cc4ff"),
            ColorTranslator.FromHtml("#ac3ce6"),
            ColorTranslator.FromHtml("#cda90d"),
            ColorTranslator.FromHtml("#4763ad"),
            ColorTranslator.FromHtml("#29a6b8"),
            ColorTranslator.FromHtml("#ba9d7d"),
            ColorTranslator.FromHtml("#c9e0ff"),
            ColorTranslator.FromHtml("#f0f096"),
            ColorTranslator.FromHtml("#ed8ca0"),
            ColorTranslator.FromHtml("#fa8a89"),
            ColorTranslator.FromHtml("#fcf0a6"),
            ColorTranslator.FromHtml("#f0f0f0"),
            ColorTranslator.FromHtml("#f0c850"),
            ColorTranslator.FromHtml("#9a9a9a"),
            ColorTranslator.FromHtml("#4d4d4d"),
            ColorTranslator.FromHtml("#f19998"),
            ColorTranslator.FromHtml("#65b4d3"),
            ColorTranslator.FromHtml("#abeeab"),
            ColorTranslator.FromHtml("#ffa356"),
            ColorTranslator.FromHtml("#f0f0f0"),
            ColorTranslator.FromHtml("#ebef1e"),
            ColorTranslator.FromHtml("#ff950e"),
            ColorTranslator.FromHtml("#f63ca1"),
            ColorTranslator.FromHtml("#e03233"),
            ColorTranslator.FromHtml("#8466e2"),
            ColorTranslator.FromHtml("#ff8554"),
            ColorTranslator.FromHtml("#386638"),
            ColorTranslator.FromHtml("#aedbf2"),
            ColorTranslator.FromHtml("#2f5c73"),
            ColorTranslator.FromHtml("#9b9b9b"),
            ColorTranslator.FromHtml("#7e6b29"),
            ColorTranslator.FromHtml("#5eb6e6"),
            ColorTranslator.FromHtml("#43396e"),
            ColorTranslator.FromHtml("#e03233"),
            ColorTranslator.FromHtml("#f0c84f"),
            ColorTranslator.FromHtml("#cb3694"),
            ColorTranslator.FromHtml("#cdcdcd"),
            ColorTranslator.FromHtml("#1d6498"),
            ColorTranslator.FromHtml("#d6740f"),
            ColorTranslator.FromHtml("#887d8e"),
            ColorTranslator.FromHtml("#f0c84f"),
            ColorTranslator.FromHtml("#5eb6e6"),
            ColorTranslator.FromHtml("#5eb6e6"),
            ColorTranslator.FromHtml("#72cc72"),
            ColorTranslator.FromHtml("#f0c84f"),
            ColorTranslator.FromHtml("#f0c84f"),
            ColorTranslator.FromHtml("#2a2a22"),
            ColorTranslator.FromHtml("#f0c84f"),
            ColorTranslator.FromHtml("#5eb6e6"),
            ColorTranslator.FromHtml("#e03233"),
            ColorTranslator.FromHtml("#711918"),
            ColorTranslator.FromHtml("#5eb6e6"),
            ColorTranslator.FromHtml("#2f5c73"),
            ColorTranslator.FromHtml("#522f29"),
            ColorTranslator.FromHtml("#414f4f"),
            ColorTranslator.FromHtml("#f0a001"),
            ColorTranslator.FromHtml("#9fc8a6"),
            ColorTranslator.FromHtml("#a44bf1"),
            ColorTranslator.FromHtml("#5eb6e6"),
            ColorTranslator.FromHtml("#222419"),
        };
        private readonly IconEntry[] _onScreenIconsMap = new[]
        {
            
            
            
new IconEntry((ExtendedBlipSprite)512, 646, 15),
new IconEntry((ExtendedBlipSprite)533, 646, 36),
new IconEntry((ExtendedBlipSprite)563, 646, 57),
new IconEntry((ExtendedBlipSprite)579, 646, 79),
new IconEntry((ExtendedBlipSprite)595, 646, 104),
new IconEntry((ExtendedBlipSprite)611, 646, 122),
new IconEntry((ExtendedBlipSprite)627, 646, 150),
new IconEntry((ExtendedBlipSprite)644, 646, 168),
new IconEntry((ExtendedBlipSprite)668, 646, 192),
new IconEntry((ExtendedBlipSprite)676, 646, 218),
new IconEntry((ExtendedBlipSprite)728, 646, 239),
new IconEntry((ExtendedBlipSprite)748, 646, 263),
new IconEntry((ExtendedBlipSprite)762, 646, 286),
new IconEntry((ExtendedBlipSprite)778, 646, 307),
new IconEntry((ExtendedBlipSprite)824, 646, 351),

            
new IconEntry((ExtendedBlipSprite)513, 686, 16),
new IconEntry((ExtendedBlipSprite)534, 686, 38),
new IconEntry((ExtendedBlipSprite)564, 686, 59),
new IconEntry((ExtendedBlipSprite)580, 686, 80),
new IconEntry((ExtendedBlipSprite)596, 686, 105),
new IconEntry((ExtendedBlipSprite)612, 686, 125),
new IconEntry((ExtendedBlipSprite)628, 686, 151),
new IconEntry((ExtendedBlipSprite)645, 686, 172),
new IconEntry((ExtendedBlipSprite)665, 686, 196),
new IconEntry((ExtendedBlipSprite)685, 686, 218),
new IconEntry((ExtendedBlipSprite)730, 686, 241),
new IconEntry((ExtendedBlipSprite)747, 686, 261),
new IconEntry((ExtendedBlipSprite)761, 686, 281),
new IconEntry((ExtendedBlipSprite)782, 686, 305),
new IconEntry((ExtendedBlipSprite)825, 686, 351),

            
new IconEntry((ExtendedBlipSprite)514, 726, 16),
new IconEntry((ExtendedBlipSprite)543, 726, 40),
new IconEntry((ExtendedBlipSprite)565, 726, 59),
new IconEntry((ExtendedBlipSprite)581, 726, 80),
new IconEntry((ExtendedBlipSprite)597, 726, 103),
new IconEntry((ExtendedBlipSprite)613, 726, 127),
new IconEntry((ExtendedBlipSprite)629, 726, 150),
new IconEntry((ExtendedBlipSprite)646, 726, 170),
new IconEntry((ExtendedBlipSprite)666, 726, 192),
new IconEntry((ExtendedBlipSprite)678, 726, 216),
new IconEntry((ExtendedBlipSprite)731, 726, 240),
new IconEntry((ExtendedBlipSprite)755, 726, 260),
new IconEntry((ExtendedBlipSprite)758, 726, 285),
new IconEntry((ExtendedBlipSprite)779, 726, 305),
new IconEntry((ExtendedBlipSprite)823, 726, 353),

            
new IconEntry((ExtendedBlipSprite)515, 766, 15),
new IconEntry((ExtendedBlipSprite)545, 766, 39),
new IconEntry((ExtendedBlipSprite)566, 766, 59),
new IconEntry((ExtendedBlipSprite)582, 766, 80),
new IconEntry((ExtendedBlipSprite)598, 766, 105),
new IconEntry((ExtendedBlipSprite)614, 766, 126),
new IconEntry((ExtendedBlipSprite)631, 766, 151),
new IconEntry((ExtendedBlipSprite)647, 766, 173),
new IconEntry((ExtendedBlipSprite)660, 766, 194),
new IconEntry((ExtendedBlipSprite)679, 766, 216),
new IconEntry((ExtendedBlipSprite)732, 766, 241),
new IconEntry((ExtendedBlipSprite)759, 766, 264),
new IconEntry((ExtendedBlipSprite)772, 766, 287),
new IconEntry((ExtendedBlipSprite)784, 766, 310),
new IconEntry((ExtendedBlipSprite)799, 766, 330),
new IconEntry((ExtendedBlipSprite)820, 766, 350),

            
new IconEntry((ExtendedBlipSprite)521, 806, 15),
new IconEntry((ExtendedBlipSprite)546, 806, 33),
new IconEntry((ExtendedBlipSprite)567, 806, 58),
new IconEntry((ExtendedBlipSprite)583, 806, 80),
new IconEntry((ExtendedBlipSprite)599, 806, 103),
new IconEntry((ExtendedBlipSprite)615, 806, 126),
new IconEntry((ExtendedBlipSprite)632, 806, 151),
new IconEntry((ExtendedBlipSprite)648, 806, 171),
new IconEntry((ExtendedBlipSprite)658, 806, 192),
new IconEntry((ExtendedBlipSprite)683, 806, 219),
new IconEntry((ExtendedBlipSprite)733, 806, 242),
new IconEntry((ExtendedBlipSprite)752, 806, 262),
new IconEntry((ExtendedBlipSprite)766, 806, 287),
new IconEntry((ExtendedBlipSprite)777, 806, 306),
new IconEntry((ExtendedBlipSprite)800, 806, 329),
new IconEntry((ExtendedBlipSprite)821, 806, 350),

            
new IconEntry((ExtendedBlipSprite)523, 846, 15),
new IconEntry((ExtendedBlipSprite)547, 846, 32),
new IconEntry((ExtendedBlipSprite)568, 846, 59),
new IconEntry((ExtendedBlipSprite)584, 846, 80),
new IconEntry((ExtendedBlipSprite)600, 846, 104),
new IconEntry((ExtendedBlipSprite)616, 846, 125),
new IconEntry((ExtendedBlipSprite)633, 846, 150),
new IconEntry((ExtendedBlipSprite)649, 846, 169),
new IconEntry((ExtendedBlipSprite)659, 846, 194),
new IconEntry((ExtendedBlipSprite)684, 846, 217),
new IconEntry((ExtendedBlipSprite)735, 846, 238),
new IconEntry((ExtendedBlipSprite)751, 846, 261),
new IconEntry((ExtendedBlipSprite)771, 846, 284),
new IconEntry((ExtendedBlipSprite)786, 846, 302),
new IconEntry((ExtendedBlipSprite)801, 846, 327),
new IconEntry((ExtendedBlipSprite)818, 846, 349),

            
new IconEntry((ExtendedBlipSprite)522, 886, 16),
new IconEntry((ExtendedBlipSprite)550, 886, 37),
new IconEntry((ExtendedBlipSprite)569, 886, 59),
new IconEntry((ExtendedBlipSprite)585, 886, 79),
new IconEntry((ExtendedBlipSprite)601, 886, 103),
new IconEntry((ExtendedBlipSprite)617, 886, 125),
new IconEntry((ExtendedBlipSprite)634, 886, 147),
new IconEntry((ExtendedBlipSprite)650, 886, 170),
new IconEntry((ExtendedBlipSprite)669, 886, 193),
new IconEntry((ExtendedBlipSprite)682, 886, 218),
new IconEntry((ExtendedBlipSprite)736, 886, 238),
new IconEntry((ExtendedBlipSprite)754, 886, 262),
new IconEntry((ExtendedBlipSprite)773, 886, 284),
new IconEntry((ExtendedBlipSprite)780, 886, 305),
new IconEntry((ExtendedBlipSprite)785, 886, 327),
new IconEntry((ExtendedBlipSprite)812, 886, 351),

            
new IconEntry((ExtendedBlipSprite)524, 926, 17),
new IconEntry((ExtendedBlipSprite)548, 926, 36),
new IconEntry((ExtendedBlipSprite)570, 926, 58),
new IconEntry((ExtendedBlipSprite)586, 926, 83),
new IconEntry((ExtendedBlipSprite)602, 926, 105),
new IconEntry((ExtendedBlipSprite)618, 926, 127),
new IconEntry((ExtendedBlipSprite)635, 926, 149),
new IconEntry((ExtendedBlipSprite)651, 926, 172),
new IconEntry((ExtendedBlipSprite)662, 926, 192),
new IconEntry((ExtendedBlipSprite)680, 926, 215),
new IconEntry((ExtendedBlipSprite)734, 926, 239),
new IconEntry((ExtendedBlipSprite)757, 926, 261),
new IconEntry((ExtendedBlipSprite)774, 926, 283),
new IconEntry((ExtendedBlipSprite)788, 926, 308),
new IconEntry((ExtendedBlipSprite)802, 937, 324),  
new IconEntry((ExtendedBlipSprite)826, 926, 351),

            
new IconEntry((ExtendedBlipSprite)525, 966, 15),
new IconEntry((ExtendedBlipSprite)549, 966, 38),
new IconEntry((ExtendedBlipSprite)571, 966, 55),
new IconEntry((ExtendedBlipSprite)587, 966, 82),
new IconEntry((ExtendedBlipSprite)603, 966, 103),
new IconEntry((ExtendedBlipSprite)619, 966, 129),
new IconEntry((ExtendedBlipSprite)636, 966, 152),
new IconEntry((ExtendedBlipSprite)652, 966, 174),
new IconEntry((ExtendedBlipSprite)663, 966, 194),
new IconEntry((ExtendedBlipSprite)681, 966, 215),
new IconEntry((ExtendedBlipSprite)737, 966, 237),
new IconEntry((ExtendedBlipSprite)745, 966, 262),
new IconEntry((ExtendedBlipSprite)767, 966, 284),
new IconEntry((ExtendedBlipSprite)789, 966, 311),
new IconEntry((ExtendedBlipSprite)806, 966, 327),
new IconEntry((ExtendedBlipSprite)813, 966, 353),

            
new IconEntry((ExtendedBlipSprite)526, 1006, 14),
new IconEntry((ExtendedBlipSprite)556, 1006, 39),
new IconEntry((ExtendedBlipSprite)572, 1006, 56),
new IconEntry((ExtendedBlipSprite)588, 1006, 81),
new IconEntry((ExtendedBlipSprite)604, 1006, 103),
new IconEntry((ExtendedBlipSprite)620, 1006, 125),
new IconEntry((ExtendedBlipSprite)637, 1006, 149),
new IconEntry((ExtendedBlipSprite)653, 1006, 173),
new IconEntry((ExtendedBlipSprite)664, 1006, 194),
new IconEntry((ExtendedBlipSprite)724, 1006, 216),
new IconEntry((ExtendedBlipSprite)740, 1006, 241),
new IconEntry((ExtendedBlipSprite)756, 1006, 262),
new IconEntry((ExtendedBlipSprite)775, 1006, 283),
new IconEntry((ExtendedBlipSprite)790, 1006, 309),
new IconEntry((ExtendedBlipSprite)808, 1006, 330),
new IconEntry((ExtendedBlipSprite)811, 1006, 349),

            
new IconEntry((ExtendedBlipSprite)527, 1046, 13),
new IconEntry((ExtendedBlipSprite)557, 1046, 39),
new IconEntry((ExtendedBlipSprite)573, 1046, 57),
new IconEntry((ExtendedBlipSprite)589, 1046, 83),
new IconEntry((ExtendedBlipSprite)605, 1046, 105),
new IconEntry((ExtendedBlipSprite)621, 1046, 127),
new IconEntry((ExtendedBlipSprite)638, 1046, 149),
new IconEntry((ExtendedBlipSprite)654, 1046, 173),
new IconEntry((ExtendedBlipSprite)671, 1046, 197),
new IconEntry((ExtendedBlipSprite)63, 1046, 217),
new IconEntry((ExtendedBlipSprite)741, 1046, 239),
new IconEntry((ExtendedBlipSprite)753, 1046, 262),
new IconEntry((ExtendedBlipSprite)768, 1046, 285),
new IconEntry((ExtendedBlipSprite)791, 1046, 308),
new IconEntry((ExtendedBlipSprite)807, 1046, 331),
new IconEntry((ExtendedBlipSprite)837, 1046, 350),

            
new IconEntry((ExtendedBlipSprite)528, 1086, 15),
new IconEntry((ExtendedBlipSprite)558, 1086, 38),
new IconEntry((ExtendedBlipSprite)574, 1086, 57),
new IconEntry((ExtendedBlipSprite)590, 1086, 81),
new IconEntry((ExtendedBlipSprite)606, 1086, 103),
new IconEntry((ExtendedBlipSprite)622, 1086, 129),
new IconEntry((ExtendedBlipSprite)639, 1086, 149),
new IconEntry((ExtendedBlipSprite)672, 1086, 194),
new IconEntry((ExtendedBlipSprite)726, 1086, 217),
new IconEntry((ExtendedBlipSprite)742, 1086, 239),
new IconEntry((ExtendedBlipSprite)746, 1086, 262),
new IconEntry((ExtendedBlipSprite)769, 1086, 285),
new IconEntry((ExtendedBlipSprite)792, 1086, 308),
new IconEntry((ExtendedBlipSprite)809, 1086, 327),
new IconEntry((ExtendedBlipSprite)827, 1086, 352),

            
new IconEntry((ExtendedBlipSprite)529, 1126, 14),
new IconEntry((ExtendedBlipSprite)559, 1126, 38),
new IconEntry((ExtendedBlipSprite)575, 1126, 59),
new IconEntry((ExtendedBlipSprite)591, 1116, 77),  
new IconEntry((ExtendedBlipSprite)607, 1126, 105),
new IconEntry((ExtendedBlipSprite)623, 1126, 126),
new IconEntry((ExtendedBlipSprite)640, 1126, 151),
new IconEntry((ExtendedBlipSprite)655, 1126, 171),
new IconEntry((ExtendedBlipSprite)673, 1126, 196),
new IconEntry((ExtendedBlipSprite)738, 1126, 216),
new IconEntry((ExtendedBlipSprite)743, 1126, 238),
new IconEntry((ExtendedBlipSprite)750, 1126, 262),
new IconEntry((ExtendedBlipSprite)770, 1126, 286),
new IconEntry((ExtendedBlipSprite)793, 1126, 307),
new IconEntry((ExtendedBlipSprite)814, 1126, 329),
new IconEntry((ExtendedBlipSprite)838, 1126, 352),

            
new IconEntry((ExtendedBlipSprite)530, 1166, 15),
new IconEntry((ExtendedBlipSprite)560, 1166, 37),
new IconEntry((ExtendedBlipSprite)576, 1166, 61),
new IconEntry((ExtendedBlipSprite)592, 1166, 79),
new IconEntry((ExtendedBlipSprite)608, 1156, 107), 
new IconEntry((ExtendedBlipSprite)624, 1166, 129),
new IconEntry((ExtendedBlipSprite)642, 1166, 151),
new IconEntry((ExtendedBlipSprite)657, 1166, 172),
new IconEntry((ExtendedBlipSprite)674, 1166, 195),
new IconEntry((ExtendedBlipSprite)739, 1166, 216),
new IconEntry((ExtendedBlipSprite)744, 1166, 241),
new IconEntry((ExtendedBlipSprite)763, 1166, 262),
new IconEntry((ExtendedBlipSprite)776, 1166, 286),
new IconEntry((ExtendedBlipSprite)794, 1166, 307),
new IconEntry((ExtendedBlipSprite)819, 1166, 328),
new IconEntry((ExtendedBlipSprite)836, 1166, 353),

            
new IconEntry((ExtendedBlipSprite)531, 1206, 15),
new IconEntry((ExtendedBlipSprite)561, 1206, 37),
new IconEntry((ExtendedBlipSprite)577, 1206, 59),
new IconEntry((ExtendedBlipSprite)593, 1206, 83),
new IconEntry((ExtendedBlipSprite)609, 1206, 106),
new IconEntry((ExtendedBlipSprite)625, 1206, 129),
new IconEntry((ExtendedBlipSprite)641, 1206, 149),
new IconEntry((ExtendedBlipSprite)667, 1206, 175),
new IconEntry((ExtendedBlipSprite)675, 1206, 195),
new IconEntry((ExtendedBlipSprite)729, 1206, 220),
new IconEntry((ExtendedBlipSprite)749, 1206, 241),
new IconEntry((ExtendedBlipSprite)764, 1206, 262),
new IconEntry((ExtendedBlipSprite)781, 1206, 286),
new IconEntry((ExtendedBlipSprite)795, 1206, 308),
new IconEntry((ExtendedBlipSprite)817, 1206, 329),
new IconEntry((ExtendedBlipSprite)835, 1206, 352),

            
new IconEntry((ExtendedBlipSprite)532, 1246, 14),
new IconEntry((ExtendedBlipSprite)562, 1246, 39),
new IconEntry((ExtendedBlipSprite)578, 1246, 57),
new IconEntry((ExtendedBlipSprite)594, 1246, 83),
new IconEntry((ExtendedBlipSprite)610, 1246, 102),
new IconEntry((ExtendedBlipSprite)626, 1246, 127),
new IconEntry((ExtendedBlipSprite)643, 1246, 150),
new IconEntry((ExtendedBlipSprite)661, 1246, 173),
new IconEntry((ExtendedBlipSprite)677, 1246, 195),
new IconEntry((ExtendedBlipSprite)727, 1246, 217),
new IconEntry((ExtendedBlipSprite)760, 1246, 240),
new IconEntry((ExtendedBlipSprite)765, 1246, 264),
new IconEntry((ExtendedBlipSprite)783, 1246, 282),
new IconEntry((ExtendedBlipSprite)428, 1240, 307),  
new IconEntry((ExtendedBlipSprite)822, 1246, 329),
new IconEntry((ExtendedBlipSprite)833, 1246, 354),


            
            
            
new IconEntry((ExtendedBlipSprite)8, 646, 377),
new IconEntry((ExtendedBlipSprite)67, 646, 399),
new IconEntry((ExtendedBlipSprite)90, 646, 419),
new IconEntry((ExtendedBlipSprite)118, 646, 442),
new IconEntry((ExtendedBlipSprite)147, 646, 464),
new IconEntry((ExtendedBlipSprite)173, 646, 484),
new IconEntry((ExtendedBlipSprite)207, 646, 508),
new IconEntry((ExtendedBlipSprite)267, 646, 529),
new IconEntry((ExtendedBlipSprite)306, 646, 552),
new IconEntry((ExtendedBlipSprite)352, 646, 572),
new IconEntry((ExtendedBlipSprite)370, 646, 595),
new IconEntry((ExtendedBlipSprite)387, 646, 619),
new IconEntry((ExtendedBlipSprite)420, 646, 640),
new IconEntry((ExtendedBlipSprite)440, 646, 661),
new IconEntry((ExtendedBlipSprite)465, 646, 682),
new IconEntry((ExtendedBlipSprite)487, 646, 702),

            
new IconEntry((ExtendedBlipSprite)16, 686, 378),
new IconEntry((ExtendedBlipSprite)68, 686, 399),
new IconEntry((ExtendedBlipSprite)93, 686, 420),
new IconEntry((ExtendedBlipSprite)119, 686, 441),
new IconEntry((ExtendedBlipSprite)149, 686, 465),
new IconEntry((ExtendedBlipSprite)174, 686, 485),
new IconEntry((ExtendedBlipSprite)208, 686, 507),
new IconEntry((ExtendedBlipSprite)269, 686, 530),
new IconEntry((ExtendedBlipSprite)307, 686, 551),
new IconEntry((ExtendedBlipSprite)354, 686, 572),
new IconEntry((ExtendedBlipSprite)371, 686, 596),
new IconEntry((ExtendedBlipSprite)388, 686, 617),
new IconEntry((ExtendedBlipSprite)421, 686, 640),
new IconEntry((ExtendedBlipSprite)442, 686, 663),
new IconEntry((ExtendedBlipSprite)463, 686, 683),
new IconEntry((ExtendedBlipSprite)486, 686, 705),

            
new IconEntry((ExtendedBlipSprite)36, 726, 378),
new IconEntry((ExtendedBlipSprite)71, 726, 399),
new IconEntry((ExtendedBlipSprite)94, 726, 422),
new IconEntry((ExtendedBlipSprite)120, 726, 442),
new IconEntry((ExtendedBlipSprite)150, 726, 463),
new IconEntry((ExtendedBlipSprite)175, 726, 486),
new IconEntry((ExtendedBlipSprite)209, 726, 507),
new IconEntry((ExtendedBlipSprite)272, 726, 529),
new IconEntry((ExtendedBlipSprite)308, 726, 552),
new IconEntry((ExtendedBlipSprite)355, 726, 572),
new IconEntry((ExtendedBlipSprite)372, 726, 596),
new IconEntry((ExtendedBlipSprite)389, 721, 620),   
new IconEntry((ExtendedBlipSprite)445, 726, 660),
new IconEntry((ExtendedBlipSprite)471, 726, 685),
new IconEntry((ExtendedBlipSprite)484, 726, 705),

            
new IconEntry((ExtendedBlipSprite)38, 766, 373),
new IconEntry((ExtendedBlipSprite)72, 766, 399),
new IconEntry((ExtendedBlipSprite)96, 766, 421),
new IconEntry((ExtendedBlipSprite)121, 766, 444),
new IconEntry((ExtendedBlipSprite)151, 766, 463),
new IconEntry((ExtendedBlipSprite)176, 766, 484),
new IconEntry((ExtendedBlipSprite)210, 766, 507),
new IconEntry((ExtendedBlipSprite)273, 766, 530),
new IconEntry((ExtendedBlipSprite)309, 766, 547),
new IconEntry((ExtendedBlipSprite)356, 766, 574),
new IconEntry((ExtendedBlipSprite)374, 766, 596),
new IconEntry((ExtendedBlipSprite)400, 766, 619),
new IconEntry((ExtendedBlipSprite)426, 766, 641),
new IconEntry((ExtendedBlipSprite)446, 766, 661),
new IconEntry((ExtendedBlipSprite)472, 766, 681),
new IconEntry((ExtendedBlipSprite)483, 766, 705),

            
new IconEntry((ExtendedBlipSprite)40, 806, 377),
new IconEntry((ExtendedBlipSprite)73, 806, 399),
new IconEntry((ExtendedBlipSprite)100, 806, 421),
new IconEntry((ExtendedBlipSprite)122, 806, 440),
new IconEntry((ExtendedBlipSprite)152, 806, 464),
new IconEntry((ExtendedBlipSprite)181, 806, 485),
new IconEntry((ExtendedBlipSprite)211, 806, 506),
new IconEntry((ExtendedBlipSprite)276, 806, 530),
new IconEntry((ExtendedBlipSprite)310, 806, 551),
new IconEntry((ExtendedBlipSprite)357, 806, 574),
new IconEntry((ExtendedBlipSprite)375, 806, 596),
new IconEntry((ExtendedBlipSprite)401, 806, 617),
new IconEntry((ExtendedBlipSprite)427, 806, 639),
new IconEntry((ExtendedBlipSprite)455, 806, 661),
new IconEntry((ExtendedBlipSprite)474, 806, 683),
new IconEntry((ExtendedBlipSprite)490, 806, 705),

            
new IconEntry((ExtendedBlipSprite)43, 846, 377),
new IconEntry((ExtendedBlipSprite)75, 846, 398),
new IconEntry((ExtendedBlipSprite)102, 846, 420),
new IconEntry((ExtendedBlipSprite)123, 846, 441),
new IconEntry((ExtendedBlipSprite)153, 846, 464),
new IconEntry((ExtendedBlipSprite)182, 846, 484),
new IconEntry((ExtendedBlipSprite)225, 846, 505),
new IconEntry((ExtendedBlipSprite)277, 846, 528),
new IconEntry((ExtendedBlipSprite)311, 846, 552),
new IconEntry((ExtendedBlipSprite)358, 846, 572),
new IconEntry((ExtendedBlipSprite)376, 846, 591),
new IconEntry((ExtendedBlipSprite)402, 846, 617),
new IconEntry((ExtendedBlipSprite)429, 846, 640),
new IconEntry((ExtendedBlipSprite)456, 846, 660),
new IconEntry((ExtendedBlipSprite)473, 846, 681),
new IconEntry((ExtendedBlipSprite)491, 846, 703),

            
new IconEntry((ExtendedBlipSprite)47, 886, 376),
new IconEntry((ExtendedBlipSprite)76, 886, 396),
new IconEntry((ExtendedBlipSprite)103, 886, 419),
new IconEntry((ExtendedBlipSprite)124, 886, 443),
new IconEntry((ExtendedBlipSprite)154, 886, 464),
new IconEntry((ExtendedBlipSprite)183, 886, 486),
new IconEntry((ExtendedBlipSprite)226, 886, 508),
new IconEntry((ExtendedBlipSprite)278, 886, 529),
new IconEntry((ExtendedBlipSprite)313, 886, 550),
new IconEntry((ExtendedBlipSprite)359, 886, 572),
new IconEntry((ExtendedBlipSprite)377, 886, 595),
new IconEntry((ExtendedBlipSprite)403, 885, 616),
new IconEntry((ExtendedBlipSprite)430, 886, 641),
new IconEntry((ExtendedBlipSprite)457, 886, 661),
new IconEntry((ExtendedBlipSprite)476, 886, 681),
new IconEntry((ExtendedBlipSprite)492, 886, 705),

            
new IconEntry((ExtendedBlipSprite)50, 926, 377),
new IconEntry((ExtendedBlipSprite)77, 919, 399),  
new IconEntry((ExtendedBlipSprite)104, 926, 420),
new IconEntry((ExtendedBlipSprite)126, 926, 442),
new IconEntry((ExtendedBlipSprite)155, 926, 463),
new IconEntry((ExtendedBlipSprite)184, 926, 486),
new IconEntry((ExtendedBlipSprite)227, 926, 507),
new IconEntry((ExtendedBlipSprite)279, 926, 529),
new IconEntry((ExtendedBlipSprite)314, 926, 547),
new IconEntry((ExtendedBlipSprite)360, 926, 574),
new IconEntry((ExtendedBlipSprite)378, 926, 594),
new IconEntry((ExtendedBlipSprite)404, 926, 618),
new IconEntry((ExtendedBlipSprite)431, 926, 639),
new IconEntry((ExtendedBlipSprite)459, 926, 661),
new IconEntry((ExtendedBlipSprite)475, 926, 681),
new IconEntry((ExtendedBlipSprite)494, 926, 702),

            
new IconEntry((ExtendedBlipSprite)51, 966, 375),
new IconEntry((ExtendedBlipSprite)78, 966, 401),
new IconEntry((ExtendedBlipSprite)105, 966, 420),
new IconEntry((ExtendedBlipSprite)127, 966, 438),
new IconEntry((ExtendedBlipSprite)156, 966, 464),
new IconEntry((ExtendedBlipSprite)186, 966, 485),
new IconEntry((ExtendedBlipSprite)229, 966, 507),
new IconEntry((ExtendedBlipSprite)280, 966, 529),
new IconEntry((ExtendedBlipSprite)315, 966, 548),
new IconEntry((ExtendedBlipSprite)361, 966, 574),
new IconEntry((ExtendedBlipSprite)379, 966, 591),
new IconEntry((ExtendedBlipSprite)405, 966, 618),
new IconEntry((ExtendedBlipSprite)432, 966, 639),
new IconEntry((ExtendedBlipSprite)458, 966, 660),
new IconEntry((ExtendedBlipSprite)477, 966, 681),
new IconEntry((ExtendedBlipSprite)499, 966, 704),

            
new IconEntry((ExtendedBlipSprite)52, 1006, 377),
new IconEntry((ExtendedBlipSprite)79, 1006, 399),
new IconEntry((ExtendedBlipSprite)106, 1006, 420),
new IconEntry((ExtendedBlipSprite)133, 1006, 441),
new IconEntry((ExtendedBlipSprite)157, 1006, 463),
new IconEntry((ExtendedBlipSprite)187, 1006, 485),
new IconEntry((ExtendedBlipSprite)237, 1006, 508),
new IconEntry((ExtendedBlipSprite)285, 1006, 528),
new IconEntry((ExtendedBlipSprite)316, 1006, 548),
new IconEntry((ExtendedBlipSprite)362, 1006, 572),
new IconEntry((ExtendedBlipSprite)380, 1010, 598),  
new IconEntry((ExtendedBlipSprite)408, 1006, 617),
new IconEntry((ExtendedBlipSprite)433, 1006, 638),
new IconEntry((ExtendedBlipSprite)460, 1006, 661),
new IconEntry((ExtendedBlipSprite)478, 1006, 682),
new IconEntry((ExtendedBlipSprite)496, 1006, 705),

            
new IconEntry((ExtendedBlipSprite)56, 1046, 378),
new IconEntry((ExtendedBlipSprite)80, 1046, 398),
new IconEntry((ExtendedBlipSprite)107, 1046, 421),
new IconEntry((ExtendedBlipSprite)134, 1046, 442),
new IconEntry((ExtendedBlipSprite)158, 1046, 463),
new IconEntry((ExtendedBlipSprite)188, 1046, 483),
new IconEntry((ExtendedBlipSprite)238, 1046, 507),
new IconEntry((ExtendedBlipSprite)289, 1046, 530),
new IconEntry((ExtendedBlipSprite)317, 1046, 550),
new IconEntry((ExtendedBlipSprite)363, 1046, 574),
new IconEntry((ExtendedBlipSprite)381, 1039, 599),  
new IconEntry((ExtendedBlipSprite)409, 1046, 616),
new IconEntry((ExtendedBlipSprite)434, 1046, 636),
new IconEntry((ExtendedBlipSprite)461, 1046, 662),
new IconEntry((ExtendedBlipSprite)479, 1046, 682),
new IconEntry((ExtendedBlipSprite)500, 1046, 702),

            
new IconEntry((ExtendedBlipSprite)59, 1086, 377),
new IconEntry((ExtendedBlipSprite)84, 1086, 400),
new IconEntry((ExtendedBlipSprite)108, 1086, 420),
new IconEntry((ExtendedBlipSprite)135, 1086, 444),
new IconEntry((ExtendedBlipSprite)159, 1086, 463),
new IconEntry((ExtendedBlipSprite)189, 1086, 483),
new IconEntry((ExtendedBlipSprite)251, 1086, 507),
new IconEntry((ExtendedBlipSprite)290, 1086, 528),
new IconEntry((ExtendedBlipSprite)318, 1086, 549),
new IconEntry((ExtendedBlipSprite)365, 1086, 573),
new IconEntry((ExtendedBlipSprite)382, 1086, 597),
new IconEntry((ExtendedBlipSprite)410, 1086, 619),
new IconEntry((ExtendedBlipSprite)435, 1086, 633),
new IconEntry((ExtendedBlipSprite)467, 1086, 661),
new IconEntry((ExtendedBlipSprite)480, 1086, 683),
new IconEntry((ExtendedBlipSprite)497, 1086, 703),

            
new IconEntry((ExtendedBlipSprite)60, 1126, 377),
new IconEntry((ExtendedBlipSprite)85, 1126, 399),
new IconEntry((ExtendedBlipSprite)109, 1118, 418),  
new IconEntry((ExtendedBlipSprite)136, 1126, 440),
new IconEntry((ExtendedBlipSprite)160, 1126, 464),
new IconEntry((ExtendedBlipSprite)197, 1126, 485),
new IconEntry((ExtendedBlipSprite)252, 1126, 507),
new IconEntry((ExtendedBlipSprite)291, 1126, 527),
new IconEntry((ExtendedBlipSprite)326, 1126, 551),
new IconEntry((ExtendedBlipSprite)366, 1126, 572),
new IconEntry((ExtendedBlipSprite)383, 1126, 600),
new IconEntry((ExtendedBlipSprite)411, 1126, 617),
new IconEntry((ExtendedBlipSprite)441, 1126, 638),
new IconEntry((ExtendedBlipSprite)469, 1126, 661),
new IconEntry((ExtendedBlipSprite)481, 1126, 681),
new IconEntry((ExtendedBlipSprite)498, 1126, 701),

            
new IconEntry((ExtendedBlipSprite)61, 1166, 377),
new IconEntry((ExtendedBlipSprite)86, 1166, 397),
new IconEntry((ExtendedBlipSprite)110, 1166, 418),
new IconEntry((ExtendedBlipSprite)137, 1166, 440),
new IconEntry((ExtendedBlipSprite)162, 1166, 464),
new IconEntry((ExtendedBlipSprite)198, 1166, 482),
new IconEntry((ExtendedBlipSprite)253, 1166, 508),
new IconEntry((ExtendedBlipSprite)293, 1166, 530),
new IconEntry((ExtendedBlipSprite)348, 1166, 550),
new IconEntry((ExtendedBlipSprite)367, 1157, 570),  
new IconEntry((ExtendedBlipSprite)384, 1166, 598),
new IconEntry((ExtendedBlipSprite)414, 1166, 619),
new IconEntry((ExtendedBlipSprite)437, 1166, 639),
new IconEntry((ExtendedBlipSprite)468, 1166, 660),
new IconEntry((ExtendedBlipSprite)488, 1166, 684),
new IconEntry((ExtendedBlipSprite)501, 1166, 706),

            
new IconEntry((ExtendedBlipSprite)64, 1206, 378),
new IconEntry((ExtendedBlipSprite)88, 1206, 400),
new IconEntry((ExtendedBlipSprite)112, 1206, 418),
new IconEntry((ExtendedBlipSprite)140, 1206, 442),
new IconEntry((ExtendedBlipSprite)163, 1206, 463),
new IconEntry((ExtendedBlipSprite)205, 1206, 485),
new IconEntry((ExtendedBlipSprite)255, 1206, 506),
new IconEntry((ExtendedBlipSprite)304, 1206, 531),
new IconEntry((ExtendedBlipSprite)350, 1206, 549),
new IconEntry((ExtendedBlipSprite)368, 1206, 574),
new IconEntry((ExtendedBlipSprite)385, 1206, 597),
new IconEntry((ExtendedBlipSprite)415, 1206, 619),
new IconEntry((ExtendedBlipSprite)439, 1206, 639),
new IconEntry((ExtendedBlipSprite)464, 1206, 661),
new IconEntry((ExtendedBlipSprite)489, 1206, 685),
new IconEntry((ExtendedBlipSprite)493, 1206, 703),

            
new IconEntry((ExtendedBlipSprite)66, 1246, 375),
new IconEntry((ExtendedBlipSprite)89, 1246, 399),
new IconEntry((ExtendedBlipSprite)113, 1246, 420),
new IconEntry((ExtendedBlipSprite)141, 1246, 443),
new IconEntry((ExtendedBlipSprite)164, 1246, 465),
new IconEntry((ExtendedBlipSprite)206, 1246, 485),
new IconEntry((ExtendedBlipSprite)266, 1246, 507),
new IconEntry((ExtendedBlipSprite)305, 1246, 528),
new IconEntry((ExtendedBlipSprite)351, 1246, 551),
new IconEntry((ExtendedBlipSprite)369, 1246, 575),
new IconEntry((ExtendedBlipSprite)386, 1246, 598),
new IconEntry((ExtendedBlipSprite)419, 1246, 615),
new IconEntry((ExtendedBlipSprite)436, 1246, 641),
new IconEntry((ExtendedBlipSprite)466, 1246, 662),
new IconEntry((ExtendedBlipSprite)485, 1246, 688),
new IconEntry((ExtendedBlipSprite)495, 1246, 708),

            
            
            
new IconEntry((ExtendedBlipSprite)829, 28, 417),
new IconEntry((ExtendedBlipSprite)852, 28, 450),
new IconEntry((ExtendedBlipSprite)870, 28, 483),
new IconEntry((ExtendedBlipSprite)888, 28, 517),
new IconEntry((ExtendedBlipSprite)912, 28, 550),
new IconEntry((ExtendedBlipSprite)937, 28, 583),
new IconEntry((ExtendedBlipSprite)951, 28, 613),

            
new IconEntry((ExtendedBlipSprite)834, 60, 418),
new IconEntry((ExtendedBlipSprite)853, 60, 451),
new IconEntry((ExtendedBlipSprite)871, 60, 484),
new IconEntry((ExtendedBlipSprite)889, 60, 514),
new IconEntry((ExtendedBlipSprite)913, 60, 549),
new IconEntry((ExtendedBlipSprite)936, 60, 585),
new IconEntry((ExtendedBlipSprite)953, 60, 613),

            
new IconEntry((ExtendedBlipSprite)830, 93, 422),
new IconEntry((ExtendedBlipSprite)854, 93, 450),
new IconEntry((ExtendedBlipSprite)872, 93, 484),
new IconEntry((ExtendedBlipSprite)890, 93, 519),
new IconEntry((ExtendedBlipSprite)914, 93, 547),
new IconEntry((ExtendedBlipSprite)938, 93, 584),
new IconEntry((ExtendedBlipSprite)954, 93, 617),

            
new IconEntry((ExtendedBlipSprite)831, 126, 421),
new IconEntry((ExtendedBlipSprite)857, 126, 450),
new IconEntry((ExtendedBlipSprite)873, 126, 482),
new IconEntry((ExtendedBlipSprite)899, 126, 513),
new IconEntry((ExtendedBlipSprite)934, 129, 577),
new IconEntry((ExtendedBlipSprite)955, 126, 612),

            
new IconEntry((ExtendedBlipSprite)828, 162, 412),
new IconEntry((ExtendedBlipSprite)860, 156, 454),
new IconEntry((ExtendedBlipSprite)874, 156, 482),
new IconEntry((ExtendedBlipSprite)900, 156, 517),
new IconEntry((ExtendedBlipSprite)915, 156, 551),
new IconEntry((ExtendedBlipSprite)935, 156, 517),
new IconEntry((ExtendedBlipSprite)956, 156, 551),

            
new IconEntry((ExtendedBlipSprite)841, 196, 412),
new IconEntry((ExtendedBlipSprite)859, 191, 451),
new IconEntry((ExtendedBlipSprite)875, 191, 491),
new IconEntry((ExtendedBlipSprite)901, 191, 519),
new IconEntry((ExtendedBlipSprite)916, 191, 547),
new IconEntry((ExtendedBlipSprite)939, 191, 583),
new IconEntry((ExtendedBlipSprite)957, 191, 615),

            
new IconEntry((ExtendedBlipSprite)847, 224, 420),
new IconEntry((ExtendedBlipSprite)861, 224, 450),
new IconEntry((ExtendedBlipSprite)876, 224, 479),
new IconEntry((ExtendedBlipSprite)902, 224, 518),
new IconEntry((ExtendedBlipSprite)917, 224, 552),
new IconEntry((ExtendedBlipSprite)942, 224, 582),

            
new IconEntry((ExtendedBlipSprite)839, 255, 417),
new IconEntry((ExtendedBlipSprite)858, 255, 450),
new IconEntry((ExtendedBlipSprite)877, 255, 480),
new IconEntry((ExtendedBlipSprite)903, 255, 518),
new IconEntry((ExtendedBlipSprite)918, 255, 553),
new IconEntry((ExtendedBlipSprite)943, 255, 584),

            
new IconEntry((ExtendedBlipSprite)840, 287, 416),
new IconEntry((ExtendedBlipSprite)862, 287, 455),
new IconEntry((ExtendedBlipSprite)878, 287, 482),
new IconEntry((ExtendedBlipSprite)904, 287, 518),
new IconEntry((ExtendedBlipSprite)922, 287, 547),
new IconEntry((ExtendedBlipSprite)944, 287, 585),

            
new IconEntry((ExtendedBlipSprite)846, 320, 418),
new IconEntry((ExtendedBlipSprite)864, 320, 445),
new IconEntry((ExtendedBlipSprite)879, 320, 480),
new IconEntry((ExtendedBlipSprite)905, 320, 519),
new IconEntry((ExtendedBlipSprite)923, 320, 550),
new IconEntry((ExtendedBlipSprite)945, 320, 582),

            
new IconEntry((ExtendedBlipSprite)845, 354, 419),
new IconEntry((ExtendedBlipSprite)863, 363, 444),  
new IconEntry((ExtendedBlipSprite)880, 354, 483),
new IconEntry((ExtendedBlipSprite)906, 354, 517),
new IconEntry((ExtendedBlipSprite)940, 354, 549),
new IconEntry((ExtendedBlipSprite)946, 354, 584),

            
new IconEntry((ExtendedBlipSprite)842, 385, 417),
new IconEntry((ExtendedBlipSprite)865, 385, 445),
new IconEntry((ExtendedBlipSprite)882, 385, 475),
new IconEntry((ExtendedBlipSprite)883, 385, 491),
new IconEntry((ExtendedBlipSprite)910, 385, 520),
new IconEntry((ExtendedBlipSprite)924, 385, 548),
new IconEntry((ExtendedBlipSprite)947, 385, 585),

            
new IconEntry((ExtendedBlipSprite)844, 418, 420),
new IconEntry((ExtendedBlipSprite)866, 418, 450),
new IconEntry((ExtendedBlipSprite)893, 418, 482),
new IconEntry((ExtendedBlipSprite)908, 418, 519),
new IconEntry((ExtendedBlipSprite)941, 418, 550),
new IconEntry((ExtendedBlipSprite)952, 418, 581),

            
new IconEntry((ExtendedBlipSprite)843, 450, 418),
new IconEntry((ExtendedBlipSprite)867, 450, 449),
new IconEntry((ExtendedBlipSprite)885, 450, 480),
new IconEntry((ExtendedBlipSprite)907, 450, 523),
new IconEntry((ExtendedBlipSprite)925, 450, 549),
new IconEntry((ExtendedBlipSprite)948, 450, 585),

            
new IconEntry((ExtendedBlipSprite)850, 482, 418),
new IconEntry((ExtendedBlipSprite)868, 482, 454),
new IconEntry((ExtendedBlipSprite)886, 482, 482),
new IconEntry((ExtendedBlipSprite)909, 482, 516),
new IconEntry((ExtendedBlipSprite)928, 482, 548),
new IconEntry((ExtendedBlipSprite)949, 482, 580),

            
new IconEntry((ExtendedBlipSprite)851, 516, 417),
new IconEntry((ExtendedBlipSprite)869, 523, 445),  
new IconEntry((ExtendedBlipSprite)887, 516, 484),
new IconEntry((ExtendedBlipSprite)911, 516, 516),
new IconEntry((ExtendedBlipSprite)931, 516, 549),
new IconEntry((ExtendedBlipSprite)950, 516, 585),
        };

        public readonly struct IconEntry
        {
            public readonly ExtendedBlipSprite Key;
            public readonly int X;
            public readonly int Y;

            public IconEntry(ExtendedBlipSprite key, int x, int y)
            {
                Key = key;
                X = x;
                Y = y;
            }
        }

        public struct PredefinedBlipData
        {
            public string Name;
            public Vector3 Position;
            public int IconId, ColorId;
            public float Size;
            public bool IsFlashing, IsShortRange;

            public PredefinedBlipData(string name, Vector3 pos, int icon, int color, float size, bool flash, bool shortRange)
            {
                Name = name;
                Position = pos;
                IconId = icon;
                ColorId = color;
                Size = size;
                IsFlashing = flash;
                IsShortRange = shortRange;
            }
        }

        static Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }
        public Main()
        {
            _iniParser = new FileIniDataParser();
            EnsureDirectoryExist(_logFilePath);

            _logWriter = new StreamWriter(new FileStream(
            _logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, true))
            {
                AutoFlush = true
            };

            _availableGroups.Add(_createGroupLabel);

            KeyDown += OnKeyDown;
            Aborted += OnAborted;
            Interval = _defaultInterval;

            
            _ = InitializeAsync();
        }
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;

            if (_assemblyCache.ContainsKey(name))
            {
                return _assemblyCache[name];
            }

            Assembly assembly = Assembly.GetExecutingAssembly();

            string resource = null;
            string[] resources = assembly.GetManifestResourceNames();

            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i].EndsWith(name + ".dll"))
                {
                    resource = resources[i];
                    break;
                }
            }

            if (resource == null)
            {
                return null;
            }

            using (Stream stream = assembly.GetManifestResourceStream(resource))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                Assembly loaded = Assembly.Load(bytes);
                _assemblyCache[name] = loaded;
                return loaded;
            }
        }

        private async Task InitializeAsync()
        {
            await CheckAndMigrateFromIniAsync();

            await LoadSettingsAsync();

            _mainMenu = CreateAndRegisterMenu("Advanced Custom Blips", $"Main Menu {CURRENT_SCRIPT_VERSION}");

            InitializeAddBlipMenu();
            InitializeGlobalSettingsMenu();
            InitializeManageBlipsMenu();

            await LoadAndCreateBlips();

            if (_globalSettings.ShowBlipLoadNotification && _activeBlips.Count > 0)
            {
                Notification.PostTicker($"~g~Loaded {_activeBlips.Count} blip(s).", true);
            }

            if (_globalSettings.AutoCheckForUpdatesOnStartup)
            {
                await CheckForUpdatesAsync();
            }

            _shouldUseModdedTextureSheet = _globalSettings.UseModdedTextureSheet;

            _customSprite = new CustomSprite(
                LoadTempfile("AdvancedCustomBlips.Vanilla_Texture_Sheet.image.png"),
                new SizeF(640f, 360f),
                new PointF(Screen.Width * 0.49f, 5)
            );

            _customSprite1 = new CustomSprite(
                LoadTempfile("AdvancedCustomBlips.Vanilla_Texture_Sheet.image1.png"),
                new SizeF(640f, 350f),
                new PointF(Screen.Width * 0.49f, Screen.Height * 0.51f)
            );

            _customSprite2 = new CustomSprite(
                LoadTempfile("AdvancedCustomBlips.Vanilla_Texture_Sheet.image2.png"),
                new SizeF(520f, 230f),
                new PointF(Screen.Width * 0.01f, Screen.Height * 0.56f)
            );

            _mousePointer = new CustomSprite(
                LoadTempfile("AdvancedCustomBlips.Vanilla_Texture_Sheet.Mouse Pointer.png"),
                new SizeF(13.72f, 19.9f),
                new PointF(0f, 0f)
            );

            
            Tick += OnTick;
        }
        private string LoadTempfile(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new Exception($"Resource not found: {resourceName}");
                }

                string tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{Path.GetFileName(resourceName)}");

                using (FileStream fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.CopyTo(fs);
                }

                File.SetAttributes(tempPath, FileAttributes.Hidden);

                _tempFiles.Add(tempPath);

                return tempPath;
            }
        }
        private async Task CheckAndMigrateFromIniAsync()
        {
            
            if (File.Exists(_jsonFilePath) || File.Exists(_settingsFilePath) || _migrationCompleted)
            {
                return;
            }

            
            if (!File.Exists(_iniFilePath))
            {
                return; 
            }

            try
            {
                Notification.PostTicker("~y~[Migration] Detected INI file. Migrating to JSON...", true);

                
                CreateIniBackup();

                
                await MigrateSettingsFromIniAsync();

                
                await MigrateBlipsFromIniAsync();

                _migrationCompleted = true;

                Notification.PostTicker("~g~[Migration] Successfully migrated from INI to JSON!", true);
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, "Migration from INI to JSON failed");
                Notification.PostTicker("~r~[Migration] Failed! Check log for details.", true);
            }
        }
        private void CreateIniBackup()
        {
            try
            {
                EnsureDirectoryExist(_iniBackupPath);
                File.Copy(_iniFilePath, _iniBackupPath, overwrite: true);
                Notification.PostTicker($"INI backup created at: {_iniBackupPath}", true);
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, "Failed to create INI backup");
            }
        }
        private async Task MigrateSettingsFromIniAsync()
        {
            IniData iniData = LoadIniDataFromDisk(); 

            if (!iniData.Sections.ContainsSection("Settings"))
            {
                _globalSettings = new SettingsData(); 
                await SaveSettingsAsync();
                return;
            }

            KeyDataCollection settings = iniData["Settings"];

            _globalSettings = new SettingsData
            {
                ToggleCoordinatesKey = settings.ContainsKey("Toggle_Coordinates_Key")
                    ? settings["Toggle_Coordinates_Key"] : "F1",
                ReloadBlipsKey = settings.ContainsKey("Reload_Blips_Key")
                    ? settings["Reload_Blips_Key"] : "F2",
                ToggleBlipsVisibilityKey = settings.ContainsKey("Toggle_Blips_Visibility_Key")
                    ? settings["Toggle_Blips_Visibility_Key"] : "F3",
                ToggleModdedTextureSheetKey = "F5",
                OpenMenuKey = settings.ContainsKey("Open_Menu_Key")
                    ? settings["Open_Menu_Key"] : "F10",
                ShowBlipLoadNotification = ParseBoolOption(settings.ContainsKey("Show_Blip_Added_Notification")
                    ? settings["Show_Blip_Added_Notification"] : "ON"),
                EnableAddOnBlips = ParseBoolOption(settings.ContainsKey("Enable_AddOn_Blips")
                    ? settings["Enable_AddOn_Blips"] : "OFF"),
                AutoCheckForUpdatesOnStartup = true, 
                UseModdedTextureSheet = false
            };

            await SaveSettingsAsync();
            Notification.PostTicker("Settings migrated from INI to JSON", true);
        }
        private bool ParseBoolOption(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Equals("ON", StringComparison.OrdinalIgnoreCase);
        }
        private async Task MigrateBlipsFromIniAsync()
        {
            IniData iniData = LoadIniDataFromDisk();
            List<BlipData> blipDataList = new List<BlipData>();

            foreach (SectionData section in iniData.Sections)
            {
                if (section.SectionName.Equals("Settings", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (KeyData key in section.Keys)
                    {
                        fields[key.KeyName] = key.Value;
                    }

                    
                    BlipData blipData = new BlipData
                    {
                        Name = fields.ContainsKey("Blip_Name") ? fields["Blip_Name"] : section.SectionName,
                        Group = "Migrated", 
                        Icon = ParseEnumOrInt<ExtendedBlipSprite>(fields.ContainsKey("Blip_Icon") ? fields["Blip_Icon"] : "0"),
                        Color = ParseEnumOrInt<ExtendedBlipColor>(fields.ContainsKey("Blip_Color") ? fields["Blip_Color"] : "0"),
                        Size = ParseFloat(fields.ContainsKey("Blip_Size") ? fields["Blip_Size"] : "1.0"),
                        Transparency = 255, 
                        Flash = ParseBoolOption(fields.ContainsKey("Flashing_State") ? fields["Flashing_State"] : "OFF"),
                        FlashInterval = ParseInt(fields.ContainsKey("Flash_Interval") ? fields["Flash_Interval"] : "100"),
                        ShortRange = ParseBoolOption(fields.ContainsKey("Short_Range_State") ? fields["Short_Range_State"] : "ON"),
                        Position = new Position
                        {
                            X = ParseFloat(fields.ContainsKey("X") ? fields["X"] : "0"),
                            Y = ParseFloat(fields.ContainsKey("Y") ? fields["Y"] : "0"),
                            Z = ParseFloat(fields.ContainsKey("Z") ? fields["Z"] : "0")
                        }
                    };

                    blipDataList.Add(blipData);
                }
                catch (Exception ex)
                {
                    DisplayAndLogError(ex, $"Failed to migrate blip section: {section.SectionName}");
                }
            }

            
            if (blipDataList.Count > 0)
            {
                EnsureDirectoryExist(_jsonFilePath);
                using (FileStream stream = new FileStream(_jsonFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    foreach (BlipData blip in blipDataList)
                    {
                        await writer.WriteLineAsync(JsonConvert.SerializeObject(blip));
                    }
                }
                Notification.PostTicker($"Migrated {blipDataList.Count} blips from INI to JSON", true);
            }
        }
        private int ParseEnumOrInt<TEnum>(string value) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            if (Enum.TryParse<TEnum>(value, ignoreCase: true, out TEnum enumResult))
                return Convert.ToInt32(enumResult);

            if (int.TryParse(value, out int intResult))
                return intResult;

            return 0;
        }
        private float ParseFloat(string value)
        {
            return float.TryParse(value, out float result) ? result : 0f;
        }
        private int ParseInt(string value)
        {
            return int.TryParse(value, out int result) ? result : 100;
        }
        private IniData LoadIniDataFromDisk()  
        {
            try
            {

                EnsureDirectoryExist(_iniFilePath);

                if (!File.Exists(_iniFilePath))
                {
                    return new IniData();
                }

                return _iniParser.ReadFile(_iniFilePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                DisplayAndLogError(ex, $"Access denied while loading INI file from: {_iniFilePath}. Right click on the {_iniFilePath} file -> Select and press 'Properties' -> In the 'General' tab -> Look for 'Attributes' -> Make sure that the 'Read-only' checkbox is unchecked.");
                return new IniData(); 
            }
            catch (DirectoryNotFoundException ex)
            {
                DisplayAndLogError(ex, $"Directory not found: {Path.GetDirectoryName(_iniFilePath)}");
                return new IniData(); 
            }
            catch (FileNotFoundException fileEx)
            {
                DisplayAndLogError(fileEx, $"INI file not found: {_iniFilePath}");
                return new IniData(); 
            }
            catch (IOException ex)
            {
                DisplayAndLogError(ex, $"I/O error while loading INI from: {_iniFilePath}");
                return new IniData(); 
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, "Error reading INI file");
                return new IniData(); 
            }
        }
        private void InitializeManageBlipsMenu()
        {
            _manageBlipsMenu = CreateAndRegisterMenu("Manage Existing Blips", "Select a Group to Manage");
            _mainMenu.AddSubMenu(_manageBlipsMenu, "").Title = "Manage Existing Blips";

            
            _manageBlipsMenu.Opening += (sender, e) => RefreshManageBlipsMenu();
        }
        private void RefreshManageBlipsMenu()
        {
            _manageBlipsMenu.Clear();

            
            for (int a = 0; a < _availableGroups.Count; a++)
            {
                string group = _availableGroups[a];
                if (group == _createGroupLabel)
                {
                    continue;
                }
                NativeMenu groupMenu = CreateAndRegisterMenu(group, $"Manage Blips in \"{group}\" Group");
                groupMenu.Description = $"Edit, Remove, and Teleport Blips in \"{group}\" Group";
                
                List<Blip> groupBlips = new List<Blip>();
                for (int b = 0; b < _activeBlips.Count; b++)
                {
                    Blip blip = _activeBlips[b];
                    if (blip != null && blip.Exists() && _blipToGroupMap.TryGetValue(blip.Handle, out string blipGroup))
                    {
                        if (blipGroup == group)
                        {
                            groupBlips.Add(blip);
                        }
                    }
                }

                
                for (int c = 0; c < groupBlips.Count; c++)
                {
                    Blip blip = groupBlips[c];
                    
                    string blipName = blip.Name;
                    string groupName = _blipToGroupMap[blip.Handle];
                    int currentIcon = Function.Call<int>(Hash.GET_BLIP_SPRITE, blip);
                    int currentColor = Function.Call<int>(Hash.GET_BLIP_COLOUR, blip);
                    float currentSize = blip.ScaleX;
                    bool currentFlash = blip.IsFlashing;
                    int currentFlashInterval = blip.FlashInterval;
                    bool currentShortRange = blip.IsShortRange;
                    int currentTransparency = blip.Alpha;
                    Vector3 currentPosition = blip.Position;

                    NativeMenu blipMenu = CreateAndRegisterMenu(blipName, $"Edit \"{blipName}\" blip");

                    
                    NativeItem nameItem = new NativeItem($"Name ({blipName})", "This is the name of the blip in the game and in the JSON file.");
                    nameItem.Activated += (menu, item) =>
                    {
                        string newName = PromptUserForInput();
                        nameItem.Title = $"Name ({newName})";
                    };

                    int count = _availableGroups.Count;
                    string[] groups = new string[count];
                    for (int d = 0; d < count; d++)
                    {
                        groups[d] = _availableGroups[d];
                    }
                    NativeListItem<string> groupItem = new NativeListItem<string>("Current Group:", groups)
                    {
                        SelectedItem = groupName,
                        Description = $"Assign this blip to an existing group or create a new one. To create a new group switch list until it shows \"{_createGroupLabel}~w~\" then select it and you will be able to type the group name."
                    };
                    groupItem.Activated += (sourceMenu, clickedItem) =>
                    {
                        if (groupItem.SelectedIndex != 0)
                        {
                            return;
                        }

                        string userInput = PromptUserForInput();
                        if (userInput == null)
                        {
                            Notification.PostTicker("~r~Group name cannot be empty.", true);
                            return;
                        }
                        if (!_availableGroupsSet.Add(userInput))
                        {
                            Notification.PostTicker($"~y~Group \"~w~{userInput}~y~\" already exists.", true);
                            groupItem.SelectedIndex = _availableGroups.IndexOf(userInput);
                            return;
                        }

                        _availableGroups.Add(userInput);

                        
                        groupItem.Clear();
                        for (int e = 0; e < _availableGroups.Count; e++)
                        {
                            groupItem.Add(_availableGroups[e]);
                        }

                        groupItem.SelectedIndex = _availableGroups.Count - 1;
                        _shouldRefreshGroupItems = true;

                        Notification.PostTicker($"~g~Group \"~w~{userInput}~g~\" created successfully.", true);
                    };

                    
                    NativeListItem<string> iconItem = CreateEnumListItem<ExtendedBlipSprite>("Icon", true,
                        Array.IndexOf(_spriteValues, (ExtendedBlipSprite)currentIcon), "Modifiy the icon type for the blip. Either scroll through the list or select it, then enter either the name or ID.");
                    iconItem.Activated += (menu, item) =>
                    {
                        string input = PromptUserForInput()?.Trim();
                        if (!TrySelectEnumByInput<ExtendedBlipSprite>(iconItem, input))
                        {
                            Notification.PostTicker("~r~Invalid icon ID or name.", true);
                        }
                    };

                    
                    NativeListItem<string> colorItem = CreateEnumListItem<ExtendedBlipColor>("Color", true,
                        GetSelectedEnumIndex<ExtendedBlipColor>(currentColor), "Modifiy the color of the blip (white for the default color of the blip). Either scroll through the list or select it, then name either the name or ID.");
                    colorItem.Activated += (menu, item) =>
                    {
                        string input = PromptUserForInput()?.Trim();
                        if (!TrySelectEnumByInput<ExtendedBlipColor>(colorItem, input))
                        {
                            Notification.PostTicker("~r~Invalid color ID or name.", true);
                        }
                    };

                    
                    NativeItem sizeItem = new NativeItem($"Size/Scale ({currentSize})", "It controls the size/scale of the blip; set it to 1.0 if you want the default size of the blip.");
                    sizeItem.Activated += (menu, item) =>
                    {
                        string input = PromptUserForInput(true);
                        if (float.TryParse(input, out float newSize))
                        {
                            sizeItem.Title = $"Size/Scale ({newSize})";
                        }
                        else
                        {
                            Notification.PostTicker($"~r~Invalid ~r~{sizeItem.Title}~r~ input. Enter a numeric value.", true);
                        }
                    };

                    
                    NativeItem transparencyItem = new NativeItem($"Transparency ({currentTransparency})", "This adjusts how transparent the blip is; set it to 255 if you want to use the default transparency of the blip.");
                    transparencyItem.Activated += (menu, item) =>
                    {
                        string input = PromptUserForInput(true);
                        if (float.TryParse(input, out float value))
                        {
                            int alpha = (int)Math.Round(value);
                            alpha = Math.Max(0, Math.Min(255, alpha)); 

                            transparencyItem.Title = $"Transparency ({alpha})";
                        }
                        else
                        {
                            Notification.PostTicker($"~r~Invalid {transparencyItem.Title}~r~ input. Enter a number (0–255).", true);
                        }
                    };

                    
                    NativeCheckboxItem flashItem = CreateCheckboxItem("Flashing", currentFlash, "Blinking effect for the blip.");

                    
                    NativeItem flashIntervalItem = new NativeItem($"Flash interval ({currentFlashInterval} ms)", "This adjusts how fast the blip blinks; set it to 100 if you want to use the default flash speed. Flash speed is in milliseconds (higher = slower). Please note that if the flashing state is turned off, then this setting will be completely ignored. Turn it on to see the effect.");
                    flashIntervalItem.Activated += (menu, item) =>
                    {
                        string input = PromptUserForInput(true);
                        if (float.TryParse(input, out float value))
                        {
                            int interval = Math.Max(1, (int)Math.Round(value)); 
                            flashIntervalItem.Title = $"Flash interval ({interval} ms)";
                        }
                        else
                        {
                            Notification.PostTicker($"~r~Invalid {flashIntervalItem.Title}~r~ input. Enter a number.", true);
                        }
                    };

                    
                    NativeCheckboxItem shortRangeItem = CreateCheckboxItem("Short Range", currentShortRange, "Blip only shows when nearby.");

                    
                    NativeItem xItem = new NativeItem($"X Position ({currentPosition.X})", "The X coordinate of the blip");
                    xItem.Activated += (menu, item) =>
                    {
                        string input = PromptUserForInput(true);

                        if (float.TryParse(input, out float newX))
                        {
                            xItem.Title = $"X Position ({newX})";
                        }
                        else
                        {
                            Notification.PostTicker($"~r~Invalid ~r~{xItem.Title}~r~ input. Enter a numeric value.", true);
                        }

                    };

                    
                    NativeItem yItem = new NativeItem($"Y Position ({currentPosition.Y})", "The Y coordinate of the blip");
                    yItem.Activated += (menu, item) =>
                    {
                        string input = PromptUserForInput(true);

                        if (float.TryParse(input, out float newY))
                        {
                            yItem.Title = $"Y Position ({newY})";
                        }
                        else
                        {
                            Notification.PostTicker($"~r~Invalid ~r~{yItem.Title}~r~ input. Enter a numeric value.", true);
                        }

                    };

                    
                    NativeItem zItem = new NativeItem($"Z Position ({currentPosition.Z})", "The Z coordinate of the blip");
                    zItem.Activated += (menu, item) =>
                    {
                        string input = PromptUserForInput(true);

                        if (float.TryParse(input, out float newZ))
                        {
                            zItem.Title = $"Z Position ({newZ})";
                        }
                        else
                        {
                            Notification.PostTicker($"~r~Invalid ~r~{zItem.Title}~r~ input. Enter a numeric value.", true);
                        }
                    };

                    

                    
                    BlipData oldBlipData = new BlipData
                    {
                        Name = ExtractTitle(nameItem.Title),
                        Group = groupItem.SelectedItem,
                        Icon = GetSelectedEnumValue<ExtendedBlipSprite>(iconItem),
                        Color = GetSelectedEnumValue<ExtendedBlipColor>(colorItem),
                        Size = float.Parse(ExtractTitle(sizeItem.Title)),
                        Transparency = int.Parse(ExtractTitle(transparencyItem.Title)),
                        Flash = flashItem.Checked,
                        FlashInterval = int.Parse(ExtractTitle(flashIntervalItem.Title, true)),
                        ShortRange = shortRangeItem.Checked,
                        Position = new Position
                        {
                            X = float.Parse(ExtractTitle(xItem.Title)),
                            Y = float.Parse(ExtractTitle(yItem.Title)),
                            Z = float.Parse(ExtractTitle(zItem.Title))
                        }
                    };

                    
                    NativeItem teleportBtnItem = new NativeItem("~b~Teleport to blip", "Teleports to this blip's location.");
                    teleportBtnItem.Activated += (menu, item) =>
                    {
                        try
                        {
                            Ped player = Game.Player.Character;
                            if (!IsValidPlayer(player))
                            {
                                Notification.PostTicker("~r~Failed to acquire valid player. Unable to teleport to blip.", true);
                                return;
                            }

                            Vector3 blipPosition = new Vector3(oldBlipData.Position.X, oldBlipData.Position.Y, oldBlipData.Position.Z);
                            Vehicle playerVehicle = player.CurrentVehicle;
                            if (playerVehicle != null && playerVehicle.Exists())
                            {
                                playerVehicle.Position = blipPosition;
                            }
                            else
                            {
                                player.Position = blipPosition;
                            }
                            Notification.PostTicker($"~g~Teleported to blip successfully!", true);
                        }
                        catch (Exception ex)
                        {
                            DisplayAndLogError(ex, "Teleporting to blip");
                            Notification.PostTicker("~r~Failed to teleport to blip.", true);
                        }
                    };

                    
                    NativeItem copyBtnItem = new NativeItem("~y~Copy blip at current position", "Creates a copy of this blip with the exact same properties at your current location.");
                    copyBtnItem.Activated += async (menu, item) =>
                    {
                        try
                        {
                            Ped player = Game.Player.Character;
                            if (!IsValidPlayer(player))
                            {
                                Notification.PostTicker("~r~Failed to acquire valid player. Unable to copy blip to current location.", true);
                                return;
                            }

                            Vector3 playerPos = GetPlayerCoordinates(player);

                            BlipData copiedBlipData = new BlipData
                            {
                                Name = oldBlipData.Name,
                                Group = oldBlipData.Group,
                                Icon = oldBlipData.Icon,
                                Color = oldBlipData.Color,
                                Size = oldBlipData.Size,
                                Transparency = oldBlipData.Transparency,
                                Flash = oldBlipData.Flash,
                                FlashInterval = oldBlipData.FlashInterval,
                                ShortRange = oldBlipData.ShortRange,
                                Position = new Position
                                {
                                    X = playerPos.X,
                                    Y = playerPos.Y,
                                    Z = playerPos.Z
                                }
                            };

                            
                            CreateBlipFromData(copiedBlipData);

                            
                            await SaveBlipToFileAsync(copiedBlipData);

                            Notification.PostTicker("~g~Blip copied successfully at your current position!", true);
                            blipMenu.Visible = false;
                        }
                        catch (Exception ex)
                        {
                            DisplayAndLogError(ex, "Copying blip");
                            Notification.PostTicker("~r~Failed to copy blip.", true);
                        }
                    };

                    
                    NativeItem deleteBtn = new NativeItem("~r~Delete Blip", "Permanently remove this blip from game and JSON file. There is no undo. Confirm with \"yes\"");
                    deleteBtn.Activated += async (sender, item) =>
                    {
                        string userInput = PromptUserForInput()?.ToLower().Trim();
                        if (userInput != "yes")
                        {
                            Notification.PostTicker($"~r~Invalid input. Confirm with \"yes\" to delete the blip.", true);
                            return;
                        }

                        
                        if (blip != null && blip.Exists())
                        {
                            blip.Delete();
                            _activeBlips.Remove(blip);
                        }

                        
                        if (oldBlipData != null)
                        {
                            _blipToGroupMap.Remove(blip.Handle);
                        }

                        blipMenu.Visible = false;
                        await DeleteBlipFromFileAsync(oldBlipData);
                    };

                    NativeItem updatePosBtn = new NativeItem("~y~Update to Current Position", "Set X, Y, and Z to your current location. ~y~Press the saave button to apply changes.");
                    updatePosBtn.Activated += (sender, item) =>
                    {
                        Ped player = Game.Player.Character;
                        if (!IsValidPlayer(player))
                        {
                            Notification.PostTicker("~r~Failed to acquire valid player. Unable to update blip location to current location.", true);
                            return;
                        }

                        Vector3 playerPos = GetPlayerCoordinates(player);
                        Position blipPosition = oldBlipData.Position;
                        blipPosition.X = playerPos.X;
                        blipPosition.Y = playerPos.Y;
                        blipPosition.Z = playerPos.Z;

                        Notification.PostTicker("~y~Position updated to current player coordinates. Press the save button below to apply changes.", true);
                    };

                    NativeItem saveBtnItem = new NativeItem("~g~Save changes");
                    saveBtnItem.Activated += async (menu, item) =>
                    {
                        string currentLabel = _availableGroups[groupItem.SelectedIndex];
                        bool isCreateGroupLabel = currentLabel == _createGroupLabel;
                        if (isCreateGroupLabel)
                        {
                            groupItem.SelectedIndex = _availableGroups.IndexOf(groupName);
                        }

                        Vector3 newPos = new Vector3(float.Parse(ExtractTitle(xItem.Title)), float.Parse(ExtractTitle(yItem.Title)), float.Parse(ExtractTitle(zItem.Title)));

                        BlipData newBlipData = new BlipData
                        {
                            Name = ExtractTitle(nameItem.Title),
                            Group = groupItem.SelectedItem,
                            Icon = GetSelectedEnumValue<ExtendedBlipSprite>(iconItem),
                            Color = GetSelectedEnumValue<ExtendedBlipColor>(colorItem),
                            Size = float.Parse(ExtractTitle(sizeItem.Title)),
                            Transparency = int.Parse(ExtractTitle(transparencyItem.Title)),
                            Flash = flashItem.Checked,
                            FlashInterval = int.Parse(ExtractTitle(flashIntervalItem.Title, true)),
                            ShortRange = shortRangeItem.Checked,
                            Position = new Position
                            {
                                X = newPos.X,
                                Y = newPos.Y,
                                Z = newPos.Z
                            }
                        };

                        
                        try
                        {
                            
                            if (blip != null && blip.Exists())
                            {
                                blip.Delete();
                                _activeBlips.Remove(blip);
                            }

                            
                            if (oldBlipData != null)
                            {
                                _blipToGroupMap.Remove(blip.Handle);
                            }

                            
                            CreateBlipFromData(newBlipData);
                            blipMenu.Visible = false;
                            await DeleteBlipFromFileAsync(oldBlipData);
                            await Task.Delay(1_00);
                            await SaveBlipToFileAsync(newBlipData);
                        }
                        catch (Exception ex)
                        {
                            DisplayAndLogError(ex, "Recreating blip");
                            Notification.PostTicker("~r~Failed to recreate blip!", true);
                            return;
                        }
                    };

                    
                    blipMenu.Add(nameItem);
                    blipMenu.Add(groupItem);
                    blipMenu.Add(iconItem);
                    blipMenu.Add(colorItem);
                    blipMenu.Add(sizeItem);
                    blipMenu.Add(transparencyItem);
                    blipMenu.Add(flashItem);
                    blipMenu.Add(flashIntervalItem);
                    blipMenu.Add(shortRangeItem);
                    blipMenu.Add(xItem);
                    blipMenu.Add(yItem);
                    blipMenu.Add(zItem);
                    blipMenu.Add(teleportBtnItem);
                    blipMenu.Add(updatePosBtn);
                    blipMenu.Add(copyBtnItem);
                    blipMenu.Add(deleteBtn);
                    blipMenu.Add(saveBtnItem);

                    groupMenu.AddSubMenu(blipMenu, "").Title = blipName;
                }

                _manageBlipsMenu.AddSubMenu(groupMenu, "").Title = group;
            }

            
            NativeItem refreshItem = new NativeItem("~y~Refresh List", "Refresh the list of groups and blips");
            refreshItem.Activated += (menu, item) => RefreshManageBlipsMenu();

            _manageBlipsMenu.Add(refreshItem);
        }
        private string ExtractTitle(string input, bool removeUnits = false)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            int start = input.IndexOf('(');
            int end = input.IndexOf(')');

            if (start < 0 || end <= start)
                return string.Empty;

            string result = input.Substring(start + 1, end - start - 1).Trim();

            if (removeUnits)
            {
                
                int spaceIndex = result.IndexOf(' ');
                if (spaceIndex > 0)
                    result = result.Substring(0, spaceIndex);
            }

            return result;
        }
        private int GetSelectedEnumIndex<TEnum>(int value) where TEnum : struct, Enum
        {
            Array values = GetEnumValuesCached(typeof(TEnum));
            for (int i = 0; i < values.Length; i++)
            {
                if (Convert.ToInt32(values.GetValue(i)) == value)
                {
                    return i;
                }
            }
            return 0;
        }

        
        private void OnAborted(object sender, EventArgs e)
        {
            DeleteAllGameBlips();

            if (_previewBlip != null && _previewBlip.Exists())
            {
                _previewBlip.Delete();
            }

            for (int i = 0; i < _tempFiles.Count; i++)
            {
                try
                {
                    string file = _tempFiles[i];
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    DisplayAndLogError(ex, "Error Deleting Temp Files");
                }
            }

            _logWriter?.Flush();
            _logWriter?.Dispose();
            _logWriter = null;
        }
        private void OnTick(object sender, EventArgs e)
        {
            
            Interval = (_uiMenuPool.AreAnyVisible ||                            
                        _showCoordsOnScreen ||
                        _waitingForKeyAssignment ||
                        _blipQueue.Count > 0 ||
                        Function.Call<int>(Hash.UPDATE_ONSCREEN_KEYBOARD) == 0) 
                        ? _activeInterval : _defaultInterval;

            if (_blipQueue.Count > 0)
            {
                int count = Math.Min(_blipsPerTick, _blipQueue.Count);

                for (int i = 0; i < count; i++)
                {
                    CreateBlipFromData(_blipQueue.Dequeue());
                }
            }

            bool isLeftMousePressed = Game.IsControlJustPressed(Control.CursorAccept);
            
            if (isLeftMousePressed)
            {
                _addBlipMenu.AcceptsInput = false;
            }
            else
            {
                _addBlipMenu.AcceptsInput = true;
            }

            
            _uiMenuPool.Process();

            int index = _colorItem.SelectedIndex;
            if (index != _lastKnownColorItemIndex)
            {
                Color color = (uint)index < _blipColorsArray.Length ? _blipColorsArray[index] : Color.White;

                _sprite.Color = color;
                _sprite1.Color = color;
                _sprite2.Color = color;

                _customSprite.Color = color;
                _customSprite1.Color = color;
                _customSprite2.Color = color;

                _lastKnownColorItemIndex = index;
            }

            float ScreenWidth = Screen.Width;
            float ScreenHeight = Screen.Height;
            if (_lastKnownScreenWidth != ScreenWidth || _lastKnownScreenHeight != ScreenHeight)
            {
                float screenWidthForSprite1And2 = ScreenWidth * 0.49f;

                _sprite.Position = new PointF(screenWidthForSprite1And2, 5);
                _sprite1.Position = new PointF(screenWidthForSprite1And2, ScreenHeight * 0.51f);
                _sprite2.Position = new PointF(ScreenWidth * 0.01f, ScreenHeight * 0.56f);

                _customSprite.Position = _sprite.Position;
                _customSprite1.Position = _sprite1.Position;
                _customSprite2.Position = _sprite2.Position;

                _lastKnownScreenWidth = ScreenWidth;
                _lastKnownScreenHeight = ScreenHeight;
            }

            int selectedItemIndex = _addBlipMenu.SelectedIndex;
            if (_addBlipMenu.Visible)
            {
                if (selectedItemIndex == 2 || selectedItemIndex == 3)
                {
                    if (_shouldUseModdedTextureSheet)
                    {
                        _sprite.Draw();
                        _sprite1.Draw();
                        _sprite2.Draw();
                        _addBlipMenu.MouseBehavior = MenuMouseBehavior.Movement;
                    }
                    else
                    {
                        _customSprite.Draw();
                        _customSprite1.Draw();
                        _customSprite2.Draw();

                        float cx = Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, (int)Control.CursorX);
                        float cy = Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, (int)Control.CursorY);

                        
                        int screenW = (int)ScreenWidth;
                        int screenH = (int)ScreenHeight;
                        int px = (int)(cx * screenW);
                        int py = (int)(cy * screenH);

                        _mousePointer.Position = new Point(px, py);

                        _mousePointer.Draw();

                        _addBlipMenu.MouseBehavior = MenuMouseBehavior.Disabled;
                    }

                    
                    _scaleform.CallFunction("CLEAR_ALL");
                    Function.Call(Hash.DRAW_SCALEFORM_MOVIE_FULLSCREEN, _scaleform.Handle, 255, 255, 255, 255, 0);

                    if (isLeftMousePressed && TryFindClosestIconWithinRadius((int)(Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, 239) * (int)ScreenWidth), (int)(Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, 240) * (int)ScreenHeight), out ExtendedBlipSprite clickedIcon) && _iconItem != null)
                    {
                        _iconItem.SelectedIndex = Array.IndexOf(_spriteValues, clickedIcon);
                    }
                }
                else
                {
                    _addBlipMenu.MouseBehavior = MenuMouseBehavior.Movement;
                }

                if (_didWeInitializePreviewBlip)
                {
                    Ped player0 = Game.Player.Character;
                    if (IsValidPlayer(player0))
                    {
                        Vector3 playerPos = GetPlayerCoordinates(player0);
                        Vector3 playerForward = player0.ForwardVector;
                        Vector3 previewPos = playerPos + (playerForward * 18);

                        if (_lastKnownPreviewPos != previewPos)
                        {
                            _previewBlip.Position = previewPos;
                            _lastKnownPreviewPos = previewPos;
                        }
                    }
                }
            }

            if (_shouldRefreshGroupItems)
            {
                _groupItem.Clear();

                for (int i = 0; i < _availableGroups.Count; i++)
                {
                    _groupItem.Add(_availableGroups[i]);
                }

                if (!string.IsNullOrEmpty(_pendingGroupSelection))
                {
                    int index2 = _availableGroups.IndexOf(_pendingGroupSelection);

                    if (index2 >= 0 && index2 < _groupItem.Items.Count)
                    {
                        _groupItem.SelectedIndex = index2;
                    }

                    _pendingGroupSelection = null;
                }

                _shouldRefreshGroupItems = false;
            }

            if (_waitingForKeyAssignment)
            {
                
                Game.DisableAllControlsThisFrame();

                
                _settingsMenu.Visible = false;
                for (int i = 1; i <= 22; i++)
                {
                    Function.Call(Hash.HIDE_HUD_COMPONENT_THIS_FRAME, i);
                }
                Function.Call(Hash.DISABLE_FRONTEND_THIS_FRAME);
                Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);

                
                _alpha += (int)(150f * Game.LastFrameTime);
                _alpha = Math.Min(_alpha, 170);
                DrawFadeOverlay(_waitingForKeyAssignment ? GetTypingText() : _typedOverlayText);
            }
            else if (_alpha > 0)
            {
                _alpha -= (int)(150f * Game.LastFrameTime);
                _alpha = Math.Max(_alpha, 0);

                DrawFadeOverlay(_waitingForKeyAssignment ? GetTypingText() : _typedOverlayText);
            }

            if (!_showCoordsOnScreen)
            {
                return;
            }

            Ped player = Game.Player.Character;
            if (!IsValidPlayer(player))
            {
                return;
            }

            Vector3 playerCoordinates = GetPlayerCoordinates(player);

            
            Function.Call(Hash.BEGIN_TEXT_COMMAND_PRINT, "CELL_EMAIL_BCON");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, $"~b~X: ~w~{playerCoordinates.X:F2} ~b~Y: ~w~{playerCoordinates.Y:F2} ~b~Z: ~w~{playerCoordinates.Z:F2}");
            Function.Call(Hash.END_TEXT_COMMAND_PRINT);
        }
        private async void OnKeyDown(object sender, KeyEventArgs e)
        {
            Keys key = e.KeyCode;
            string keyName = key.ToString();

            
            if (_waitingForKeyAssignment && key != Keys.None)
            {
                _waitingForKeyAssignment = false;
                _settingsMenu.Visible = true;

                switch (_pendingKeyBind)
                {
                    case PendingKeyBind.ToggleCoordinates:
                        _globalSettings.ToggleCoordinatesKey = keyName;
                        break;
                    case PendingKeyBind.ReloadBlips:
                        _globalSettings.ReloadBlipsKey = keyName;
                        break;
                    case PendingKeyBind.ToggleBlipVisibility:
                        _globalSettings.ToggleBlipsVisibilityKey = keyName;
                        break;
                    case PendingKeyBind.ToggleModdedTextureSheet:
                        _globalSettings.ToggleModdedTextureSheetKey = keyName;
                        break;
                    case PendingKeyBind.OpenMenu:
                        _globalSettings.OpenMenuKey = keyName;
                        break;
                }

                
                if (_pendingListItem != null)
                {
                    int index = Array.IndexOf(_keysNames, keyName);
                    if (index >= 0)
                    {
                        _pendingListItem.SelectedIndex = index;
                    }

                    _pendingListItem = null;
                }

                await SaveSettingsAsync();
            }

            if (keyName == _globalSettings.ToggleCoordinatesKey)
            {
                _showCoordsOnScreen = !_showCoordsOnScreen;
                Notification.PostTicker(_showCoordsOnScreen ? "Coordinates display enabled." : "Coordinates display disabled.", true);
            }
            else if (keyName == _globalSettings.ReloadBlipsKey)
            {
                DeleteAllGameBlips();
                await LoadAndCreateBlips();
                if (_activeBlips.Count > 0)
                {
                    Notification.PostTicker($"~g~Reloaded {_activeBlips.Count} blip(s).", true);
                }
            }
            else if (keyName == _globalSettings.ToggleBlipsVisibilityKey)
            {
                ToggleBlipsVisibility();
            }
            else if (keyName == _globalSettings.OpenMenuKey)
            {
                if (!_uiMenuPool.AreAnyVisible)
                {
                    _mainMenu.Visible = !_mainMenu.Visible;
                }
            }
            else if (keyName == _globalSettings.ToggleModdedTextureSheetKey)
            {
                _shouldUseModdedTextureSheet = !_shouldUseModdedTextureSheet;
                _ = SaveModdedTextureSheetSettingAsync();
            }
        }

        private void InitializeAddBlipMenu()
        {
            _addBlipMenu = CreateAndRegisterMenu("Add New Blip", "Enter Blip Details");

            _addBlipMenu.Closing += (sender, e) =>
            {
                
                _addBlipMenu.SelectedIndex = 0;
                Function.Call(Hash.SET_BLIP_DISPLAY, _previewBlip?.Handle, 0);
            };
            _addBlipMenu.Opening += (s, e) =>
            {
                Function.Call(Hash.SET_BLIP_DISPLAY, _previewBlip?.Handle, 2);
            };

            NativeItem nameItem = new NativeItem("Name", "This will be the name of the blip in the game and in the JSON file.");
            nameItem.Activated += (sourceMenu, clickedItem) =>
            {
                string userInput = PromptUserForInput();
                nameItem.Title = $"Name ({userInput})";
                _nameItemStorageTemp = userInput;
            };

            int count = _availableGroups.Count;
            string[] groups = new string[count];
            for (int i = 0; i < count; i++)
            {
                groups[i] = _availableGroups[i];
            }
            _groupItem = new NativeListItem<string>("Add To Group:", groups)
            {
                SelectedIndex = 0,
                Description = $"Assign this blip to an existing group or create a new one. To create a new group switch list until it shows \"{_createGroupLabel}~w~\" then select it and you will be able to type the group name."
            };
            _groupItem.Activated += (sourceMenu, clickedItem) =>
            {
                if (_groupItem.SelectedIndex != 0)
                {
                    return;
                }

                string userInput = PromptUserForInput();
                if (userInput == null)
                {
                    Notification.PostTicker("~r~Group name cannot be empty.", true);
                    return;
                }
                if (!_availableGroupsSet.Add(userInput))
                {
                    Notification.PostTicker($"~y~Group \"~w~{userInput}~y~\" already exists.", true);
                    _groupItem.SelectedIndex = _availableGroups.IndexOf(userInput);
                    _groupStorageTemp = userInput;
                    return;
                }

                _availableGroups.Add(userInput);

                
                _groupItem.Clear();
                for (int i = 0; i < _availableGroups.Count; i++)
                {
                    _groupItem.Add(_availableGroups[i]);
                }

                _groupItem.SelectedIndex = _availableGroups.Count - 1;
                _groupStorageTemp = userInput;

                Notification.PostTicker($"~g~Group \"~w~{userInput}~g~\" created successfully.", true);
            };

            _iconItem = CreateEnumListItem<ExtendedBlipSprite>("Icon", true, 0, "Choose the icon type for the blip. Either scroll through the list or select it, then enter either the name or ID.");
            _iconItem.Activated += (sourceMenu, clickedItem) =>
            {
                string input = PromptUserForInput()?.Trim();

                if (!TrySelectEnumByInput<ExtendedBlipSprite>(_iconItem, input))
                {
                    Notification.PostTicker("~r~Invalid icon ID or name.", true);
                }
            };
            _iconItem.ItemChanged += (sender, index) =>
            {
                UpdatePreviewBlip(GetSelectedEnumValue<ExtendedBlipSprite>(_iconItem), GetSelectedEnumValue<ExtendedBlipColor>(_colorItem), _sizeItemStorageTemp);
            };

            _colorItem = CreateEnumListItem<ExtendedBlipColor>("Color", true, 0, "Choose the color of the blip (white for the default color of the blip). Either scroll through the list or select it, then name either the name or ID.");
            _colorItem.Activated += (sourceMenu, clickedItem) =>
            {
                string input = PromptUserForInput()?.Trim();

                if (!TrySelectEnumByInput<ExtendedBlipColor>(_colorItem, input))
                {
                    Notification.PostTicker("~r~Invalid color ID or name.", true);
                }
            };
            _colorItem.ItemChanged += (sender, index) =>
            {
                UpdatePreviewBlip(GetSelectedEnumValue<ExtendedBlipSprite>(_iconItem), GetSelectedEnumValue<ExtendedBlipColor>(_colorItem), _sizeItemStorageTemp); ;
            };

            NativeItem sizeItem = new NativeItem("~y~Size/Scale", "It controls the size/scale of the blip; leave it if you want the default size of the blip (1.0).");
            sizeItem.Activated += (sourceMenu, clickedItem) =>
            {
                string userInput = PromptUserForInput(true);

                if (float.TryParse(userInput, out float value))
                {
                    sizeItem.Title = $"~y~Size/Scale ({value})";
                    _sizeItemStorageTemp = value;
                    UpdatePreviewBlip(GetSelectedEnumValue<ExtendedBlipSprite>(_iconItem), GetSelectedEnumValue<ExtendedBlipColor>(_colorItem), _sizeItemStorageTemp); ;
                }
                else
                {
                    Notification.PostTicker($"~r~Invalid ~r~{sizeItem.Title}~r~ input. Enter a numeric value.", true);
                }
            };

            NativeItem xInputItem = new NativeItem("~y~X Position", "The X coordinate of the blip—leave it if you want to use the current X coordinate of the player.");
            xInputItem.Activated += (sourceMenu, clickedItem) =>
            {
                string userInput = PromptUserForInput(true);

                if (float.TryParse(userInput, out float value))
                {
                    xInputItem.Title = $"~y~X Position ({value})";
                    _xItemStorageTemp = value;
                }
                else
                {
                    Notification.PostTicker($"~r~Invalid ~r~{xInputItem.Title}~r~ input. Enter a numeric value.", true);
                }
            };

            NativeItem yInputItem = new NativeItem("~y~Y Position", "The Y coordinate of the blip—leave it if you want to use the current Y coordinate of the player.");
            yInputItem.Activated += (sourceMenu, clickedItem) =>
            {
                string userInput = PromptUserForInput(true);

                if (float.TryParse(userInput, out float value))
                {
                    yInputItem.Title = $"~y~Y Position ({value})";
                    _yItemStorageTemp = value;
                }
                else
                {
                    Notification.PostTicker($"~r~Invalid ~r~{yInputItem.Title}~r~ input. Enter a numeric value.", true);
                }
            };

            NativeItem zInputItem = new NativeItem("~y~Z Position", "The Z coordinate of the blip—leave it if you want to use the current Z coordinate of the player.");
            zInputItem.Activated += (sourceMenu, clickedItem) =>
            {
                string userInput = PromptUserForInput(true);

                if (float.TryParse(userInput, out float value))
                {
                    zInputItem.Title = $"~y~Z Position ({value})";
                    _zItemStorageTemp = value;
                }
                else
                {
                    Notification.PostTicker($"~r~Invalid ~r~{zInputItem.Title}~r~ input. Enter a numeric value.", true);
                }
            };

            NativeCheckboxItem flashItem = CreateCheckboxItem("Flashing", false, "Blinking effect for the blip.");
            flashItem.CheckboxChanged += (sender, isChecked) =>
            {
                RecreatePreviewBlip(
                    GetSelectedEnumValue<ExtendedBlipSprite>(_iconItem),
                    GetSelectedEnumValue<ExtendedBlipColor>(_colorItem),
                    _sizeItemStorageTemp,
                    flash: flashItem.Checked,
                    flashInterval: _flashIntervalItemStorageTemp ?? 100,
                    transparency: _transparencyItemStorageTemp ?? 255
                );
            };

            NativeItem flashIntervalItem = new NativeItem("~y~Flash interval", "This adjusts how fast the blip blinks; leave it if you want to use the default flash speed (100 ms). Flash speed is in milliseconds (higher = slower). Please note that if the flashing state is turned off, then this setting will be completely ignored. Turn it on to see the effect.");
            flashIntervalItem.Activated += (sourceMenu, clickedItem) =>
            {
                string userInput = PromptUserForInput(true);

                if (float.TryParse(userInput, out float value))
                {
                    int interval = Math.Max(1, (int)Math.Round(value));
                    flashIntervalItem.Title = $"~y~Flash interval ({interval} ms)";
                    _flashIntervalItemStorageTemp = interval;

                    
                    RecreatePreviewBlip(
                        GetSelectedEnumValue<ExtendedBlipSprite>(_iconItem),
                        GetSelectedEnumValue<ExtendedBlipColor>(_colorItem),
                        _sizeItemStorageTemp,
                        flash: flashItem.Checked,
                        flashInterval: interval,
                        transparency: _transparencyItemStorageTemp ?? 255
                    );
                }
                else
                {
                    Notification.PostTicker($"~r~Invalid {flashIntervalItem.Title}~r~ input. Enter a number. ", true);
                }
            };

            NativeCheckboxItem shortRangeItem = CreateCheckboxItem("Short Range", true, "Blip only shows when nearby.");

            NativeItem transparencyItem = new NativeItem("~y~Transparency", "This adjusts how transparent the blip is; leave it if you want to use the default transparency of the blip (255).");
            transparencyItem.Activated += (sourceMenu, clickedItem) =>
            {
                string userInput = PromptUserForInput(true);

                if (float.TryParse(userInput, out float value))
                {
                    int alpha = (int)Math.Round(value);
                    alpha = Math.Max(0, Math.Min(255, alpha)); 

                    transparencyItem.Title = $"~y~Transparency ({alpha}) ";
                    _transparencyItemStorageTemp = alpha;

                    
                    UpdatePreviewBlip(
                        GetSelectedEnumValue<ExtendedBlipSprite>(_iconItem),
                        GetSelectedEnumValue<ExtendedBlipColor>(_colorItem),
                        _sizeItemStorageTemp,
                        flash: flashItem.Checked,
                        flashInterval: _flashIntervalItemStorageTemp,
                        transparency: alpha
                    );
                }
                else
                {
                    Notification.PostTicker($"~r~Invalid {transparencyItem.Title}~r~ input. Enter a number (0–255). ", true);
                }
            };

            NativeItem saveBlipBtn = new NativeItem("~g~Save Blip");
            saveBlipBtn.Activated += async (menu, clickedItem) =>
            {
                
                bool hasX = _xItemStorageTemp.HasValue;
                bool hasY = _yItemStorageTemp.HasValue;
                bool hasZ = _zItemStorageTemp.HasValue;

                float xCoord = _xItemStorageTemp ?? 0f;
                float yCoord = _yItemStorageTemp ?? 0f;
                float zCoord = _zItemStorageTemp ?? 0f;

                Vector3 pos;

                
                string FormatCoordList(List<string> items)
                {
                    if (items.Count == 0)
                    {
                        return string.Empty;
                    }
                    if (items.Count == 1)
                    {
                        return items[0];
                    }
                    if (items.Count == 2)
                    {
                        return $"{items[0]} and {items[1]}";
                    }

                    return $"{items[0]}, {items[1]} and {items[2]}";
                }

                
                List<string> missing = new List<string>();
                if (!hasX)
                {
                    missing.Add("x");
                }
                if (!hasY)
                {
                    missing.Add("y");
                }
                if (!hasZ)
                {
                    missing.Add("z");
                }

                if (missing.Count > 0)
                {
                    string missingText = FormatCoordList(missing);
                    Notification.PostTicker($"~y~Missing coordinate(s): {missingText}. Using the current player's corresponding coordinate(s) for the missing axis/axes.", true);

                    Ped player = Game.Player.Character;
                    if (!IsValidPlayer(player))
                    {
                        Notification.PostTicker("~r~Failed to acquire valid player or character.", true);
                        return;
                    }

                    Vector3 playerPos = GetPlayerCoordinates(player);

                    
                    if (!hasX)
                    {
                        xCoord = playerPos.X;
                    }
                    if (!hasY)
                    {
                        yCoord = playerPos.Y;
                    }
                    if (!hasZ)
                    {
                        zCoord = playerPos.Z;
                    }

                    pos = new Vector3(xCoord, yCoord, zCoord);
                }
                else
                {
                    
                    List<string> invalid = new List<string>();
                    if (float.IsNaN(xCoord) || float.IsInfinity(xCoord))
                    {
                        invalid.Add("x");
                    }
                    if (float.IsNaN(yCoord) || float.IsInfinity(yCoord))
                    {
                        invalid.Add("y");
                    }
                    if (float.IsNaN(zCoord) || float.IsInfinity(zCoord))
                    {
                        invalid.Add("z");
                    }

                    if (invalid.Count > 0)
                    {
                        string invalidText = FormatCoordList(invalid);
                        Notification.PostTicker($"~y~Invalid coordinate(s): {invalidText}. Please provide valid numeric x, y, and z values. Using current player coordinates.", true);

                        Ped player = Game.Player.Character;
                        if (!IsValidPlayer(player))
                        {
                            Notification.PostTicker("~r~Failed to acquire valid player or character.", true);
                            return;
                        }

                        pos = GetPlayerCoordinates(player);
                    }
                    else
                    {
                        
                        pos = new Vector3(xCoord, yCoord, zCoord);
                    }
                }
                string currentLabel = _availableGroups[_groupItem.SelectedIndex];
                bool isCreateGroupLabel = currentLabel == _createGroupLabel;
                if (isCreateGroupLabel)
                {
                    
                    if (_availableGroupsSet.Add(_groupStorageTemp))
                    {
                        _availableGroups.Add(_groupStorageTemp);
                        _pendingGroupSelection = _groupStorageTemp;
                        _shouldRefreshGroupItems = true;
                    }
                    else
                    {
                        _groupItem.SelectedIndex = _availableGroups.IndexOf(_groupStorageTemp);
                    }
                }

                BlipData blipData = new BlipData
                {
                    Name = _nameItemStorageTemp,
                    Group = isCreateGroupLabel ? "Custom" : currentLabel,
                    Icon = GetSelectedEnumValue<ExtendedBlipSprite>(_iconItem),
                    Color = GetSelectedEnumValue<ExtendedBlipColor>(_colorItem),
                    Size = _sizeItemStorageTemp ?? 1.0f,
                    Position = new Position { X = pos.X, Y = pos.Y, Z = pos.Z },
                    Flash = flashItem.Checked,
                    FlashInterval = _flashIntervalItemStorageTemp ?? 100,
                    ShortRange = shortRangeItem.Checked,
                    Transparency = _transparencyItemStorageTemp ?? 255
                };
                CreateBlipFromData(blipData);
                await SaveBlipToFileAsync(blipData);
                Notification.PostTicker("~g~Created blip successfully", true);

                _xItemStorageTemp = null; xInputItem.Title = "~y~X Position";
                _yItemStorageTemp = null; yInputItem.Title = "~y~Y Position";
                _zItemStorageTemp = null; zInputItem.Title = "~y~Z Position";
            };

            _addBlipMenu.Add(nameItem);
            _addBlipMenu.Add(_groupItem);
            _addBlipMenu.Add(_iconItem);
            _addBlipMenu.Add(_colorItem);
            _addBlipMenu.Add(sizeItem);
            _addBlipMenu.Add(transparencyItem);
            _addBlipMenu.Add(flashItem);
            _addBlipMenu.Add(flashIntervalItem);
            _addBlipMenu.Add(shortRangeItem);
            _addBlipMenu.Add(xInputItem);
            _addBlipMenu.Add(yInputItem);
            _addBlipMenu.Add(zInputItem);
            _addBlipMenu.Add(saveBlipBtn);

            _mainMenu.AddSubMenu(_addBlipMenu, "").Title = "Add New Blip";
        }
        private void InitializeGlobalSettingsMenu()
        {
            _settingsMenu = CreateAndRegisterMenu("Global Settings", "Configure Keys & Options");

            NativeListItem<string> openMenuKeyItem = CreateEnumListItem<Keys>("Open Menu Key", false, GetKeyIndex(_globalSettings.OpenMenuKey), "Choose the key to open the main menu. ~y~Either scroll through the list or press enter then press the key you want to assign");
            BindKeyAssignment(openMenuKeyItem, PendingKeyBind.OpenMenu, "Open Menu Key");

            NativeListItem<string> toggleCoordKeyItem = CreateEnumListItem<Keys>("Toggle Coordinates Key", false, GetKeyIndex(_globalSettings.ToggleCoordinatesKey), "Choose the key for coordinates display. ~y~Either scroll through the list or press enter then press the key you want to assign");
            BindKeyAssignment(toggleCoordKeyItem, PendingKeyBind.ToggleCoordinates, "Toggle Coordinates Key");

            NativeListItem<string> reloadKeyItem = CreateEnumListItem<Keys>("Reload Blips Key", false, GetKeyIndex(_globalSettings.ReloadBlipsKey), "Choose the key to reload script. ~y~Either scroll through the list or press enter then press the key you want to assign");
            BindKeyAssignment(reloadKeyItem, PendingKeyBind.ReloadBlips, "Reload Blips Key");

            NativeListItem<string> toggleVisibilityKeyItem = CreateEnumListItem<Keys>("Toggle Blips Visibility Key", false, GetKeyIndex(_globalSettings.ToggleBlipsVisibilityKey), "Choose the key for add/loaded blips visibility. ~y~Either scroll through the list or press enter then press the key you want to assign");
            BindKeyAssignment(toggleVisibilityKeyItem, PendingKeyBind.ToggleBlipVisibility, "Toggle Blips Visibility Key");

            NativeListItem<string> toggleModdedTextureSheetKeyItem = CreateEnumListItem<Keys>("Toggle Modded Texture Sheet Key", false, GetKeyIndex(_globalSettings.ToggleModdedTextureSheetKey), "Choose the key for switching between vanilla and modded texture sheet that displays in \"Add New Blip\" menu for the icons on scrren. ~y~Either scroll through the list or press enter then press the key you want to assign");
            BindKeyAssignment(toggleModdedTextureSheetKeyItem, PendingKeyBind.ToggleModdedTextureSheet, "Toggle Modded Texture Sheet Key");

            NativeCheckboxItem notifyBlipItem = CreateCheckboxItem("Show Blip Notification", _globalSettings.ShowBlipLoadNotification, "Shows a notification when blips are loaded from the JSON file. ~y~Press the save button to save changes.");
            NativeCheckboxItem enableAddOnBlipItem = CreateCheckboxItem("Enable Add-On Blips", _globalSettings.EnableAddOnBlips, "Loads all blips from the Add-On blips mod if the mod is installed. ~y~Press the save button to apply changes.");
            NativeCheckboxItem autoCheckUpdatesItem = CreateCheckboxItem("Auto-check for updates on startup", _globalSettings.AutoCheckForUpdatesOnStartup, "Automatically checks for script updates when the game starts. ~y~Press the save button to apply changes.");
            NativeItem checkUpdatesBtn = new NativeItem("~b~Check for Updates Now");
            checkUpdatesBtn.Activated += (s, i) =>
            {
                Task.Run(async () => await CheckForUpdatesAsync());
            };

            NativeItem restToDefaultBtn = new NativeItem("~b~Reset to Defaults", "Resets all options and keys to default. ~y~Press the save button to apply changes.");
            restToDefaultBtn.Activated += (s, i) =>
            {
                openMenuKeyItem.SelectedItem = "F10";
                toggleCoordKeyItem.SelectedItem = "F1";
                reloadKeyItem.SelectedItem = "F2";
                toggleVisibilityKeyItem.SelectedItem = "F3";
                toggleModdedTextureSheetKeyItem.SelectedItem = "F5";
                notifyBlipItem.Checked = true;
                enableAddOnBlipItem.Checked = false;
                autoCheckUpdatesItem.Checked = true;
                _shouldUseModdedTextureSheet = false;
            };

            NativeItem saveSettingsBtn = new NativeItem("~g~Save Settings");
            saveSettingsBtn.Activated += async (sourceMenu, clickedItem) =>
            {
                _globalSettings.ToggleCoordinatesKey = toggleCoordKeyItem.SelectedItem;
                _globalSettings.ReloadBlipsKey = reloadKeyItem.SelectedItem;
                _globalSettings.ToggleBlipsVisibilityKey = toggleVisibilityKeyItem.SelectedItem;
                _globalSettings.ToggleModdedTextureSheetKey = toggleModdedTextureSheetKeyItem.SelectedItem;
                _globalSettings.OpenMenuKey = openMenuKeyItem.SelectedItem;
                _globalSettings.ShowBlipLoadNotification = notifyBlipItem.Checked;
                _globalSettings.EnableAddOnBlips = enableAddOnBlipItem.Checked;
                _globalSettings.AutoCheckForUpdatesOnStartup = autoCheckUpdatesItem.Checked;
                _globalSettings.UseModdedTextureSheet = _shouldUseModdedTextureSheet;

                await SaveSettingsAsync();

                Notification.PostTicker("~g~Settings saved to JSON.", true);

                if (_globalSettings.EnableAddOnBlips)
                {
                    await ToggleAddOnBlipsAsync();
                    return;
                }
            };

            _categoryBlipMenu = CreateAndRegisterMenu("Category", "Enable/Disable Blip Categories");

            NativeItem gasStationBlipsItem = CreateCategoryButtonItem("Gas Station", "Gas Stations", _gasStationsCategoryBlips);
            NativeItem ATMBlipsItem = CreateCategoryButtonItem("ATM", "ATMs", _ATMCategoryBlips);
            NativeItem medicalCenterBlipsItem = CreateCategoryButtonItem("Medical Center", "Medical Centers", _medicalCenterCategoryBlips);
            NativeItem policeDepartmentBlipsItem = CreateCategoryButtonItem("Police Department", "Police Departments", _policeDepartmentCategoryBlips);
            NativeItem fireDepartmentBlipsItem = CreateCategoryButtonItem("Fire Department", "Fire Departments", _fireDepartmentCategoryBlips);
            NativeItem marketBlipsItem = CreateCategoryButtonItem("Market", "Markets", _marketCategoryBlips);
            NativeItem metroStationBlipsItem = CreateCategoryButtonItem("Metro Station", "Metro Stations", _metroStationCategoryBlips);

            _categoryBlipMenu.Add(gasStationBlipsItem);
            _categoryBlipMenu.Add(marketBlipsItem);
            _categoryBlipMenu.Add(policeDepartmentBlipsItem);
            _categoryBlipMenu.Add(fireDepartmentBlipsItem);
            _categoryBlipMenu.Add(ATMBlipsItem);
            _categoryBlipMenu.Add(medicalCenterBlipsItem);
            _categoryBlipMenu.Add(metroStationBlipsItem);

            _settingsMenu.Add(openMenuKeyItem);
            _settingsMenu.Add(toggleCoordKeyItem);
            _settingsMenu.Add(reloadKeyItem);
            _settingsMenu.Add(toggleVisibilityKeyItem);
            _settingsMenu.Add(toggleModdedTextureSheetKeyItem);
            _settingsMenu.Add(notifyBlipItem);
            _settingsMenu.Add(enableAddOnBlipItem);
            _settingsMenu.Add(autoCheckUpdatesItem);
            _settingsMenu.Add(checkUpdatesBtn);
            _settingsMenu.Add(restToDefaultBtn);
            _settingsMenu.AddSubMenu(_categoryBlipMenu, "").Title = "~y~Category Menu";
            _settingsMenu.Add(saveSettingsBtn);

            _mainMenu.AddSubMenu(_settingsMenu, "").Title = "Global Settings";
        }

        private NativeItem CreateCategoryButtonItem(string name, string groupName, PredefinedBlipData[] categoryData) 
        {
            NativeItem buttonItem = new NativeItem($"~y~Toggle {name} Blips", $"Add predefined {name} blips to both map and JSON file. ~y~Press again to delete.~y~");

            buttonItem.Activated += async (sender, item) =>
            {
                
                bool isActive = false;

                foreach (BlipData b in _blipDataCache)
                {
                    if (b.Group == groupName)
                    {
                        isActive = true;
                        break;
                    }
                }

                if (isActive)
                {
                    
                    List<BlipData> blipsToRemove = new List<BlipData>();

                    for (int x = _activeBlips.Count - 1; x >= 0; x--)
                    {
                        Blip potentialBlip = _activeBlips[x];
                        if (potentialBlip == null || !potentialBlip.Exists()) continue;

                        string blipGroup = _blipToGroupMap[potentialBlip.Handle];
                        if (blipGroup == groupName)
                        {
                            Vector3 pos = potentialBlip.Position;
                            BlipData potentialBlipData = new BlipData
                            {
                                Name = potentialBlip.Name,
                                Group = blipGroup,
                                Icon = Function.Call<int>(Hash.GET_BLIP_SPRITE, potentialBlip),
                                Color = Function.Call<int>(Hash.GET_BLIP_COLOUR, potentialBlip),
                                Size = potentialBlip.ScaleX,
                                Position = new Position { X = pos.X, Y = pos.Y, Z = pos.Z },
                                Flash = potentialBlip.IsFlashing,
                                FlashInterval = potentialBlip.FlashInterval,
                                ShortRange = potentialBlip.IsShortRange,
                                Transparency = potentialBlip.Alpha
                            };

                            potentialBlip.Delete();
                            _activeBlips.RemoveAt(x);
                            _blipDataCache.Remove(potentialBlipData);
                            blipsToRemove.Add(potentialBlipData);
                        }
                    }

                    
                    if (blipsToRemove.Count > 0)
                    {
                        _ = Task.Run(async () =>
                        {
                            List<BlipData> allJsonBlips = await LoadBlipsFromFileAsync();

                            
                            for (int i = allJsonBlips.Count - 1; i >= 0; i--)
                            {
                                
                                for (int j = 0; j < blipsToRemove.Count; j++)
                                {
                                    if (allJsonBlips[i].Equals(blipsToRemove[j]))
                                    {
                                        allJsonBlips.RemoveAt(i);
                                        break; 
                                    }
                                }
                            }

                            using (FileStream stream = new FileStream(_jsonFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                            using (StreamWriter writer = new StreamWriter(stream))
                            {
                                for (int i = 0; i < allJsonBlips.Count; i++)
                                {
                                    string jsonLine = JsonConvert.SerializeObject(allJsonBlips[i]);
                                    await writer.WriteLineAsync(jsonLine);
                                }
                            }
                        });
                    }
                }
                else
                {
                    
                    for (int i = 0; i < categoryData.Length; i++)
                    {
                        PredefinedBlipData blipProperties = categoryData[i];
                        Vector3 pos = blipProperties.Position;

                        BlipData blipData = new BlipData
                        {
                            Name = blipProperties.Name,
                            Group = groupName,
                            Icon = blipProperties.IconId,
                            Color = blipProperties.ColorId,
                            Size = blipProperties.Size,
                            Position = new Position { X = pos.X, Y = pos.Y, Z = pos.Z },
                            Flash = blipProperties.IsFlashing,
                            FlashInterval = 100,
                            ShortRange = blipProperties.IsShortRange,
                            Transparency = 255
                        };

                        if (_blipDataCache.Add(blipData))
                        {
                            CreateBlipFromData(blipData);
                            await SaveBlipToFileAsync(blipData);
                        }
                    }
                }
            };

            return buttonItem; 
        }
        private void RecreatePreviewBlip(int? iconId, int? colorId, float? size, bool? flash = null, int? flashInterval = null, int? transparency = null)
        {
            
            if (_previewBlip != null && _previewBlip.Exists())
            {
                _previewBlip.Delete();
                _previewBlip = null;
            }
            _didWeInitializePreviewBlip = false;

            
            UpdatePreviewBlip(iconId, colorId, size, flash, flashInterval, transparency);
        }
        private void UpdatePreviewBlip(int? iconId, int? colorId, float? size, bool? flash = null, int? flashInterval = null, int? transparency = null)
        {
            if (!_didWeInitializePreviewBlip)
            {
                Ped player = Game.Player.Character;
                if (!IsValidPlayer(player))
                {
                    return;
                }

                Vector3 playerPos = GetPlayerCoordinates(player);
                Vector3 playerForward = player.ForwardVector;
                Vector3 previewPos = playerPos + (playerForward * 17);

                _previewBlip = World.CreateBlip(previewPos);
                int blipHandle = _previewBlip.Handle;
                Function.Call(Hash.SET_BLIP_SPRITE, blipHandle, 0);
                _previewBlip.Name = "Preview Blip";
                _previewBlip.IsShortRange = true;
                _previewBlip.IsFlashing = false;
                _previewBlip.Scale = 1.0f;
                Function.Call(Hash.SET_BLIP_COLOUR, blipHandle, 0);
                _previewBlip.Alpha = 255;

                _didWeInitializePreviewBlip = true;
            }

            if (iconId != null)
            {
                Function.Call(Hash.SET_BLIP_SPRITE, _previewBlip.Handle, (int)iconId);
            }

            if (colorId != null)
            {
                Function.Call(Hash.SET_BLIP_COLOUR, _previewBlip.Handle, (int)colorId);
            }

            if (size != null)
            {
                _previewBlip.Scale = (float)size;
            }

            
            if (flash.HasValue)
            {
                _previewBlip.IsFlashing = flash.Value;
            }

            if (flashInterval.HasValue)
            {
                _previewBlip.FlashInterval = flashInterval.Value;
            }

            
            if (transparency.HasValue)
            {
                _previewBlip.Alpha = transparency.Value;
            }
        }
        private void DrawFadeOverlay(string text)
        {
            float t = _alpha / 170f;
            t = t * t * (3f - 2f * t);
            int easedAlpha = (int)(t * 170);

            Function.Call(Hash.DRAW_RECT, 0.5f, 0.5f, 1.0f, 1.0f, 0, 0, 0, easedAlpha);

            int textAlpha = _waitingForKeyAssignment
                ? (int)(Math.Pow(_alpha / 170f, 0.75f) * 170)
                : easedAlpha;

            Function.Call(Hash.SET_TEXT_FONT, 0);
            Function.Call(Hash.SET_TEXT_SCALE, 0.5f, 0.45f);
            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, textAlpha);
            Function.Call(Hash.SET_TEXT_CENTRE, true);
            Function.Call(Hash.SET_TEXT_OUTLINE);
            Function.Call(Hash.SET_TEXT_DROP_SHADOW);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, 0.5f, 0.5f);
        }
        private int GetKeyIndex(string key)
        {
            Enum.TryParse(key, true, out Keys parsedKey);
            return Array.IndexOf(_keysNames, parsedKey.ToString());
        }

        
        private NativeMenu CreateAndRegisterMenu(string title, string subtitle)
        {
            NativeMenu menu = new NativeMenu(title, subtitle)
            {
                KeepNameCasing = true
            };

            _uiMenuPool.Add(menu);
            return menu;
        }
        private NativeListItem<string> CreateEnumListItem<TEnum>(string title, bool indexIncluded, int defaultIndex = 0, string description = null) where TEnum : Enum
        {
            Array values = GetEnumValuesCached(typeof(TEnum));
            int count = values.Length;

            string[] items = new string[count];

            if (indexIncluded)
            {
                for (int i = 0; i < count; i++)
                {
                    TEnum value = (TEnum)values.GetValue(i);
                    items[i] = $"{value} ({Convert.ToInt32(value)})";
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    items[i] = values.GetValue(i).ToString();
                }
            }

            NativeListItem<string> item = new NativeListItem<string>(title, items)
            {
                SelectedIndex = defaultIndex
            };

            if (!string.IsNullOrWhiteSpace(description))
            {
                item.Description = description;
            }

            return item;
        }
        private NativeCheckboxItem CreateCheckboxItem(string title, bool defaultValue, string description = null)
        {
            NativeCheckboxItem item = new NativeCheckboxItem(title, defaultValue)
            {
                Description = description
            };

            return item;
        }
        private int GetSelectedEnumValue<TEnum>(NativeListItem<string> item) where TEnum : struct, Enum
        {
            string selected = item.SelectedItem;
            if (string.IsNullOrWhiteSpace(selected))
                return 0;

            int parenIndex = selected.IndexOf('(');
            string enumName = parenIndex > 0
                ? selected.Substring(0, parenIndex).Trim()
                : selected;

            if (Enum.TryParse(enumName, true, out TEnum result))
                return Convert.ToInt32(result);

            return 0;
        }
        private string PromptUserForInput(bool isNumber = false)
        {
            Function.Call(Hash.DISPLAY_ONSCREEN_KEYBOARD, 1, "", "", "", "", "", "", 30);

            int state;
            do
            {
                Wait(0);
                state = Function.Call<int>(Hash.UPDATE_ONSCREEN_KEYBOARD);
            } while (state == 0);


            if (Function.Call<int>(Hash.UPDATE_ONSCREEN_KEYBOARD) == 1)
            {
                string result = Function.Call<string>(Hash.GET_ONSCREEN_KEYBOARD_RESULT);

                
                return isNumber ? result?.Replace(',', '.') : result;
            }

            return null;
        }
        private bool TryFindClosestIconWithinRadius(int clickX, int clickY, out ExtendedBlipSprite foundKey)
        {
            const int radius = 10;
            const int radiusSq = radius * radius;

            int bestD2 = int.MaxValue;
            foundKey = default;

            IconEntry[] icons = _onScreenIconsMap;
            int length = icons.Length;

            bool foundAny = false;

            for (int i = 0; i < length; i++)
            {
                IconEntry entry = icons[i];

                int dx = entry.X - clickX;
                if ((uint)(dx + radius) > radius * 2)
                {
                    continue;
                }

                int dy = entry.Y - clickY;
                if ((uint)(dy + radius) > radius * 2)
                {
                    continue;
                }

                int d2 = dx * dx + dy * dy;

                if (d2 > radiusSq)
                {
                    continue;
                }

                if (d2 == 0)
                {
                    foundKey = entry.Key;
                    return true;
                }

                if (!foundAny || d2 < bestD2 || (d2 == bestD2 && (int)entry.Key < (int)foundKey))
                {
                    bestD2 = d2;
                    foundKey = entry.Key;
                    foundAny = true;
                }
            }

            return foundAny;
        }
        private bool IsValidPlayer(Ped player)
        {
            return player != null && player.Exists();
        }
        private Vector3 GetPlayerCoordinates(Ped player)
        {
            Vehicle currentVehicle = player.CurrentVehicle;

            if (player.IsInVehicle() && currentVehicle != null && currentVehicle.Exists())
            {
                return currentVehicle.Position;
            }

            return player.Position;
        }
        private Array GetEnumValuesCached(Type type)
        {
            if (!_enumCache.TryGetValue(type, out Array values))
            {
                values = Enum.GetValues(type);
                _enumCache[type] = values;
            }
            return values;
        }
        private bool TrySelectEnumByInput<TEnum>(NativeListItem<string> listItem, string input) where TEnum : struct, Enum
        {
            Array values = GetEnumValuesCached(typeof(TEnum));

            if (int.TryParse(input, out int id))
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (Convert.ToInt32(values.GetValue(i)) == id)
                    {
                        listItem.SelectedIndex = i;
                        return true;
                    }
                }
            }

            if (Enum.TryParse(input, true, out TEnum parsed))
            {
                int index = Array.IndexOf(values, parsed);
                if (index >= 0)
                {
                    listItem.SelectedIndex = index;
                    return true;
                }
            }

            return false;
        }
        private void BindKeyAssignment(NativeListItem menuItem, PendingKeyBind settingName, string displayName)
        {
            menuItem.Activated += (sender, item) =>
            {
                _fullOverlayText = $"~y~KEY BINDING~w~~n~Press a key to assign~n~~b~{displayName}";
                _typedOverlayText = string.Empty;
                _typingIndex = 0;
                _lastTypeTime = Game.GameTime;

                _waitingForKeyAssignment = true;
                _pendingKeyBind = settingName;
                _pendingListItem = (NativeListItem<string>)menuItem;
            };
        }
        private void ToggleBlipsVisibility()
        {
            for (int i = 0; i < _activeBlips.Count; i++)
            {
                Blip blip = _activeBlips[i];

                if (blip != null && blip.Exists())
                {
                    Function.Call(Hash.SET_BLIP_DISPLAY, blip.Handle, _areBlipsVisible ? 0 : 2);
                }
            }

            _areBlipsVisible = !_areBlipsVisible;

            Notification.PostTicker(_areBlipsVisible ? "Custom Blips are now visible." : "Custom Blips are now hidden.", true);
        }
        private string GetTypingText()
        {
            if (_typingIndex >= _fullOverlayText.Length)
            {
                return _fullOverlayText;
            }

            if (Game.GameTime - _lastTypeTime >= TYPE_SPEED)
            {
                _lastTypeTime = Game.GameTime;
                _typingIndex++;

                
                if (_typingIndex < _fullOverlayText.Length && _fullOverlayText[_typingIndex - 1] == '~')
                {
                    while (_typingIndex < _fullOverlayText.Length && _fullOverlayText[_typingIndex] != '~')
                    {
                        _typingIndex++;
                    }

                    if (_typingIndex < _fullOverlayText.Length)
                    {
                        _typingIndex++;
                    }
                }

                _typedOverlayText = _fullOverlayText.Substring(0, _typingIndex);
            }

            return _typedOverlayText;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DeleteAllGameBlips()
        {
            for (int i = _activeBlips.Count - 1; i >= 0; i--)
            {
                Blip blip = _activeBlips[i];
                if (blip != null && blip.Exists())
                {
                    blip.Delete();
                }
            }

            _activeBlips.Clear();
        }

        
        private async Task SaveBlipToFileAsync(BlipData blipData)
        {
            await _fileLock.WaitAsync();
            try
            {
                EnsureDirectoryExist(_jsonFilePath);

                
                string json = JsonConvert.SerializeObject(blipData);

                using (FileStream stream = new FileStream(_jsonFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, 8192, true))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    await writer.WriteLineAsync(json); 
                }

                _blipDataCache.Add(blipData);
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, "Saving blip to JSON file");
            }
            finally
            {
                _fileLock.Release();
            }
        }
        private async Task<List<BlipData>> LoadBlipsFromFileAsync()
        {
            List<BlipData> list = new List<BlipData>();

            if (!File.Exists(_jsonFilePath))
            {
                return list;
            }

            using (FileStream stream = new FileStream(_jsonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        try
                        {
                            BlipData item = JsonConvert.DeserializeObject<BlipData>(line);
                            if (item != null)
                            {
                                list.Add(item);
                                _blipDataCache.Add(item);
                            }
                        }
                        catch (Exception ex)
                        {
                            DisplayAndLogError(ex, "Corrupted JSON line skipped");
                        }

                    }
                }
            }

            return list;
        }
        private async Task DeleteBlipFromFileAsync(BlipData blipToDelete)
        {
            if (blipToDelete == null)
                return;

            await _fileLock.WaitAsync();
            try
            {
                if (!File.Exists(_jsonFilePath))
                    return;

                
                List<BlipData> blips = await LoadBlipsFromFileAsync();

                blips.RemoveAll(b => b.Equals(blipToDelete));

                using (FileStream stream = new FileStream(
                    _jsonFilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    8192,
                    true))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    foreach (BlipData blip in blips)
                    {
                        await writer.WriteLineAsync(JsonConvert.SerializeObject(blip));
                    }
                }

                _blipDataCache.Remove(blipToDelete);
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, "Deleting blip from JSON file");
            }
            finally
            {
                _fileLock.Release();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task LoadAndCreateBlips()
        {
            List<BlipData> blips = await LoadBlipsFromFileAsync();

            for (int i = 0; i < blips.Count; i++)
            {
                _blipQueue.Enqueue(blips[i]);
            }
        }
        private async Task LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    _globalSettings = new SettingsData();
                    await SaveSettingsAsync(); 
                    return;
                }

                using (FileStream stream = new FileStream(_settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = await reader.ReadToEndAsync();
                    _globalSettings = JsonConvert.DeserializeObject<SettingsData>(json) ?? new SettingsData();
                }
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, "Failed to load settings asynchronously. Using defaults.");
                _globalSettings = new SettingsData();
            }
        }
        private async Task SaveSettingsAsync()
        {
            try
            {
                EnsureDirectoryExist(_settingsFilePath);
                string json = JsonConvert.SerializeObject(_globalSettings, Formatting.Indented);

                using (FileStream stream = new FileStream(_settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(json);
                }
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, "Failed to save settings asynchronously.");
            }
        }
        private async Task SaveModdedTextureSheetSettingAsync()
        {
            
            if (!File.Exists(_settingsFilePath))
                return;

            await _fileLock.WaitAsync();
            try
            {
                
                _globalSettings.UseModdedTextureSheet = _shouldUseModdedTextureSheet;

                
                string json = JsonConvert.SerializeObject(_globalSettings, Formatting.Indented);

                
                using (FileStream stream = new FileStream(
                    _settingsFilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    8192,  
                    FileOptions.Asynchronous))
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, "Error saving texture sheet status.");
            }
            finally
            {
                _fileLock.Release();
            }
        }
        private async Task ToggleAddOnBlipsAsync()
        {
            if (!Directory.Exists(_addOnBlipsFilesPath))
            {
                Notification.PostTicker("~y~Add-On Blips mod is not installed.", true);
                return;
            }

            
            bool isActive = false;
            foreach (BlipData b in _blipDataCache)
            {
                if (b.Group == "Add-On")
                {
                    isActive = true;
                    break;
                }
            }

            if (isActive)
            {
                
                
                

                List<BlipData> blipsToRemove = new List<BlipData>();

                for (int x = _activeBlips.Count - 1; x >= 0; x--)
                {
                    Blip blip = _activeBlips[x];
                    if (blip == null || !blip.Exists()) continue;

                    if (!_blipToGroupMap.TryGetValue(blip.Handle, out string group))
                        continue;

                    if (group == "Add-On")
                    {
                        Vector3 pos = blip.Position;

                        BlipData data = new BlipData
                        {
                            Name = blip.Name,
                            Group = group,
                            Icon = Function.Call<int>(Hash.GET_BLIP_SPRITE, blip),
                            Color = Function.Call<int>(Hash.GET_BLIP_COLOUR, blip),
                            Size = blip.ScaleX,
                            Position = new Position { X = pos.X, Y = pos.Y, Z = pos.Z },
                            Flash = blip.IsFlashing,
                            FlashInterval = blip.FlashInterval,
                            ShortRange = blip.IsShortRange,
                            Transparency = blip.Alpha
                        };

                        blip.Delete();
                        _activeBlips.RemoveAt(x);
                        _blipDataCache.Remove(data);
                        blipsToRemove.Add(data);
                    }
                }

                
                if (blipsToRemove.Count > 0)
                {

                    List<BlipData> jsonBlips = await LoadBlipsFromFileAsync();

                    for (int i = jsonBlips.Count - 1; i >= 0; i--)
                    {
                        for (int j = 0; j < blipsToRemove.Count; j++)
                        {
                            if (jsonBlips[i].Equals(blipsToRemove[j]))
                            {
                                jsonBlips.RemoveAt(i);
                                break;
                            }
                        }
                    }

                    using (FileStream stream = new FileStream(_jsonFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        for (int g = 0; g < jsonBlips.Count; g++)
                        {
                            await writer.WriteLineAsync(JsonConvert.SerializeObject(jsonBlips[g]));
                        }
                    }
                }
            }
            else
            {
                
                
                

                string[] files = Directory.GetFiles(_addOnBlipsFilesPath, "*.txt");
                if (files.Length == 0)
                    return;
                for (int h = 0; h < files.Length; h++)
                {
                    string content = File.ReadAllText(files[h]);
                    content = content.Trim();

                    string[] parts = content.Split(';');
                    if (parts.Length < 3)
                    {
                        DisplayAndLogError(null, $"Malformed blip file: {content}");
                        continue;
                    }

                    string[] coords = parts[0].Split(',');
                    if (coords.Length < 3 ||
                        !float.TryParse(coords[0], out float x) ||
                        !float.TryParse(coords[1], out float y) ||
                        !float.TryParse(coords[2], out float z))
                    {
                        DisplayAndLogError(null, $"Invalid coords: {content}");
                        continue;
                    }

                    string name = parts[1].Trim();
                    string iconName = parts[2].Trim();

                    int iconId = Enum.TryParse(iconName, true, out BlipSprite icon)
                        ? (int)icon
                        : (int)ExtendedBlipSprite.radar_level;

                    BlipData data = new BlipData
                    {
                        Name = name,
                        Group = "Add-On",
                        Icon = iconId,
                        Color = 0,
                        Size = 1f,
                        Position = new Position { X = x, Y = y, Z = z },
                        Flash = false,
                        FlashInterval = 100,
                        ShortRange = true,
                        Transparency = 255
                    };

                    if (_blipDataCache.Add(data))
                    {
                        CreateBlipFromData(data);
                        await SaveBlipToFileAsync(data);
                    }
                }
            }
        }
        private void CreateBlipFromData(BlipData data)
        {
            if (data == null || data.Position == null)
            {
                return;
            }

            Position p = data.Position;

            Blip blip = World.CreateBlip(new Vector3(p.X, p.Y, p.Z));
            int blipHandle = blip.Handle;
            Function.Call(Hash.SET_BLIP_SPRITE, blipHandle, data.Icon);
            blip.Name = data.Name;
            blip.IsShortRange = data.ShortRange;
            blip.IsFlashing = data.Flash;
            blip.Scale = data.Size;
            Function.Call(Hash.SET_BLIP_COLOUR, blipHandle, data.Color);
            blip.FlashInterval = data.FlashInterval;
            blip.Alpha = data.Transparency;

            _activeBlips.Add(blip);
            if (_availableGroupsSet.Add(data.Group))
            {
                _availableGroups.Add(data.Group);
                _shouldRefreshGroupItems = true;
            }
            _blipToGroupMap[blip.Handle] = data.Group;
        }
        private void DisplayAndLogError(Exception ex, string contextInfo = null)
        {
            string methodName = ex.TargetSite?.Name ?? "Unknown Method";  
            string innerMessage = ex.InnerException?.Message != null ? $"\nInner Exception: {ex.InnerException.Message}" : string.Empty;
            string message = $"~r~Error in {methodName}: {ex.Message}{innerMessage}";

            
            if (!string.IsNullOrEmpty(contextInfo))
            {
                message += $"\nContext: {contextInfo}";
            }

            Notification.PostTicker(message, true);
            try
            {
                
                EnsureDirectoryExist(_logFilePath);

                _logWriter.WriteLine($"{DateTime.Now}: {methodName}: {ex}");
                if (contextInfo != null)
                {
                    _logWriter.WriteLine($"Context: {contextInfo}");
                }
                _logWriter.WriteLine($"Stack Trace: {ex.StackTrace}");
                _logWriter.WriteLine();
                _logWriter.WriteLine(new string('#', 80)); 
                _logWriter.WriteLine();

            }
            catch (UnauthorizedAccessException logEx)
            {
                
                
                FileAttributes attributes = File.GetAttributes(_logFilePath);
                if (File.Exists(_logFilePath) && (attributes & FileAttributes.ReadOnly) != 0)
                {
                    attributes &= ~FileAttributes.ReadOnly;
                    File.SetAttributes(_logFilePath, attributes);
                    
                    _logWriter.WriteLine($"{DateTime.Now}: {methodName}: {ex}");
                    if (contextInfo != null)
                    {
                        _logWriter.WriteLine($"Context: {contextInfo}");
                    }
                    _logWriter.WriteLine($"Stack Trace: {ex.StackTrace}");
                    _logWriter.WriteLine();
                    _logWriter.WriteLine(new string('#', 80)); 
                    _logWriter.WriteLine();
                    return; 
                }
                else
                {
                    
                    Notification.PostTicker($"~r~Failed to log exception (access denied): {logEx.Message}", true);
                    throw;
                }
            }
            catch (DirectoryNotFoundException)
            {
                Notification.PostTicker($"Directory not found: {Path.GetDirectoryName(_logFilePath)}", true);
            }
            catch (IOException logEx)
            {
                Notification.PostTicker($"~r~Failed to log exception (I/O error): {logEx.Message}", true);
            }
            catch (Exception logEx)
            {
                Notification.PostTicker($"~r~Failed to log exception: {logEx.Message}", true);
            }
        }
        private void EnsureDirectoryExist(string filePath)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            catch (PathTooLongException ex)
            {
                DisplayAndLogError(ex, $"Error ensuring directory exists for path (path too long): {filePath}");
            }
            catch (IOException ex)
            {
                DisplayAndLogError(ex, $"Error ensuring directory exists for path (I/O error): {filePath}");
            }
            catch (UnauthorizedAccessException ex)
            {
                
                
                FileAttributes attributes = File.GetAttributes(filePath);
                if (File.Exists(filePath) && (attributes & FileAttributes.ReadOnly) != 0)
                {
                    attributes &= ~FileAttributes.ReadOnly;
                    File.SetAttributes(filePath, attributes);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    return; 
                }
                else
                {
                    
                    DisplayAndLogError(ex, $"Error ensuring directory exists for path (access denied): {filePath}. Tried fixing it but failed.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, $"Error ensuring directory exists for path: {filePath}");
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "AdvancedCustomBlips-UpdateChecker/1.0"); 
                    client.Timeout = TimeSpan.FromSeconds(15); 

                    string htmlContent = await client.GetStringAsync(MOD_PAGE_URL);

                    
                    Match match = Regex.Match(htmlContent, VERSION_REGEX_PATTERN, RegexOptions.IgnoreCase);
                    if (match.Success && match.Groups.Count > 1)
                    {
                        string remoteVersion = "v" + match.Groups[1].Value; 

                        
                        int versionComparison = string.Compare(CURRENT_SCRIPT_VERSION, remoteVersion, StringComparison.OrdinalIgnoreCase);

                        if (versionComparison < 0) 
                        {
                            Notification.PostTicker($"~g~[Advanced Custom Blips] Update Available! ~w~{remoteVersion}~n~~b~Download from gta5-mods.com", true);
                        }
                        else if (versionComparison > 0)
                        {
                            
                        }
                        else 
                        {
                            Notification.PostTicker($"~g~[Advanced Custom Blips] Up to date. Version: {CURRENT_SCRIPT_VERSION}", true);
                        }
                    }
                    else
                    {
                        
                        Notification.PostTicker("~o~[Advanced Custom Blips] Update Check: Could not parse version.", true);
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                
                DisplayAndLogError(httpEx, "~o~[Advanced Custom Blips] Update Check: Network error.");
            }
            catch (TaskCanceledException tcEx) 
            {
                
                DisplayAndLogError(tcEx, "~o~[Advanced Custom Blips] Update Check: Timed out.");
            }
            catch (Exception ex)
            {
                
                DisplayAndLogError(ex, "~o~[Advanced Custom Blips] Update Check: An error occurred.");
            }
        }
    }

    public sealed class BlipData : IEquatable<BlipData>
    {
        public string Name { get; set; } = "";
        public string Group { get; set; } = "";

        public int Icon { get; set; }
        public int Color { get; set; }
        public float Size { get; set; } = 1.0f;
        public int Transparency { get; set; } = 255;

        public bool Flash { get; set; } = false;
        public int FlashInterval { get; set; } = 100;
        public bool ShortRange { get; set; } = true;

        public Position Position { get; set; }

        private static bool NearlyEqual(float a, float b, float epsilon = 0.0001f)
        {
            return Math.Abs(a - b) < epsilon;
        }

        public override bool Equals(object obj)
        {
            return obj is BlipData other && Equals(other);
        }

        public bool Equals(BlipData other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (Position is null || other.Position is null)
            {
                if (Position != other.Position)
                    return false;
            }
            else if (!Position.Equals(other.Position))
            {
                return false;
            }

            return
                Name == other.Name &&
                Group == other.Group &&
                Icon == other.Icon &&
                Color == other.Color &&
                NearlyEqual(Size, other.Size) &&
                Transparency == other.Transparency &&
                Flash == other.Flash &&
                FlashInterval == other.FlashInterval &&
                ShortRange == other.ShortRange;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Name?.GetHashCode() ?? 0);
                hash = hash * 23 + (Group?.GetHashCode() ?? 0);
                hash = hash * 23 + Icon;
                hash = hash * 23 + Color;
                hash = hash * 23 + Math.Round(Size, 4).GetHashCode();
                hash = hash * 23 + Transparency;
                hash = hash * 23 + Flash.GetHashCode();
                hash = hash * 23 + FlashInterval;
                hash = hash * 23 + ShortRange.GetHashCode();
                hash = hash * 23 + (Position?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
    public sealed class Position : IEquatable<Position>
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public bool Equals(Position other)
        {
            if (other is null)
            {
                return false;
            }

            return
                Math.Abs(X - other.X) < 0.0001f &&
                Math.Abs(Y - other.Y) < 0.0001f &&
                Math.Abs(Z - other.Z) < 0.0001f;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Position);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Math.Round(X, 4).GetHashCode();
                hash = hash * 23 + Math.Round(Y, 4).GetHashCode();
                hash = hash * 23 + Math.Round(Z, 4).GetHashCode();
                return hash;
            }
        }
    }
    public sealed class SettingsData
    {
        public string ToggleCoordinatesKey { get; set; } = "F1";
        public string ReloadBlipsKey { get; set; } = "F2";
        public string ToggleBlipsVisibilityKey { get; set; } = "F3";
        public string ToggleModdedTextureSheetKey { get; set; } = "F5";
        public string OpenMenuKey { get; set; } = "F10";
        public bool ShowBlipLoadNotification { get; set; } = true;
        public bool EnableAddOnBlips { get; set; } = false;
        public bool AutoCheckForUpdatesOnStartup { get; set; } = true;
        public bool UseModdedTextureSheet { get; set; } = false;
    }

    public enum PendingKeyBind
    {
        None,
        ToggleCoordinates,
        ReloadBlips,
        ToggleBlipVisibility,
        ToggleModdedTextureSheet,
        OpenMenu
    }
    public enum ExtendedBlipSprite  
    {
        radar_level = 1,
        radar_lower = 2,
        radar_police_ped = 3,
        radar_wanted_radius = 4,
        radar_area_blip = 5,
        radar_centre = 6,
        radar_north = 7,
        radar_waypoint = 8,
        radar_radius_blip = 9,
        radar_radius_outline_blip = 10,
        radar_weapon_higher = 11,
        radar_weapon_lower = 12,
        radar_higher_ai = 13,
        radar_lower_ai = 14,
        radar_police_heli_spin = 15,
        radar_police_plane_move = 16,
        Number1 = 17,
        radar_numbered_2 = 18,
        radar_numbered_3 = 19,
        radar_numbered_4 = 20,
        radar_numbered_5 = 21,
        radar_numbered_6 = 22,
        radar_numbered_7 = 23,
        radar_numbered_8 = 24,
        radar_numbered_9 = 25,
        radar_numbered_10 = 26,
        radar_mp_crew1 = 27,
        radar_mp_friendlies = 28,
        radar_cable_car = 36,
        radar_raceflag = 38,
        radar_safehouse = 40,
        radar_police = 41,
        radar_police_chase = 42,
        radar_police_heli = 43,
        radar_snitch = 47,
        radar_crim_carsteal = 50,
        radar_crim_drugs = 51,
        radar_crim_holdups = 52,
        radar_cop_patrol = 56,
        radar_cop_player = 57,
        radar_crim_wanted = 58,
        radar_heist = 59,
        radar_police_station = 60,
        radar_hospital = 61,
        radar_elevator = 63,
        radar_helicopter = 64,
        radar_random_character = 66,
        radar_security_van = 67,
        radar_tow_truck = 68,
        radar_barber = 71,
        radar_car_mod_shop = 72,
        radar_clothes_store = 73,
        radar_tattoo = 75,
        radar_armenian_family = 76,
        radar_lester_family = 77,
        radar_michael_family = 78,
        radar_trevor_family = 79,
        radar_jewelry_heist = 80,
        radar_rampage = 84,
        radar_vinewood_tours = 85,
        radar_lamar_family = 86,
        radar_franklin_family = 88,
        radar_chinese_strand = 89,
        radar_flight_school = 90,
        radar_bar = 93,
        radar_base_jump = 94,
        radar_biolab_heist = 96,
        radar_car_wash = 100,
        radar_comedy_club = 102,
        radar_darts = 103,
        radar_docks_heist = 104,
        radar_fbi_heist = 105,
        radar_fbi_officers_strand = 106,
        radar_finale_bank_heist = 107,
        radar_financier_strand = 108,
        radar_golf = 109,
        radar_gun_shop = 110,
        radar_michael_family_exile = 112,
        radar_nice_house_heist = 113,
        radar_rural_bank_heist = 118,
        radar_shooting_range = 119,
        radar_solomon_strand = 120,
        radar_strip_club = 121,
        radar_tennis = 122,
        radar_trevor_family_exile = 123,
        radar_michael_trevor_family = 124,
        radar_triathlon = 126,
        radar_off_road_racing = 127,
        radar_gang_cops = 128,
        radar_gang_mexicans = 129,
        radar_gang_bikers = 130,
        radar_snitch_red = 133,
        radar_crim_cuff_keys = 134,
        radar_cinema = 135,
        radar_music_venue = 136,
        radar_police_station_blue = 137,
        radar_weed_stash = 140,
        radar_hunting = 141,
        radar_objective_blue = 143,
        radar_arms_dealing = 147,
        radar_celebrity_theft = 149,
        radar_weapon_assault_rifle = 150,
        radar_weapon_bat = 151,
        radar_weapon_grenade = 152,
        radar_weapon_health = 153,
        radar_weapon_knife = 154,
        radar_weapon_molotov = 155,
        radar_weapon_pistol = 156,
        radar_weapon_rocket = 157,
        radar_weapon_shotgun = 158,
        radar_weapon_smg = 159,
        radar_weapon_sniper = 160,
        radar_mp_noise = 161,
        radar_poi = 162,
        radar_passive = 163,
        radar_usingmenu = 164,
        radar_gang_cops_partner = 171,
        radar_weapon_minigun = 173,
        GrenadeLauncher = 174,
        radar_weapon_armour = 175,
        radar_property_takeover = 176,
        radar_gang_mexicans_highlight = 177,
        radar_gang_bikers_highlight = 178,
        radar_property_takeover_bikers = 181,
        radar_property_takeover_cops = 182,
        radar_property_takeover_vagos = 183,
        radar_camera = 184,
        radar_centre_red = 185,
        radar_handcuff_keys_bikers = 186,
        radar_handcuff_keys_vagos = 187,
        radar_handcuffs_closed_bikers = 188,
        radar_handcuffs_closed_vagos = 189,
        radar_yoga = 197,
        radar_taxi = 198,
        Number11 = 199,
        radar_numbered_12 = 200,
        radar_numbered_13 = 201,
        radar_numbered_14 = 202,
        radar_numbered_15 = 203,
        radar_numbered_16 = 204,
        radar_shrink = 205,
        radar_epsilon = 206,
        radar_financier_strand_grey = 207,
        radar_trevor_family_grey = 208,
        radar_trevor_family_red = 209,
        radar_franklin_family_grey = 210,
        radar_franklin_family_blue = 211,
        radar_franklin_c = 214,
        radar_gang_vehicle = 225,
        radar_gang_vehicle_bikers = 226,
        radar_gang_vehicle_cops = 227,
        radar_guncar = 229,
        radar_gang_cops_highlight = 233,
        radar_custody_bikers = 237,
        radar_custody_vagos = 238,
        radar_arms_dealing_air = 251,
        radar_playerstate_arrested = 252,
        radar_playerstate_custody = 253,
        radar_playerstate_keyholder = 255,
        radar_playerstate_partner = 256,
        radar_fairground = 266,
        radar_property = 267,
        radar_gang_highlight = 268,
        radar_altruist = 269,
        radar_ai = 270,
        radar_on_mission = 271,
        radar_cash_pickup = 272,
        radar_chop = 273,
        radar_dead = 274,
        radar_cash_lost = 276,
        radar_cash_vagos = 277,
        radar_cash_cops = 278,
        radar_hooker = 279,
        radar_friend = 280,
        radar_custody_dropoff = 285,
        radar_onmission_cops = 286,
        radar_onmission_lost = 287,
        radar_onmission_vagos = 288,
        radar_crim_carsteal_cops = 289,
        radar_crim_carsteal_bikers = 290,
        radar_crim_carsteal_vagos = 291,
        radar_simeon_family = 293,
        radar_bounty_hit = 303,
        radar_ugc_mission = 304,
        radar_horde = 305,
        radar_cratedrop = 306,
        radar_plane_drop = 307,
        radar_sub = 308,
        radar_race = 309,
        radar_deathmatch = 310,
        radar_arm_wrestling = 311,
        radar_shootingrange_gunshop = 313,
        radar_race_air = 314,
        radar_race_land = 315,
        radar_race_sea = 316,
        radar_tow = 317,
        radar_garbage = 318,
        radar_getaway_car = 326,
        radar_gang_bike = 348,
        radar_property_for_sale = 350,
        radar_gang_attack_package = 351,
        radar_martin_madrazzo = 352,
        radar_enemy_heli_spin = 353,
        radar_boost = 354,
        radar_devin = 355,
        radar_dock = 356,
        radar_garage = 357,
        radar_golf_flag = 358,
        radar_hangar = 359,
        radar_helipad = 360,
        radar_jerry_can = 361,
        radar_mask = 362,
        radar_heist_prep = 363,
        radar_incapacitated = 364,
        radar_spawn_point_pickup = 365,
        radar_boilersuit = 366,
        radar_completed = 367,
        radar_rockets = 368,
        radar_garage_for_sale = 369,
        radar_helipad_for_sale = 370,
        radar_dock_for_sale = 371,
        radar_hangar_for_sale = 372,
        radar_business = 374,
        radar_business_for_sale = 375,
        radar_race_bike = 376,
        radar_parachute = 377,
        radar_team_deathmatch = 378,
        radar_race_foot = 379,
        radar_vehicle_deathmatch = 380,
        radar_barry = 381,
        radar_dom = 382,
        radar_maryann = 383,
        radar_cletus = 384,
        radar_josh = 385,
        radar_minute = 386,
        radar_omega = 387,
        radar_tonya = 388,
        radar_paparazzo = 389,
        radar_aim = 390,
        radar_creator = 398,
        radar_creator_direction = 399,
        radar_abigail = 400,
        radar_blimp = 401,
        radar_repair = 402,
        radar_testosterone = 403,
        radar_dinghy = 404,
        radar_fanatic = 405,
        Invisible = 406,
        radar_info_icon = 407,
        radar_capture_the_flag = 408,
        radar_last_team_standing = 409,
        radar_boat = 410,
        radar_capture_the_flag_base = 411,
        radar_mp_crew2 = 412,
        radar_capture_the_flag_base_nobag = 414,
        radar_weapon_jerrycan = 415,
        radar_rp = 416,
        radar_level_inside = 417,
        radar_bounty_hit_inside = 418,
        radar_capture_the_usaflag = 419,
        radar_capture_the_usaflag_outline = 420,
        radar_tank = 421,
        radar_player_heli = 422,
        radar_player_plane = 423,
        radar_centre_stroke = 425,
        radar_player_guncar = 426,
        radar_player_boat = 427,
        radar_mp_heist = 428,
        radar_temp_1 = 429,
        radar_temp_2 = 430,
        radar_temp_3 = 431,
        radar_temp_4 = 432,
        radar_temp_5 = 433,
        radar_temp_6 = 434,
        radar_race_stunt = 435,
        radar_hot_property = 436,
        radar_urbanwarfare_versus = 437,
        radar_king_of_the_castle = 438,
        radar_player_king = 439,
        radar_dead_drop = 440,
        radar_penned_in = 441,
        radar_beast = 442,
        radar_edge_pointer = 443,
        radar_edge_crosstheline = 444,
        radar_mp_lamar = 445,
        radar_bennys = 446,
        radar_corner_number_1 = 447,
        radar_corner_number_2 = 448,
        radar_corner_number_3 = 449,
        radar_corner_number_4 = 450,
        radar_corner_number_5 = 451,
        radar_corner_number_6 = 452,
        radar_corner_number_7 = 453,
        radar_corner_number_8 = 454,
        radar_yacht = 455,
        radar_finders_keepers = 456,
        radar_assault_package = 457,
        radar_hunt_the_boss = 458,
        radar_sightseer = 459,
        radar_turreted_limo = 460,
        radar_belly_of_the_beast = 461,
        radar_yacht_location = 462,
        radar_pickup_beast = 463,
        radar_pickup_zoned = 464,
        radar_pickup_random = 465,
        radar_pickup_slow_time = 466,
        radar_pickup_swap = 467,
        radar_pickup_thermal = 468,
        radar_pickup_weed = 469,
        radar_weapon_railgun = 470,
        radar_seashark = 471,
        radar_pickup_hidden = 472,
        radar_warehouse = 473,
        radar_warehouse_for_sale = 474,
        radar_office = 475,
        radar_office_for_sale = 476,
        radar_truck = 477,
        radar_contraband = 478,
        radar_trailer = 479,
        radar_vip = 480,
        radar_cargobob = 481,
        radar_area_outline_blip = 482,
        radar_pickup_accelerator = 483,
        radar_pickup_ghost = 484,
        radar_pickup_detonator = 485,
        radar_pickup_bomb = 486,
        radar_pickup_armoured = 487,
        radar_stunt = 488,
        radar_weapon_lives = 489,
        radar_stunt_premium = 490,
        radar_adversary = 491,
        radar_biker_clubhouse = 492,
        radar_biker_caged_in = 493,
        radar_biker_turf_war = 494,
        radar_biker_joust = 495,
        radar_production_weed = 496,
        radar_production_crack = 497,
        radar_production_fake_id = 498,
        radar_production_meth = 499,
        radar_production_money = 500,
        radar_package = 501,
        radar_capture_1 = 502,
        radar_capture_2 = 503,
        radar_capture_3 = 504,
        radar_capture_4 = 505,
        radar_capture_5 = 506,
        radar_capture_6 = 507,
        radar_capture_7 = 508,
        radar_capture_8 = 509,
        radar_capture_9 = 510,
        radar_capture_10 = 511,
        radar_quad = 512,
        radar_bus = 513,
        radar_drugs_package = 514,
        radar_pickup_jump = 515,
        radar_adversary_4 = 516,
        radar_adversary_8 = 517,
        radar_adversary_10 = 518,
        radar_adversary_12 = 519,
        radar_adversary_16 = 520,
        radar_laptop = 521,
        radar_pickup_deadline = 522,
        radar_sports_car = 523,
        radar_warehouse_vehicle = 524,
        radar_reg_papers = 525,
        radar_police_station_dropoff = 526,
        radar_junkyard = 527,
        radar_ex_vech_1 = 528,
        radar_ex_vech_2 = 529,
        radar_ex_vech_3 = 530,
        radar_ex_vech_4 = 531,
        radar_ex_vech_5 = 532,
        radar_ex_vech_6 = 533,
        radar_ex_vech_7 = 534,
        radar_target_a = 535,
        radar_target_b = 536,
        radar_target_c = 537,
        radar_target_d = 538,
        radar_target_e = 539,
        radar_target_f = 540,
        radar_target_g = 541,
        radar_target_h = 542,
        radar_jugg = 543,
        radar_pickup_repair = 544,
        radar_steeringwheel = 545,
        radar_trophy = 546,
        radar_pickup_rocket_boost = 547,
        radar_pickup_homing_rocket = 548,
        radar_pickup_machinegun = 549,
        radar_pickup_parachute = 550,
        radar_pickup_time_5 = 551,
        radar_pickup_time_10 = 552,
        radar_pickup_time_15 = 553,
        radar_pickup_time_20 = 554,
        radar_pickup_time_30 = 555,
        radar_supplies = 556,
        radar_property_bunker = 557,
        radar_gr_wvm_1 = 558,
        radar_gr_wvm_2 = 559,
        radar_gr_wvm_3 = 560,
        radar_gr_wvm_4 = 561,
        radar_gr_wvm_5 = 562,
        radar_gr_wvm_6 = 563,
        radar_gr_covert_ops = 564,
        radar_adversary_bunker = 565,
        radar_gr_moc_upgrade = 566,
        radar_gr_w_upgrade = 567,
        radar_sm_cargo = 568,
        radar_sm_hangar = 569,
        radar_tf_checkpoint = 570,
        radar_race_tf = 571,
        radar_sm_wp1 = 572,
        radar_sm_wp2 = 573,
        radar_sm_wp3 = 574,
        radar_sm_wp4 = 575,
        radar_sm_wp5 = 576,
        radar_sm_wp6 = 577,
        radar_sm_wp7 = 578,
        radar_sm_wp8 = 579,
        radar_sm_wp9 = 580,
        radar_sm_wp10 = 581,
        radar_sm_wp11 = 582,
        radar_sm_wp12 = 583,
        radar_sm_wp13 = 584,
        radar_sm_wp14 = 585,
        radar_nhp_bag = 586,
        radar_nhp_chest = 587,
        radar_nhp_orbit = 588,
        radar_nhp_veh1 = 589,
        radar_nhp_base = 590,
        radar_nhp_overlay = 591,
        radar_nhp_turret = 592,
        radar_nhp_mg_firewall = 593,
        radar_nhp_mg_node = 594,
        radar_nhp_wp1 = 595,
        radar_nhp_wp2 = 596,
        radar_nhp_wp3 = 597,
        radar_nhp_wp4 = 598,
        radar_nhp_wp5 = 599,
        radar_nhp_wp6 = 600,
        radar_nhp_wp7 = 601,
        radar_nhp_wp8 = 602,
        radar_nhp_wp9 = 603,
        radar_nhp_cctv = 604,
        radar_nhp_starterpack = 605,
        radar_nhp_turret_console = 606,
        radar_nhp_mg_mir_rotate = 607,
        radar_nhp_mg_mir_static = 608,
        radar_nhp_mg_proxy = 609,
        radar_acsr_race_target = 610,
        radar_acsr_race_hotring = 611,
        radar_acsr_wp1 = 612,
        radar_acsr_wp2 = 613,
        radar_bat_club_property = 614,
        radar_bat_cargo = 615,
        radar_bat_truck = 616,
        radar_bat_hack_jewel = 617,
        radar_bat_hack_gold = 618,
        radar_bat_keypad = 619,
        radar_bat_hack_target = 620,
        radar_pickup_dtb_health = 621,
        radar_pickup_dtb_blast_increase = 622,
        radar_pickup_dtb_blast_decrease = 623,
        radar_pickup_dtb_bomb_increase = 624,
        radar_pickup_dtb_bomb_decrease = 625,
        radar_bat_rival_club = 626,
        radar_bat_drone = 627,
        radar_bat_cash_reg = 628,
        radar_cctv = 629,
        radar_bat_assassinate = 630,
        radar_bat_pbus = 631,
        radar_bat_wp1 = 632,
        radar_bat_wp2 = 633,
        radar_bat_wp3 = 634,
        radar_bat_wp4 = 635,
        radar_bat_wp5 = 636,
        radar_bat_wp6 = 637,
        radar_blimp_2 = 638,
        radar_oppressor_2 = 639,
        radar_bat_wp7 = 640,
        radar_arena_series = 641,
        radar_arena_premium = 642,
        radar_arena_workshop = 643,
        radar_race_wars = 644,
        radar_arena_turret = 645,
        radar_arena_rc_car = 646,
        radar_arena_rc_workshop = 647,
        radar_arena_trap_fire = 648,
        radar_arena_trap_flip = 649,
        radar_arena_trap_sea = 650,
        radar_arena_trap_turn = 651,
        radar_arena_trap_pit = 652,
        radar_arena_trap_mine = 653,
        radar_arena_trap_bomb = 654,
        radar_arena_trap_wall = 655,
        radar_arena_trap_brd = 656,
        radar_arena_trap_sbrd = 657,
        radar_arena_bruiser = 658,
        radar_arena_brutus = 659,
        radar_arena_cerberus = 660,
        radar_arena_deathbike = 661,
        radar_arena_dominator = 662,
        radar_arena_impaler = 663,
        radar_arena_imperator = 664,
        radar_arena_issi = 665,
        radar_arena_sasquatch = 666,
        radar_arena_scarab = 667,
        radar_arena_slamvan = 668,
        radar_arena_zr380 = 669,
        radar_ap = 670,
        radar_comic_store = 671,
        radar_cop_car = 672,
        radar_rc_time_trials = 673,
        radar_king_of_the_hill = 674,
        radar_king_of_the_hill_teams = 675,
        radar_rucksack = 676,
        radar_shipping_container = 677,
        radar_agatha = 678,
        radar_casino = 679,
        radar_casino_table_games = 680,
        radar_casino_wheel = 681,
        radar_casino_concierge = 682,
        radar_casino_chips = 683,
        radar_casino_horse_racing = 684,
        radar_adversary_featured = 685,
        radar_roulette_1 = 686,
        radar_roulette_2 = 687,
        radar_roulette_3 = 688,
        radar_roulette_4 = 689,
        radar_roulette_5 = 690,
        radar_roulette_6 = 691,
        radar_roulette_7 = 692,
        radar_roulette_8 = 693,
        radar_roulette_9 = 694,
        radar_roulette_10 = 695,
        radar_roulette_11 = 696,
        radar_roulette_12 = 697,
        radar_roulette_13 = 698,
        radar_roulette_14 = 699,
        radar_roulette_15 = 700,
        radar_roulette_16 = 701,
        radar_roulette_17 = 702,
        radar_roulette_18 = 703,
        radar_roulette_19 = 704,
        radar_roulette_20 = 705,
        radar_roulette_21 = 706,
        radar_roulette_22 = 707,
        radar_roulette_23 = 708,
        radar_roulette_24 = 709,
        radar_roulette_25 = 710,
        radar_roulette_26 = 711,
        radar_roulette_27 = 712,
        radar_roulette_28 = 713,
        radar_roulette_29 = 714,
        radar_roulette_30 = 715,
        radar_roulette_31 = 716,
        radar_roulette_32 = 717,
        radar_roulette_33 = 718,
        radar_roulette_34 = 719,
        radar_roulette_35 = 720,
        radar_roulette_36 = 721,
        radar_roulette_0 = 722,
        radar_roulette_00 = 723,
        radar_limo = 724,
        radar_weapon_alien = 725,
        radar_race_open_wheel = 726,
        radar_rappel = 727,
        radar_swap_car = 728,
        radar_scuba_gear = 729,
        radar_cpanel_1 = 730,
        radar_cpanel_2 = 731,
        radar_cpanel_3 = 732,
        radar_cpanel_4 = 733,
        radar_snow_truck = 734,
        radar_buggy_1 = 735,
        radar_buggy_2 = 736,
        radar_zhaba = 737,
        radar_gerald = 738,
        radar_ron = 739,
        radar_arcade = 740,
        radar_drone_controls = 741,
        radar_rc_tank = 742,
        radar_stairs = 743,
        radar_camera_2 = 744,
        radar_winky = 745,
        radar_mini_sub = 746,
        radar_kart_retro = 747,
        radar_kart_modern = 748,
        radar_military_quad = 749,
        radar_military_truck = 750,
        radar_ship_wheel = 751,
        radar_ufo = 752,
        radar_seasparrow2 = 753,
        radar_dinghy2 = 754,
        radar_patrol_boat = 755,
        radar_retro_sports_car = 756,
        radar_squadee = 757,
        radar_folding_wing_jet = 758,
        radar_valkyrie2 = 759,
        radar_sub2 = 760,
        radar_bolt_cutters = 761,
        radar_rappel_gear = 762,
        radar_keycard = 763,
        radar_password = 764,
        radar_island_heist_prep = 765,
        radar_island_party = 766,
        radar_control_tower = 767,
        radar_underwater_gate = 768,
        radar_power_switch = 769,
        radar_compound_gate = 770,
        radar_rappel_point = 771,
        radar_keypad = 772,
        radar_sub_controls = 773,
        radar_sub_periscope = 774,
        radar_sub_missile = 775,
        radar_painting = 776,
        radar_car_meet = 777,
        radar_car_test_area = 778,
        radar_auto_shop_property = 779,
        radar_docks_export = 780,
        radar_prize_car = 781,
        radar_test_car = 782,
        radar_car_robbery_board = 783,
        radar_car_robbery_prep = 784,
        radar_street_race_series = 785,
        radar_pursuit_series = 786,
        radar_car_meet_organiser = 787,
        radar_securoserv = 788,
        radar_bounty_collectibles = 789,
        radar_movie_collectibles = 790,
        radar_trailer_ramp = 791,
        radar_race_organiser = 792,
        radar_chalkboard_list = 793,
        radar_export_vehicle = 794,
        radar_train = 795,
        radar_heist_diamond = 796,
        radar_heist_doomsday = 797,
        radar_heist_island = 798,
        radar_slamvan2 = 799,
        radar_crusader = 800,
        radar_construction_outfit = 801,
        radar_overlay_jammed = 802,
        radar_heist_island_unavailable = 803,
        radar_heist_diamond_unavailable = 804,
        radar_heist_doomsday_unavailable = 805,
        radar_placeholder_7 = 806,
        radar_placeholder_8 = 807,
        radar_placeholder_9 = 808,
        radar_featured_series = 809,
        radar_vehicle_for_sale = 810,
        radar_van_keys = 811,
        radar_suv_service = 812,
        radar_security_contract = 813,
        radar_safe = 814,
        radar_ped_r = 815,
        radar_ped_e = 816,
        radar_payphone = 817,
        radar_patriot3 = 818,
        radar_music_studio = 819,
        radar_jubilee = 820,
        radar_granger2 = 821,
        radar_explosive_charge = 822,
        radar_deity = 823,
        radar_d_champion = 824,
        radar_buffalo4 = 825,
        radar_agency = 826,
        radar_biker_bar = 827,
        radar_simeon_overlay = 828,
        radar_junk_skydive = 829,
        radar_luxury_car_showroom = 830,
        radar_car_showroom = 831,
        radar_car_showroom_simeon = 832,
        radar_flaming_skull = 833,
        radar_weapon_ammo = 834,
        radar_community_series = 835,
        radar_cayo_series = 836,
        radar_clubhouse_contract = 837,
        radar_agent_ulp = 838,
        radar_acid = 839,
        radar_acid_lab = 840,
        radar_dax_overlay = 841,
        radar_dead_drop_package = 842,
        radar_downtown_cab = 843,
        radar_gun_van = 844,
        radar_stash_house = 845,
        radar_tractor = 846,
        radar_warehouse_juggalo = 847,
        radar_warehouse_juggalo_dax = 848,
        radar_weapon_crowbar = 849,
        radar_duffel_bag = 850,
        radar_oil_tanker = 851,
        radar_acid_lab_tent = 852,
        radar_van_burrito = 853,
        radar_acid_boost = 854,
        radar_ped_gang_leader = 855,
        radar_multistorey_garage = 856,
        radar_seized_asset_sales = 857,
        radar_cayo_attrition = 858,
        radar_bicycle = 859,
        radar_bicycle_trial = 860,
        radar_raiju = 861,
        radar_conada2 = 862,
        radar_overlay_ready_for_sell = 863,
        radar_overlay_missing_supplies = 864,
        radar_streamer216 = 865,
        radar_signal_jammer = 866,
        radar_salvage_yard = 867,
        radar_robbery_prep_equipment = 868,
        radar_robbery_prep_overlay = 869,
        radar_yusuf = 870,
        radar_vincent = 871,
        radar_vinewood_garage = 872,
        radar_lstb = 873,
        radar_cctv_workstation = 874,
        radar_hacking_device = 875,
        radar_race_drag = 876,
        radar_race_drift = 877,
        radar_casino_prep = 878,
        radar_planning_wall = 879,
        radar_weapon_crate = 880,
        radar_weapon_snowball = 881,
        radar_train_signals_green = 882,
        radar_train_signals_red = 883,
        radar_office_transporter = 884,
        radar_yankton_survival = 885,
        radar_daily_bounty = 886,
        radar_bounty_target = 887,
        radar_filming_schedule = 888,
        radar_pizza_this = 889,
        radar_aircraft_carrier = 890,
        radar_weapon_emp = 891,
        radar_maude_eccles = 892,
        radar_bail_bonds_office = 893,
        radar_weapon_emp_mine = 894,
        radar_zombie_disease = 895,
        radar_zombie_proximity = 896,
        radar_zombie_fire = 897,
        radar_animal_possessed = 898,
        radar_mobile_phone = 899,
        radar_garment_factory = 900,
        radar_garment_factory_for_sale = 901,
        radar_garment_factory_equipment = 902,
        radar_field_hangar = 903,
        radar_field_hangar_for_sale = 904,
        radar_cargobob_ch53 = 905,
        radar_chopper_lift_ammo = 906,
        radar_chopper_lift_armor = 907,
        radar_chopper_lift_explosives = 908,
        radar_chopper_lift_upgrade = 909,
        radar_chopper_lift_weapon = 910,
        radar_cargo_ship = 911,
        radar_submarine_missile = 912,
        radar_propeller_engine = 913,
        radar_shark = 914,
        radar_fast_travel = 915,
        radar_plane_duster2 = 916,
        radar_plane_titan2 = 917,
        radar_collectible = 918,
        radar_field_hangar_discount = 919,
        radar_garment_factory_discount = 920,
        radar_weapon_gusenberg_sweeper = 921,
        radar_higher = 0,
        radar_activities = 37,
        radar_bomb_a = 44,
        radar_planning_locations = 48,
        radar_crim_player = 54,
        radar_assassins_mark = 62,
        radar_illegal_parking = 70,
        radar_drag_race_finish = 82,
        radar_eye_sky = 91,
        radar_air_hockey = 92,
        radar_basketball = 95,
        radar_cabaret_club = 99,
        radar_internet_cafe = 111,
        radar_random_female = 114,
        radar_random_male = 115,
        radar_airport = 138,
        radar_crim_saved_vehicle = 139,
        radar_pool = 142,
        radar_objective_green = 144,
        radar_objective_red = 145,
        radar_objective_yellow = 146,
        radar_mp_friend = 148,
        radar_triathlon_cycling = 179,
        radar_triathlon_swimming = 180,
        radar_camera_badger = 192,
        radar_camera_facade = 193,
        radar_camera_ifruit = 194,
        radar_franklin_a = 212,
        radar_franklin_b = 213,
        radar_gang_vehicle_vagos = 228,
        radar_driving_bikers = 230,
        radar_driving_cops = 231,
        radar_driving_vagos = 232,
        radar_shield_bikers = 234,
        radar_shield_cops = 235,
        radar_shield_vagos = 236,
        radar_playerstate_driving = 254,
        radar_ztype = 262,
        radar_stinger = 263,
        radar_packer = 264,
        radar_monroe = 265,
        radar_territory_locked = 275,
        radar_mission_2to4 = 281,
        radar_mission_2to8 = 282,
        radar_mission_2to12 = 283,
        radar_mission_2to16 = 284,
        radar_band_strand = 292,
        radar_mission_1 = 294,
        radar_mission_2 = 295,
        radar_friend_darts = 296,
        radar_friend_comedyclub = 297,
        radar_friend_cinema = 298,
        radar_friend_tennis = 299,
        radar_friend_stripclub = 300,
        radar_friend_livemusic = 301,
        radar_friend_golf = 302,
        radar_mission_1to2 = 312,
        radar_drill = 319,
        radar_spikes = 320,
        radar_firetruck = 321,
        radar_minigun2 = 322,
        radar_bugstar = 323,
        radar_submarine = 324,
        radar_chinook = 325,
        radar_mission_bikers_1 = 327,
        radar_mission_bikers_1to2 = 328,
        radar_mission_bikers_2 = 329,
        radar_mission_bikers_2to4 = 330,
        radar_mission_bikers_2to8 = 331,
        radar_mission_bikers_2to12 = 332,
        radar_mission_bikers_2to16 = 333,
        radar_mission_cops_1 = 334,
        radar_mission_cops_1to2 = 335,
        radar_mission_cops_2 = 336,
        radar_mission_cops_2to4 = 337,
        radar_mission_cops_2to8 = 338,
        radar_mission_cops_2to12 = 339,
        radar_mission_cops_2to16 = 340,
        radar_mission_vagos_1 = 341,
        radar_mission_vagos_1to2 = 342,
        radar_mission_vagos_2 = 343,
        radar_mission_vagos_2to4 = 344,
        radar_mission_vagos_2to8 = 345,
        radar_mission_vagos_2to12 = 346,
        radar_mission_vagos_2to16 = 347,
        radar_gas_grenade = 349,
        radar_placeholder_6 = 373,
        radar_cratedrop_background = 391,
        radar_green_and_net_player1 = 392,
        radar_green_and_net_player2 = 393,
        radar_green_and_net_player3 = 394,
        radar_green_and_friendly = 395,
        radar_net_player1_and_net_player2 = 396,
        radar_net_player1_and_net_player3 = 397,
        radar_capture_the_flag_outline = 413,
        radar_player_jet = 424,
        radar_empty = 30,
        radar_empty1 = 31,
        radar_script_objective = 32,
        radar_empty2 = 33,
        radar_empty3 = 34,
        radar_station = 35,
        radar_bomb_c = 46,
        radar_burger_shot = 98,
        radar_restaurant = 117,
        radar_gang_professionals = 132,
        radar_friend_franklin_x = 166,
        radar_friend_michael_p = 167,
        radar_friend_michael_x = 168,
        radar_friend_trevor_p = 169,
        radar_friend_trevor_x = 170,
        radar_handcuffs_open_vagos = 191,
        radar_crim_arrest_vagos = 196,
        radar_numbered_red_2 = 216,
        radar_numbered_red_3 = 217,
        radar_numbered_red_4 = 218,
        radar_numbered_red_5 = 219,
        radar_numbered_red_6 = 220,
        radar_numbered_red_7 = 221,
        radar_numbered_red_8 = 222,
        radar_numbered_red_9 = 223,
        radar_numbered_red_10 = 224,
        radar_gang_wanted_bikers_1 = 240,
        radar_gang_wanted_bikers_2 = 241,
        radar_gang_wanted_bikers_3 = 242,
        radar_gang_wanted_bikers_4 = 243,
        radar_gang_wanted_bikers_5 = 244,
        radar_gang_wanted_vagos = 245,
        radar_gang_wanted_vagos_1 = 246,
        radar_gang_wanted_vagos_2 = 247,
        radar_gang_wanted_vagos_3 = 248,
        radar_gang_wanted_vagos_4 = 249,
        radar_gang_wanted_vagos_5 = 250,
        radar_gang_wanted_2 = 258,
        radar_gang_wanted_3 = 259,
        radar_gang_wanted_4 = 260,
        radar_gang_wanted_5 = 261,
        radar_weapon_tear_gas = 922,
        radar_dog = 923,
        radar_bobcat_security = 924,
        radar_smoke_shop = 925,
        radar_smoke_shop_for_sale = 926,
        radar_smoke_shop_attention = 927,
        radar_helitours = 928,
        radar_helitours_for_sale = 929,
        radar_helitours_attention = 930,
        radar_car_wash_business = 931,
        radar_car_wash_business_for_sale = 932,
        radar_car_wash_business_attention = 933,
        radar_attention = 934,
        radar_alarm = 935,
        radar_helitours_discount = 936,
        radar_smoke_shop_discount = 937,
        radar_car_wash_business_discount = 938,
        radar_real_estate = 939,
        radar_medical_courier = 940,
        radar_gruppe_sechs = 941,
        radar_fire_station = 942,
        radar_fire_truck = 943,
        radar_alpha_mail = 944,
        radar_ls_meteor = 945,
        radar_four20_survival = 946,
        radar_community_mission_series = 947,
        radar_property_mansion = 948,
        radar_ai_keypad = 949,
        radar_taxi_self_drive = 950,
        radar_train_subway = 951,
        radar_trashbag = 952,
        radar_mission_creator = 953,
        radar_cat = 954,
        radar_mansion_ai_m = 955,
        radar_mansion_ai_f = 956,
        radar_mansion_ai_gang = 957
    }
    public enum ExtendedBlipColor  
    {
        White = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
        Yellow = 66,
        WhiteNotPure = 4,
        Yellow2 = 5,
        NetPlayer1 = 6,
        NetPlayer2 = 7,
        NetPlayer3 = 8,
        NetPlayer4 = 9,
        NetPlayer5 = 10,
        NetPlayer6 = 11,
        NetPlayer7 = 12,
        NetPlayer8 = 13,
        NetPlayer9 = 14,
        NetPlayer10 = 15,
        NetPlayer11 = 16,
        NetPlayer12 = 17,
        NetPlayer13 = 18,
        NetPlayer14 = 19,
        NetPlayer15 = 20,
        NetPlayer16 = 21,
        NetPlayer17 = 22,
        NetPlayer18 = 23,
        NetPlayer19 = 24,
        NetPlayer20 = 25,
        NetPlayer21 = 26,
        NetPlayer22 = 27,
        NetPlayer23 = 28,
        NetPlayer24 = 29,
        NetPlayer25 = 30,
        NetPlayer26 = 31,
        NetPlayer27 = 32,
        NetPlayer28 = 33,
        NetPlayer29 = 34,
        NetPlayer30 = 35,
        NetPlayer31 = 36,
        NetPlayer32 = 37,
        Freemode = 38,
        InactiveMission = 39,
        GreyDark = 40,
        RedLight = 41,
        Michael = 42,
        Franklin = 43,
        Trevor = 44,
        GolfPlayer1 = 45,
        GolfPlayer2 = 46,
        GolfPlayer3 = 47,
        GolfPlayer4 = 48,
        Red2 = 49,
        Purple = 50,
        Orange = 51,
        GreenDark = 52,
        BlueLight = 53,
        BlueDark = 54,
        Grey = 55,
        YellowDark = 56,
        Blue2 = 57,
        PurpleDark = 58,
        Red3 = 59,
        Yellow3 = 60,
        Pink = 61,
        GreyLight = 62,
        Gang = 63,
        Gang2 = 64,
        Gang3 = 65,
        Blue3 = 67,
        Blue4 = 68,
        Green2 = 69,
        Yellow4 = 70,
        Yellow5 = 71,
        White2 = 72,
        Yellow6 = 73,
        Blue5 = 74,
        Red4 = 75,
        RedDark = 76,
        Blue6 = 77,
        BlueDark2 = 78,
        RedDark2 = 79,
        MenuYellow = 80,
        SimpleBlipDefault = 81,
        Waypoint = 82,
        Blue7 = 83,

        
        Blue8 = 84,
        TransparentBlack = 85
    }
}