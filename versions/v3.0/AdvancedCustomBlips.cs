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
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.Concurrent;

using IniParser;
using IniParser.Model;
using LemonUI;
using LemonUI.Menus;

using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;

namespace AdvancedCustomBlips
{
    public class Main : Script
    {
        
        private readonly ObjectPool _uiMenuPool;
        private NativeMenu _mainMenu, _settingsMenu, _addBlipMenu, _teleportMenu, _editMenu, _categoryMenu;
        private NativeMenu _blipManagerMenu = null;

        
        private static readonly HashSet<Blip> _activeBlips = new HashSet<Blip>();  

        
        private static readonly HashSet<PredefinedBlipData> _gasStationsCategoryBlips = new HashSet<PredefinedBlipData>
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

        private static readonly HashSet<PredefinedBlipData> _marketCategoryBlips = new HashSet<PredefinedBlipData>
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

        private static readonly HashSet<PredefinedBlipData> _policeDepartmentCategoryBlips = new HashSet<PredefinedBlipData>
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

        private static readonly HashSet<PredefinedBlipData> _fireDepartmentCategoryBlips = new HashSet<PredefinedBlipData>
        {
new PredefinedBlipData("Fire Department", new Vector3(-644.46f, -114.09f, 37.91f), 648, 47, 1.0f, false, true),
new PredefinedBlipData("Fire Department", new Vector3(1200.60f, -1459.13f, 34.77f), 648, 47, 1.0f, false, true),
new PredefinedBlipData("Fire Department", new Vector3(214.89f, -1639.34f, 29.60f), 648, 47, 1.0f, false, true),
new PredefinedBlipData("Fire Department", new Vector3(-2113.49f, 2834.13f, 32.81f), 648, 47, 1.0f, false, true),
new PredefinedBlipData("Fire Department", new Vector3(1697.68f, 3585.90f, 40.33f), 648, 47, 1.0f, false, true),
new PredefinedBlipData("Fire Department", new Vector3(-381.85f, 6121.45f, 31.48f), 648, 47, 1.0f, false, true),
new PredefinedBlipData("Fire Department", new Vector3(-1034.89f, -2383.48f, 14.09f), 648, 47, 1.0f, false, true),
        };

        private static readonly HashSet<PredefinedBlipData> _ATMCategoryBlips = new HashSet<PredefinedBlipData>
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

