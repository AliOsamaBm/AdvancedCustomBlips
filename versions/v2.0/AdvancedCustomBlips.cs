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
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.Concurrent;

using IniParser;
using IniParser.Model;
using NativeUI;

using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;

namespace AdvancedCustomBlips
{
    public class Main : Script
    {
        
        private MenuPool _uiMenuPool;
        private UIMenu _mainMenu, _settingsMenu, _addBlipMenu, _teleportMenu, _editMenu, _categoryMenu;
        private UIMenu _blipManagerMenu = null;

        
        private static HashSet<Blip> _activeBlips = new HashSet<Blip>();  

        
        private static HashSet<Blip> _activeGasStationBlips = new HashSet<Blip>();
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

        private static HashSet<Blip> _activeMarketBlips = new HashSet<Blip>();
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

        private static HashSet<Blip> _activePoliceDepartmentBlips = new HashSet<Blip>();
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

        private static HashSet<Blip> _activeFireDepartmentBlips = new HashSet<Blip>();
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

        private static HashSet<Blip> _activeATMBlips = new HashSet<Blip>();
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

        private static HashSet<Blip> _activeMetroStationBlips = new HashSet<Blip>();
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

        private static HashSet<Blip> _activeMedicalCenterBlips = new HashSet<Blip>();
        private static readonly HashSet<PredefinedBlipData> _medicalCenterCategoryBlips = new HashSet<PredefinedBlipData>
        {
new PredefinedBlipData("Medical Center", new Vector3(355.37f, -596.21f, 74.17f), 61, -1, 1.0f, false, true),
new PredefinedBlipData("Medical Center", new Vector3(341.01f, -1396.80f, 32.51f), 61, -1, 1.0f, false, true),
new PredefinedBlipData("Medical Center", new Vector3(-468.68f, -337.11f, 91.01f), 61, -1, 1.0f, false, true),
new PredefinedBlipData("Medical Center", new Vector3(1840.82f, 3670.38f, 33.68f), 61, -1, 1.0f, false, true),
new PredefinedBlipData("Medical Center", new Vector3(-243.96f, 6327.12f, 37.62f), 61, -1, 1.0f, false, true),
        };

        private Dictionary<UIMenuItem, float> _addBlipCoordinateInputs = new Dictionary<UIMenuItem, float>();  
        private Dictionary<UIMenuItem, float> _editBlipCoordinateInputs = new Dictionary<UIMenuItem, float>();  
        private Dictionary<UIMenuItem, string> _blipNameInputs = new Dictionary<UIMenuItem, string>(); 

        
        private readonly string _iniFilePath = Path.Combine("scripts", "Advanced Custom Blips.ini");  
        private readonly string _logFilePath = Path.Combine("scripts", "Advanced Custom Blips Log.txt");  
        private FileIniDataParser _iniParser;
        private readonly object _fileWriteLock = new object();  

        
        private static bool _hasLoadedBlips = false;  
        private bool _showCoordsOnScreen = false;  
        private bool _showBlipLoadNotification = true;  
        private bool _areBlipsVisible = true;  
        private int _blipCount = 0;  

        
        private Keys? _keyToggleCoords = Keys.F1;  
        private Keys? _keyReloadBlips = Keys.F2;  
        private Keys? _keyToggleBlipVisibility = Keys.F3;  
        private Keys? _keyOpenMenu = Keys.F10; 

        private const string _invalidName = "Invalid name. Name cannot be empty, white space, or contains '_'.";
        private const int _defaultInterval = 1000; 
        private const int _activeInterval = 10;    

        
        private Blip _previewBlip = null;
        private const float _previewDistance = 17.0f; 

        
        private bool _waitingForKeyAssignment = false;
        private string _pendingKeyBind = null; 
        private UIMenuListItem _pendingListItem = null;

        
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
        private struct PredefinedBlipData
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

        public Main()
        {
            _iniParser = new FileIniDataParser();

            _uiMenuPool = new MenuPool();
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
            
            DeleteAllGameBlips(true);
            _hasLoadedBlips = false;
            _blipCount = 0;  
        }
        private void OnTick(object sender, EventArgs e)  
        {
            
            bool needsFastInterval = _uiMenuPool.IsAnyMenuOpen() || 
                                     _showCoordsOnScreen ||         
                                     _waitingForKeyAssignment ||
                                     Function.Call<int>(Hash.UPDATE_ONSCREEN_KEYBOARD) == 0; 

            
            Interval = needsFastInterval ? _activeInterval : _defaultInterval;

            _uiMenuPool.ProcessMenus();

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
                        _pendingListItem.Index = newIndex;

                    _pendingListItem = null;
                }

                
                Dictionary<string, string> kvp = new Dictionary<string, string>
                {
                    ["Toggle_Coordinates_Key"] = _keyToggleCoords.ToString(),
                    ["Reload_Blips_Key"] = _keyReloadBlips.ToString(),
                    ["Toggle_Blips_Visibility_Key"] = _keyToggleBlipVisibility.ToString(),
                    ["Open_Menu_Key"] = _keyOpenMenu.ToString(),
                    ["Show_Blip_Added_Notification"] = _showBlipLoadNotification ? "ON" : "OFF"
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

            
            if (_keyOpenMenu.HasValue && e.KeyCode == _keyOpenMenu.Value && !_uiMenuPool.IsAnyMenuOpen())
            {
                _mainMenu.Visible = !_mainMenu.Visible;
            }
        }


        

        
        public void EnsureDirectoryExists(string filePath)  
        {
            try
            {
                Path.GetFullPath(filePath);  
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
                DisplayAndLogError(ex, $"Error ensuring directory exists for path (access denied): {filePath}. Try this fix to slove the error: Right click on the {filePath} file -> Select and press 'Properties' -> In the 'General' tab -> Look for 'Attributes' -> Make sure that the 'Read-only' checkbox is unchecked.");
            }
            catch (Exception ex)
            {
                DisplayAndLogError(ex, $"Error ensuring directory exists for path: {filePath}");
            }
        }
        private IniData LoadIniDataFromDisk()  
        {
            try
            {
                lock (_fileWriteLock)
                {
                    EnsureDirectoryExists(_iniFilePath);

                    if (!File.Exists(_iniFilePath))
                    {
                        IniData iniData = new IniData();
                        iniData.Sections.AddSection("Settings");

                        iniData["Settings"]["Toggle_Coordinates_Key"] = _keyToggleCoords.ToString();
                        iniData["Settings"]["Reload_Blips_Key"] = _keyReloadBlips.ToString();
                        iniData["Settings"]["Toggle_Blips_Visibility_Key"] = _keyToggleBlipVisibility.ToString();
                        iniData["Settings"]["Open_Menu_Key"] = _keyOpenMenu.ToString();
                        iniData["Settings"]["Show_Blip_Added_Notification"] = "ON";

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
                    EnsureDirectoryExists(_iniFilePath);

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

        
        private void DeleteAllGameBlips(bool deleteCat)  
        {
            ClearBlipSet(_activeBlips);
            if (deleteCat)
            {
                ClearBlipSet(_activeGasStationBlips);
                ClearBlipSet(_activeMarketBlips);
                ClearBlipSet(_activePoliceDepartmentBlips);
                ClearBlipSet(_activeFireDepartmentBlips);
                ClearBlipSet(_activeATMBlips);
                ClearBlipSet(_activeMetroStationBlips);
                ClearBlipSet(_activeMedicalCenterBlips);
            }
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
                
                DeleteAllGameBlips(false);
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

                        string parseErrorMessage;

                        
                        if (!TryParseEnumOrInt<ExtendedBlipSprite>(blipFields["Blip_Icon"], blipSection.SectionName, "Blip_Icon", out ExtendedBlipSprite iconSprite, out parseErrorMessage))
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

            
            _addBlipMenu.OnMenuClose += (sender) =>
            {
                DeletePreviewBlip();
            };

            UIMenuItem nameItem = new UIMenuItem("Name", "This will be the name of the blip in the game and in the INI file.");
            nameItem.Activated += (sourceMenu, clickedItem) =>
            {
                string userInput = PromptUserForInput("Enter the name of the blip");
                if (IsValidName(userInput))
                {
                    nameItem.Text = $"Name ({userInput})";
                    _blipNameInputs[nameItem] = userInput;
                }
                else
                {
                    Notification.PostTicker($"~r~{_invalidName}", true);
                }
            };

            UIMenuListItem iconItem = CreateEnumListItem<ExtendedBlipSprite>("Icon", true, 0, "Choose the icon type for the blip.");

            UIMenuListItem colorItem = CreateEnumListItem<ExtendedBlipColor>("Color", true, 0, "Choose the color of the blip (white for the default color of the blip).");

            UIMenuItem sizeItem = new UIMenuItem("~y~Size/Scale", "It controls the size/scale of the blip; leave it if you want the default size of the blip.");
            
            BindFloatInput(sizeItem, "Enter Size/Scale", "Size", 1.0f, "~y~Using default size.", _addBlipCoordinateInputs);

            UIMenuItem _xInputItem = new UIMenuItem("~y~X Position", "The X coordinate of the blip, leave it if you want to use the current coordinates of the player.");
            BindFloatInput(_xInputItem, "Enter X Position", "X", null, null, _addBlipCoordinateInputs); 

            UIMenuItem _yInputItem = new UIMenuItem("~y~Y Position", "The Y coordinate of the blip, leave it if you want to use the current coordinates of the player.");
            BindFloatInput(_yInputItem, "Enter Y Position", "Y", null, null, _addBlipCoordinateInputs);

            UIMenuItem _zInputItem = new UIMenuItem("~y~Z Position", "The Z coordinate of the blip, leave it if you want to use the current coordinates of the player.");
            BindFloatInput(_zInputItem, "Enter Z Position", "Z", null, null, _addBlipCoordinateInputs);

            UIMenuItem flashIntervalItem = new UIMenuItem("~y~Flash interval", "This adjusts how fast the blip blinks; leave it if you want to use the default flash speed (100 ms). Flash speed is in milliseconds (higher = slower). Please note that if the flashing state is turned off, then this setting will be completely ignored. Turn it on to see the effect.");
            BindFloatInput(flashIntervalItem, "Enter Flash Interval", "Flash Interval", 100f, "~y~Invalid flash interval. Adjusted for 100 ms.", _addBlipCoordinateInputs);

            UIMenuCheckboxItem shortRangeItem = CreateCheckboxItem("Short Range", true, "Blip only shows when nearby.");

            UIMenuCheckboxItem flashItem = CreateCheckboxItem("Flashing", false, "Blinking effect for the blip.");

            
            flashItem.CheckboxEvent += (sender, @checked) => { UpdatePreviewBlip(iconItem, colorItem, sizeItem, flashItem, flashIntervalItem, shortRangeItem); };

            
            iconItem.OnListChanged += (sender, index) => { UpdatePreviewBlip(iconItem, colorItem, sizeItem, flashItem, flashIntervalItem, shortRangeItem); };

            
            colorItem.OnListChanged += (sender, index) => { UpdatePreviewBlip(iconItem, colorItem, sizeItem, flashItem, flashIntervalItem, shortRangeItem); };

            
            shortRangeItem.CheckboxEvent += (sender, @checked) => { UpdatePreviewBlip(iconItem, colorItem, sizeItem, flashItem, flashIntervalItem, shortRangeItem); };

            AddItemToMenu(nameItem, _addBlipMenu);
            AddItemToMenu(iconItem, _addBlipMenu);
            AddItemToMenu(colorItem, _addBlipMenu);
            AddItemToMenu(sizeItem, _addBlipMenu);
            AddItemToMenu(_xInputItem, _addBlipMenu);
            AddItemToMenu(_yInputItem, _addBlipMenu);
            AddItemToMenu(_zInputItem, _addBlipMenu);
            AddItemToMenu(flashItem, _addBlipMenu);
            AddItemToMenu(flashIntervalItem, _addBlipMenu);
            AddItemToMenu(shortRangeItem, _addBlipMenu);

            UIMenuItem saveBlipBtn = new UIMenuItem("~g~Save Blip");
            saveBlipBtn.Activated += (menu, clickedItem) =>
            {
                
                DeletePreviewBlip();

                if (!_blipNameInputs.TryGetValue(nameItem, out string blipName))
                {
                    Notification.PostTicker($"~r~Failed to create blip. {_invalidName} Please enter a valid name.", true);
                    return;
                }
                if (!IsValidName(blipName))
                {
                    Notification.PostTicker($"~r~{_invalidName}", true);
                    return;
                }

                
                float xCoord, yCoord, zCoord;
                bool hasX = _addBlipCoordinateInputs.TryGetValue(_xInputItem, out xCoord);
                bool hasY = _addBlipCoordinateInputs.TryGetValue(_yInputItem, out yCoord);
                bool hasZ = _addBlipCoordinateInputs.TryGetValue(_zInputItem, out zCoord);
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
                    ["Blip_Icon"] = GetSelectedEnumValue<ExtendedBlipSprite>(iconItem).ToString(),
                    ["Blip_Size"] = _addBlipCoordinateInputs.ContainsKey(sizeItem) ? _addBlipCoordinateInputs[sizeItem].ToString("F2") : "1.00",
                    ["Blip_Color"] = GetSelectedEnumValue<ExtendedBlipColor>(colorItem).ToString(),
                    ["Flashing_State"] = flashItem.Checked ? "ON" : "OFF",
                    ["Flash_Interval"] = _addBlipCoordinateInputs.ContainsKey(flashIntervalItem) ? ((int)_addBlipCoordinateInputs[flashIntervalItem]).ToString() : "100",
                    ["Short_Range_State"] = shortRangeItem.Checked ? "ON" : "OFF",
                    ["X"] = pos.Value.X.ToString("F2"),
                    ["Y"] = pos.Value.Y.ToString("F2"),
                    ["Z"] = pos.Value.Z.ToString("F2")
                };

                SaveToIniAndNotify(blipSectionName, keyValuePairs, "~g~Blip saved to INI.");
                RefreshManageBlipsMenu();

                
                _addBlipCoordinateInputs.Remove(_xInputItem); _xInputItem.Text = "~y~X Position";
                _addBlipCoordinateInputs.Remove(_yInputItem); _yInputItem.Text = "~y~Y Position";
                _addBlipCoordinateInputs.Remove(_zInputItem); _zInputItem.Text = "~y~Z Position";
                _addBlipCoordinateInputs.Remove(sizeItem); sizeItem.Text = "~y~Size/Scale";
                _addBlipCoordinateInputs.Remove(flashIntervalItem); flashIntervalItem.Text = "~y~Flash interval";
            };
            AddItemToMenu(saveBlipBtn, _addBlipMenu);

            _mainMenu.BindMenuToItem(_addBlipMenu, new UIMenuItem("Add New Blip"));
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

            UIMenuListItem openMenuKeyItem = CreateEnumListItem<Keys>("Open Menu Key", false, openMenuIndex, "Choose the key to open the main menu.");
            UIMenuListItem toggleCoordKeyItem = CreateEnumListItem<Keys>("Toggle Coordinates Key", false, toggleIndex, "Choose the key for coordinates display.");
            UIMenuListItem reloadKeyItem = CreateEnumListItem<Keys>("Reload Blips Key", false, reloadIndex, "Choose the key to reload script.");
            UIMenuListItem toggleVisibilityKeyItem = CreateEnumListItem<Keys>("Toggle Blip Visibility Key", false, visibilityIndex, "Choose the key for add/loaded blips visibility.");
            UIMenuCheckboxItem notifyBlipItem = CreateCheckboxItem("Show Blip Notification", _showBlipLoadNotification, "Shows a notification when blips are loaded from INI file.");

            BindKeyAssignment(toggleCoordKeyItem, "Toggle_Coordinates_Key", "Toggle Coordinates");
            BindKeyAssignment(reloadKeyItem, "Reload_Blips_Key", "Reload Blips");
            BindKeyAssignment(toggleVisibilityKeyItem, "Toggle_Blips_Visibility_Key", "Toggle Blips Visibility");
            BindKeyAssignment(openMenuKeyItem, "Open_Menu_Key", "Open Menu Key");

            AddItemToMenu(openMenuKeyItem, _settingsMenu);
            AddItemToMenu(toggleCoordKeyItem, _settingsMenu);
            AddItemToMenu(reloadKeyItem, _settingsMenu);
            AddItemToMenu(toggleVisibilityKeyItem, _settingsMenu);
            AddItemToMenu(notifyBlipItem, _settingsMenu);

            _categoryMenu = CreateAndRegisterMenu("Category", "Enable or disable blip category");

            AddItemToMenu(CreateCategoryCheckbox("Gas Station", _gasStationsCategoryBlips, _activeGasStationBlips), _categoryMenu);
            AddItemToMenu(CreateCategoryCheckbox("Market", _marketCategoryBlips, _activeMarketBlips), _categoryMenu);
            AddItemToMenu(CreateCategoryCheckbox("Police Department", _policeDepartmentCategoryBlips, _activePoliceDepartmentBlips), _categoryMenu);
            AddItemToMenu(CreateCategoryCheckbox("Fire Department", _fireDepartmentCategoryBlips, _activeFireDepartmentBlips), _categoryMenu);
            AddItemToMenu(CreateCategoryCheckbox("ATM", _ATMCategoryBlips, _activeATMBlips), _categoryMenu);
            AddItemToMenu(CreateCategoryCheckbox("Metro Station", _metroStationCategoryBlips, _activeMetroStationBlips), _categoryMenu);
            AddItemToMenu(CreateCategoryCheckbox("Medical Center", _medicalCenterCategoryBlips, _activeMedicalCenterBlips), _categoryMenu);


            UIMenuItem saveSettingsBtn = new UIMenuItem("~g~Save Settings");
            saveSettingsBtn.Activated += (sourceMenu, clickedItem) =>
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
                {
                    ["Toggle_Coordinates_Key"] = toggleCoordKeyItem.CurrentItem(),
                    ["Reload_Blips_Key"] = reloadKeyItem.CurrentItem(),
                    ["Toggle_Blips_Visibility_Key"] = toggleVisibilityKeyItem.CurrentItem(),
                    ["Open_Menu_Key"] = openMenuKeyItem.CurrentItem(),
                    ["Show_Blip_Added_Notification"] = notifyBlipItem.Checked ? "ON" : "OFF"
                };

                SaveToIniAndNotify("Settings", keyValuePairs, "~g~Settings saved to INI.");
            };

            _mainMenu.BindMenuToItem(_settingsMenu, new UIMenuItem("Global Settings"));
            _settingsMenu.BindMenuToItem(_categoryMenu, new UIMenuItem("Category Menu"));
            AddItemToMenu(saveSettingsBtn, _settingsMenu);
        }
        private void RefreshManageBlipsMenu()  
        {
            if (_blipManagerMenu == null)
            {
                _blipManagerMenu = CreateAndRegisterMenu("Manage Blips", "Edit, Remove, and Teleport Blips");

                UIMenuItem manageItem = new UIMenuItem("Manage Existing Blips");
                AddItemToMenu(manageItem, _mainMenu);
                _mainMenu.BindMenuToItem(_blipManagerMenu, manageItem);
            }

            
            _blipManagerMenu.Clear();

            IniData iniData = LoadIniDataFromDisk();

            foreach (SectionData blipSection in iniData.Sections)
            {
                if (blipSection.SectionName == "Settings")
                {
                    continue;
                }

                UIMenuItem blipItem = new UIMenuItem(blipSection.SectionName);

                AddItemToMenu(blipItem, _blipManagerMenu);

                blipItem.Activated += (sourceMenu, clickedItem) =>
                {
                    
                    

                    UIMenu editMenu = CreateBlipEditMenu(iniData, blipSection);

                    
                    _blipManagerMenu.BindMenuToItem(editMenu, clickedItem);
                };
            }

            _blipManagerMenu.RefreshIndex();

            
            UIMenuItem removeAllBtn = new UIMenuItem("~r~Remove ALL Blips", "Permanently delete all custom blips from in-game and INI file. There is no undo, confirm with 'yes'");
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

                UIMenuItem item = new UIMenuItem(blipSection.SectionName);
                item.Activated += (m, i) =>
                {
                    TeleportToBlip(blipSection);
                };
                AddItemToMenu(item, _teleportMenu);
            }

            
            UIMenuItem openTeleportMenuBtn = new UIMenuItem("~y~Teleport to a Blip");
            AddItemToMenu(openTeleportMenuBtn, _blipManagerMenu);
            _blipManagerMenu.BindMenuToItem(_teleportMenu, openTeleportMenuBtn);

        }
        private UIMenu CreateBlipEditMenu(IniData iniData, SectionData blipSection)  
        {
            string sectionName = blipSection.SectionName;

            
            Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyData key in blipSection.Keys)
            {
                fields[key.KeyName] = key.Value;
            }

            
            string baseName = sectionName.Substring(0, sectionName.LastIndexOf('_'));
            string idPart = sectionName.Substring(sectionName.LastIndexOf('_') + 1);

            string parseErrorMessage;

            TryParseEnumOrInt<ExtendedBlipSprite>(fields["Blip_Icon"], blipSection.SectionName, "Blip_Icon", out ExtendedBlipSprite iconSprite, out parseErrorMessage);
            TryParseField<float>(fields, "Blip_Size", blipSection.SectionName, out float sizeVal, out parseErrorMessage);
            TryParseEnumOrInt<ExtendedBlipColor>(fields["Blip_Color"], blipSection.SectionName, "Blip_Color", out ExtendedBlipColor colorVal, out parseErrorMessage);
            TryParseField<int>(fields, "Flash_Interval", blipSection.SectionName, out int flashIntervalVal, out parseErrorMessage);
            TryParseField<float>(fields, "X", blipSection.SectionName, out float xVal, out  parseErrorMessage);
            TryParseField<float>(fields, "Y", blipSection.SectionName, out float yVal, out  parseErrorMessage);
            TryParseField<float>(fields, "Z", blipSection.SectionName, out float zVal, out  parseErrorMessage);
            TryParseEnumOrInt<BoolOption>(fields["Short_Range_State"], blipSection.SectionName, "Short_Range_State", out BoolOption shortRangeState, out parseErrorMessage);
            TryParseEnumOrInt<BoolOption>(fields["Flashing_State"], blipSection.SectionName, "Flashing_State", out BoolOption flashingState, out parseErrorMessage);
            
            bool isFlashing = (flashingState == BoolOption.ON);
            bool isShortRange = (shortRangeState == BoolOption.ON);

            
            _editMenu = CreateAndRegisterMenu($"Edit: {baseName}", "Modify Blip Properties");

            
            UIMenuItem nameItem = new UIMenuItem("Name", "This is the name of the blip in the game and in the INI file.");
            nameItem.Text = $"Name ({baseName})";
            _blipNameInputs[nameItem] = baseName;

            nameItem.Activated += (sender, item) =>
            {
                string userInput = PromptUserForInput("Enter the name of the blip", 30);
                if (IsValidName(userInput))
                {
                    nameItem.Text = $"Name ({userInput})";
                    _blipNameInputs[nameItem] = userInput;
                }
                else
                {
                    Notification.PostTicker($"~r~{_invalidName}", true);
                }
            };

            
            
            int iconValue = (int)iconSprite;

            
            List<object> iconList = Enum.GetValues(typeof(ExtendedBlipSprite))
                .Cast<ExtendedBlipSprite>()
                .Select(e => $"{e} ({(int)e})")
                .Cast<object>()
                .ToList();

            
            int iconIndex = iconList.FindIndex(item => item.ToString().EndsWith($"({iconValue})"));

            
            if (iconIndex < 0) iconIndex = 0;

            
            UIMenuListItem iconItem = new UIMenuListItem("Icon", iconList, iconIndex)
            {
                Description = "Choose the icon type for the blip."
            };

            
            
            int colorValue = (int)colorVal;

            
            List<object> colorList = Enum.GetValues(typeof(ExtendedBlipColor))
                .Cast<ExtendedBlipColor>()
                .Select(e => $"{e} ({(int)e})")
                .Cast<object>()
                .ToList();

            
            int colorIndex = colorList.FindIndex(item => item.ToString().EndsWith($"({colorValue})"));

            
            if (colorIndex < 0)
            {
                colorIndex = 0;
            }

            
            UIMenuListItem colorItem = new UIMenuListItem("Color", colorList, colorIndex)
            {
                Description = "Choose the color of the blip (white for the default color of the blip)."
            };

            
            UIMenuItem sizeItem = new UIMenuItem("~y~Size/Scale", "It controls the size/scale of the blip; put 1.0 if you want the default size of the blip.");
            if (sizeVal > 0) _editBlipCoordinateInputs[sizeItem] = sizeVal; 
            sizeItem.Text = sizeVal > 0 ? $"~y~Size/Scale ({sizeVal:F2})" : "~y~Size/Scale";
            BindFloatInput(sizeItem, "Enter Size/Scale", "Size", 1.0f, "~y~Using default size.", _editBlipCoordinateInputs);

            
            UIMenuItem _xInputItem = new UIMenuItem("~y~X Position", "The X coordinate of the blip. Press the update coordinates button then save button if you want to use your current coordinates.");
            _xInputItem.Text = $"~y~X Position ({xVal:F2})";
            _editBlipCoordinateInputs[_xInputItem] = xVal; 
            BindFloatInput(_xInputItem, "Enter X Position", "X", null, null, _editBlipCoordinateInputs);

            UIMenuItem _yInputItem = new UIMenuItem("~y~Y Position", "The Y coordinate of the blip. Press the update coordinates button then save button if you want to use your current coordinates.");
            _yInputItem.Text = $"~y~Y Position ({yVal:F2})";
            _editBlipCoordinateInputs[_yInputItem] = yVal; 
            BindFloatInput(_yInputItem, "Enter Y Position", "Y", null, null, _editBlipCoordinateInputs);

            UIMenuItem _zInputItem = new UIMenuItem("~y~Z Position", "The Z coordinate of the blip. Press the update coordinates button then save button if you want to use your current coordinates.");
            _zInputItem.Text = $"~y~Z Position ({zVal:F2})";
            _editBlipCoordinateInputs[_zInputItem] = zVal; 
            BindFloatInput(_zInputItem, "Enter Z Position", "Z", null, null, _editBlipCoordinateInputs);

            UIMenuItem flashIntervalItem = new UIMenuItem("~y~Flash interval", "This adjusts how fast the blip blinks; put 100 if you want to use the default flash speed. Flash speed is in milliseconds (higher = slower). Please note that if the flashing state is turned off, then this setting will be completely ignored. Turn it on to see the effect.");
            if (flashIntervalVal > 0) _editBlipCoordinateInputs[flashIntervalItem] = flashIntervalVal; 
            flashIntervalItem.Text = flashIntervalVal > 0 ? $"~y~Flash interval ({flashIntervalVal})" : "~y~Flash interval";
            BindFloatInput(flashIntervalItem, "Enter Flash Interval", "Flash Interval", 100f, "~y~Invalid flash interval. Adjusted for 100 ms.", _editBlipCoordinateInputs);

            
            UIMenuCheckboxItem flashItem = CreateCheckboxItem("Flashing", isFlashing, "Blinking effect for the blip.");

            
            UIMenuCheckboxItem shortRangeItem = CreateCheckboxItem("Short Range", isShortRange, "Blip only shows when nearby.");

            
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

            
            UIMenuItem updatePosBtn = new UIMenuItem("~y~Update to Current Position");
            updatePosBtn.Description = "Set X, Y, and Z to your current location.";
            updatePosBtn.Activated += (sender, item) =>
            {
                Player player = Game.Player;
                if (IsValidPlayer(player))
                {
                    Vector3 pos = GetPlayerCoordinates(player);
                    _editBlipCoordinateInputs[_xInputItem] = pos.X; 
                    _editBlipCoordinateInputs[_yInputItem] = pos.Y; 
                    _editBlipCoordinateInputs[_zInputItem] = pos.Z; 
                    _xInputItem.Text = $"~y~X Position ({pos.X:F2})";
                    _yInputItem.Text = $"~y~Y Position ({pos.Y:F2})";
                    _zInputItem.Text = $"~y~Z Position ({pos.Z:F2})";
                    Notification.PostTicker("~y~Position updated to current player coordinates. Press the save button below to apply changes.", true);
                }
            };
            AddItemToMenu(updatePosBtn, _editMenu);

            
            UIMenuItem teleportBtn = new UIMenuItem("~y~Teleport to Blip");
            teleportBtn.Description = "Teleport yourself to this blip's location.";
            teleportBtn.Activated += (sender, item) =>
            {
                TeleportToBlip(blipSection);
            };
            AddItemToMenu(teleportBtn, _editMenu);

            
            UIMenuItem saveBtn = new UIMenuItem("~g~Save Changes");
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

            
            UIMenuItem deleteBtn = new UIMenuItem("~r~Delete Blip");
            deleteBtn.Description = "Permanently remove this blip from game and INI file. There is no undo.";
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

            
            UIMenuItem copyBtn = new UIMenuItem("~y~Copy Blip to Current Position");
            copyBtn.Description = "Create a new blip with the same properties at your current location.";
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
        private UIMenu CreateAndRegisterMenu(string title, string subtitle)
        {
            UIMenu menu = new UIMenu(title, subtitle);
            _uiMenuPool.Add(menu);
            return menu;
        }
        private void UpdatePreviewBlip(UIMenuListItem iconItem, UIMenuListItem colorItem, UIMenuItem sizeItem, UIMenuCheckboxItem flashItem, UIMenuItem flashIntervalItem, UIMenuCheckboxItem shortRangeItem)
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
        public string PromptUserForInput(string title = "Enter text", int maxLength = 30)  
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
        private void BindFloatInput(UIMenuItem item, string keyboardTitle, string labelPrefix, float? defaultValue = null, string fallbackWarning = null, Dictionary<UIMenuItem, float> inputDictionary = null)
        {
            item.Activated += (sourceMenu, clickedItem) =>
            {
                string userInput = PromptUserForInput(keyboardTitle);
                if (float.TryParse(userInput, out float value))
                {
                    item.Text = $"~y~{labelPrefix} ({value})";
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
        private UIMenuListItem CreateEnumListItem<TEnum>(string title, bool indexIncluded, int defaultIndex = 0, string description = null) where TEnum : Enum
        {
            List<object> list = new List<object>();
            if (indexIncluded)
            {
                
                list = Enum.GetValues(typeof(TEnum))
                    .Cast<TEnum>()
                    .Select(e => $"{e} ({Convert.ToInt32(e)})")
                    .Cast<object>()
                    .ToList();
            }
            else
            {
                
                list = Enum.GetNames(typeof(TEnum)).Cast<object>().ToList();
            }

            UIMenuListItem item = new UIMenuListItem(title, list, defaultIndex);
            if (!string.IsNullOrWhiteSpace(description))
            {
                item.Description = description;
            }

            return item;
        }
        private UIMenuCheckboxItem CreateCheckboxItem(string title, bool defaultValue, string description = null)
        {
            UIMenuCheckboxItem item = new UIMenuCheckboxItem(title, defaultValue);
            if (!string.IsNullOrWhiteSpace(description))
            {
                item.Description = description;
            }

            return item;
        }
        private void AddItemToMenu(UIMenuItem item, UIMenu menu)
        {
            menu.AddItem(item);
        }
        private int GetSelectedEnumValue<TEnum>(UIMenuListItem item) where TEnum : Enum
        {
            return Convert.ToInt32(Enum.Parse(typeof(TEnum), item.CurrentItem().Split(' ')[0]));
        }
        private UIMenuCheckboxItem CreateCategoryCheckbox(string name, HashSet<PredefinedBlipData> categoryData, HashSet<Blip> activeSet)
        {
            var checkbox = CreateCheckboxItem(name, false);
            checkbox.CheckboxEvent += (sender, isChecked) =>
            {
                if (isChecked)
                {
                    foreach (var data in categoryData)
                    {
                        Blip blip = World.CreateBlip(data.Position);
                        Function.Call(Hash.SET_BLIP_SPRITE, blip.Handle, data.IconId);
                        Function.Call(Hash.SET_BLIP_COLOUR, blip.Handle, data.ColorId);
                        blip.Scale = data.Size;
                        blip.Name = data.Name;
                        blip.IsFlashing = data.IsFlashing;
                        blip.IsShortRange = data.IsShortRange;
                        activeSet.Add(blip);
                    }
                }
                else
                {
                    foreach (var blip in activeSet)
                    {
                        if (blip.Exists())
                        {
                            blip.Delete();
                        }
                    }
                    activeSet.Clear();
                }
            };
            return checkbox;
        }
        private void BindKeyAssignment(UIMenuListItem menuItem, string settingName, string displayName)
        {
            menuItem.Activated += (sender, item) =>
            {
                _waitingForKeyAssignment = true;
                _pendingKeyBind = settingName;
                _pendingListItem = menuItem;
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
        private void ClearBlipSet(HashSet<Blip> blipSet)
        {
            foreach (var blip in blipSet)
            {
                if (blip != null && blip.Exists())
                {
                    blip.Delete();
                }
            }
            blipSet.Clear();
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
                    
                    EnsureDirectoryExists(_logFilePath);

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
        radar_higher = 0,
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
        radar_mp_crew = 27,
        radar_mp_friendlies = 28,
        radar_cable_car = 36,
        radar_activities = 37,
        radar_raceflag = 38,
        radar_safehouse = 40,
        radar_police = 41,
        radar_police_chase = 42,
        radar_police_heli = 43,
        radar_bomb_a = 44,
        radar_snitch = 47,
        radar_planning_locations = 48,
        radar_crim_carsteal = 50,
        radar_crim_drugs = 51,
        radar_crim_holdups = 52,
        radar_crim_player = 54,
        radar_cop_patrol = 56,
        radar_cop_player = 57,
        radar_crim_wanted = 58,
        radar_heist = 59,
        radar_police_station = 60,
        radar_hospital = 61,
        radar_assassins_mark = 62,
        radar_elevator = 63,
        radar_helicopter = 64,
        radar_random_character = 66,
        radar_security_van = 67,
        radar_tow_truck = 68,
        radar_illegal_parking = 70,
        radar_barber = 71,
        radar_car_mod_shop = 72,
        radar_clothes_store = 73,
        radar_tattoo = 75,
        radar_armenian_family = 76,
        radar_lester_family = 77,
        radar_michael_family = 78,
        radar_trevor_family = 79,
        radar_jewelry_heist = 80,
        radar_drag_race_finish = 82,
        radar_rampage = 84,
        radar_vinewood_tours = 85,
        radar_lamar_family = 86,
        radar_franklin_family = 88,
        radar_chinese_strand = 89,
        radar_flight_school = 90,
        radar_eye_sky = 91,
        radar_air_hockey = 92,
        radar_bar = 93,
        radar_base_jump = 94,
        radar_basketball = 95,
        radar_biolab_heist = 96,
        radar_cabaret_club = 99,
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
        radar_internet_cafe = 111,
        radar_michael_family_exile = 112,
        radar_nice_house_heist = 113,
        radar_random_female = 114,
        radar_random_male = 115,
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
        radar_airport = 138,
        radar_crim_saved_vehicle = 139,
        radar_weed_stash = 140,
        radar_hunting = 141,
        radar_pool = 142,
        radar_objective_blue = 143,
        radar_objective_green = 144,
        radar_objective_red = 145,
        radar_objective_yellow = 146,
        radar_arms_dealing = 147,
        radar_mp_friend = 148,
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
        radar_weapon_armour = 175,
        radar_property_takeover = 176,
        radar_gang_mexicans_highlight = 177,
        radar_gang_bikers_highlight = 178,
        radar_triathlon_cycling = 179,
        radar_triathlon_swimming = 180,
        radar_property_takeover_bikers = 181,
        radar_property_takeover_cops = 182,
        radar_property_takeover_vagos = 183,
        radar_camera = 184,
        radar_centre_red = 185,
        radar_handcuff_keys_bikers = 186,
        radar_handcuff_keys_vagos = 187,
        radar_handcuffs_closed_bikers = 188,
        radar_handcuffs_closed_vagos = 189,
        radar_camera_badger = 192,
        radar_camera_facade = 193,
        radar_camera_ifruit = 194,
        radar_yoga = 197,
        radar_taxi = 198,
        radar_shrink = 205,
        radar_epsilon = 206,
        radar_financier_strand_grey = 207,
        radar_trevor_family_grey = 208,
        radar_trevor_family_red = 209,
        radar_franklin_family_grey = 210,
        radar_franklin_family_blue = 211,
        radar_franklin_a = 212,
        radar_franklin_b = 213,
        radar_franklin_c = 214,
        radar_gang_vehicle = 225,
        radar_gang_vehicle_bikers = 226,
        radar_gang_vehicle_cops = 227,
        radar_gang_vehicle_vagos = 228,
        radar_guncar = 229,
        radar_driving_bikers = 230,
        radar_driving_cops = 231,
        radar_driving_vagos = 232,
        radar_gang_cops_highlight = 233,
        radar_shield_bikers = 234,
        radar_shield_cops = 235,
        radar_shield_vagos = 236,
        radar_custody_bikers = 237,
        radar_custody_vagos = 238,
        radar_arms_dealing_air = 251,
        radar_playerstate_arrested = 252,
        radar_playerstate_custody = 253,
        radar_playerstate_driving = 254,
        radar_playerstate_keyholder = 255,
        radar_playerstate_partner = 256,
        radar_ztype = 262,
        radar_stinger = 263,
        radar_packer = 264,
        radar_monroe = 265,
        radar_fairground = 266,
        radar_property = 267,
        radar_gang_highlight = 268,
        radar_altruist = 269,
        radar_ai = 270,
        radar_on_mission = 271,
        radar_cash_pickup = 272,
        radar_chop = 273,
        radar_dead = 274,
        radar_territory_locked = 275,
        radar_cash_lost = 276,
        radar_cash_vagos = 277,
        radar_cash_cops = 278,
        radar_hooker = 279,
        radar_friend = 280,
        radar_mission_2to4 = 281,
        radar_mission_2to8 = 282,
        radar_mission_2to12 = 283,
        radar_mission_2to16 = 284,
        radar_custody_dropoff = 285,
        radar_onmission_cops = 286,
        radar_onmission_lost = 287,
        radar_onmission_vagos = 288,
        radar_crim_carsteal_cops = 289,
        radar_crim_carsteal_bikers = 290,
        radar_crim_carsteal_vagos = 291,
        radar_band_strand = 292,
        radar_simeon_family = 293,
        radar_mission_1 = 294,
        radar_mission_2 = 295,
        radar_friend_darts = 296,
        radar_friend_comedyclub = 297,
        radar_friend_cinema = 298,
        radar_friend_tennis = 299,
        radar_friend_stripclub = 300,
        radar_friend_livemusic = 301,
        radar_friend_golf = 302,
        radar_bounty_hit = 303,
        radar_ugc_mission = 304,
        radar_horde = 305,
        radar_cratedrop = 306,
        radar_plane_drop = 307,
        radar_sub = 308,
        radar_race = 309,
        radar_deathmatch = 310,
        radar_arm_wrestling = 311,
        radar_mission_1to2 = 312,
        radar_shootingrange_gunshop = 313,
        radar_race_air = 314,
        radar_race_land = 315,
        radar_race_sea = 316,
        radar_tow = 317,
        radar_garbage = 318,
        radar_drill = 319,
        radar_spikes = 320,
        radar_firetruck = 321,
        radar_minigun2 = 322,
        radar_bugstar = 323,
        radar_submarine = 324,
        radar_chinook = 325,
        radar_getaway_car = 326,
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
        radar_gang_bike = 348,
        radar_gas_grenade = 349,
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
        radar_placeholder_6 = 373,
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
        radar_cratedrop_background = 391,
        radar_green_and_net_player1 = 392,
        radar_green_and_net_player2 = 393,
        radar_green_and_net_player3 = 394,
        radar_green_and_friendly = 395,
        radar_net_player1_and_net_player2 = 396,
        radar_net_player1_and_net_player3 = 397,
        radar_creator = 398,
        radar_creator_direction = 399,
        radar_abigail = 400,
        radar_blimp = 401,
        radar_repair = 402,
        radar_testosterone = 403,
        radar_dinghy = 404,
        radar_fanatic = 405,
        radar_info_icon = 407,
        radar_capture_the_flag = 408,
        radar_last_team_standing = 409,
        radar_boat = 410,
        radar_capture_the_flag_base = 411,
        radar_capture_the_flag_outline = 412,
        radar_capture_the_flag_base_nobag = 413,
        radar_weapon_jerrycan = 414,
        radar_rp = 415,
        radar_level_inside = 416,
        radar_bounty_hit_inside = 417,
        radar_capture_the_usaflag = 418,
        radar_capture_the_usaflag_outline = 419,
        radar_tank = 420,
        radar_player_heli = 421,
        radar_player_plane = 422,
        radar_player_jet = 423,
        radar_centre_stroke = 424,
        radar_player_guncar = 425,
        radar_player_boat = 426,
        radar_mp_heist = 427,
        radar_temp_1 = 428,
        radar_temp_2 = 429,
        radar_temp_3 = 430,
        radar_temp_4 = 431,
        radar_temp_5 = 432,
        radar_temp_6 = 433,
        radar_race_stunt = 434,
        radar_hot_property = 435,
        radar_urbanwarfare_versus = 436,
        radar_king_of_the_castle = 437,
        radar_player_king = 438,
        radar_dead_drop = 439,
        radar_penned_in = 440,
        radar_beast = 441,
        radar_edge_pointer = 442,
        radar_edge_crosstheline = 443,
        radar_mp_lamar = 444,
        radar_bennys = 445,
        radar_corner_number_1 = 446,
        radar_corner_number_2 = 447,
        radar_corner_number_3 = 448,
        radar_corner_number_4 = 449,
        radar_corner_number_5 = 450,
        radar_corner_number_6 = 451,
        radar_corner_number_7 = 452,
        radar_corner_number_8 = 453,
        radar_yacht = 454,
        radar_finders_keepers = 455,
        radar_assault_package = 456,
        radar_hunt_the_boss = 457,
        radar_sightseer = 458,
        radar_turreted_limo = 459,
        radar_belly_of_the_beast = 460,
        radar_yacht_location = 461,
        radar_pickup_beast = 462,
        radar_pickup_zoned = 463,
        radar_pickup_random = 464,
        radar_pickup_slow_time = 465,
        radar_pickup_swap = 466,
        radar_pickup_thermal = 467,
        radar_pickup_weed = 468,
        radar_weapon_railgun = 469,
        radar_seashark = 470,
        radar_pickup_hidden = 471,
        radar_warehouse = 472,
        radar_warehouse_for_sale = 473,
        radar_office = 474,
        radar_office_for_sale = 475,
        radar_truck = 476,
        radar_contraband = 477,
        radar_trailer = 478,
        radar_vip = 479,
        radar_cargobob = 480,
        radar_area_outline_blip = 481,
        radar_pickup_accelerator = 482,
        radar_pickup_ghost = 483,
        radar_pickup_detonator = 484,
        radar_pickup_bomb = 485,
        radar_pickup_armoured = 486,
        radar_stunt = 487,
        radar_weapon_lives = 488,
        radar_stunt_premium = 489,
        radar_adversary = 490,
        radar_biker_clubhouse = 491,
        radar_biker_caged_in = 492,
        radar_biker_turf_war = 493,
        radar_biker_joust = 494,
        radar_production_weed = 495,
        radar_production_crack = 496,
        radar_production_fake_id = 497,
        radar_production_meth = 498,
        radar_production_money = 499,
        radar_package = 500,
        radar_capture_1 = 501,
        radar_capture_2 = 502,
        radar_capture_3 = 503,
        radar_capture_4 = 504,
        radar_capture_5 = 505,
        radar_capture_6 = 506,
        radar_capture_7 = 507,
        radar_capture_8 = 508,
        radar_capture_9 = 509,
        radar_capture_10 = 510,
        radar_quad = 511,
        radar_bus = 512,
        radar_drugs_package = 513,
        radar_pickup_jump = 514,
        radar_adversary_4 = 515,
        radar_adversary_8 = 516,
        radar_adversary_10 = 517,
        radar_adversary_12 = 518,
        radar_adversary_16 = 519,
        radar_laptop = 520,
        radar_pickup_deadline = 521,
        radar_sports_car = 522,
        radar_warehouse_vehicle = 523,
        radar_reg_papers = 524,
        radar_police_station_dropoff = 525,
        radar_junkyard = 526,
        radar_ex_vech_1 = 527,
        radar_ex_vech_2 = 528,
        radar_ex_vech_3 = 529,
        radar_ex_vech_4 = 530,
        radar_ex_vech_5 = 531,
        radar_ex_vech_6 = 532,
        radar_ex_vech_7 = 533,
        radar_target_a = 534,
        radar_target_b = 535,
        radar_target_c = 536,
        radar_target_d = 537,
        radar_target_e = 538,
        radar_target_f = 539,
        radar_target_g = 540,
        radar_target_h = 541,
        radar_jugg = 542,
        radar_pickup_repair = 543,
        radar_steeringwheel = 544,
        radar_trophy = 545,
        radar_pickup_rocket_boost = 546,
        radar_pickup_homing_rocket = 547,
        radar_pickup_machinegun = 548,
        radar_pickup_parachute = 549,
        radar_pickup_time_5 = 550,
        radar_pickup_time_10 = 551,
        radar_pickup_time_15 = 552,
        radar_pickup_time_20 = 553,
        radar_pickup_time_30 = 554,
        radar_supplies = 555,
        radar_property_bunker = 556,
        radar_gr_wvm_1 = 557,
        radar_gr_wvm_2 = 558,
        radar_gr_wvm_3 = 559,
        radar_gr_wvm_4 = 560,
        radar_gr_wvm_5 = 561,
        radar_gr_wvm_6 = 562,
        radar_gr_covert_ops = 563,
        radar_adversary_bunker = 564,
        radar_gr_moc_upgrade = 565,
        radar_gr_w_upgrade = 566,
        radar_sm_cargo = 567,
        radar_sm_hangar = 568,
        radar_tf_checkpoint = 569,
        radar_race_tf = 570,
        radar_sm_wp1 = 571,
        radar_sm_wp2 = 572,
        radar_sm_wp3 = 573,
        radar_sm_wp4 = 574,
        radar_sm_wp5 = 575,
        radar_sm_wp6 = 576,
        radar_sm_wp7 = 577,
        radar_sm_wp8 = 578,
        radar_sm_wp9 = 579,
        radar_sm_wp10 = 580,
        radar_sm_wp11 = 581,
        radar_sm_wp12 = 582,
        radar_sm_wp13 = 583,
        radar_sm_wp14 = 584,
        radar_nhp_bag = 585,
        radar_nhp_chest = 586,
        radar_nhp_orbit = 587,
        radar_nhp_veh1 = 588,
        radar_nhp_base = 589,
        radar_nhp_overlay = 590,
        radar_nhp_turret = 591,
        radar_nhp_mg_firewall = 592,
        radar_nhp_mg_node = 593,
        radar_nhp_wp1 = 594,
        radar_nhp_wp2 = 595,
        radar_nhp_wp3 = 596,
        radar_nhp_wp4 = 597,
        radar_nhp_wp5 = 598,
        radar_nhp_wp6 = 599,
        radar_nhp_wp7 = 600,
        radar_nhp_wp8 = 601,
        radar_nhp_wp9 = 602,
        radar_nhp_cctv = 603,
        radar_nhp_starterpack = 604,
        radar_nhp_turret_console = 605,
        radar_nhp_mg_mir_rotate = 606,
        radar_nhp_mg_mir_static = 607,
        radar_nhp_mg_proxy = 608,
        radar_acsr_race_target = 609,
        radar_acsr_race_hotring = 610,
        radar_acsr_wp1 = 611,
        radar_acsr_wp2 = 612,
        radar_bat_club_property = 613,
        radar_bat_cargo = 614,
        radar_bat_truck = 615,
        radar_bat_hack_jewel = 616,
        radar_bat_hack_gold = 617,
        radar_bat_keypad = 618,
        radar_bat_hack_target = 619,
        radar_pickup_dtb_health = 620,
        radar_pickup_dtb_blast_increase = 621,
        radar_pickup_dtb_blast_decrease = 622,
        radar_pickup_dtb_bomb_increase = 623,
        radar_pickup_dtb_bomb_decrease = 624,
        radar_bat_rival_club = 625,
        radar_bat_drone = 626,
        radar_bat_cash_reg = 627,
        radar_cctv = 628,
        radar_bat_assassinate = 629,
        radar_bat_pbus = 630,
        radar_bat_wp1 = 631,
        radar_bat_wp2 = 632,
        radar_bat_wp3 = 633,
        radar_bat_wp4 = 634,
        radar_bat_wp5 = 635,
        radar_bat_wp6 = 636,
        radar_blimp_2 = 637,
        radar_oppressor_2 = 638,
        radar_bat_wp7 = 639,
        radar_arena_series = 640,
        radar_arena_premium = 641,
        radar_arena_workshop = 642,
        radar_race_wars = 643,
        radar_arena_turret = 644,
        radar_arena_rc_car = 645,
        radar_arena_rc_workshop = 646,
        radar_arena_trap_fire = 647,
        radar_arena_trap_flip = 648,
        radar_arena_trap_sea = 649,
        radar_arena_trap_turn = 650,
        radar_arena_trap_pit = 651,
        radar_arena_trap_mine = 652,
        radar_arena_trap_bomb = 653,
        radar_arena_trap_wall = 654,
        radar_arena_trap_brd = 655,
        radar_arena_trap_sbrd = 656,
        radar_arena_bruiser = 657,
        radar_arena_brutus = 658,
        radar_arena_cerberus = 659,
        radar_arena_deathbike = 660,
        radar_arena_dominator = 661,
        radar_arena_impaler = 662,
        radar_arena_imperator = 663,
        radar_arena_issi = 664,
        radar_arena_sasquatch = 665,
        radar_arena_scarab = 666,
        radar_arena_slamvan = 667,
        radar_arena_zr380 = 668,
        radar_ap = 669,
        radar_comic_store = 670,
        radar_cop_car = 671,
        radar_rc_time_trials = 672,
        radar_king_of_the_hill = 673,
        radar_king_of_the_hill_teams = 674,
        radar_rucksack = 675,
        radar_shipping_container = 676,
        radar_agatha = 677,
        radar_casino = 678,
        radar_casino_table_games = 679,
        radar_casino_wheel = 680,
        radar_casino_concierge = 681,
        radar_casino_chips = 682,
        radar_casino_horse_racing = 683,
        radar_adversary_featured = 684,
        radar_roulette_1 = 685,
        radar_roulette_2 = 686,
        radar_roulette_3 = 687,
        radar_roulette_4 = 688,
        radar_roulette_5 = 689,
        radar_roulette_6 = 690,
        radar_roulette_7 = 691,
        radar_roulette_8 = 692,
        radar_roulette_9 = 693,
        radar_roulette_10 = 694,
        radar_roulette_11 = 695,
        radar_roulette_12 = 696,
        radar_roulette_13 = 697,
        radar_roulette_14 = 698,
        radar_roulette_15 = 699,
        radar_roulette_16 = 700,
        radar_roulette_17 = 701,
        radar_roulette_18 = 702,
        radar_roulette_19 = 703,
        radar_roulette_20 = 704,
        radar_roulette_21 = 705,
        radar_roulette_22 = 706,
        radar_roulette_23 = 707,
        radar_roulette_24 = 708,
        radar_roulette_25 = 709,
        radar_roulette_26 = 710,
        radar_roulette_27 = 711,
        radar_roulette_28 = 712,
        radar_roulette_29 = 713,
        radar_roulette_30 = 714,
        radar_roulette_31 = 715,
        radar_roulette_32 = 716,
        radar_roulette_33 = 717,
        radar_roulette_34 = 718,
        radar_roulette_35 = 719,
        radar_roulette_36 = 720,
        radar_roulette_0 = 721,
        radar_roulette_00 = 722,
        radar_limo = 723,
        radar_weapon_alien = 724,
        radar_race_open_wheel = 725,
        radar_rappel = 726,
        radar_swap_car = 727,
        radar_scuba_gear = 728,
        radar_cpanel_1 = 729,
        radar_cpanel_2 = 730,
        radar_cpanel_3 = 731,
        radar_cpanel_4 = 732,
        radar_snow_truck = 733,
        radar_buggy_1 = 734,
        radar_buggy_2 = 735,
        radar_zhaba = 736,
        radar_gerald = 737,
        radar_ron = 738,
        radar_arcade = 739,
        radar_drone_controls = 740,
        radar_rc_tank = 741,
        radar_stairs = 742,
        radar_camera_2 = 743,
        radar_winky = 744,
        radar_mini_sub = 745,
        radar_kart_retro = 746,
        radar_kart_modern = 747,
        radar_military_quad = 748,
        radar_military_truck = 749,
        radar_ship_wheel = 750,
        radar_ufo = 751,
        radar_seasparrow2 = 752,
        radar_dinghy2 = 753,
        radar_patrol_boat = 754,
        radar_retro_sports_car = 755,
        radar_squadee = 756,
        radar_folding_wing_jet = 757,
        radar_valkyrie2 = 758,
        radar_sub2 = 759,
        radar_bolt_cutters = 760,
        radar_rappel_gear = 761,
        radar_keycard = 762,
        radar_password = 763,
        radar_island_heist_prep = 764,
        radar_island_party = 765,
        radar_control_tower = 766,
        radar_underwater_gate = 767,
        radar_power_switch = 768,
        radar_compound_gate = 769,
        radar_rappel_point = 770,
        radar_keypad = 771,
        radar_sub_controls = 772,
        radar_sub_periscope = 773,
        radar_sub_missile = 774,
        radar_painting = 775,
        radar_car_meet = 776,
        radar_car_test_area = 777,
        radar_auto_shop_property = 778,
        radar_docks_export = 779,
        radar_prize_car = 780,
        radar_test_car = 781,
        radar_car_robbery_board = 782,
        radar_car_robbery_prep = 783,
        radar_street_race_series = 784,
        radar_pursuit_series = 785,
        radar_car_meet_organiser = 786,
        radar_securoserv = 787,
        radar_bounty_collectibles = 788,
        radar_movie_collectibles = 789,
        radar_trailer_ramp = 790,
        radar_race_organiser = 791,
        radar_chalkboard_list = 792,
        radar_export_vehicle = 793,
        radar_train = 794,
        radar_heist_diamond = 795,
        radar_heist_doomsday = 796,
        radar_heist_island = 797,
        radar_slamvan2 = 798,
        radar_crusader = 799,
        radar_construction_outfit = 800,
        radar_overlay_jammed = 801,
        radar_heist_island_unavailable = 802,
        radar_heist_diamond_unavailable = 803,
        radar_heist_doomsday_unavailable = 804,
        radar_placeholder_7 = 805,
        radar_placeholder_8 = 806,
        radar_placeholder_9 = 807,
        radar_featured_series = 808,
        radar_vehicle_for_sale = 809,
        radar_van_keys = 810,
        radar_suv_service = 811,
        radar_security_contract = 812,
        radar_safe = 813,
        radar_ped_r = 814,
        radar_ped_e = 815,
        radar_payphone = 816,
        radar_patriot3 = 817,
        radar_music_studio = 818,
        radar_jubilee = 819,
        radar_granger2 = 820,
        radar_explosive_charge = 821,
        radar_deity = 822,
        radar_d_champion = 823,
        radar_buffalo4 = 824,
        radar_agency = 825,
        radar_biker_bar = 826,
        radar_simeon_overlay = 827,
        radar_junk_skydive = 828,
        radar_luxury_car_showroom = 829,
        radar_car_showroom = 830,
        radar_car_showroom_simeon = 831,
        radar_flaming_skull = 832,
        radar_weapon_ammo = 833,
        radar_community_series = 834,
        radar_cayo_series = 835,
        radar_clubhouse_contract = 836,
        radar_agent_ulp = 837,
        radar_acid = 838,
        radar_acid_lab = 839,
        radar_dax_overlay = 840,
        radar_dead_drop_package = 841,
        radar_downtown_cab = 842,
        radar_gun_van = 843,
        radar_stash_house = 844,
        radar_tractor = 845,
        radar_warehouse_juggalo = 846,
        radar_warehouse_juggalo_dax = 847,
        radar_weapon_crowbar = 848,
        radar_duffel_bag = 849,
        radar_oil_tanker = 850,
        radar_acid_lab_tent = 851,
        radar_van_burrito = 852,
        radar_acid_boost = 853,
        radar_ped_gang_leader = 854,
        radar_multistorey_garage = 855,
        radar_seized_asset_sales = 856,
        radar_cayo_attrition = 857,
        radar_bicycle = 858,
        radar_bicycle_trial = 859,
        radar_raiju = 860,
        radar_conada2 = 861,
        radar_overlay_ready_for_sell = 862,
        radar_overlay_missing_supplies = 863,
        radar_streamer216 = 864,
        radar_signal_jammer = 865,
        radar_salvage_yard = 866,
        radar_robbery_prep_equipment = 867,
        radar_robbery_prep_overlay = 868,
        radar_yusuf = 869,
        radar_vincent = 870,
        radar_vinewood_garage = 871,
        radar_lstb = 872,
        radar_cctv_workstation = 873,
        radar_hacking_device = 874,
        radar_race_drag = 875,
        radar_race_drift = 876,
        radar_casino_prep = 877,
        radar_planning_wall = 878,
        radar_weapon_crate = 879,
        radar_weapon_snowball = 880,
        radar_train_signals_green = 881,
        radar_train_signals_red = 882,
        radar_office_transporter = 883,
        radar_yankton_survival = 884,
        radar_daily_bounty = 885,
        radar_bounty_target = 886,
        radar_filming_schedule = 887,
        radar_pizza_this = 888,
        radar_aircraft_carrier = 889,
        radar_weapon_emp = 890,
        radar_maude_eccles = 891,
        radar_bail_bonds_office = 892,
        radar_weapon_emp_mine = 893,
        radar_zombie_disease = 894,
        radar_zombie_proximity = 895,
        radar_zombie_fire = 896,
        radar_animal_possessed = 897,
        radar_mobile_phone = 898,
        radar_garment_factory = 899,
        radar_garment_factory_for_sale = 900,
        radar_garment_factory_equipment = 901,
        radar_field_hangar = 902,
        radar_field_hangar_for_sale = 903,
        radar_cargobob_ch53 = 904,
        radar_chopper_lift_ammo = 905,
        radar_chopper_lift_armor = 906,
        radar_chopper_lift_explosives = 907,
        radar_chopper_lift_upgrade = 908,
        radar_chopper_lift_weapon = 909,
        radar_cargo_ship = 910,
        radar_submarine_missile = 911,
        radar_propeller_engine = 912,
        radar_shark = 913,
        radar_fast_travel = 914,
        radar_plane_duster2 = 915,
        radar_plane_titan2 = 916,
        radar_collectible = 917,
        radar_field_hangar_discount = 918,
        radar_garment_factory_discount = 919,
        radar_weapon_gusenberg_sweeper = 920,
        radar_weapon_gusenb = 921,
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