        private static readonly HashSet<PredefinedBlipData> _metroStationCategoryBlips = new HashSet<PredefinedBlipData>
        {
new PredefinedBlipData("Metro Station", new Vector3(-825.84f, -112.67f, 27.96f), 795, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(274.72f, -1204.29f, 38.90f), 795, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-540.97f, -1280.31f, 26.90f), 795, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-947.24f, -2339.23f, 4.51f), 795, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-1040.81f, -2743.34f, 13.45f), 795, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-245.46f, -335.18f, 29.48f), 795, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-1369.87f, -527.97f, 29.82f), 795, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-490.10f, -697.08f, 32.73f), 795, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(-215.27f, -1035.11f, 30.14f), 795, 1, 0.9f, false, true),
new PredefinedBlipData("Metro Station", new Vector3(119.46f, -1730.48f, 30.11f), 795, 1, 0.9f, false, true),
        };

        private static readonly HashSet<PredefinedBlipData> _medicalCenterCategoryBlips = new HashSet<PredefinedBlipData>
        {
new PredefinedBlipData("Medical Center", new Vector3(355.37f, -596.21f, 74.17f), 61, -1, 1.0f, false, true),
new PredefinedBlipData("Medical Center", new Vector3(341.01f, -1396.80f, 32.51f), 61, -1, 1.0f, false, true),
new PredefinedBlipData("Medical Center", new Vector3(-468.68f, -337.11f, 91.01f), 61, -1, 1.0f, false, true),
new PredefinedBlipData("Medical Center", new Vector3(1840.82f, 3670.38f, 33.68f), 61, -1, 1.0f, false, true),
new PredefinedBlipData("Medical Center", new Vector3(-243.96f, 6327.12f, 37.62f), 61, -1, 1.0f, false, true),
        };

        private readonly Dictionary<NativeItem, float> _addBlipCoordinateInputs = new Dictionary<NativeItem, float>();  
        private readonly Dictionary<NativeItem, float> _editBlipCoordinateInputs = new Dictionary<NativeItem, float>();  
        private readonly Dictionary<NativeItem, string> _blipNameInputs = new Dictionary<NativeItem, string>(); 

        
        private readonly string _iniFilePath = Path.Combine("scripts", "Advanced Custom Blips.ini");  
        private readonly string _logFilePath = Path.Combine("scripts", "Advanced Custom Blips Log.txt");  
        private readonly string _addOnBlipsFilesPath = Path.Combine("scripts", "AddonBlips");
        private readonly FileIniDataParser _iniParser;
        private readonly object _fileWriteLock = new object();  

        
        private static bool _hasLoadedBlips = false;  
        private bool _showCoordsOnScreen = false;  
        private bool _showBlipLoadNotification = true;  
        private bool _enableAddOnBlips = false;
        private bool _areBlipsVisible = true;  
        private int _blipCount = 0;  

        
        private Keys? _keyToggleCoords = Keys.F1;  
        private Keys? _keyReloadBlips = Keys.F2;  
        private Keys? _keyToggleBlipVisibility = Keys.F3;  
        private Keys? _keyOpenMenu = Keys.F10; 

        private const string _invalidName = "Invalid name. Name cannot be empty, white space, or contains '_'.";
        private const int _defaultInterval = 1000; 
        private const int _activeInterval = 1;    

        
        private Blip _previewBlip = null;
        private const float _previewDistance = 17.0f; 

        
        private bool _waitingForKeyAssignment = false;
        private string _pendingKeyBind = null; 
        private NativeListItem<string> _pendingListItem = null;
        private NativeItem _nameItem;
        private NativeListItem<string> _iconItem;
        private NativeListItem<string> _colorItem;
        private NativeItem _sizeItem;
        private NativeItem _xInputItem;
        private NativeItem _yInputItem;
        private NativeItem _zInputItem;
        private NativeItem _flashIntervalItem;
        private NativeCheckboxItem _shortRangeItem;
        private NativeCheckboxItem _flashItem;

        
        private readonly ConcurrentDictionary<string, string> _requiredBlipProperties = new ConcurrentDictionary<string, string>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            ["BLIP_NAME"] = "Blip name (e.g., 'Hospital')",
            ["BLIP_ICON"] = "Blip icon ID (e.g., 1 or radar_level for standard marker)",
            ["BLIP_SIZE"] = "Blip scale size (e.g., 1.0)",
            ["BLIP_COLOR"] = "Blip color ID (e.g., 1 or Red for red)",
            ["FLASHING_STATE"] = "Flashing state (ON/OFF)",
            ["FLASH_INTERVAL"] = "Flash interval in milliseconds (e.g., 500)",
            ["X"] = "X coordinate (e.g., -2451.3)",
            ["Y"] = "Y coordinate (e.g., 2979.5)",
            ["Z"] = "Z coordinate (e.g., 30.0)",
            ["SHORT_RANGE_STATE"] = "Short range state (ON/OFF)"
        };
        public struct PredefinedBlipData
        {
            public string Name;
            public Vector3 Position;
            public int IconId;
            public int ColorId;
            public float Size;
            public bool IsFlashing;
            public bool IsShortRange;

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

        private static readonly Dictionary<ExtendedBlipSprite, IconsOnScreenPosition> _onScreenIconsMap = new Dictionary<ExtendedBlipSprite, IconsOnScreenPosition>()
        {
            
            
{
    (ExtendedBlipSprite)512,
    new IconsOnScreenPosition(646, 15)
},
{
    (ExtendedBlipSprite)533,
    new IconsOnScreenPosition(646, 36)
},
{
    (ExtendedBlipSprite)563,
    new IconsOnScreenPosition(641, 57)
},
{
    (ExtendedBlipSprite)579,
    new IconsOnScreenPosition(646, 79)
},
{
    (ExtendedBlipSprite)595,
    new IconsOnScreenPosition(646, 104)
},
{
    (ExtendedBlipSprite)611,
    new IconsOnScreenPosition(645, 122)
},
{
    (ExtendedBlipSprite)627,
    new IconsOnScreenPosition(647, 150)
},
{
    (ExtendedBlipSprite)644,
    new IconsOnScreenPosition(645, 168)
},
{
    (ExtendedBlipSprite)668,
    new IconsOnScreenPosition(645, 192)
},
{
    (ExtendedBlipSprite)676,
    new IconsOnScreenPosition(648, 218)
},
{
    (ExtendedBlipSprite)728,
    new IconsOnScreenPosition(645, 239)
},
{
    (ExtendedBlipSprite)748,
    new IconsOnScreenPosition(647, 263)
},
{
    (ExtendedBlipSprite)762,
    new IconsOnScreenPosition(646, 286)
},
{
    (ExtendedBlipSprite)778,
    new IconsOnScreenPosition(646, 307)
},
{
    (ExtendedBlipSprite)824,
    new IconsOnScreenPosition(648, 351)
},

           
           {
    (ExtendedBlipSprite)513,
    new IconsOnScreenPosition(686, 16)
},
{
    (ExtendedBlipSprite)534,
    new IconsOnScreenPosition(686, 38)
},
{
    (ExtendedBlipSprite)564,
    new IconsOnScreenPosition(685, 59)
},
{
    (ExtendedBlipSprite)580,
    new IconsOnScreenPosition(685, 80)
},
{
    (ExtendedBlipSprite)596,
    new IconsOnScreenPosition(686, 105)
},
{
    (ExtendedBlipSprite)612,
    new IconsOnScreenPosition(686, 125)
},
{
    (ExtendedBlipSprite)628,
    new IconsOnScreenPosition(686, 151)
},
{
    (ExtendedBlipSprite)645,
    new IconsOnScreenPosition(687, 172)
},
{
    (ExtendedBlipSprite)665,
    new IconsOnScreenPosition(685, 196)
},
{
    (ExtendedBlipSprite)685,
    new IconsOnScreenPosition(686, 218)
},
{
    (ExtendedBlipSprite)730,
    new IconsOnScreenPosition(686, 241)
},
{
    (ExtendedBlipSprite)747,
    new IconsOnScreenPosition(686, 261)
},
{
    (ExtendedBlipSprite)761,
    new IconsOnScreenPosition(686, 281)
},
{
    (ExtendedBlipSprite)782,
    new IconsOnScreenPosition(686, 305)
},
{
    (ExtendedBlipSprite)825,
    new IconsOnScreenPosition(686, 351)
},

          
          {
    (ExtendedBlipSprite)514,
    new IconsOnScreenPosition(726, 16)
},
{
    (ExtendedBlipSprite)543,
    new IconsOnScreenPosition(725, 40)
},
{
    (ExtendedBlipSprite)565,
    new IconsOnScreenPosition(727, 59)
},
{
    (ExtendedBlipSprite)581,
    new IconsOnScreenPosition(726, 80)
},
{
    (ExtendedBlipSprite)597,
    new IconsOnScreenPosition(725, 103)
},
{
    (ExtendedBlipSprite)613,
    new IconsOnScreenPosition(726, 127)
},
{
    (ExtendedBlipSprite)629,
    new IconsOnScreenPosition(727, 150)
},
{
    (ExtendedBlipSprite)646,
    new IconsOnScreenPosition(726, 170)
},
{
    (ExtendedBlipSprite)666,
    new IconsOnScreenPosition(726, 192)
},
{
    (ExtendedBlipSprite)678,
    new IconsOnScreenPosition(725, 216)
},
{
    (ExtendedBlipSprite)731,
    new IconsOnScreenPosition(726, 240)
},
{
    (ExtendedBlipSprite)755,
    new IconsOnScreenPosition(727, 260)
},
{
    (ExtendedBlipSprite)758,
    new IconsOnScreenPosition(724, 285)
},
{
    (ExtendedBlipSprite)779,
    new IconsOnScreenPosition(725, 305)
},
{
    (ExtendedBlipSprite)823,
    new IconsOnScreenPosition(727, 353)
},
          
          {
    (ExtendedBlipSprite)515,
    new IconsOnScreenPosition(766, 15)
},
{
    (ExtendedBlipSprite)545,
    new IconsOnScreenPosition(766, 39)
},
{
    (ExtendedBlipSprite)566,
    new IconsOnScreenPosition(765, 59)
},
{
    (ExtendedBlipSprite)582,
    new IconsOnScreenPosition(766, 80)
},
{
    (ExtendedBlipSprite)598,
    new IconsOnScreenPosition(766, 105)
},
{
    (ExtendedBlipSprite)614,
    new IconsOnScreenPosition(767, 126)
},
{
    (ExtendedBlipSprite)631,
    new IconsOnScreenPosition(766, 151)
},
{
    (ExtendedBlipSprite)647,
    new IconsOnScreenPosition(766, 173)
},
{
    (ExtendedBlipSprite)660,
    new IconsOnScreenPosition(767, 194)
},
{
    (ExtendedBlipSprite)679,
    new IconsOnScreenPosition(766, 216)
},
{
    (ExtendedBlipSprite)732,
    new IconsOnScreenPosition(767, 241)
},
{
    (ExtendedBlipSprite)759,
    new IconsOnScreenPosition(766, 264)
},
{
    (ExtendedBlipSprite)772,
    new IconsOnScreenPosition(767, 287)
},
{
    (ExtendedBlipSprite)784,
    new IconsOnScreenPosition(765, 310)
},
{
    (ExtendedBlipSprite)799,
    new IconsOnScreenPosition(765, 330)
},
{
    (ExtendedBlipSprite)820,
    new IconsOnScreenPosition(768, 350)
},
           
           {
    (ExtendedBlipSprite)521,
    new IconsOnScreenPosition(805, 15)
},
{
    (ExtendedBlipSprite)546,
    new IconsOnScreenPosition(807, 33)
},
{
    (ExtendedBlipSprite)567,
    new IconsOnScreenPosition(803, 58)
},
{
    (ExtendedBlipSprite)583,
    new IconsOnScreenPosition(806, 80)
},
{
    (ExtendedBlipSprite)599,
    new IconsOnScreenPosition(805, 103)
},
{
    (ExtendedBlipSprite)615,
    new IconsOnScreenPosition(806, 126)
},
{
    (ExtendedBlipSprite)632,
    new IconsOnScreenPosition(806, 151)
},
{
    (ExtendedBlipSprite)648,
    new IconsOnScreenPosition(806, 171)
},
{
    (ExtendedBlipSprite)658,
    new IconsOnScreenPosition(806, 192)
},
{
    (ExtendedBlipSprite)683,
    new IconsOnScreenPosition(806, 219)
},
{
    (ExtendedBlipSprite)733,
    new IconsOnScreenPosition(807, 242)
},
{
    (ExtendedBlipSprite)752,
    new IconsOnScreenPosition(806, 262)
},
{
    (ExtendedBlipSprite)766,
    new IconsOnScreenPosition(802, 287)
},
{
    (ExtendedBlipSprite)777,
    new IconsOnScreenPosition(806, 306)
},
{
    (ExtendedBlipSprite)800,
    new IconsOnScreenPosition(806, 329)
},
{
    (ExtendedBlipSprite)821,
    new IconsOnScreenPosition(806, 350)
},
           
           {
    (ExtendedBlipSprite)523,
    new IconsOnScreenPosition(846, 15)
},
{
    (ExtendedBlipSprite)547,
    new IconsOnScreenPosition(844, 32)
},
{
    (ExtendedBlipSprite)568,
    new IconsOnScreenPosition(845, 59)
},
{
    (ExtendedBlipSprite)584,
    new IconsOnScreenPosition(845, 80)
},
{
    (ExtendedBlipSprite)600,
    new IconsOnScreenPosition(846, 104)
},
{
    (ExtendedBlipSprite)616,
    new IconsOnScreenPosition(844, 125)
},
{
    (ExtendedBlipSprite)633,
    new IconsOnScreenPosition(845, 150)
},
{
    (ExtendedBlipSprite)649,
    new IconsOnScreenPosition(845, 169)
},
{
    (ExtendedBlipSprite)659,
    new IconsOnScreenPosition(847, 194)
},
{
    (ExtendedBlipSprite)684,
    new IconsOnScreenPosition(847, 217)
},
{
    (ExtendedBlipSprite)735,
    new IconsOnScreenPosition(844, 238)
},
{
    (ExtendedBlipSprite)751,
    new IconsOnScreenPosition(846, 261)
},
{
    (ExtendedBlipSprite)771,
    new IconsOnScreenPosition(846, 284)
},
{
    (ExtendedBlipSprite)786,
    new IconsOnScreenPosition(846, 302)
},
{
    (ExtendedBlipSprite)801,
    new IconsOnScreenPosition(846, 327)
},
{
    (ExtendedBlipSprite)818,
    new IconsOnScreenPosition(845, 349)
},

           
           {
    (ExtendedBlipSprite)522,
    new IconsOnScreenPosition(885, 16)
},
{
    (ExtendedBlipSprite)550,
    new IconsOnScreenPosition(887, 37)
},
{
    (ExtendedBlipSprite)569,
    new IconsOnScreenPosition(887, 59)
},
{
    (ExtendedBlipSprite)585,
    new IconsOnScreenPosition(887, 79)
},
{
    (ExtendedBlipSprite)601,
    new IconsOnScreenPosition(885, 103)
},
{
    (ExtendedBlipSprite)617,
    new IconsOnScreenPosition(885, 125)
},
{
    (ExtendedBlipSprite)634,
    new IconsOnScreenPosition(885, 147)
},
{
    (ExtendedBlipSprite)650,
    new IconsOnScreenPosition(886, 170)
},
{
    (ExtendedBlipSprite)669,
    new IconsOnScreenPosition(886, 193)
},
{
    (ExtendedBlipSprite)682,
    new IconsOnScreenPosition(886, 218)
},
{
    (ExtendedBlipSprite)736,
    new IconsOnScreenPosition(886, 238)
},
{
    (ExtendedBlipSprite)754,
    new IconsOnScreenPosition(886, 262)
},
{
    (ExtendedBlipSprite)773,
    new IconsOnScreenPosition(885, 284)
},
{
    (ExtendedBlipSprite)780,
    new IconsOnScreenPosition(886, 305)
},
{
    (ExtendedBlipSprite)785,
    new IconsOnScreenPosition(884, 327)
},
{
    (ExtendedBlipSprite)812,
    new IconsOnScreenPosition(885, 351)
},

          
          {
    (ExtendedBlipSprite)524,
    new IconsOnScreenPosition(926, 17)
},
{
    (ExtendedBlipSprite)548,
    new IconsOnScreenPosition(926, 36)
},
{
    (ExtendedBlipSprite)570,
    new IconsOnScreenPosition(925, 58)
},
{
    (ExtendedBlipSprite)586,
    new IconsOnScreenPosition(926, 83)
},
{
    (ExtendedBlipSprite)602,
    new IconsOnScreenPosition(926, 105)
},
{
    (ExtendedBlipSprite)618,
    new IconsOnScreenPosition(925, 127)
},
{
    (ExtendedBlipSprite)635,
    new IconsOnScreenPosition(927, 149)
},
{
    (ExtendedBlipSprite)651,
    new IconsOnScreenPosition(925, 172)
},
{
    (ExtendedBlipSprite)662,
    new IconsOnScreenPosition(926, 192)
},
{
    (ExtendedBlipSprite)680,
    new IconsOnScreenPosition(926, 215)
},
{
    (ExtendedBlipSprite)734,
    new IconsOnScreenPosition(926, 239)
},
{
    (ExtendedBlipSprite)757,
    new IconsOnScreenPosition(926, 261)
},
{
    (ExtendedBlipSprite)774,
    new IconsOnScreenPosition(925, 283)
},
{
    (ExtendedBlipSprite)788,
    new IconsOnScreenPosition(926, 308)
},
{
    (ExtendedBlipSprite)802,
    new IconsOnScreenPosition(937, 324)
},
{
    (ExtendedBlipSprite)826,
    new IconsOnScreenPosition(925, 351)
},
         
         {
    (ExtendedBlipSprite)525,
    new IconsOnScreenPosition(964, 15)
},
{
    (ExtendedBlipSprite)549,
    new IconsOnScreenPosition(966, 38)
},
{
    (ExtendedBlipSprite)571,
    new IconsOnScreenPosition(965, 55)
},
{
    (ExtendedBlipSprite)587,
    new IconsOnScreenPosition(965, 82)
},
{
    (ExtendedBlipSprite)603,
    new IconsOnScreenPosition(966, 103)
},
{
    (ExtendedBlipSprite)619,
    new IconsOnScreenPosition(966, 129)
},
{
    (ExtendedBlipSprite)636,
    new IconsOnScreenPosition(965, 152)
},
{
    (ExtendedBlipSprite)652,
    new IconsOnScreenPosition(965, 174)
},
{
    (ExtendedBlipSprite)663,
    new IconsOnScreenPosition(965, 194)
},
{
    (ExtendedBlipSprite)681,
    new IconsOnScreenPosition(964, 215)
},
{
    (ExtendedBlipSprite)737,
    new IconsOnScreenPosition(964, 237)
},
{
    (ExtendedBlipSprite)745,
    new IconsOnScreenPosition(966, 262)
},
{
    (ExtendedBlipSprite)767,
    new IconsOnScreenPosition(966, 284)
},
{
    (ExtendedBlipSprite)789,
    new IconsOnScreenPosition(963, 311)
},
{
    (ExtendedBlipSprite)806,
    new IconsOnScreenPosition(966, 327)
},
{
    (ExtendedBlipSprite)813,
    new IconsOnScreenPosition(965, 353)
},
           
           {
    (ExtendedBlipSprite)526,
    new IconsOnScreenPosition(1007, 14)
},
{
    (ExtendedBlipSprite)556,
    new IconsOnScreenPosition(1005, 39)
},
{
    (ExtendedBlipSprite)572,
    new IconsOnScreenPosition(1005, 56)
},
{
    (ExtendedBlipSprite)588,
    new IconsOnScreenPosition(1005, 81)
},
{
    (ExtendedBlipSprite)604,
    new IconsOnScreenPosition(1006, 103)
},
{
    (ExtendedBlipSprite)620,
    new IconsOnScreenPosition(1006, 125)
},
{
    (ExtendedBlipSprite)637,
    new IconsOnScreenPosition(1006, 149)
},
{
    (ExtendedBlipSprite)653,
    new IconsOnScreenPosition(1006, 173)
},
{
    (ExtendedBlipSprite)664,
    new IconsOnScreenPosition(1006, 194)
},
{
    (ExtendedBlipSprite)724,
    new IconsOnScreenPosition(1006, 216)
},
{
    (ExtendedBlipSprite)740,
    new IconsOnScreenPosition(1007, 241)
},
{
    (ExtendedBlipSprite)756,
    new IconsOnScreenPosition(1006, 262)
},
{
    (ExtendedBlipSprite)775,
    new IconsOnScreenPosition(1006, 283)
},
{
    (ExtendedBlipSprite)790,
    new IconsOnScreenPosition(1006, 309)
},
{
    (ExtendedBlipSprite)808,
    new IconsOnScreenPosition(1005, 330)
},
{
    (ExtendedBlipSprite)811,
    new IconsOnScreenPosition(1005, 349)
},

            
            {
    (ExtendedBlipSprite)527,
    new IconsOnScreenPosition(1045, 13)
},
{
    (ExtendedBlipSprite)557,
    new IconsOnScreenPosition(1045, 39)
},
{
    (ExtendedBlipSprite)573,
    new IconsOnScreenPosition(1045, 57)
},
{
    (ExtendedBlipSprite)589,
    new IconsOnScreenPosition(1046, 83)
},
{
    (ExtendedBlipSprite)605,
    new IconsOnScreenPosition(1046, 105)
},
{
    (ExtendedBlipSprite)621,
    new IconsOnScreenPosition(1046, 127)
},
{
    (ExtendedBlipSprite)638,
    new IconsOnScreenPosition(1044, 149)
},
{
    (ExtendedBlipSprite)654,
    new IconsOnScreenPosition(1045, 173)
},
{
    (ExtendedBlipSprite)671,
    new IconsOnScreenPosition(1046, 197)
},
{
    (ExtendedBlipSprite)741,
    new IconsOnScreenPosition(1046, 239)
},
{
    (ExtendedBlipSprite)753,
    new IconsOnScreenPosition(1045, 262)
},
{
    (ExtendedBlipSprite)768,
    new IconsOnScreenPosition(1047, 285)
},
{
    (ExtendedBlipSprite)791,
    new IconsOnScreenPosition(1046, 308)
},
{
    (ExtendedBlipSprite)807,
    new IconsOnScreenPosition(1047, 331)
},
{
    (ExtendedBlipSprite)837,
    new IconsOnScreenPosition(1046, 350)
},
           
           {
    (ExtendedBlipSprite)528,
    new IconsOnScreenPosition(1088, 15)
},
{
    (ExtendedBlipSprite)558,
    new IconsOnScreenPosition(1087, 38)
},
{
    (ExtendedBlipSprite)574,
    new IconsOnScreenPosition(1086, 57)
},
{
    (ExtendedBlipSprite)590,
    new IconsOnScreenPosition(1085, 81)
},
{
    (ExtendedBlipSprite)606,
    new IconsOnScreenPosition(1086, 103)
},
{
    (ExtendedBlipSprite)622,
    new IconsOnScreenPosition(1086, 129)
},
{
    (ExtendedBlipSprite)639,
    new IconsOnScreenPosition(1086, 149)
},
{
    (ExtendedBlipSprite)672,
    new IconsOnScreenPosition(1086, 194)
},
{
    (ExtendedBlipSprite)726,
    new IconsOnScreenPosition(1084, 217)
},
{
    (ExtendedBlipSprite)742,
    new IconsOnScreenPosition(1086, 239)
},
{
    (ExtendedBlipSprite)746,
    new IconsOnScreenPosition(1088, 262)
},
{
    (ExtendedBlipSprite)769,
    new IconsOnScreenPosition(1086, 285)
},
{
    (ExtendedBlipSprite)792,
    new IconsOnScreenPosition(1085, 308)
},
{
    (ExtendedBlipSprite)809,
    new IconsOnScreenPosition(1083, 327)
},
{
    (ExtendedBlipSprite)827,
    new IconsOnScreenPosition(1086, 352)
},

{
    (ExtendedBlipSprite)529,
    new IconsOnScreenPosition(1125, 14)
},
{
    (ExtendedBlipSprite)530,
    new IconsOnScreenPosition(1165, 15)
},
{
    (ExtendedBlipSprite)531,
    new IconsOnScreenPosition(1206, 15)
},
{
    (ExtendedBlipSprite)532,
    new IconsOnScreenPosition(1246, 14)
},
{
    (ExtendedBlipSprite)559,
    new IconsOnScreenPosition(1125, 38)
},
{
    (ExtendedBlipSprite)560,
    new IconsOnScreenPosition(1165, 37)
},
{
    (ExtendedBlipSprite)561,
    new IconsOnScreenPosition(1206, 37)
},
{
    (ExtendedBlipSprite)562,
    new IconsOnScreenPosition(1245, 39)
},
{
    (ExtendedBlipSprite)575,
    new IconsOnScreenPosition(1125, 59)
},
{
    (ExtendedBlipSprite)576,
    new IconsOnScreenPosition(1166, 61)
},
{
    (ExtendedBlipSprite)577,
    new IconsOnScreenPosition(1205, 59)
},
{
    (ExtendedBlipSprite)578,
    new IconsOnScreenPosition(1245, 57)
},
{
    (ExtendedBlipSprite)591,
    new IconsOnScreenPosition(1115, 77)
},
{
    (ExtendedBlipSprite)592,
    new IconsOnScreenPosition(1164, 79)
},
{
    (ExtendedBlipSprite)593,
    new IconsOnScreenPosition(1205, 83)
},
{
    (ExtendedBlipSprite)594,
    new IconsOnScreenPosition(1244, 83)
},
{
    (ExtendedBlipSprite)607,
    new IconsOnScreenPosition(1125, 105)
},
{
    (ExtendedBlipSprite)608,
    new IconsOnScreenPosition(1157, 107)
},
{
    (ExtendedBlipSprite)609,
    new IconsOnScreenPosition(1205, 106)
},
{
    (ExtendedBlipSprite)610,
    new IconsOnScreenPosition(1243, 102)
},
{
    (ExtendedBlipSprite)623,
    new IconsOnScreenPosition(1125, 126)
},
{
    (ExtendedBlipSprite)624,
    new IconsOnScreenPosition(1165, 129)
},
{
    (ExtendedBlipSprite)625,
    new IconsOnScreenPosition(1205, 129)
},
{
    (ExtendedBlipSprite)626,
    new IconsOnScreenPosition(1244, 127)
},
{
    (ExtendedBlipSprite)640,
    new IconsOnScreenPosition(1125, 151)
},
{
    (ExtendedBlipSprite)642,
    new IconsOnScreenPosition(1164, 151)
},
{
    (ExtendedBlipSprite)641,
    new IconsOnScreenPosition(1205, 149)
},
{
    (ExtendedBlipSprite)643,
    new IconsOnScreenPosition(1244, 150)
},
{
    (ExtendedBlipSprite)655,
    new IconsOnScreenPosition(1124, 171)
},
{
    (ExtendedBlipSprite)673,
    new IconsOnScreenPosition(1126, 196)
},
{
    (ExtendedBlipSprite)738,
    new IconsOnScreenPosition(1127, 216)
},
{
    (ExtendedBlipSprite)743,
    new IconsOnScreenPosition(1126, 238)
},
{
    (ExtendedBlipSprite)750,
    new IconsOnScreenPosition(1125, 262)
},
{
    (ExtendedBlipSprite)770,
    new IconsOnScreenPosition(1126, 286)
},
{
    (ExtendedBlipSprite)793,
    new IconsOnScreenPosition(1127, 307)
},
{
    (ExtendedBlipSprite)814,
    new IconsOnScreenPosition(1126, 329)
},
{
    (ExtendedBlipSprite)838,
    new IconsOnScreenPosition(1125, 352)
},
{
    (ExtendedBlipSprite)657,
    new IconsOnScreenPosition(1165, 172)
},
{
    (ExtendedBlipSprite)674,
    new IconsOnScreenPosition(1166, 195)
},
{
    (ExtendedBlipSprite)739,
    new IconsOnScreenPosition(1165, 216)
},
{
    (ExtendedBlipSprite)744,
    new IconsOnScreenPosition(1165, 241)
},
{
    (ExtendedBlipSprite)763,
    new IconsOnScreenPosition(1166, 262)
},
{
    (ExtendedBlipSprite)776,
    new IconsOnScreenPosition(1166, 286)
},
{
    (ExtendedBlipSprite)794,
    new IconsOnScreenPosition(1165, 307)
},
{
    (ExtendedBlipSprite)819,
    new IconsOnScreenPosition(1165, 328)
},
{
    (ExtendedBlipSprite)836,
    new IconsOnScreenPosition(1167, 353)
},
{
    (ExtendedBlipSprite)667,
    new IconsOnScreenPosition(1207, 175)
},
{
    (ExtendedBlipSprite)675,
    new IconsOnScreenPosition(1204, 195)
},
{
    (ExtendedBlipSprite)729,
    new IconsOnScreenPosition(1204, 220)
},
{
    (ExtendedBlipSprite)749,
    new IconsOnScreenPosition(1205, 241)
},
{
    (ExtendedBlipSprite)764,
    new IconsOnScreenPosition(1205, 262)
},
{
    (ExtendedBlipSprite)781,
    new IconsOnScreenPosition(1206, 286)
},
{
    (ExtendedBlipSprite)795,
    new IconsOnScreenPosition(1206, 308)
},
{
    (ExtendedBlipSprite)817,
    new IconsOnScreenPosition(1205, 329)
},
{
    (ExtendedBlipSprite)835,
    new IconsOnScreenPosition(1205, 352)
},
{
    (ExtendedBlipSprite)661,
    new IconsOnScreenPosition(1246, 173)
},
{
    (ExtendedBlipSprite)677,
    new IconsOnScreenPosition(1245, 195)
},
{
    (ExtendedBlipSprite)727,
    new IconsOnScreenPosition(1246, 217)
},
{
    (ExtendedBlipSprite)760,
    new IconsOnScreenPosition(1246, 240)
},
{
    (ExtendedBlipSprite)765,
    new IconsOnScreenPosition(1246, 264)
},
{
    (ExtendedBlipSprite)783,
    new IconsOnScreenPosition(1245, 282)
},
{
    (ExtendedBlipSprite)428,
    new IconsOnScreenPosition(1240, 307)
},
{
    (ExtendedBlipSprite)822,
    new IconsOnScreenPosition(1244, 329)
},
{
    (ExtendedBlipSprite)833,
    new IconsOnScreenPosition(1246, 354)
},

{
    (ExtendedBlipSprite)8,
    new IconsOnScreenPosition(646, 377)
},
{
    (ExtendedBlipSprite)67,
    new IconsOnScreenPosition(645, 399)
},
{
    (ExtendedBlipSprite)90,
    new IconsOnScreenPosition(646, 419)
},
{
    (ExtendedBlipSprite)118,
    new IconsOnScreenPosition(645, 442)
},
{
    (ExtendedBlipSprite)147,
    new IconsOnScreenPosition(646, 464)
},
{
    (ExtendedBlipSprite)173,
    new IconsOnScreenPosition(644, 484)
},
{
    (ExtendedBlipSprite)207,
    new IconsOnScreenPosition(646, 508)
},
{
    (ExtendedBlipSprite)267,
    new IconsOnScreenPosition(643, 529)
},
{
    (ExtendedBlipSprite)306,
    new IconsOnScreenPosition(647, 552)
},
{
    (ExtendedBlipSprite)352,
    new IconsOnScreenPosition(641, 572)
},
{
    (ExtendedBlipSprite)370,
    new IconsOnScreenPosition(646, 595)
},
{
    (ExtendedBlipSprite)387,
    new IconsOnScreenPosition(643, 619)
},
{
    (ExtendedBlipSprite)420,
    new IconsOnScreenPosition(643, 640)
},
{
    (ExtendedBlipSprite)440,
    new IconsOnScreenPosition(645, 661)
},
{
    (ExtendedBlipSprite)465,
    new IconsOnScreenPosition(646, 682)
},
{
    (ExtendedBlipSprite)487,
    new IconsOnScreenPosition(647, 702)
},
{
    (ExtendedBlipSprite)16,
    new IconsOnScreenPosition(686, 378)
},
{
    (ExtendedBlipSprite)68,
    new IconsOnScreenPosition(685, 399)
},
{
    (ExtendedBlipSprite)93,
    new IconsOnScreenPosition(686, 420)
},
{
    (ExtendedBlipSprite)119,
    new IconsOnScreenPosition(685, 441)
},
{
    (ExtendedBlipSprite)149,
    new IconsOnScreenPosition(681, 465)
},
{
    (ExtendedBlipSprite)174,
    new IconsOnScreenPosition(685, 485)
},
{
    (ExtendedBlipSprite)208,
    new IconsOnScreenPosition(685, 507)
},
{
    (ExtendedBlipSprite)269,
    new IconsOnScreenPosition(685, 530)
},
{
    (ExtendedBlipSprite)307,
    new IconsOnScreenPosition(686, 551)
},
{
    (ExtendedBlipSprite)354,
    new IconsOnScreenPosition(685, 572)
},
{
    (ExtendedBlipSprite)371,
    new IconsOnScreenPosition(686, 596)
},
{
    (ExtendedBlipSprite)388,
    new IconsOnScreenPosition(683, 617)
},
{
    (ExtendedBlipSprite)421,
    new IconsOnScreenPosition(686, 640)
},
{
    (ExtendedBlipSprite)442,
    new IconsOnScreenPosition(687, 663)
},
{
    (ExtendedBlipSprite)463,
    new IconsOnScreenPosition(686, 683)
},
{
    (ExtendedBlipSprite)486,
    new IconsOnScreenPosition(685, 705)
},

{
    (ExtendedBlipSprite)36,
    new IconsOnScreenPosition(727, 378)
},
{
    (ExtendedBlipSprite)71,
    new IconsOnScreenPosition(726, 399)
},
{
    (ExtendedBlipSprite)94,
    new IconsOnScreenPosition(726, 422)
},
{
    (ExtendedBlipSprite)120,
    new IconsOnScreenPosition(726, 442)
},
{
    (ExtendedBlipSprite)150,
    new IconsOnScreenPosition(722, 463)
},
{
    (ExtendedBlipSprite)175,
    new IconsOnScreenPosition(725, 486)
},
{
    (ExtendedBlipSprite)209,
    new IconsOnScreenPosition(725, 507)
},
{
    (ExtendedBlipSprite)272,
    new IconsOnScreenPosition(726, 529)
},
{
    (ExtendedBlipSprite)308,
    new IconsOnScreenPosition(728, 552)
},
{
    (ExtendedBlipSprite)355,
    new IconsOnScreenPosition(722, 572)
},
{
    (ExtendedBlipSprite)372,
    new IconsOnScreenPosition(726, 596)
},
{
    (ExtendedBlipSprite)389,
    new IconsOnScreenPosition(721, 620)
},
{
    (ExtendedBlipSprite)445,
    new IconsOnScreenPosition(721, 660)
},
{
    (ExtendedBlipSprite)471,
    new IconsOnScreenPosition(726, 685)
},
{
    (ExtendedBlipSprite)484,
    new IconsOnScreenPosition(725, 705)
},
{
    (ExtendedBlipSprite)38,
    new IconsOnScreenPosition(767, 373)
},
{
    (ExtendedBlipSprite)72,
    new IconsOnScreenPosition(761, 399)
},
{
    (ExtendedBlipSprite)96,
    new IconsOnScreenPosition(766, 421)
},
{
    (ExtendedBlipSprite)121,
    new IconsOnScreenPosition(768, 444)
},
{
    (ExtendedBlipSprite)151,
    new IconsOnScreenPosition(766, 463)
},
{
    (ExtendedBlipSprite)176,
    new IconsOnScreenPosition(766, 484)
},
{
    (ExtendedBlipSprite)210,
    new IconsOnScreenPosition(763, 507)
},
{
    (ExtendedBlipSprite)273,
    new IconsOnScreenPosition(766, 530)
},
{
    (ExtendedBlipSprite)309,
    new IconsOnScreenPosition(767, 547)
},
{
    (ExtendedBlipSprite)356,
    new IconsOnScreenPosition(767, 574)
},
{
    (ExtendedBlipSprite)374,
    new IconsOnScreenPosition(766, 596)
},
{
    (ExtendedBlipSprite)400,
    new IconsOnScreenPosition(763, 619)
},
{
    (ExtendedBlipSprite)426,
    new IconsOnScreenPosition(766, 641)
},
{
    (ExtendedBlipSprite)446,
    new IconsOnScreenPosition(765, 661)
},
{
    (ExtendedBlipSprite)472,
    new IconsOnScreenPosition(765, 681)
},
{
    (ExtendedBlipSprite)483,
    new IconsOnScreenPosition(765, 705)
},

{
    (ExtendedBlipSprite)40,
    new IconsOnScreenPosition(805, 377)
},
{
    (ExtendedBlipSprite)73,
    new IconsOnScreenPosition(805, 399)
},
{
    (ExtendedBlipSprite)100,
    new IconsOnScreenPosition(806, 421)
},
{
    (ExtendedBlipSprite)122,
    new IconsOnScreenPosition(806, 440)
},
{
    (ExtendedBlipSprite)152,
    new IconsOnScreenPosition(804, 464)
},
{
    (ExtendedBlipSprite)181,
    new IconsOnScreenPosition(806, 485)
},
{
    (ExtendedBlipSprite)211,
    new IconsOnScreenPosition(802, 506)
},
{
    (ExtendedBlipSprite)276,
    new IconsOnScreenPosition(807, 530)
},
{
    (ExtendedBlipSprite)310,
    new IconsOnScreenPosition(806, 551)
},
{
    (ExtendedBlipSprite)357,
    new IconsOnScreenPosition(806, 574)
},
{
    (ExtendedBlipSprite)375,
    new IconsOnScreenPosition(806, 596)
},
{
    (ExtendedBlipSprite)401,
    new IconsOnScreenPosition(802, 617)
},
{
    (ExtendedBlipSprite)427,
    new IconsOnScreenPosition(806, 639)
},
{
    (ExtendedBlipSprite)455,
    new IconsOnScreenPosition(807, 661)
},
{
    (ExtendedBlipSprite)474,
    new IconsOnScreenPosition(805, 683)
},
{
    (ExtendedBlipSprite)490,
    new IconsOnScreenPosition(802, 705)
},
{
    (ExtendedBlipSprite)43,
    new IconsOnScreenPosition(846, 377)
},
{
    (ExtendedBlipSprite)75,
    new IconsOnScreenPosition(845, 398)
},
{
    (ExtendedBlipSprite)63,
    new IconsOnScreenPosition(1046, 217)
},
{
    (ExtendedBlipSprite)102,
    new IconsOnScreenPosition(847, 420)
},
{
    (ExtendedBlipSprite)123,
    new IconsOnScreenPosition(846, 441)
},
{
    (ExtendedBlipSprite)153,
    new IconsOnScreenPosition(846, 464)
},
{
    (ExtendedBlipSprite)182,
    new IconsOnScreenPosition(846, 484)
},
{
    (ExtendedBlipSprite)225,
    new IconsOnScreenPosition(845, 505)
},
{
    (ExtendedBlipSprite)277,
    new IconsOnScreenPosition(846, 528)
},
{
    (ExtendedBlipSprite)311,
    new IconsOnScreenPosition(845, 552)
},
{
    (ExtendedBlipSprite)358,
    new IconsOnScreenPosition(845, 572)
},
{
    (ExtendedBlipSprite)376,
    new IconsOnScreenPosition(845, 591)
},
{
    (ExtendedBlipSprite)402,
    new IconsOnScreenPosition(846, 617)
},
{
    (ExtendedBlipSprite)429,
    new IconsOnScreenPosition(845, 640)
},
{
    (ExtendedBlipSprite)456,
    new IconsOnScreenPosition(846, 660)
},
{
    (ExtendedBlipSprite)473,
    new IconsOnScreenPosition(847, 681)
},
{
    (ExtendedBlipSprite)491,
    new IconsOnScreenPosition(846, 703)
},
{
    (ExtendedBlipSprite)47,
    new IconsOnScreenPosition(885, 376)
},
{
    (ExtendedBlipSprite)76,
    new IconsOnScreenPosition(885, 396)
},
{
    (ExtendedBlipSprite)103,
    new IconsOnScreenPosition(887, 419)
},
{
    (ExtendedBlipSprite)124,
    new IconsOnScreenPosition(886, 443)
},
{
    (ExtendedBlipSprite)154,
    new IconsOnScreenPosition(884, 464)
},
{
    (ExtendedBlipSprite)183,
    new IconsOnScreenPosition(886, 486)
},
{
    (ExtendedBlipSprite)226,
    new IconsOnScreenPosition(886, 508)
},
{
    (ExtendedBlipSprite)278,
    new IconsOnScreenPosition(885, 529)
},
{
    (ExtendedBlipSprite)313,
    new IconsOnScreenPosition(885, 550)
},
{
    (ExtendedBlipSprite)359,
    new IconsOnScreenPosition(887, 572)
},
{
    (ExtendedBlipSprite)377,
    new IconsOnScreenPosition(886, 595)
},
{
    (ExtendedBlipSprite)403,
    new IconsOnScreenPosition(885, 616)
},
{
    (ExtendedBlipSprite)430,
    new IconsOnScreenPosition(887, 641)
},
{
    (ExtendedBlipSprite)457,
    new IconsOnScreenPosition(886, 661)
},
{
    (ExtendedBlipSprite)476,
    new IconsOnScreenPosition(886, 681)
},
{
    (ExtendedBlipSprite)492,
    new IconsOnScreenPosition(883, 705)
},
{
    (ExtendedBlipSprite)50,
    new IconsOnScreenPosition(927, 377)
},
{
    (ExtendedBlipSprite)77,
    new IconsOnScreenPosition(919, 399)
},
{
    (ExtendedBlipSprite)104,
    new IconsOnScreenPosition(926, 420)
},
{
    (ExtendedBlipSprite)126,
    new IconsOnScreenPosition(925, 442)
},
{
    (ExtendedBlipSprite)155,
    new IconsOnScreenPosition(924, 463)
},
{
    (ExtendedBlipSprite)184,
    new IconsOnScreenPosition(926, 486)
},
{
    (ExtendedBlipSprite)227,
    new IconsOnScreenPosition(925, 507)
},
{
    (ExtendedBlipSprite)279,
    new IconsOnScreenPosition(927, 529)
},
{
    (ExtendedBlipSprite)314,
    new IconsOnScreenPosition(926, 547)
},
{
    (ExtendedBlipSprite)360,
    new IconsOnScreenPosition(926, 574)
},
{
    (ExtendedBlipSprite)378,
    new IconsOnScreenPosition(925, 594)
},
{
    (ExtendedBlipSprite)404,
    new IconsOnScreenPosition(925, 618)
},
{
    (ExtendedBlipSprite)431,
    new IconsOnScreenPosition(926, 639)
},
{
    (ExtendedBlipSprite)459,
    new IconsOnScreenPosition(924, 661)
},
{
    (ExtendedBlipSprite)475,
    new IconsOnScreenPosition(925, 681)
},
{
    (ExtendedBlipSprite)494,
    new IconsOnScreenPosition(926, 702)
},

{
    (ExtendedBlipSprite)51,
    new IconsOnScreenPosition(965, 375)
},
{
    (ExtendedBlipSprite)78,
    new IconsOnScreenPosition(966, 401)
},
{
    (ExtendedBlipSprite)105,
    new IconsOnScreenPosition(967, 420)
},
{
    (ExtendedBlipSprite)127,
    new IconsOnScreenPosition(964, 438)
},
{
    (ExtendedBlipSprite)156,
    new IconsOnScreenPosition(962, 464)
},
{
    (ExtendedBlipSprite)186,
    new IconsOnScreenPosition(962, 485)
},
{
    (ExtendedBlipSprite)229,
    new IconsOnScreenPosition(968, 507)
},
{
    (ExtendedBlipSprite)280,
    new IconsOnScreenPosition(966, 529)
},
{
    (ExtendedBlipSprite)315,
    new IconsOnScreenPosition(965, 548)
},
{
    (ExtendedBlipSprite)361,
    new IconsOnScreenPosition(967, 574)
},
{
    (ExtendedBlipSprite)379,
    new IconsOnScreenPosition(963, 591)
},
{
    (ExtendedBlipSprite)405,
    new IconsOnScreenPosition(960, 618)
},
{
    (ExtendedBlipSprite)432,
    new IconsOnScreenPosition(967, 639)
},
{
    (ExtendedBlipSprite)458,
    new IconsOnScreenPosition(965, 660)
},
{
    (ExtendedBlipSprite)477,
    new IconsOnScreenPosition(966, 681)
},
{
    (ExtendedBlipSprite)499,
    new IconsOnScreenPosition(966, 704)
},
{
    (ExtendedBlipSprite)52,
    new IconsOnScreenPosition(1006, 377)
},
{
    (ExtendedBlipSprite)79,
    new IconsOnScreenPosition(1006, 399)
},
{
    (ExtendedBlipSprite)106,
    new IconsOnScreenPosition(1005, 420)
},
{
    (ExtendedBlipSprite)133,
    new IconsOnScreenPosition(1004, 441)
},
{
    (ExtendedBlipSprite)157,
    new IconsOnScreenPosition(1005, 463)
},
{
    (ExtendedBlipSprite)187,
    new IconsOnScreenPosition(1003, 485)
},
{
    (ExtendedBlipSprite)237,
    new IconsOnScreenPosition(1006, 508)
},
{
    (ExtendedBlipSprite)285,
    new IconsOnScreenPosition(1005, 528)
},
{
    (ExtendedBlipSprite)316,
    new IconsOnScreenPosition(1004, 548)
},
{
    (ExtendedBlipSprite)362,
    new IconsOnScreenPosition(1005, 572)
},
{
    (ExtendedBlipSprite)380,
    new IconsOnScreenPosition(1010, 598)
},
{
    (ExtendedBlipSprite)408,
    new IconsOnScreenPosition(1005, 617)
},
{
    (ExtendedBlipSprite)433,
    new IconsOnScreenPosition(1006, 638)
},
{
    (ExtendedBlipSprite)460,
    new IconsOnScreenPosition(1005, 661)
},
{
    (ExtendedBlipSprite)478,
    new IconsOnScreenPosition(1005, 682)
},
{
    (ExtendedBlipSprite)496,
    new IconsOnScreenPosition(1006, 705)
},
{
    (ExtendedBlipSprite)56,
    new IconsOnScreenPosition(1046, 378)
},
{
    (ExtendedBlipSprite)80,
    new IconsOnScreenPosition(1046, 398)
},
{
    (ExtendedBlipSprite)107,
    new IconsOnScreenPosition(1046, 421)
},
{
    (ExtendedBlipSprite)134,
    new IconsOnScreenPosition(1043, 442)
},
{
    (ExtendedBlipSprite)158,
    new IconsOnScreenPosition(1042, 463)
},
{
    (ExtendedBlipSprite)188,
    new IconsOnScreenPosition(1043, 483)
},
{
    (ExtendedBlipSprite)238,
    new IconsOnScreenPosition(1045, 507)
},
{
    (ExtendedBlipSprite)289,
    new IconsOnScreenPosition(1044, 530)
},
{
    (ExtendedBlipSprite)317,
    new IconsOnScreenPosition(1043, 550)
},
{
    (ExtendedBlipSprite)363,
    new IconsOnScreenPosition(1041, 574)
},
{
    (ExtendedBlipSprite)381,
    new IconsOnScreenPosition(1039, 599)
},
{
    (ExtendedBlipSprite)409,
    new IconsOnScreenPosition(1045, 616)
},
{
    (ExtendedBlipSprite)434,
    new IconsOnScreenPosition(1044, 636)
},
{
    (ExtendedBlipSprite)461,
    new IconsOnScreenPosition(1046, 662)
},
{
    (ExtendedBlipSprite)479,
    new IconsOnScreenPosition(1042, 682)
},
{
    (ExtendedBlipSprite)500,
    new IconsOnScreenPosition(1044, 702)
},
{
    (ExtendedBlipSprite)59,
    new IconsOnScreenPosition(1085, 377)
},
{
    (ExtendedBlipSprite)84,
    new IconsOnScreenPosition(1085, 400)
},
{
    (ExtendedBlipSprite)108,
    new IconsOnScreenPosition(1086, 420)
},
{
    (ExtendedBlipSprite)135,
    new IconsOnScreenPosition(1083, 444)
},
{
    (ExtendedBlipSprite)159,
    new IconsOnScreenPosition(1083, 463)
},
{
    (ExtendedBlipSprite)189,
    new IconsOnScreenPosition(1084, 483)
},
{
    (ExtendedBlipSprite)251,
    new IconsOnScreenPosition(1085, 507)
},
{
    (ExtendedBlipSprite)290,
    new IconsOnScreenPosition(1084, 528)
},
{
    (ExtendedBlipSprite)318,
    new IconsOnScreenPosition(1085, 549)
},
{
    (ExtendedBlipSprite)365,
    new IconsOnScreenPosition(1085, 573)
},
{
    (ExtendedBlipSprite)382,
    new IconsOnScreenPosition(1083, 597)
},
{
    (ExtendedBlipSprite)410,
    new IconsOnScreenPosition(1086, 619)
},
{
    (ExtendedBlipSprite)435,
    new IconsOnScreenPosition(1084, 633)
},
{
    (ExtendedBlipSprite)467,
    new IconsOnScreenPosition(1085, 661)
},
{
    (ExtendedBlipSprite)480,
    new IconsOnScreenPosition(1085, 683)
},
{
    (ExtendedBlipSprite)497,
    new IconsOnScreenPosition(1086, 703)
},

{
    (ExtendedBlipSprite)60,
    new IconsOnScreenPosition(1125, 377)
},
{
    (ExtendedBlipSprite)85,
    new IconsOnScreenPosition(1124, 399)
},
{
    (ExtendedBlipSprite)109,
    new IconsOnScreenPosition(1118, 418)
},
{
    (ExtendedBlipSprite)136,
    new IconsOnScreenPosition(1125, 440)
},
{
    (ExtendedBlipSprite)160,
    new IconsOnScreenPosition(1121, 464)
},
{
    (ExtendedBlipSprite)197,
    new IconsOnScreenPosition(1125, 485)
},
{
    (ExtendedBlipSprite)252,
    new IconsOnScreenPosition(1124, 507)
},
{
    (ExtendedBlipSprite)291,
    new IconsOnScreenPosition(1126, 527)
},
{
    (ExtendedBlipSprite)326,
    new IconsOnScreenPosition(1125, 551)
},
{
    (ExtendedBlipSprite)366,
    new IconsOnScreenPosition(1127, 572)
},
{
    (ExtendedBlipSprite)383,
    new IconsOnScreenPosition(1121, 600)
},
{
    (ExtendedBlipSprite)411,
    new IconsOnScreenPosition(1126, 617)
},
{
    (ExtendedBlipSprite)441,
    new IconsOnScreenPosition(1125, 638)
},
{
    (ExtendedBlipSprite)469,
    new IconsOnScreenPosition(1126, 661)
},
{
    (ExtendedBlipSprite)481,
    new IconsOnScreenPosition(1126, 681)
},
{
    (ExtendedBlipSprite)498,
    new IconsOnScreenPosition(1126, 701)
},
{
    (ExtendedBlipSprite)61,
    new IconsOnScreenPosition(1166, 377)
},
{
    (ExtendedBlipSprite)86,
    new IconsOnScreenPosition(1160, 397)
},
{
    (ExtendedBlipSprite)110,
    new IconsOnScreenPosition(1162, 418)
},
{
    (ExtendedBlipSprite)137,
    new IconsOnScreenPosition(1168, 440)
},
{
    (ExtendedBlipSprite)162,
    new IconsOnScreenPosition(1166, 464)
},
{
    (ExtendedBlipSprite)198,
    new IconsOnScreenPosition(1164, 482)
},
{
    (ExtendedBlipSprite)253,
    new IconsOnScreenPosition(1166, 508)
},
{
    (ExtendedBlipSprite)293,
    new IconsOnScreenPosition(1166, 530)
},
{
    (ExtendedBlipSprite)348,
    new IconsOnScreenPosition(1164, 550)
},
{
    (ExtendedBlipSprite)367,
    new IconsOnScreenPosition(1157, 570)
},
{
    (ExtendedBlipSprite)384,
    new IconsOnScreenPosition(1161, 598)
},
{
    (ExtendedBlipSprite)414,
    new IconsOnScreenPosition(1167, 619)
},
{
    (ExtendedBlipSprite)437,
    new IconsOnScreenPosition(1164, 639)
},
{
    (ExtendedBlipSprite)468,
    new IconsOnScreenPosition(1164, 660)
},
{
    (ExtendedBlipSprite)488,
    new IconsOnScreenPosition(1160, 684)
},
{
    (ExtendedBlipSprite)501,
    new IconsOnScreenPosition(1166, 706)
},
{
    (ExtendedBlipSprite)64,
    new IconsOnScreenPosition(1206, 378)
},
{
    (ExtendedBlipSprite)88,
    new IconsOnScreenPosition(1204, 400)
},
{
    (ExtendedBlipSprite)112,
    new IconsOnScreenPosition(1207, 418)
},
{
    (ExtendedBlipSprite)140,
    new IconsOnScreenPosition(1206, 442)
},
{
    (ExtendedBlipSprite)163,
    new IconsOnScreenPosition(1205, 463)
},
{
    (ExtendedBlipSprite)205,
    new IconsOnScreenPosition(1206, 485)
},
{
    (ExtendedBlipSprite)255,
    new IconsOnScreenPosition(1205, 506)
},
{
    (ExtendedBlipSprite)304,
    new IconsOnScreenPosition(1203, 531)
},
{
    (ExtendedBlipSprite)350,
    new IconsOnScreenPosition(1206, 549)
},
{
    (ExtendedBlipSprite)368,
    new IconsOnScreenPosition(1206, 574)
},
{
    (ExtendedBlipSprite)385,
    new IconsOnScreenPosition(1204, 597)
},
{
    (ExtendedBlipSprite)415,
    new IconsOnScreenPosition(1206, 619)
},
{
    (ExtendedBlipSprite)439,
    new IconsOnScreenPosition(1205, 639)
},
{
    (ExtendedBlipSprite)464,
    new IconsOnScreenPosition(1206, 661)
},
{
    (ExtendedBlipSprite)489,
    new IconsOnScreenPosition(1206, 685)
},
{
    (ExtendedBlipSprite)493,
    new IconsOnScreenPosition(1206, 703)
},
{
    (ExtendedBlipSprite)66,
    new IconsOnScreenPosition(1249, 375)
},
{
    (ExtendedBlipSprite)89,
    new IconsOnScreenPosition(1245, 399)
},
{
    (ExtendedBlipSprite)113,
    new IconsOnScreenPosition(1244, 420)
},
{
    (ExtendedBlipSprite)141,
    new IconsOnScreenPosition(1247, 443)
},
{
    (ExtendedBlipSprite)164,
    new IconsOnScreenPosition(1244, 465)
},
{
    (ExtendedBlipSprite)206,
    new IconsOnScreenPosition(1244, 485)
},
{
    (ExtendedBlipSprite)266,
    new IconsOnScreenPosition(1246, 507)
},
{
    (ExtendedBlipSprite)305,
    new IconsOnScreenPosition(1247, 528)
},
{
    (ExtendedBlipSprite)351,
    new IconsOnScreenPosition(1246, 551)
},
{
    (ExtendedBlipSprite)369,
    new IconsOnScreenPosition(1246, 575)
},
{
    (ExtendedBlipSprite)386,
    new IconsOnScreenPosition(1241, 598)
},
{
    (ExtendedBlipSprite)419,
    new IconsOnScreenPosition(1243, 615)
},
{
    (ExtendedBlipSprite)436,
    new IconsOnScreenPosition(1245, 641)
},
{
    (ExtendedBlipSprite)466,
    new IconsOnScreenPosition(1246, 662)
},
{
    (ExtendedBlipSprite)485,
    new IconsOnScreenPosition(1245, 688)
},
{
    (ExtendedBlipSprite)495,
    new IconsOnScreenPosition(1246, 708)
},

{
    (ExtendedBlipSprite)829,
    new IconsOnScreenPosition(27, 413)
},
{
    (ExtendedBlipSprite)852,
    new IconsOnScreenPosition(28, 445)
},
{
    (ExtendedBlipSprite)870,
    new IconsOnScreenPosition(28, 475)
},
{
    (ExtendedBlipSprite)888,
    new IconsOnScreenPosition(27, 503)
},
{
    (ExtendedBlipSprite)912,
    new IconsOnScreenPosition(28, 530)
},
{
    (ExtendedBlipSprite)834,
    new IconsOnScreenPosition(60, 416)
},
{
    (ExtendedBlipSprite)853,
    new IconsOnScreenPosition(60, 446)
},
{
    (ExtendedBlipSprite)871,
    new IconsOnScreenPosition(60, 473)
},
{
    (ExtendedBlipSprite)889,
    new IconsOnScreenPosition(58, 503)
},
{
    (ExtendedBlipSprite)913,
    new IconsOnScreenPosition(60, 532)
},
{
    (ExtendedBlipSprite)830,
    new IconsOnScreenPosition(93, 417)
},
{
    (ExtendedBlipSprite)854,
    new IconsOnScreenPosition(93, 444)
},
{
    (ExtendedBlipSprite)872,
    new IconsOnScreenPosition(93, 476)
},
{
    (ExtendedBlipSprite)890,
    new IconsOnScreenPosition(93, 502)
},
{
    (ExtendedBlipSprite)914,
    new IconsOnScreenPosition(94, 529)
},
{
    (ExtendedBlipSprite)831,
    new IconsOnScreenPosition(125, 418)
},
{
    (ExtendedBlipSprite)857,
    new IconsOnScreenPosition(125, 445)
},
{
    (ExtendedBlipSprite)873,
    new IconsOnScreenPosition(125, 474)
},
{
    (ExtendedBlipSprite)899,
    new IconsOnScreenPosition(124, 500)
},
{
    (ExtendedBlipSprite)828,
    new IconsOnScreenPosition(166, 410)
},
{
    (ExtendedBlipSprite)860,
    new IconsOnScreenPosition(156, 446)
},
{
    (ExtendedBlipSprite)874,
    new IconsOnScreenPosition(157, 470)
},
{
    (ExtendedBlipSprite)900,
    new IconsOnScreenPosition(158, 504)
},
{
    (ExtendedBlipSprite)915,
    new IconsOnScreenPosition(155, 529)
},
{
    (ExtendedBlipSprite)841,
    new IconsOnScreenPosition(196, 409)
},
{
    (ExtendedBlipSprite)859,
    new IconsOnScreenPosition(189, 444)
},
{
    (ExtendedBlipSprite)875,
    new IconsOnScreenPosition(189, 477)
},
{
    (ExtendedBlipSprite)901,
    new IconsOnScreenPosition(190, 502)
},
{
    (ExtendedBlipSprite)916,
    new IconsOnScreenPosition(189, 527)
},
{
    (ExtendedBlipSprite)847,
    new IconsOnScreenPosition(224, 417)
},
{
    (ExtendedBlipSprite)861,
    new IconsOnScreenPosition(224, 445)
},
{
    (ExtendedBlipSprite)876,
    new IconsOnScreenPosition(222, 468)
},
{
    (ExtendedBlipSprite)902,
    new IconsOnScreenPosition(223, 504)
},
{
    (ExtendedBlipSprite)917,
    new IconsOnScreenPosition(223, 527)
},
{
    (ExtendedBlipSprite)839,
    new IconsOnScreenPosition(255, 414)
},
{
    (ExtendedBlipSprite)858,
    new IconsOnScreenPosition(255, 443)
},
{
    (ExtendedBlipSprite)877,
    new IconsOnScreenPosition(255, 469)
},
{
    (ExtendedBlipSprite)903,
    new IconsOnScreenPosition(256, 502)
},
{
    (ExtendedBlipSprite)918,
    new IconsOnScreenPosition(254, 530)
},
{
    (ExtendedBlipSprite)862,
    new IconsOnScreenPosition(288, 446)
},
{
    (ExtendedBlipSprite)878,
    new IconsOnScreenPosition(283, 473)
},
{
    (ExtendedBlipSprite)904,
    new IconsOnScreenPosition(288, 502)
},
{
    (ExtendedBlipSprite)846,
    new IconsOnScreenPosition(319, 417)
},
{
    (ExtendedBlipSprite)864,
    new IconsOnScreenPosition(326, 438)
},
{
    (ExtendedBlipSprite)879,
    new IconsOnScreenPosition(320, 473)
},
{
    (ExtendedBlipSprite)905,
    new IconsOnScreenPosition(320, 504)
},
{
    (ExtendedBlipSprite)845,
    new IconsOnScreenPosition(353, 415)
},
{
    (ExtendedBlipSprite)863,
    new IconsOnScreenPosition(362, 437)
},
{
    (ExtendedBlipSprite)880,
    new IconsOnScreenPosition(352, 472)
},
{
    (ExtendedBlipSprite)906,
    new IconsOnScreenPosition(351, 508)
},
{
    (ExtendedBlipSprite)842,
    new IconsOnScreenPosition(385, 416)
},
{
    (ExtendedBlipSprite)865,
    new IconsOnScreenPosition(384, 442)
},
{
    (ExtendedBlipSprite)882,
    new IconsOnScreenPosition(386, 467)
},
{
    (ExtendedBlipSprite)883,
    new IconsOnScreenPosition(384, 479)
},

{
    (ExtendedBlipSprite)910,
    new IconsOnScreenPosition(384, 506)
},
{
    (ExtendedBlipSprite)844,
    new IconsOnScreenPosition(418, 414)
},
{
    (ExtendedBlipSprite)866,
    new IconsOnScreenPosition(417, 445)
},
{
    (ExtendedBlipSprite)893,
    new IconsOnScreenPosition(416, 471)
},
{
    (ExtendedBlipSprite)908,
    new IconsOnScreenPosition(417, 506)
},
{
    (ExtendedBlipSprite)843,
    new IconsOnScreenPosition(450, 412)
},
{
    (ExtendedBlipSprite)867,
    new IconsOnScreenPosition(450, 447)
},
{
    (ExtendedBlipSprite)885,
    new IconsOnScreenPosition(450, 471)
},
{
    (ExtendedBlipSprite)907,
    new IconsOnScreenPosition(450, 506)
},
{
    (ExtendedBlipSprite)850,
    new IconsOnScreenPosition(482, 421)
},
{
    (ExtendedBlipSprite)868,
    new IconsOnScreenPosition(483, 447)
},
{
    (ExtendedBlipSprite)886,
    new IconsOnScreenPosition(483, 473)
},
{
    (ExtendedBlipSprite)909,
    new IconsOnScreenPosition(482, 506)
},
{
    (ExtendedBlipSprite)851,
    new IconsOnScreenPosition(514, 415)
},
{
    (ExtendedBlipSprite)869,
    new IconsOnScreenPosition(521, 439)
},
{
    (ExtendedBlipSprite)887,
    new IconsOnScreenPosition(515, 473)
},
{
    (ExtendedBlipSprite)911,
    new IconsOnScreenPosition(514, 503)
},
};

        private Scaleform _scaleform;

        public struct IconsOnScreenPosition
        {
            public int X;
            public int Y;

            public IconsOnScreenPosition(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        private static readonly Dictionary<ExtendedBlipColor, Color> _blipsColors = new Dictionary<ExtendedBlipColor, Color>()
        {
            { (ExtendedBlipColor)1,  ColorTranslator.FromHtml("#e03233")},
            { (ExtendedBlipColor)2,  ColorTranslator.FromHtml("#72cc72")},
            { (ExtendedBlipColor)3,  ColorTranslator.FromHtml("#5cb6e8")},
            { (ExtendedBlipColor)4,  ColorTranslator.FromHtml("#f0f0f0")},
            { (ExtendedBlipColor)5,  ColorTranslator.FromHtml("#f0c84f")},
            { (ExtendedBlipColor)6,  ColorTranslator.FromHtml("#c25050")},
            { (ExtendedBlipColor)7,  ColorTranslator.FromHtml("#9c6eaf")},
            { (ExtendedBlipColor)8,  ColorTranslator.FromHtml("#ff7bc4")},
            { (ExtendedBlipColor)9,  ColorTranslator.FromHtml("#f79f7b")},
            { (ExtendedBlipColor)10, ColorTranslator.FromHtml("#b29084")},

            { (ExtendedBlipColor)11,  ColorTranslator.FromHtml("#8dcea6")},
            { (ExtendedBlipColor)12,  ColorTranslator.FromHtml("#72a9ae")},
            { (ExtendedBlipColor)13,  ColorTranslator.FromHtml("#d3d1e7")},
            { (ExtendedBlipColor)14,  ColorTranslator.FromHtml("#8f7f99")},
            { (ExtendedBlipColor)15,  ColorTranslator.FromHtml("#6ac5c0")},
            { (ExtendedBlipColor)16,  ColorTranslator.FromHtml("#d5c498")},
            { (ExtendedBlipColor)17,  ColorTranslator.FromHtml("#ea8e4f")},
            { (ExtendedBlipColor)18,  ColorTranslator.FromHtml("#98cbea")},
            { (ExtendedBlipColor)19,  ColorTranslator.FromHtml("#b26287")},
            { (ExtendedBlipColor)20, ColorTranslator.FromHtml("#908d7a")},

            { (ExtendedBlipColor)21,  ColorTranslator.FromHtml("#a5755e")},
            { (ExtendedBlipColor)22,  ColorTranslator.FromHtml("#b0a7a8")},
            { (ExtendedBlipColor)23,  ColorTranslator.FromHtml("#e88e9a")},
            { (ExtendedBlipColor)24,  ColorTranslator.FromHtml("#bcd65b")},
            { (ExtendedBlipColor)25,  ColorTranslator.FromHtml("#0d7b56")},
            { (ExtendedBlipColor)26,  ColorTranslator.FromHtml("#7cc4ff")},
            { (ExtendedBlipColor)27,  ColorTranslator.FromHtml("#ac3ce6")},
            { (ExtendedBlipColor)28,  ColorTranslator.FromHtml("#cda90d")},
            { (ExtendedBlipColor)29,  ColorTranslator.FromHtml("#4763ad")},
            { (ExtendedBlipColor)30, ColorTranslator.FromHtml("#29a6b8")},

            { (ExtendedBlipColor)31,  ColorTranslator.FromHtml("#ba9d7d")},
            { (ExtendedBlipColor)32,  ColorTranslator.FromHtml("#c9e0ff")},
            { (ExtendedBlipColor)33,  ColorTranslator.FromHtml("#f0f096")},
            { (ExtendedBlipColor)34,  ColorTranslator.FromHtml("#ed8ca0")},
            { (ExtendedBlipColor)35,  ColorTranslator.FromHtml("#fa8a89")},
            { (ExtendedBlipColor)36,  ColorTranslator.FromHtml("#fcf0a6")},
            { (ExtendedBlipColor)37,  ColorTranslator.FromHtml("#f0f0f0")},
            { (ExtendedBlipColor)38,  ColorTranslator.FromHtml("#2c6eb8")},
            { (ExtendedBlipColor)39,  ColorTranslator.FromHtml("#9a9a9a")},
            { (ExtendedBlipColor)40, ColorTranslator.FromHtml("#4d4d4d")},

            { (ExtendedBlipColor)41,  ColorTranslator.FromHtml("#f19998")},
            { (ExtendedBlipColor)42,  ColorTranslator.FromHtml("#65b4d3")},
            { (ExtendedBlipColor)43,  ColorTranslator.FromHtml("#abeeab")},
            { (ExtendedBlipColor)44,  ColorTranslator.FromHtml("#ffa356")},
            { (ExtendedBlipColor)45,  ColorTranslator.FromHtml("#f0f0f0")},
            { (ExtendedBlipColor)46,  ColorTranslator.FromHtml("#ebef1e")},
            { (ExtendedBlipColor)47,  ColorTranslator.FromHtml("#ff950e")},
            { (ExtendedBlipColor)48,  ColorTranslator.FromHtml("#f63ca1")},
            { (ExtendedBlipColor)49,  ColorTranslator.FromHtml("#e03233")},
            { (ExtendedBlipColor)50, ColorTranslator.FromHtml("#8466e2")},

            { (ExtendedBlipColor)51,  ColorTranslator.FromHtml("#ff8554")},
            { (ExtendedBlipColor)52,  ColorTranslator.FromHtml("#386638")},
            { (ExtendedBlipColor)53,  ColorTranslator.FromHtml("#aedbf2")},
            { (ExtendedBlipColor)54,  ColorTranslator.FromHtml("#2f5c73")},
            { (ExtendedBlipColor)55,  ColorTranslator.FromHtml("#9b9b9b")},
            { (ExtendedBlipColor)56,  ColorTranslator.FromHtml("#7e6b29")},
            { (ExtendedBlipColor)57,  ColorTranslator.FromHtml("#5eb6e6")},
            { (ExtendedBlipColor)58,  ColorTranslator.FromHtml("#43396e")},
            { (ExtendedBlipColor)59,  ColorTranslator.FromHtml("#e03233")},
            { (ExtendedBlipColor)60, ColorTranslator.FromHtml("#f0c84f")},

            { (ExtendedBlipColor)61,  ColorTranslator.FromHtml("#cb3694")},
            { (ExtendedBlipColor)62,  ColorTranslator.FromHtml("#cdcdcd")},
            { (ExtendedBlipColor)63,  ColorTranslator.FromHtml("#1d6498")},
            { (ExtendedBlipColor)64,  ColorTranslator.FromHtml("#d6740f")},
            { (ExtendedBlipColor)65,  ColorTranslator.FromHtml("#887d8e")},
            { (ExtendedBlipColor)66,  ColorTranslator.FromHtml("#f0c84f")},
            { (ExtendedBlipColor)67,  ColorTranslator.FromHtml("#5eb6e6")},
            { (ExtendedBlipColor)68,  ColorTranslator.FromHtml("#5eb6e6")},
            { (ExtendedBlipColor)69,  ColorTranslator.FromHtml("#72cc72")},
            { (ExtendedBlipColor)70, ColorTranslator.FromHtml("#f0c84f")},

            { (ExtendedBlipColor)71,  ColorTranslator.FromHtml("#f0c84f")},
            { (ExtendedBlipColor)72,  ColorTranslator.FromHtml("#2a2a22")},
            { (ExtendedBlipColor)73,  ColorTranslator.FromHtml("#f0c84f")},
            { (ExtendedBlipColor)74,  ColorTranslator.FromHtml("#5eb6e6")},
            { (ExtendedBlipColor)75,  ColorTranslator.FromHtml("#e03233")},
            { (ExtendedBlipColor)76,  ColorTranslator.FromHtml("#711918")},
            { (ExtendedBlipColor)77,  ColorTranslator.FromHtml("#5eb6e6")},
            { (ExtendedBlipColor)78,  ColorTranslator.FromHtml("#2f5c73")},
            { (ExtendedBlipColor)79,  ColorTranslator.FromHtml("#522f29")},
            { (ExtendedBlipColor)80, ColorTranslator.FromHtml("#414f4f")},

            { (ExtendedBlipColor)81,  ColorTranslator.FromHtml("#f0a001")},
            { (ExtendedBlipColor)82,  ColorTranslator.FromHtml("#9fc8a6")},
            { (ExtendedBlipColor)83,  ColorTranslator.FromHtml("#a44bf1")},
            { (ExtendedBlipColor)84,  ColorTranslator.FromHtml("#5eb6e6")},
            { (ExtendedBlipColor)85,  ColorTranslator.FromHtml("#222419")},
        };

        public Main()
        {
            _iniParser = new FileIniDataParser();

            _uiMenuPool = new ObjectPool();
            Tick += OnTick;  
            KeyDown += OnKeyDown;  
            Aborted += OnAborted;  
            Interval = _defaultInterval;

            
            if (!_hasLoadedBlips)
            {
                ParseIniAndCreateBlips();
                _hasLoadedBlips = true;
            }
            InitializeAllMenus();
        }


        
        private void OnAborted(object sender, EventArgs e)  
        {
            
            DeleteAllGameBlips();
            _hasLoadedBlips = false;
            _blipCount = 0;  
        }
        private void OnTick(object sender, EventArgs e)  
        {
            
            bool needsFastInterval = _uiMenuPool.AreAnyVisible || 
                                     _showCoordsOnScreen ||         
                                     _waitingForKeyAssignment ||
                                     Function.Call<int>(Hash.UPDATE_ONSCREEN_KEYBOARD) == 0; 

            
            Interval = needsFastInterval ? _activeInterval : _defaultInterval;

            bool isRightClickPressed = Game.IsControlJustPressed(GTA.Control.CursorAccept);
            if (isRightClickPressed)
            {
                _addBlipMenu.AcceptsInput = false;
            }
            else
            {
                _addBlipMenu.AcceptsInput = true;
            }

            _uiMenuPool.Process();

            if (_addBlipMenu.SelectedIndex == 1 || _addBlipMenu.SelectedIndex == 2)
            {
                float ScreenWidth = GTA.UI.Screen.Width;
                float ScreenHeight = GTA.UI.Screen.Height;

                Color color = TryGetColor(_colorItem);

                Sprite sprite = new Sprite("minimap", "blips_texturesheet_ng_2", new SizeF(640f, 360f), new PointF(ScreenWidth * 0.49f, 5), color);
                sprite.Draw();

                Sprite sprite1 = new Sprite("minimap", "blips_texturesheet_ng", new SizeF(640f, 350f), new PointF(ScreenWidth * 0.49f, ScreenHeight * 0.51f), color);
                sprite1.Draw();

                Sprite sprite2 = new Sprite("minimap", "blips_texturesheet_ng_3", new SizeF(520f, 230f), new PointF(ScreenWidth * 0.01f, ScreenHeight * 0.56f), color);
                sprite2.Draw();

                _scaleform = Scaleform.RequestMovie("instructional_buttons");
                _scaleform.CallFunction("CLEAR_ALL");
                Function.Call(Hash.DRAW_SCALEFORM_MOVIE_FULLSCREEN, _scaleform.Handle, 255, 255, 255, 255, 0);

                float cx = Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, (int)GTA.Control.CursorX);
                float cy = Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, (int)GTA.Control.CursorY);

                
                int screenW = (int)ScreenWidth;
                int screenH = (int)ScreenHeight;
                int px = (int)(cx * screenW);
                int py = (int)(cy * screenH);

                if (isRightClickPressed)
                {
                    if (TryFindClosestIconWithinRadius(px, py, out ExtendedBlipSprite clickedIcon))
                    {
                        if (_iconItem != null)
                        {
                            Array enumValues = Enum.GetValues(typeof(ExtendedBlipSprite));
                            int newIndex = Array.IndexOf(enumValues, clickedIcon);

                            _iconItem.SelectedIndex = newIndex;

                            UpdatePreviewBlip(_iconItem, _colorItem, _sizeItem, _flashItem, _flashIntervalItem, _shortRangeItem);
                        }
                    }
                }
            }

            if (_waitingForKeyAssignment == true)
            {
                Game.DisableAllControlsThisFrame();
            }

            Player player = Game.Player;
            if (!IsValidPlayer(player))
            {
                return;
            }

            Vector3 playerCoordinates = GetPlayerCoordinates(player);

            if (_showCoordsOnScreen)
            {
                
                Function.Call(Hash.BEGIN_TEXT_COMMAND_PRINT, "CELL_EMAIL_BCON");
                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, $"~b~X: ~w~{playerCoordinates.X:0.00} ~b~Y: ~w~{playerCoordinates.Y:0.00} ~b~Z: ~w~{playerCoordinates.Z:0.00}");
                Function.Call(Hash.END_TEXT_COMMAND_PRINT);
            }
        }
        private Color TryGetColor(NativeListItem<string> colorItem)
        {
            if (colorItem.SelectedIndex == 0)
            {
                return Color.White;
            }

            if (_blipsColors.TryGetValue((ExtendedBlipColor)colorItem.SelectedIndex, out Color color))
            {
                return color;
            }

            return Color.White;
        }
        private void OnKeyDown(object sender, KeyEventArgs e)  
        {
            if (_waitingForKeyAssignment && e.KeyCode != Keys.None && e.KeyCode != Keys.Enter)
            {
                _waitingForKeyAssignment = false;

                
                switch (_pendingKeyBind)
                {
                    case "Toggle_Coordinates_Key":
                        _keyToggleCoords = e.KeyCode;
                        break;
                    case "Reload_Blips_Key":
                        _keyReloadBlips = e.KeyCode;
                        break;
                    case "Toggle_Blips_Visibility_Key":
                        _keyToggleBlipVisibility = e.KeyCode;
                        break;
                    case "Open_Menu_Key":
                        _keyOpenMenu = e.KeyCode;
                        break;
                }

                
                if (_pendingListItem != null)
                {
                    List<string> keysList = Enum.GetNames(typeof(Keys)).ToList();
                    int newIndex = keysList.IndexOf(e.KeyCode.ToString());
                    if (newIndex >= 0)
                        _pendingListItem.SelectedIndex = newIndex;

                    _pendingListItem = null;
                }

                
                Dictionary<string, string> kvp = new Dictionary<string, string>
                {
                    ["Toggle_Coordinates_Key"] = _keyToggleCoords.ToString(),
                    ["Reload_Blips_Key"] = _keyReloadBlips.ToString(),
                    ["Toggle_Blips_Visibility_Key"] = _keyToggleBlipVisibility.ToString(),
                    ["Open_Menu_Key"] = _keyOpenMenu.ToString(),
                    ["Show_Blip_Added_Notification"] = _showBlipLoadNotification ? "ON" : "OFF",
                    ["Enable_AddOn_Blips"] = _enableAddOnBlips ? "ON" : "OFF"
                };
                SaveToIniAndNotify("Settings", kvp, "~g~Key assigned and saved.");
                return; 
            }

            
            if (_keyToggleCoords.HasValue && e.KeyCode == _keyToggleCoords.Value)
            {
                _showCoordsOnScreen = !_showCoordsOnScreen;
                Notification.PostTicker(_showCoordsOnScreen ? "Coordinates display enabled." : "Coordinates display disabled.", true);
            }

            
            if (_keyReloadBlips.HasValue && e.KeyCode == _keyReloadBlips.Value)
            {
                ParseIniAndCreateBlips();
                Notification.PostTicker("Advanced Custom Blips Reloaded.", true);
            }

            
            if (_keyToggleBlipVisibility.HasValue && e.KeyCode == _keyToggleBlipVisibility.Value)
            {
                ToggleBlipsVisibility();
            }

            
            if (_keyOpenMenu.HasValue && e.KeyCode == _keyOpenMenu.Value && !_uiMenuPool.AreAnyVisible)
            {
                _mainMenu.Visible = !_mainMenu.Visible;
            }
        }

        private static bool TryFindClosestIconWithinRadius(int clickX, int clickY, out ExtendedBlipSprite foundKey)
        {
            foundKey = default;

            int radiusSq = 8 * 8;
            long bestD2 = long.MaxValue; 
            ExtendedBlipSprite bestKey = default;
            bool any = false;

            foreach (var kv in _onScreenIconsMap)
            {
                var key = kv.Key;
                var pos = kv.Value;
                int dx = pos.X - clickX;
                int dy = pos.Y - clickY;
                long d2 = (long)dx * dx + (long)dy * dy;

                if (d2 <= radiusSq)
                {
                    
                    if (!any || d2 < bestD2 || (d2 == bestD2 && ((int)key) < ((int)bestKey)))
                    {
                        bestD2 = d2;
                        bestKey = key;
                    }
                    any = true;
                }
            }

            if (!any) return false;

            foundKey = bestKey;
            return true;
        }

        

        
        private void QzX9_HandleBuffer(string arg0)  
        {
            try
            {
                Path.GetFullPath(arg0);  
                string directoryPath = Path.GetDirectoryName(arg0);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            catch (PathTooLongException ex)
            {
                DisplayAndLogError(ex, $"Error ensuring directory exists for path (path too long): {arg0}");
            }
            catch (IOException ex)
            {
                DisplayAndLogError(ex, $"Error ensuring directory exists for path (I/O error): {arg0}");
            }
            catch (UnauthorizedAccessException ex)
            {
                DisplayAndLogError(ex, $"Error ensuring directory exists for path (access denied): {arg0}. Try this fix to slove the error: Right click on the {arg0} file -> Select and press 'Properties' -> In the 'General' tab -> Look for 'Attributes' -> Make sure that the 'Read-only' checkbox is unchecked.");
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, $"Error ensuring directory exists for path: {arg0}");
            }
        }
        private IniData LoadIniDataFromDisk()  
        {
            try
            {
                lock (_fileWriteLock)
                {
                    QzX9_HandleBuffer(_iniFilePath);

                    if (!File.Exists(_iniFilePath))
                    {
                        IniData iniData = new IniData();
                        iniData.Sections.AddSection("Settings");

                        iniData["Settings"]["Toggle_Coordinates_Key"] = _keyToggleCoords.ToString();
                        iniData["Settings"]["Reload_Blips_Key"] = _keyReloadBlips.ToString();
                        iniData["Settings"]["Toggle_Blips_Visibility_Key"] = _keyToggleBlipVisibility.ToString();
                        iniData["Settings"]["Open_Menu_Key"] = _keyOpenMenu.ToString();
                        iniData["Settings"]["Show_Blip_Added_Notification"] = "ON";
                        iniData["Settings"]["Enable_AddOn_Blips"] = "OFF";

                        SaveIniDataToDisk(iniData);

                        Notification.PostTicker($"~y~Advanced Custom Blips.ini created in scripts folder.", true);
                    }

                    return _iniParser.ReadFile(_iniFilePath);
                }
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
        private void SaveIniDataToDisk(IniData iniData)  
        {
            try
            {
                lock (_fileWriteLock)
                {
                    QzX9_HandleBuffer(_iniFilePath);

                    _iniParser.WriteFile(_iniFilePath, iniData);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                DisplayAndLogError(ex, $"Access denied while saving INI to: {_iniFilePath}. Right click on the  {_iniFilePath}  file -> Select and press 'Properties' -> In the 'General' tab -> Look for 'Attributes' -> Make sure that the 'Read-only' checkbox is unchecked.");
            }
            catch (IOException ex)
            {
                DisplayAndLogError(ex, $"I/O error while saving INI to: {_iniFilePath}");
            }
            catch (InvalidOperationException ex)
            {
                DisplayAndLogError(ex, $"Serialization error while saving INI to: {_iniFilePath}");
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, $"Unexpected error while saving INI to: {_iniFilePath}");
            }
        }

        
        private Keys? ParseKeyOrDefault(string keyStr, Keys defaultKey)
        {
            
            if (string.IsNullOrWhiteSpace(keyStr))
            {
                return defaultKey;
            }

            bool isValidKey = Enum.TryParse(keyStr, true, out Keys parsedKey);

            
            if (parsedKey == Keys.None)
            {
                return null;
            }

            
            if (isValidKey)
            {
                return parsedKey;
            }

            return defaultKey;
        }
        private bool TryParseField<T>(Dictionary<string, string> blipFields, string keyName, string sectionName, out T parseSuccess, out string errorMessage)
        {
            parseSuccess = default;
            errorMessage = null;

            if (!blipFields.TryGetValue(keyName, out string fieldValue) || string.IsNullOrWhiteSpace(fieldValue))
            {
                errorMessage = $"Missing or empty '{keyName}' in section '{sectionName}'.";
                return false;
            }

            try
            {
                if (typeof(T).IsEnum)
                {
                    object parsed = Enum.Parse(typeof(T), fieldValue, ignoreCase: true);
                    parseSuccess = (T)parsed;
                    return true;
                }
                else
                {
                    parseSuccess = (T)Convert.ChangeType(fieldValue, typeof(T));
                    return true;
                }
            }
            catch
            {
                
            }

            errorMessage = $"Invalid '{keyName}' value in section '{sectionName}', expected {typeof(T).Name}.";
            return false;
        }
        private bool TryParseEnumOrInt<TEnum>(string value, string sectionName, string keyName, out TEnum parseSuccess, out string errorMsg) where TEnum : struct, Enum  
        {
            parseSuccess = default;
            errorMsg = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                errorMsg = $"Missing or empty '{keyName}' in section '{sectionName}'.";
                return false;
            }

            
            if (Enum.TryParse<TEnum>(value, ignoreCase: true, out parseSuccess))
            {
                return true;
            }

            
            if (int.TryParse(value, out int intValue))
            {
                if (Enum.IsDefined(typeof(TEnum), intValue))
                {
                    parseSuccess = (TEnum)(object)intValue;
                    return true;
                }
                else
                {
                    errorMsg = $"'{keyName}' value '{value}' in section '{sectionName}' is not a valid {typeof(TEnum).Name} ID.";
                    return false;
                }
            }

            errorMsg = $"Invalid '{keyName}' value in section '{sectionName}'. Must be a valid name or ID of {typeof(TEnum).Name}.";
            return false;
        }
        private void LoadGlobalSettingsFromIni(out Keys? coordKey, out Keys? reloadKey, out Keys? visibilityKey, out bool showNotify, out int coordKeyIndex, out int reloadKeyIndex, out int visibilityKeyIndex)
        {
            coordKey = Keys.F1;
            reloadKey = Keys.F2;
            visibilityKey = Keys.F3;
            _keyOpenMenu = Keys.F10;
            showNotify = true;

            coordKeyIndex = reloadKeyIndex = visibilityKeyIndex = 0;

            IniData iniData = LoadIniDataFromDisk();
            if (!iniData.Sections.ContainsSection("Settings"))
            {
                return;
            }

            KeyDataCollection settings = iniData["Settings"];

            coordKey = ParseKeyOrDefault(settings.ContainsKey("Toggle_Coordinates_Key") ? settings["Toggle_Coordinates_Key"] : null, Keys.F1);
            reloadKey = ParseKeyOrDefault(settings.ContainsKey("Reload_Blips_Key") ? settings["Reload_Blips_Key"] : null, Keys.F2);
            visibilityKey = ParseKeyOrDefault(settings.ContainsKey("Toggle_Blips_Visibility_Key") ? settings["Toggle_Blips_Visibility_Key"] : null, Keys.F3);
            _keyOpenMenu = ParseKeyOrDefault(settings.ContainsKey("Open_Menu_Key") ? settings["Open_Menu_Key"] : null, Keys.F10);

            if (TryParseEnumOrInt<BoolOption>(settings.ContainsKey("Show_Blip_Added_Notification") ? settings["Show_Blip_Added_Notification"] : "ON", "Settings", "Show_Blip_Added_Notification", out BoolOption notifyOption, out string parseErrorMessage))
            {
                _showBlipLoadNotification = (notifyOption == BoolOption.ON);
            }
            else
            {
                DisplayAndLogError(new FormatException(), parseErrorMessage);
                _showBlipLoadNotification = true; 
            }

            if (TryParseEnumOrInt<BoolOption>(settings.ContainsKey("Enable_AddOn_Blips") ? settings["Enable_AddOn_Blips"] : "OFF", "Settings", "Enable_AddOn_Blips", out BoolOption addOnBlipsOption, out parseErrorMessage))
            {
                _enableAddOnBlips = (addOnBlipsOption == BoolOption.ON);
            }
            else
            {
                DisplayAndLogError(new FormatException(), parseErrorMessage);
                _enableAddOnBlips = false; 
            }


            List<string> keysList = Enum.GetNames(typeof(Keys)).ToList();

            coordKeyIndex = keysList.IndexOf((coordKey ?? Keys.F1).ToString());
            reloadKeyIndex = keysList.IndexOf((reloadKey ?? Keys.F2).ToString());
            visibilityKeyIndex = keysList.IndexOf((visibilityKey ?? Keys.F3).ToString());

            
            if (coordKeyIndex < 0)
            {
                coordKeyIndex = 0;
            }

            if (reloadKeyIndex < 0)
            {
                reloadKeyIndex = 0;
            }

            if (visibilityKeyIndex < 0)
            {
                visibilityKeyIndex = 0;
            }
        }

        
        private void DeleteAllGameBlips()  
        {
            foreach (var blip in _activeBlips)
            {
                if (blip != null && blip.Exists())
                {
                    blip.Delete();
                }
            }
            _activeBlips.Clear();
        }
        private void DeleteAllBlipsFromIniAndGame()  
        {
            try
            {
                IniData iniData = LoadIniDataFromDisk();

                
                List<SectionData> blipSectionsToRemove = iniData.Sections
                    .Where(s => !s.SectionName.Equals("Settings", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (SectionData blipSection in blipSectionsToRemove)
                {
                    iniData.Sections.RemoveSection(blipSection.SectionName);
                }

                SaveIniDataToDisk(iniData);
                ParseIniAndCreateBlips();
                RefreshManageBlipsMenu();

                Notification.PostTicker($"~g~Removed all {blipSectionsToRemove.Count} blips!", true);
            }
            catch (Exception ex)
            {
                Notification.PostTicker($"~r~Error removing blips: {ex.Message}", true);
            }
        }

        
        private void ParseIniAndCreateBlips()  
        {
            try
            {
                
                DeleteAllGameBlips();
                _blipCount = 0;

                
                IniData iniData = LoadIniDataFromDisk();

                
                if (iniData.Sections.ContainsSection("Settings"))
                {
                    LoadGlobalSettingsFromIni(out _keyToggleCoords, out _keyReloadBlips, out _keyToggleBlipVisibility, out _showBlipLoadNotification, out _, out _, out _);  
                }

                foreach (SectionData blipSection in iniData.Sections)
                {
                    
                    if (blipSection.SectionName == "Settings")
                    {
                        continue;
                    }

                    try
                    {
                        HashSet<string> missingRequiredKeys = new HashSet<string>();  

                        
                        
                        Dictionary<string, string> blipFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (KeyData key in blipSection.Keys)
                        {
                            blipFields[key.KeyName] = key.Value;
                        }

                        
                        foreach (KeyValuePair<string, string> kvp in _requiredBlipProperties)  
                        {
                            if (!blipFields.Keys.Contains(kvp.Key))
                            {
                                missingRequiredKeys.Add($"{kvp.Key} ({kvp.Value})");
                            }
                        }

                        if (missingRequiredKeys.Count > 0)  
                        {
                            DisplayAndLogError(new Exception("Missing required keys in section"), $"Section '{blipSection.SectionName}' is missing required keys:\n- {string.Join("\n- ", missingRequiredKeys)}");
                            continue;
                        }


                        
                        if (!TryParseEnumOrInt<ExtendedBlipSprite>(blipFields["Blip_Icon"], blipSection.SectionName, "Blip_Icon", out ExtendedBlipSprite iconSprite, out string parseErrorMessage))
                        {
                            DisplayAndLogError(new FormatException(), parseErrorMessage);
                            continue;
                        }

                        
                        if (!TryParseField<float>(blipFields, "Blip_Size", blipSection.SectionName, out float blipScale, out parseErrorMessage))
                        {
                            DisplayAndLogError(new FormatException(), parseErrorMessage);
                            continue;
                        }

                        
                        if (!TryParseEnumOrInt<ExtendedBlipColor>(blipFields["Blip_Color"], blipSection.SectionName, "Blip_Color", out ExtendedBlipColor blipColor, out parseErrorMessage))
                        {
                            DisplayAndLogError(new FormatException(), parseErrorMessage);
                            continue;
                        }

                        
                        if (!TryParseField<int>(blipFields, "Flash_Interval", blipSection.SectionName, out int flashInterval, out parseErrorMessage))
                        {
                            DisplayAndLogError(new FormatException(), parseErrorMessage);
                            continue;
                        }

                        
                        if (!TryParseField<float>(blipFields, "X", blipSection.SectionName, out float x, out parseErrorMessage))
                        {
                            DisplayAndLogError(new FormatException(), parseErrorMessage);
                            continue;
                        }

                        
                        if (!TryParseField<float>(blipFields, "Y", blipSection.SectionName, out float y, out parseErrorMessage))
                        {
                            DisplayAndLogError(new FormatException(), parseErrorMessage);
                            continue;
                        }

                        
                        if (!TryParseField<float>(blipFields, "Z", blipSection.SectionName, out float z, out parseErrorMessage))
                        {
                            DisplayAndLogError(new FormatException(), parseErrorMessage);
                            continue;
                        }

                        
                        if (!TryParseEnumOrInt<BoolOption>(blipFields["Short_Range_State"], blipSection.SectionName, "Short_Range_State", out BoolOption shortRangeState, out parseErrorMessage))
                        {
                            DisplayAndLogError(new FormatException(), parseErrorMessage);
                            continue;
                        }

                        
                        if (!TryParseEnumOrInt<BoolOption>(blipFields["Flashing_State"], blipSection.SectionName, "Flashing_State", out BoolOption flashingState, out parseErrorMessage))
                        {
                            DisplayAndLogError(new FormatException(), parseErrorMessage);
                            continue;
                        }

                        
                        string blipName = blipFields["Blip_Name"];
                        if (!IsValidName(blipName))
                        {
                            Notification.PostTicker($"~r~{_invalidName}", true);
                            continue;
                        }

                        
                        Blip blip = World.CreateBlip(new Vector3(x, y, z));  

                        Function.Call(Hash.SET_BLIP_SPRITE, blip.Handle, (int)iconSprite);  

                        blip.Name = blipName;  

                        blip.IsShortRange = (shortRangeState == BoolOption.ON);  
                        blip.IsFlashing = (flashingState == BoolOption.ON);  

                        blip.Scale = blipScale;  
                        Function.Call(Hash.SET_BLIP_COLOUR, blip.Handle, (int)blipColor);  

                        
                        if (flashInterval > 0 && blip.IsFlashing)
                        {
                            blip.FlashInterval = flashInterval;
                        }

                        _activeBlips.Add(blip);  
                        _blipCount++;  
                    }
                    catch (Exception ex)  
                    {
                        DisplayAndLogError(ex, $"Error processing {blipSection.SectionName}: {ex.Message}");
                    }
                }
                
                if (_showBlipLoadNotification)
                {
                    Notification.PostTicker($"Added ~g~{_blipCount} ~w~blips.", true);
                }
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, "Error in ParseIniAndCreateBlips method");
            }
        }


        

        
        private void InitializeAllMenus()  
        {
            try
            {
                _mainMenu = CreateAndRegisterMenu("Advanced Custom Blips", "Main Menu");

                InitializeAddBlipMenu();
                InitializeGlobalSettingsMenu();
                RefreshManageBlipsMenu();
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, "Error in InitializeAllMenus method");
            }
        }
        private void InitializeAddBlipMenu()  
        {
            _addBlipMenu = CreateAndRegisterMenu("Add New Blip", "Enter Blip Details");

            
            _addBlipMenu.Closing += (sender, e) =>
            {
                DeletePreviewBlip();
                _addBlipMenu.SelectedIndex = 0;
            };

            _nameItem = new NativeItem("Name", "This will be the name of the blip in the game and in the INI file.");
            _nameItem.Activated += (sourceMenu, clickedItem) =>
            {
                string userInput = PromptUserForInput("Enter the name of the blip");
                if (IsValidName(userInput))
                {
                    _nameItem.Title = $"Name ({userInput})";
                    _blipNameInputs[_nameItem] = userInput;
                }
                else
                {
                    Notification.PostTicker($"~r~{_invalidName}", true);
                }
            };

            _iconItem = CreateEnumListItem<ExtendedBlipSprite>("Icon", true, 0, "Choose the icon type for the blip.");

            _colorItem = CreateEnumListItem<ExtendedBlipColor>("Color", true, 0, "Choose the color of the blip (white for the default color of the blip).");

            _sizeItem = new NativeItem("~y~Size/Scale", "It controls the size/scale of the blip; leave it if you want the default size of the blip.");
            
            BindFloatInput(_sizeItem, "Enter Size/Scale", "Size", 1.0f, "~y~Using default size.", _addBlipCoordinateInputs);

            _xInputItem = new NativeItem("~y~X Position", "The X coordinate of the blip, leave it if you want to use the current coordinates of the player.");
            BindFloatInput(_xInputItem, "Enter X Position", "X", null, null, _addBlipCoordinateInputs); 

            _yInputItem = new NativeItem("~y~Y Position", "The Y coordinate of the blip, leave it if you want to use the current coordinates of the player.");
            BindFloatInput(_yInputItem, "Enter Y Position", "Y", null, null, _addBlipCoordinateInputs);

            _zInputItem = new NativeItem("~y~Z Position", "The Z coordinate of the blip, leave it if you want to use the current coordinates of the player.");
            BindFloatInput(_zInputItem, "Enter Z Position", "Z", null, null, _addBlipCoordinateInputs);

            _flashIntervalItem = new NativeItem("~y~Flash interval", "This adjusts how fast the blip blinks; leave it if you want to use the default flash speed (100 ms). Flash speed is in milliseconds (higher = slower). Please note that if the flashing state is turned off, then this setting will be completely ignored. Turn it on to see the effect.");
            BindFloatInput(_flashIntervalItem, "Enter Flash Interval", "Flash Interval", 100f, "~y~Invalid flash interval. Adjusted for 100 ms.", _addBlipCoordinateInputs);

            _shortRangeItem = CreateCheckboxItem("Short Range", true, "Blip only shows when nearby.");

            _flashItem = CreateCheckboxItem("Flashing", false, "Blinking effect for the blip.");

            
            _flashItem.CheckboxChanged += (sender, @checked) => { UpdatePreviewBlip(_iconItem, _colorItem, _sizeItem, _flashItem, _flashIntervalItem, _shortRangeItem); };

            
            _iconItem.ItemChanged += (sender, index) =>
            {
                UpdatePreviewBlip(_iconItem, _colorItem, _sizeItem, _flashItem, _flashIntervalItem, _shortRangeItem);
            };


            
            _colorItem.ItemChanged += (sender, index) => { UpdatePreviewBlip(_iconItem, _colorItem, _sizeItem, _flashItem, _flashIntervalItem, _shortRangeItem); };

            
            _shortRangeItem.CheckboxChanged += (sender, @checked) => { UpdatePreviewBlip(_iconItem, _colorItem, _sizeItem, _flashItem, _flashIntervalItem, _shortRangeItem); };

            AddItemToMenu(_nameItem, _addBlipMenu);
            AddItemToMenu(_iconItem, _addBlipMenu);
            AddItemToMenu(_colorItem, _addBlipMenu);
            AddItemToMenu(_sizeItem, _addBlipMenu);
            AddItemToMenu(_xInputItem, _addBlipMenu);
            AddItemToMenu(_yInputItem, _addBlipMenu);
            AddItemToMenu(_zInputItem, _addBlipMenu);
            AddItemToMenu(_flashItem, _addBlipMenu);
            AddItemToMenu(_flashIntervalItem, _addBlipMenu);
            AddItemToMenu(_shortRangeItem, _addBlipMenu);

            NativeItem saveBlipBtn = new NativeItem("~g~Save Blip");
            saveBlipBtn.Activated += (menu, clickedItem) =>
            {
                
                DeletePreviewBlip();

                if (!_blipNameInputs.TryGetValue(_nameItem, out string blipName))
                {
                    Notification.PostTicker($"~r~Failed to create blip. {_invalidName} Please enter a valid name.", true);
                    return;
                }
                if (!IsValidName(blipName))
                {
                    Notification.PostTicker($"~r~{_invalidName}", true);
                    return;
                }

                
                bool hasX = _addBlipCoordinateInputs.TryGetValue(_xInputItem, out float xCoord);
                bool hasY = _addBlipCoordinateInputs.TryGetValue(_yInputItem, out float yCoord);
                bool hasZ = _addBlipCoordinateInputs.TryGetValue(_zInputItem, out float zCoord);
                Vector3? pos = null;
                if (!hasX || !hasY || !hasZ)
                {
                    Notification.PostTicker("~y~Invalid or missing coordinates, please double check x, y, and z coordinates of the blip. Using current player coordinates.", true);
                    Player player = Game.Player;
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

                IniData iniData = LoadIniDataFromDisk();

                
                int nextBlipId = 1;
                while (iniData.Sections.ContainsSection($"{blipName}_{nextBlipId}"))
                {
                    nextBlipId++;
                }
                string blipSectionName = $"{blipName}_{nextBlipId}";

                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
                {
                    ["Blip_Name"] = blipName,
                    ["Blip_Icon"] = GetSelectedEnumValue<ExtendedBlipSprite>(_iconItem).ToString(),
                    ["Blip_Size"] = _addBlipCoordinateInputs.ContainsKey(_sizeItem) ? _addBlipCoordinateInputs[_sizeItem].ToString("F2") : "1.00",
                    ["Blip_Color"] = GetSelectedEnumValue<ExtendedBlipColor>(_colorItem).ToString(),
                    ["Flashing_State"] = _flashItem.Checked ? "ON" : "OFF",
                    ["Flash_Interval"] = _addBlipCoordinateInputs.ContainsKey(_flashIntervalItem) ? ((int)_addBlipCoordinateInputs[_flashIntervalItem]).ToString() : "100",
                    ["Short_Range_State"] = _shortRangeItem.Checked ? "ON" : "OFF",
                    ["X"] = pos.Value.X.ToString("F2"),
                    ["Y"] = pos.Value.Y.ToString("F2"),
                    ["Z"] = pos.Value.Z.ToString("F2")
                };

                SaveToIniAndNotify(blipSectionName, keyValuePairs, "~g~Blip saved to INI.");
                RefreshManageBlipsMenu();

                
                _addBlipCoordinateInputs.Remove(_xInputItem); _xInputItem.Title = "~y~X Position";
                _addBlipCoordinateInputs.Remove(_yInputItem); _yInputItem.Title = "~y~Y Position";
                _addBlipCoordinateInputs.Remove(_zInputItem); _zInputItem.Title = "~y~Z Position";
                _addBlipCoordinateInputs.Remove(_sizeItem); _sizeItem.Title = "~y~Size/Scale";
                _addBlipCoordinateInputs.Remove(_flashIntervalItem); _flashIntervalItem.Title = "~y~Flash interval";
            };

            AddItemToMenu(saveBlipBtn, _addBlipMenu);

            _mainMenu.AddSubMenu(_addBlipMenu, "").Title = "Add New Blip";
        }
        private void InitializeGlobalSettingsMenu()  
        {
            _settingsMenu = CreateAndRegisterMenu("Global Settings", "Configure Keys & Options");

            LoadGlobalSettingsFromIni(out _keyToggleCoords, out _keyReloadBlips, out _keyToggleBlipVisibility, out _showBlipLoadNotification, out int toggleIndex, out int reloadIndex, out int visibilityIndex);

            List<string> keysList = Enum.GetNames(typeof(Keys)).ToList();
            int openMenuIndex = keysList.IndexOf((_keyOpenMenu ?? Keys.F10).ToString());
            if (openMenuIndex < 0)
            {
                openMenuIndex = keysList.IndexOf(Keys.F10.ToString());  
            }

            NativeListItem openMenuKeyItem = CreateEnumListItem<Keys>("Open Menu Key", false, openMenuIndex, "Choose the key to open the main menu. ~y~Either scroll through the list or press enter then press the key you want to assign");
            NativeListItem toggleCoordKeyItem = CreateEnumListItem<Keys>("Toggle Coordinates Key", false, toggleIndex, "Choose the key for coordinates display. ~y~Either scroll through the list or press enter then press the key you want to assign");
            NativeListItem reloadKeyItem = CreateEnumListItem<Keys>("Reload Blips Key", false, reloadIndex, "Choose the key to reload script. ~y~Either scroll through the list or press enter then press the key you want to assign");
            NativeListItem toggleVisibilityKeyItem = CreateEnumListItem<Keys>("Toggle Blip Visibility Key", false, visibilityIndex, "Choose the key for add/loaded blips visibility. ~y~Either scroll through the list or press enter then press the key you want to assign");
            NativeCheckboxItem notifyBlipItem = CreateCheckboxItem("Show Blip Notification", _showBlipLoadNotification, "Shows a notification when blips are loaded from INI file. ~y~Press the save button to save changes.");
            NativeCheckboxItem enableAddOnBlipItem = CreateCheckboxItem("Enable Add-On Blips", _enableAddOnBlips, "Loads all blips from the AddOn blips mod if the mod is installed. ~y~Press the save button to apply changes.");
            enableAddOnBlipItem.CheckboxChanged += (sourceMenu, clickedItem) =>
            {
                if (enableAddOnBlipItem.Checked)
                {
                    _enableAddOnBlips = true;
                }
                else
                {
                    _enableAddOnBlips = false;
                }
            };

            BindKeyAssignment(toggleCoordKeyItem, "Toggle_Coordinates_Key", "Toggle Coordinates");
            BindKeyAssignment(reloadKeyItem, "Reload_Blips_Key", "Reload Blips");
            BindKeyAssignment(toggleVisibilityKeyItem, "Toggle_Blips_Visibility_Key", "Toggle Blips Visibility");
            BindKeyAssignment(openMenuKeyItem, "Open_Menu_Key", "Open Menu Key");

            AddItemToMenu(openMenuKeyItem, _settingsMenu);
            AddItemToMenu(toggleCoordKeyItem, _settingsMenu);
            AddItemToMenu(reloadKeyItem, _settingsMenu);
            AddItemToMenu(toggleVisibilityKeyItem, _settingsMenu);
            AddItemToMenu(notifyBlipItem, _settingsMenu);
            AddItemToMenu(enableAddOnBlipItem, _settingsMenu);

            _categoryMenu = CreateAndRegisterMenu("Category", "Enable or disable blip category");

            
            
            
            

            
            AddItemToMenu(CreateCategoryButtonItem("Gas Station", _gasStationsCategoryBlips, "Gas Station"), _categoryMenu); 
            AddItemToMenu(CreateCategoryButtonItem("Market", _marketCategoryBlips, "Market"), _categoryMenu);
            AddItemToMenu(CreateCategoryButtonItem("Police Department", _policeDepartmentCategoryBlips, "Police Department"), _categoryMenu);
            AddItemToMenu(CreateCategoryButtonItem("Fire Department", _fireDepartmentCategoryBlips, "Fire Department"), _categoryMenu);
            AddItemToMenu(CreateCategoryButtonItem("ATM", _ATMCategoryBlips, "ATM"), _categoryMenu);
            AddItemToMenu(CreateCategoryButtonItem("Metro Station", _metroStationCategoryBlips, "Metro Station"), _categoryMenu);
            AddItemToMenu(CreateCategoryButtonItem("Medical Center", _medicalCenterCategoryBlips, "Medical Center"), _categoryMenu);

            
            


            NativeItem saveSettingsBtn = new NativeItem("~g~Save Settings");
            saveSettingsBtn.Activated += (sourceMenu, clickedItem) =>
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
                {
                    ["Toggle_Coordinates_Key"] = toggleCoordKeyItem.ToString(),
                    ["Reload_Blips_Key"] = reloadKeyItem.ToString(),
                    ["Toggle_Blips_Visibility_Key"] = toggleVisibilityKeyItem.ToString(),
                    ["Open_Menu_Key"] = openMenuKeyItem.ToString(),
                    ["Show_Blip_Added_Notification"] = notifyBlipItem.Checked ? "ON" : "OFF",
                    ["Enable_AddOn_Blips"] = enableAddOnBlipItem.Checked ? "ON" : "OFF"
                };

                SaveToIniAndNotify("Settings", keyValuePairs, "~g~Settings saved to INI.");

                if (_enableAddOnBlips)
                {
                    LoadAddOnBlips();
                }
                else if (!_enableAddOnBlips)
                {
                    UnloadAddOnBlips();
                }

                ParseIniAndCreateBlips(); 
                RefreshManageBlipsMenu(); 
            };

            _mainMenu.AddSubMenu(_settingsMenu, "").Title = "Global Settings";

            _settingsMenu.AddSubMenu(_categoryMenu, "").Title = "~y~Category Menu";

            AddItemToMenu(saveSettingsBtn, _settingsMenu);
        }
        private void LoadAddOnBlips()
        {
            string[] addOnBlipsFiles = Directory.GetFiles(_addOnBlipsFilesPath, "*.txt");
            if (addOnBlipsFiles.Length == 0)
                return;

            
            IniData iniData = LoadIniDataFromDisk();

            foreach (string addOnBlipFile in addOnBlipsFiles)
            {
                string addOnBlip = File.ReadAllText(addOnBlipFile).Trim();

                
                string[] parts = addOnBlip.Split(';');
                if (parts.Length < 3)
                {
                    DisplayAndLogError(null, $"Malformed blip data in file: {addOnBlipFile}");
                    continue;
                }

                string[] coordinates = parts[0].Split(',');
                if (coordinates.Length < 3)
                {
                    DisplayAndLogError(null, $"Invalid coordinates in file: {addOnBlipFile}");
                    continue;
                }

                int x = int.Parse(coordinates[0].Trim());
                int y = int.Parse(coordinates[1].Trim());
                int z = int.Parse(coordinates[2].Trim());

                string blipName = parts[1].Trim();
                string blipIcon = parts[2].Trim();

                
                string sectionName = $"AddOn_{blipName}";

                
                if (!iniData.Sections.ContainsSection(sectionName))
                    iniData.Sections.AddSection(sectionName);

                iniData[sectionName]["Blip_Name"] = blipName;
                iniData[sectionName]["Blip_Icon"] = Enum.TryParse(blipIcon, true, out BlipSprite icon)
                    ? ((int)icon).ToString()
                    : ((int)ExtendedBlipSprite.radar_level).ToString();
                iniData[sectionName]["Blip_Size"] = "1.00";
                iniData[sectionName]["Blip_Color"] = "0";
                iniData[sectionName]["Flashing_State"] = "OFF";
                iniData[sectionName]["Flash_Interval"] = "100";
                iniData[sectionName]["Short_Range_State"] = "ON";
                iniData[sectionName]["X"] = x.ToString();
                iniData[sectionName]["Y"] = y.ToString();
                iniData[sectionName]["Z"] = z.ToString();
            }

            
            SaveIniDataToDisk(iniData);

            Notification.PostTicker("~g~All AddBlips mod Blips successfully loaded and saved to INI.", true);
        }
        private void UnloadAddOnBlips()
        {
            IniData iniData = LoadIniDataFromDisk();

            
            var blipSections = iniData.Sections
                .Where(s => s.SectionName.StartsWith("AddOn_", StringComparison.OrdinalIgnoreCase))
                .Select(s => s.SectionName)
                .ToList();

            if (blipSections.Count == 0)
            {
                Notification.PostTicker("~y~No AddOn Blips found to remove from INI.", true);
                return;
            }

            
            foreach (string section in blipSections)
            {
                iniData.Sections.RemoveSection(section);
            }

            
            SaveIniDataToDisk(iniData);

            Notification.PostTicker($"~r~Removed {blipSections.Count} AddOn Blip(s) from INI.", true);
        }
        private void RefreshManageBlipsMenu()  
        {
            if (_blipManagerMenu == null)
            {
                _blipManagerMenu = CreateAndRegisterMenu("Manage Blips", "Edit, Remove, and Teleport Blips");

                
                NativeSubmenuItem manageSubItem = _mainMenu.AddSubMenu(_blipManagerMenu, "");

                
                manageSubItem.Title = "Manage Existing Blips";
                manageSubItem.Description = "Edit, remove, or teleport to your custom blips.";
            }


            
            _blipManagerMenu.Clear();

            IniData iniData = LoadIniDataFromDisk();

            foreach (SectionData blipSection in iniData.Sections)
            {
                if (blipSection.SectionName == "Settings")
                {
                    continue;
                }

                NativeMenu editMenu = CreateBlipEditMenu(iniData, blipSection);

                _blipManagerMenu.AddSubMenu(editMenu, "").Title = blipSection.SectionName;
            }

            
            NativeItem removeAllBtn = new NativeItem("~r~Remove ALL Blips", "Permanently delete all custom blips from in-game and INI file. There is no undo, confirm with 'yes'");
            removeAllBtn.Activated += (menu, clickedItem) =>
            {
                string userInput = PromptUserForInput("Confirm with 'yes'");
                if (IsValidName(userInput) && userInput.ToUpper() == "YES")
                {
                    DeleteAllBlipsFromIniAndGame();
                }
                else
                {
                    Notification.PostTicker("~r~Invalid input, type 'yes' to confirm deletion.", true);
                }
            };
            AddItemToMenu(removeAllBtn, _blipManagerMenu);

            
            _teleportMenu = CreateAndRegisterMenu("Teleport Blips", "Select a blip to teleport to");

            foreach (SectionData blipSection in iniData.Sections)
            {
                if (blipSection.SectionName == "Settings")
                {
                    continue;
                }

                NativeItem item = new NativeItem(blipSection.SectionName);
                item.Activated += (m, i) =>
                {
                    TeleportToBlip(blipSection);
                };
                AddItemToMenu(item, _teleportMenu);
            }

            
            
            NativeSubmenuItem teleportSubItem = _blipManagerMenu.AddSubMenu(_teleportMenu, "");

            
            teleportSubItem.Title = "~y~Teleport to a Blip";
            teleportSubItem.Description = "Select a blip to instantly teleport there.";
        }
        private NativeMenu CreateBlipEditMenu(IniData iniData, SectionData blipSection)  
        {
            string sectionName = blipSection.SectionName;

            
            Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyData key in blipSection.Keys)
            {
                fields[key.KeyName] = key.Value;
            }

            
            string baseName = sectionName.Substring(0, sectionName.LastIndexOf('_'));
            string idPart = sectionName.Substring(sectionName.LastIndexOf('_') + 1);

            TryParseEnumOrInt<ExtendedBlipSprite>(fields["Blip_Icon"], blipSection.SectionName, "Blip_Icon", out ExtendedBlipSprite iconSprite, out string parseErrorMessage);
            TryParseField<float>(fields, "Blip_Size", blipSection.SectionName, out float sizeVal, out parseErrorMessage);
            TryParseEnumOrInt<ExtendedBlipColor>(fields["Blip_Color"], blipSection.SectionName, "Blip_Color", out ExtendedBlipColor colorVal, out parseErrorMessage);
            TryParseField<int>(fields, "Flash_Interval", blipSection.SectionName, out int flashIntervalVal, out parseErrorMessage);
            TryParseField<float>(fields, "X", blipSection.SectionName, out float xVal, out parseErrorMessage);
            TryParseField<float>(fields, "Y", blipSection.SectionName, out float yVal, out parseErrorMessage);
            TryParseField<float>(fields, "Z", blipSection.SectionName, out float zVal, out parseErrorMessage);
            TryParseEnumOrInt<BoolOption>(fields["Short_Range_State"], blipSection.SectionName, "Short_Range_State", out BoolOption shortRangeState, out parseErrorMessage);
            TryParseEnumOrInt<BoolOption>(fields["Flashing_State"], blipSection.SectionName, "Flashing_State", out BoolOption flashingState, out parseErrorMessage);

            bool isFlashing = (flashingState == BoolOption.ON);
            bool isShortRange = (shortRangeState == BoolOption.ON);

            
            _editMenu = CreateAndRegisterMenu($"Edit: {baseName}", "Modify Blip Properties");

            
            NativeItem nameItem = new NativeItem("Name", "This is the name of the blip in the game and in the INI file.")
            {
                Title = $"Name ({fields["Blip_Name"]})"
            };
            _blipNameInputs[nameItem] = baseName;

            nameItem.Activated += (sender, item) =>
            {
                string userInput = PromptUserForInput("Enter the name of the blip", 30);
                if (IsValidName(userInput))
                {
                    nameItem.Title = $"Name ({userInput})";
                    _blipNameInputs[nameItem] = userInput;
                }
                else
                {
                    Notification.PostTicker($"~r~{_invalidName}", true);
                }
            };

            
            
            int iconValue = (int)iconSprite;

            
            List<string> iconList = Enum.GetValues(typeof(ExtendedBlipSprite))
                .Cast<ExtendedBlipSprite>()
                .Select(e => $"{e} ({(int)e})")
                .ToList();

            
            int iconIndex = iconList.FindIndex(item => item.ToString().EndsWith($"({iconValue})"));

            
            if (iconIndex < 0) iconIndex = 0;

            
            
            var iconItem = new NativeListItem<string>("Icon", iconList.ToArray())
            {
                SelectedIndex = iconIndex, 
                Description = "Choose the icon type for the blip."
            };

            
            
            int colorValue = (int)colorVal;

            
            List<string> colorList = Enum.GetValues(typeof(ExtendedBlipColor))
                .Cast<ExtendedBlipColor>()
                .Select(e => $"{e} ({(int)e})")
                .ToList();

            
            int colorIndex = colorList.FindIndex(item => item.ToString().EndsWith($"({colorValue})"));

            
            if (colorIndex < 0)
            {
                colorIndex = 0;
            }

            
            var colorItem = new NativeListItem<string>("Color", iconList.ToArray())
            {
                SelectedIndex = colorIndex, 
                Description = "Choose the color of the blip (white for the default color of the blip)."
            };


            
            NativeItem sizeItem = new NativeItem("~y~Size/Scale", "It controls the size/scale of the blip; put 1.0 if you want the default size of the blip.");
            if (sizeVal > 0) _editBlipCoordinateInputs[sizeItem] = sizeVal; 
            sizeItem.Title = sizeVal > 0 ? $"~y~Size/Scale ({sizeVal:F2})" : "~y~Size/Scale";
            BindFloatInput(sizeItem, "Enter Size/Scale", "Size", 1.0f, "~y~Using default size.", _editBlipCoordinateInputs);

            
            NativeItem _xInputItem = new NativeItem("~y~X Position", "The X coordinate of the blip. Press the update coordinates button then save button if you want to use your current coordinates.")
            {
                Title = $"~y~X Position ({xVal:F2})"
            };
            _editBlipCoordinateInputs[_xInputItem] = xVal; 
            BindFloatInput(_xInputItem, "Enter X Position", "X", null, null, _editBlipCoordinateInputs);

            NativeItem _yInputItem = new NativeItem("~y~Y Position", "The Y coordinate of the blip. Press the update coordinates button then save button if you want to use your current coordinates.")
            {
                Title = $"~y~Y Position ({yVal:F2})"
            };
            _editBlipCoordinateInputs[_yInputItem] = yVal; 
            BindFloatInput(_yInputItem, "Enter Y Position", "Y", null, null, _editBlipCoordinateInputs);

            NativeItem _zInputItem = new NativeItem("~y~Z Position", "The Z coordinate of the blip. Press the update coordinates button then save button if you want to use your current coordinates.")
            {
                Title = $"~y~Z Position ({zVal:F2})"
            };
            _editBlipCoordinateInputs[_zInputItem] = zVal; 
            BindFloatInput(_zInputItem, "Enter Z Position", "Z", null, null, _editBlipCoordinateInputs);

            NativeItem flashIntervalItem = new NativeItem("~y~Flash interval", "This adjusts how fast the blip blinks; put 100 if you want to use the default flash speed. Flash speed is in milliseconds (higher = slower). Please note that if the flashing state is turned off, then this setting will be completely ignored. Turn it on to see the effect.");
            if (flashIntervalVal > 0) _editBlipCoordinateInputs[flashIntervalItem] = flashIntervalVal; 
            flashIntervalItem.Title = flashIntervalVal > 0 ? $"~y~Flash interval ({flashIntervalVal})" : "~y~Flash interval";
            BindFloatInput(flashIntervalItem, "Enter Flash Interval", "Flash Interval", 100f, "~y~Invalid flash interval. Adjusted for 100 ms.", _editBlipCoordinateInputs);

            
            NativeCheckboxItem flashItem = CreateCheckboxItem("Flashing", isFlashing, "Blinking effect for the blip.");

            
            NativeCheckboxItem shortRangeItem = CreateCheckboxItem("Short Range", isShortRange, "Blip only shows when nearby.");

            
            AddItemToMenu(nameItem, _editMenu);
            AddItemToMenu(iconItem, _editMenu);
            AddItemToMenu(colorItem, _editMenu);
            AddItemToMenu(sizeItem, _editMenu);
            AddItemToMenu(_xInputItem, _editMenu);
            AddItemToMenu(_yInputItem, _editMenu);
            AddItemToMenu(_zInputItem, _editMenu);
            AddItemToMenu(flashItem, _editMenu);
            AddItemToMenu(flashIntervalItem, _editMenu);
            AddItemToMenu(shortRangeItem, _editMenu);

            
            NativeItem updatePosBtn = new NativeItem("~y~Update to Current Position")
            {
                Description = "Set X, Y, and Z to your current location."
            };
            updatePosBtn.Activated += (sender, item) =>
            {
                Player player = Game.Player;
                if (IsValidPlayer(player))
                {
                    Vector3 pos = GetPlayerCoordinates(player);
                    _editBlipCoordinateInputs[_xInputItem] = pos.X; 
                    _editBlipCoordinateInputs[_yInputItem] = pos.Y; 
                    _editBlipCoordinateInputs[_zInputItem] = pos.Z; 
                    _xInputItem.Title = $"~y~X Position ({pos.X:F2})";
                    _yInputItem.Title = $"~y~Y Position ({pos.Y:F2})";
                    _zInputItem.Title = $"~y~Z Position ({pos.Z:F2})";
                    Notification.PostTicker("~y~Position updated to current player coordinates. Press the save button below to apply changes.", true);
                }
            };
            AddItemToMenu(updatePosBtn, _editMenu);

            
            NativeItem teleportBtn = new NativeItem("~y~Teleport to Blip")
            {
                Description = "Teleport yourself to this blip's location."
            };
            teleportBtn.Activated += (sender, item) =>
            {
                TeleportToBlip(blipSection);
            };
            AddItemToMenu(teleportBtn, _editMenu);

            
            NativeItem saveBtn = new NativeItem("~g~Save Changes");
            saveBtn.Activated += (sender, item) =>
            {
                
                if (!_blipNameInputs.TryGetValue(nameItem, out string newName) || !IsValidName(newName))
                {
                    Notification.PostTicker($"~r~Blip name is required. {_invalidName}", true);
                    return;
                }

                
                float x = _editBlipCoordinateInputs.ContainsKey(_xInputItem) ? _editBlipCoordinateInputs[_xInputItem] : xVal;
                float y = _editBlipCoordinateInputs.ContainsKey(_yInputItem) ? _editBlipCoordinateInputs[_yInputItem] : yVal;
                float z = _editBlipCoordinateInputs.ContainsKey(_zInputItem) ? _editBlipCoordinateInputs[_zInputItem] : zVal;
                float size = _editBlipCoordinateInputs.ContainsKey(sizeItem) ? _editBlipCoordinateInputs[sizeItem] : 1.0f;
                float flashInterval = _editBlipCoordinateInputs.ContainsKey(flashIntervalItem) ? _editBlipCoordinateInputs[flashIntervalItem] : 100f;

                int iconId = GetSelectedEnumValue<ExtendedBlipSprite>(iconItem);
                int colorId = GetSelectedEnumValue<ExtendedBlipColor>(colorItem);
                bool flashing = flashItem.Checked;
                bool shortRange = shortRangeItem.Checked;

                
                bool nameChanged = !newName.Equals(baseName, StringComparison.OrdinalIgnoreCase);

                string newSectionName = sectionName;
                if (nameChanged)
                {
                    int nextId = 1;
                    newSectionName = $"{newName}_{nextId}";
                    while (iniData.Sections.ContainsSection(newSectionName))
                    {
                        nextId++;
                        newSectionName = $"{newName}_{nextId}";
                    }
                }

                
                if (nameChanged)
                {
                    iniData.Sections.RemoveSection(sectionName);
                }

                
                if (!iniData.Sections.ContainsSection(newSectionName))
                {
                    iniData.Sections.AddSection(newSectionName);
                }

                Dictionary<string, string> kvp = new Dictionary<string, string>
                {
                    ["Blip_Name"] = newName,
                    ["Blip_Icon"] = iconId.ToString(),
                    ["Blip_Color"] = colorId.ToString(),
                    ["Blip_Size"] = size.ToString("F2"),
                    ["Flashing_State"] = flashing ? "ON" : "OFF",
                    ["Flash_Interval"] = ((int)flashInterval).ToString(),
                    ["Short_Range_State"] = shortRange ? "ON" : "OFF",
                    ["X"] = x.ToString("F2"),
                    ["Y"] = y.ToString("F2"),
                    ["Z"] = z.ToString("F2")
                };

                foreach (KeyValuePair<string, string> pair in kvp)
                {
                    iniData[newSectionName][pair.Key] = pair.Value;
                }

                SaveIniDataToDisk(iniData);
                Notification.PostTicker($"~g~Blip saved as '{newSectionName}'.", true);

                _editMenu.Visible = false;
                ParseIniAndCreateBlips();
                RefreshManageBlipsMenu();
            };
            AddItemToMenu(saveBtn, _editMenu);

            
            NativeItem deleteBtn = new NativeItem("~r~Delete Blip")
            {
                Description = "Permanently remove this blip from game and INI file. There is no undo."
            };
            deleteBtn.Activated += (sender, item) =>
            {
                iniData.Sections.RemoveSection(sectionName);
                SaveIniDataToDisk(iniData);
                Notification.PostTicker($"~r~Blip '{sectionName}' deleted.", true);
                _editMenu.Visible = false;
                ParseIniAndCreateBlips();
                RefreshManageBlipsMenu();
            };
            AddItemToMenu(deleteBtn, _editMenu);

            
            NativeItem copyBtn = new NativeItem("~y~Copy Blip to Current Position")
            {
                Description = "Create a new blip with the same properties at your current location."
            };
            copyBtn.Activated += (sender, item) =>
            {
                Player player = Game.Player;
                if (!IsValidPlayer(player))
                {
                    Notification.PostTicker("~r~Failed to acquire valid player or character.", true);
                    return;
                }
                Vector3 currentPlayerPos = GetPlayerCoordinates(player);

                
                

                
                
                if (!_blipNameInputs.TryGetValue(nameItem, out string newName) || !IsValidName(newName))
                {
                    Notification.PostTicker($"~r~Blip name is required for the copy. {_invalidName}", true);
                    return;
                }

                
                int iconId = GetSelectedEnumValue<ExtendedBlipSprite>(iconItem);

                
                int colorId = GetSelectedEnumValue<ExtendedBlipColor>(colorItem);

                
                float size = _editBlipCoordinateInputs.ContainsKey(sizeItem) ? _editBlipCoordinateInputs[sizeItem] : sizeVal; 

                
                bool flashing = flashItem.Checked;

                
                float flashInterval = _editBlipCoordinateInputs.ContainsKey(flashIntervalItem) ? _editBlipCoordinateInputs[flashIntervalItem] : flashIntervalVal; 

                
                bool shortRange = shortRangeItem.Checked;

                
                float x = currentPlayerPos.X;
                float y = currentPlayerPos.Y;
                float z = currentPlayerPos.Z;

                
                string baseNameForCopy = newName; 
                int nextCopyId = 1;
                string newSectionName = $"{baseNameForCopy}_{nextCopyId}";
                while (iniData.Sections.ContainsSection(newSectionName))
                {
                    nextCopyId++;
                    newSectionName = $"{baseNameForCopy}_{nextCopyId}";
                }

                
                Dictionary<string, string> newKvp = new Dictionary<string, string>
                {
                    ["Blip_Name"] = newName,
                    ["Blip_Icon"] = iconId.ToString(),
                    ["Blip_Color"] = colorId.ToString(),
                    ["Blip_Size"] = size.ToString("F2"),
                    ["Flashing_State"] = flashing ? "ON" : "OFF",
                    ["Flash_Interval"] = ((int)flashInterval).ToString(), 
                    ["Short_Range_State"] = shortRange ? "ON" : "OFF",
                    ["X"] = x.ToString("F2"),
                    ["Y"] = y.ToString("F2"),
                    ["Z"] = z.ToString("F2")
                };

                iniData.Sections.AddSection(newSectionName);
                foreach (KeyValuePair<string, string> pair in newKvp)
                {
                    iniData[newSectionName][pair.Key] = pair.Value;
                }

                SaveIniDataToDisk(iniData);
                Notification.PostTicker($"~g~Copied blip '{baseName}' as '{newSectionName}' at your position.", true);

                
                ParseIniAndCreateBlips();
                RefreshManageBlipsMenu(); 
            };
            AddItemToMenu(copyBtn, _editMenu);

            return _editMenu;
        }

        
        private void SaveToIniAndNotify(string sectionName, Dictionary<string, string> keyValuePairs, string successMessage = "~g~Settings saved to INI.")
        {
            IniData iniData = LoadIniDataFromDisk();

            if (!iniData.Sections.ContainsSection(sectionName))
            {
                iniData.Sections.AddSection(sectionName);
            }

            foreach (KeyValuePair<string, string> kvp in keyValuePairs)
            {
                iniData[sectionName][kvp.Key] = kvp.Value;
            }

            SaveIniDataToDisk(iniData);
            Notification.PostTicker(successMessage, true);
            ParseIniAndCreateBlips();
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
        private void UpdatePreviewBlip(NativeListItem<string> iconItem, NativeListItem<string> colorItem, NativeItem sizeItem, NativeCheckboxItem flashItem, NativeItem flashIntervalItem, NativeCheckboxItem shortRangeItem)
        {
            try
            {
                Player player = Game.Player;
                if (!IsValidPlayer(player)) return;

                Vector3 playerPos = GetPlayerCoordinates(player);
                Vector3 playerForward = player.Character.ForwardVector;
                Vector3 previewPos = playerPos + (playerForward * _previewDistance); 

                
                ExtendedBlipSprite icon = (ExtendedBlipSprite)GetSelectedEnumValue<ExtendedBlipSprite>(iconItem);
                ExtendedBlipColor color = (ExtendedBlipColor)GetSelectedEnumValue<ExtendedBlipColor>(colorItem);
                float size = 1.0f;
                if (_addBlipCoordinateInputs.TryGetValue(sizeItem, out float sizeVal) && sizeVal > 0)
                {
                    size = sizeVal;
                }
                bool isFlashing = flashItem.Checked;
                int flashInterval = 100; 
                if (_addBlipCoordinateInputs.TryGetValue(flashIntervalItem, out float intervalVal) && intervalVal > 0)
                {
                    flashInterval = (int)intervalVal;
                }
                bool isShortRange = shortRangeItem.Checked;

                
                if (_previewBlip == null || !_previewBlip.Exists())
                {
                    _previewBlip = World.CreateBlip(previewPos);
                }
                else
                {
                    _previewBlip.Position = previewPos; 
                }

                Function.Call(Hash.SET_BLIP_SPRITE, _previewBlip.Handle, (int)icon);
                Function.Call(Hash.SET_BLIP_COLOUR, _previewBlip.Handle, (int)color);
                _previewBlip.Scale = size;
                _previewBlip.IsFlashing = isFlashing;
                if (isFlashing)
                {
                    _previewBlip.FlashInterval = flashInterval;
                }
                _previewBlip.IsShortRange = isShortRange;
                _previewBlip.Name = "Preview"; 
                _previewBlip.Alpha = 200; 

                
                Function.Call(Hash.SET_BLIP_DISPLAY, _previewBlip.Handle, 2);

            }
            catch (Exception ex)
            {
                
                
                DisplayAndLogError(ex, "Error updating blip preview");
            }
        }
        private void DeletePreviewBlip()
        {
            if (_previewBlip != null && _previewBlip.Exists())
            {
                _previewBlip.Delete();
            }
            _previewBlip = null;
        }
        private void TeleportToBlip(SectionData sectionData)
        {
            SectionData blipSection = sectionData;
            Dictionary<string, string> blipFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyData key in blipSection.Keys)
            {
                blipFields[key.KeyName] = key.Value;
            }
            if (TryParseField<float>(blipFields, "X", blipSection.SectionName, out float x, out _) &&
                TryParseField<float>(blipFields, "Y", blipSection.SectionName, out float y, out _) &&
                TryParseField<float>(blipFields, "Z", blipSection.SectionName, out float z, out _))
            {
                Player player = Game.Player;
                if (IsValidPlayer(player))
                {
                    if (player.Character.CurrentVehicle != null && player.Character.CurrentVehicle.Exists())
                    {
                        player.Character.CurrentVehicle.Position = new Vector3(x, y, z);
                    }
                    else
                    {
                        player.Character.Position = new Vector3(x, y, z);
                    }
                    Notification.PostTicker($"Teleported to ~y~{blipSection.SectionName}~w~!", true);
                }
            }
        }
        private string PromptUserForInput(string title = "Enter text", int maxLength = 30)  
        {
            Function.Call(Hash.DISPLAY_ONSCREEN_KEYBOARD, 1, "FMMC_KEY_TIP1", title, "", "", "", "", maxLength);

            while (Function.Call<int>(Hash.UPDATE_ONSCREEN_KEYBOARD) == 0)
            {
                Script.Wait(0);
            }

            if (Function.Call<int>(Hash.UPDATE_ONSCREEN_KEYBOARD) == 1)
            {
                return Function.Call<string>(Hash.GET_ONSCREEN_KEYBOARD_RESULT);
            }

            return null;
        }
        private void BindFloatInput(NativeItem item, string keyboardTitle, string labelPrefix, float? defaultValue = null, string fallbackWarning = null, Dictionary<NativeItem, float> inputDictionary = null)
        {
            item.Activated += (sourceMenu, clickedItem) =>
            {
                string userInput = PromptUserForInput(keyboardTitle);
                if (float.TryParse(userInput, out float value))
                {
                    item.Title = $"~y~{labelPrefix} ({value})";
                    inputDictionary[item] = value; 
                }
                else
                {
                    if (fallbackWarning != null)
                    {
                        Notification.PostTicker(fallbackWarning, true);
                    }
                    else
                    {
                        Notification.PostTicker($"~r~Invalid {labelPrefix.ToLower()} input.", true);
                    }
                    if (defaultValue.HasValue)
                    {
                        inputDictionary[item] = defaultValue.Value; 
                    }
                }
            };
        }
        private NativeListItem<string> CreateEnumListItem<TEnum>(string title, bool indexIncluded, int defaultIndex = 0, string description = null) where TEnum : Enum
        {
            List<string> list = new List<string>();
            if (indexIncluded)
            {
                
                list = Enum.GetValues(typeof(TEnum))
                    .Cast<TEnum>()
                    .Select(e => $"{e} ({Convert.ToInt32(e)})")
                    .ToList();
            }
            else
            {
                
                list = Enum.GetNames(typeof(TEnum)).ToList();
            }

            NativeListItem<string> item = new NativeListItem<string>(title, list.ToArray())
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
            NativeCheckboxItem item = new NativeCheckboxItem(title, defaultValue);
            if (!string.IsNullOrWhiteSpace(description))
            {
                item.Description = description;
            }

            return item;
        }
        private void AddItemToMenu(NativeItem item, NativeMenu menu)
        {
            menu.Add(item);
        }
        private int GetSelectedEnumValue<TEnum>(NativeListItem<string> item) where TEnum : struct, Enum
        {
            string selected = item.SelectedItem;
            if (string.IsNullOrWhiteSpace(selected))
                return 0;

            
            string enumName = selected.Split(' ')[0];

            
            if (Enum.TryParse(enumName, true, out TEnum result))
                return Convert.ToInt32(result);

            return 0; 
        }
        private NativeItem CreateCategoryButtonItem(string name, HashSet<PredefinedBlipData> categoryData, string categoryPrefix) 
        {
            var buttonItem = new NativeItem($"~y~Toggle {name} Blips")
            {
                Description = $"Add predefined {name} blips to both map and INI file. ~y~Press again to delete.~y~"
            }; 

            buttonItem.Activated += (sender, item) =>
            {
                try
                {
                    IniData iniData = LoadIniDataFromDisk();
                    var x = categoryData.ToList();
                    
                    
                    var existingCategorySections = iniData.Sections.Where(sec => sec.SectionName.StartsWith($"Category_Added_Blip_{categoryPrefix}")).ToList(); 

                    if (existingCategorySections.Count > 0)
                    {
                        
                        int countRemoved = existingCategorySections.Count;
                        foreach (var section in existingCategorySections)
                        {
                            iniData.Sections.RemoveSection(section.SectionName);
                            
                        }
                        SaveIniDataToDisk(iniData);
                        ParseIniAndCreateBlips(); 
                        RefreshManageBlipsMenu(); 
                        Notification.PostTicker($"~r~Removed {countRemoved} {name} blips from INI and game.", true);
                    }
                    else
                    {
                        
                        bool anyAdded = false;
                        int countAdded = 0;

                        foreach (var data in categoryData)
                        {
                            
                            string uniqueSectionName = $"Category_Added_Blip_{data.Name} {countAdded}";

                            
                            if (!iniData.Sections.ContainsSection(uniqueSectionName))
                            {
                                
                                iniData.Sections.AddSection(uniqueSectionName);
                                iniData[uniqueSectionName]["Blip_Name"] = data.Name;
                                iniData[uniqueSectionName]["Blip_Icon"] = data.IconId.ToString();
                                iniData[uniqueSectionName]["Blip_Size"] = data.Size.ToString("F2"); 
                                iniData[uniqueSectionName]["Blip_Color"] = data.ColorId.ToString();
                                
                                iniData[uniqueSectionName]["Flashing_State"] = data.IsFlashing ? "ON" : "OFF";
                                iniData[uniqueSectionName]["Flash_Interval"] = "100"; 
                                iniData[uniqueSectionName]["Short_Range_State"] = data.IsShortRange ? "ON" : "OFF";
                                iniData[uniqueSectionName]["X"] = data.Position.X.ToString("F2");
                                iniData[uniqueSectionName]["Y"] = data.Position.Y.ToString("F2");
                                iniData[uniqueSectionName]["Z"] = data.Position.Z.ToString("F2");

                                anyAdded = true;
                                countAdded++;
                                
                            }
                            
                        }

                        if (anyAdded)
                        {
                            SaveIniDataToDisk(iniData);
                            ParseIniAndCreateBlips(); 
                            RefreshManageBlipsMenu(); 
                            Notification.PostTicker($"~g~Added {countAdded} {name} blips to INI and loaded them.", true);
                        }
                        else
                        {
                            
                            Notification.PostTicker($"~w~No new {name} blips to add (or data was empty).", true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DisplayAndLogError(ex, $"Error toggling {name} blips from category button.");
                }
            };

            return buttonItem; 
        }
        
        private void BindKeyAssignment(NativeListItem menuItem, string settingName, string displayName)
        {
            menuItem.Activated += (sender, item) =>
            {
                _waitingForKeyAssignment = true;
                _pendingKeyBind = settingName;
                _pendingListItem = (NativeListItem<string>)menuItem;
                Notification.PostTicker($"~y~Press a key to assign for {displayName}~w~...", true);
            };
        }


        
        private bool IsValidPlayer(Player player)
        {
            return player != null && player.Character != null;
        }
        private bool IsValidName(string name)
        {
            return !string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name) && !name.Contains("_");
        }
        private Vector3 GetPlayerCoordinates(Player player)  
        {
            if (!IsValidPlayer(player))
            {
                Notification.PostTicker("~r~Failed to acquire valid player or character.", true);
                return Vector3.Zero;
            }

            Ped playerCharacter = player.Character;
            Vehicle currentVehicle = playerCharacter.CurrentVehicle;

            if (playerCharacter.IsInVehicle() && currentVehicle.Exists() && currentVehicle != null)
            {
                return currentVehicle.Position;
            }

            return playerCharacter.Position;
        }
        private void ToggleBlipsVisibility()  
        {
            foreach (Blip blip in _activeBlips)
            {
                if (blip != null && blip.Exists())
                {
                    Function.Call(Hash.SET_BLIP_DISPLAY, blip.Handle, _areBlipsVisible ? 0 : 2);
                }
            }

            _areBlipsVisible = !_areBlipsVisible;

            Notification.PostTicker(_areBlipsVisible ? "Blips are now visible." : "Blips are now hidden.", true);
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

            lock (_fileWriteLock)  
            {
                try
                {
                    
                    QzX9_HandleBuffer(_logFilePath);

                    using (StreamWriter logWriter = new StreamWriter(_logFilePath, append: true))
                    {
                        logWriter.WriteLine($"{DateTime.Now}: {methodName}: {ex}");
                        if (contextInfo != null)
                        {
                            logWriter.WriteLine($"Context: {contextInfo}");
                        }
                        logWriter.WriteLine($"Stack Trace: {ex.StackTrace}");
                        logWriter.WriteLine();
                        logWriter.WriteLine(new string('#', 80)); 
                        logWriter.WriteLine();
                    }
                }
                catch (UnauthorizedAccessException logEx)
                {
                    Notification.PostTicker($"~r~Failed to log exception (access denied): {logEx.Message}", true);
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
        }
    }

    public enum BoolOption
    {
        OFF = 0,
        ON = 1
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