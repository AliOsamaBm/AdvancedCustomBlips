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
using System.Windows.Forms;
using System.Collections.Generic;

using IniParser;
using IniParser.Model;

using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;

public class CustomBlips : Script
{
    private static List<Blip> blips = new List<Blip>();
    private string iniFilePath = Path.Combine("scripts", "Custom_Blips.ini");
    public string LogFilePath = Path.Combine("scripts", "Custom_Blips_Log.txt");
    private FileIniDataParser iniParser;
    private readonly object fileLock = new object();
    private static bool blipsLoaded = false;
    private bool showCoordinates = false;

    public CustomBlips()
    {
        iniParser = new FileIniDataParser();
        Tick += OnTick;
        KeyDown += OnKeyDown;
        Aborted += OnAborted;
        Interval = 1000;
        if (blipsLoaded)
            return;
        LoadBlipsFromIni();
        blipsLoaded = true;
    }

    private void OnAborted(object sender, EventArgs e)
    {
        ClearAllBlips();
        blipsLoaded = false;
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (!showCoordinates)
            return;
        Vector3 position = Game.Player.Character.Position;

        Function.Call(Hash.BEGIN_TEXT_COMMAND_PRINT, "CELL_EMAIL_BCON");
        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, $"~b~X: ~w~{position.X:0.00} ~b~Y: ~w~{position.Y:0.00} ~b~Z: ~w~{position.Z:0.00}");
        Function.Call(Hash.END_TEXT_COMMAND_PRINT);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.F1)
            return;
        showCoordinates = !showCoordinates;
        if (showCoordinates)
            Notification.PostTicker("Coordinates display enabled", true);
        else
            Notification.PostTicker("Coordinates display disabled", true);
    }

    private void LoadBlipsFromIni()
    {
        try
        {
            ClearAllBlips();
            if (!File.Exists(iniFilePath))
            {
                Notification.PostTicker("~r~Custom_Blips.ini not found in scripts folder.", true);
            }
            else
            {
                IniData iniData = iniParser.ReadFile(iniFilePath);
                foreach (SectionData section in iniData.Sections)
                {
                    try
                    {
                        string name = iniData[section.SectionName]["Blip_Name"];
                        int icon = int.Parse(iniData[section.SectionName]["Blip_Icon"]);
                        float size = float.Parse(iniData[section.SectionName]["Blip_Size"]);
                        int color = int.Parse(iniData[section.SectionName]["Blip_Color"]);
                        string flashState = iniData[section.SectionName]["Flashing_State"];
                        int flashInterval = int.Parse(iniData[section.SectionName]["Flash_Interval"]);
                        float x = float.Parse(iniData[section.SectionName]["X"]);
                        float y = float.Parse(iniData[section.SectionName]["Y"]);
                        float z = float.Parse(iniData[section.SectionName]["Z"]);
                        string shortRange= iniData[section.SectionName]["Short_Range_State"];
                        Vector3 vector3;

                        Vector3 blipCoord = new Vector3(x, y, z);
                        Blip blip = World.CreateBlip(blipCoord);
                        blip.Sprite = (BlipSprite)icon;
                        blip.Name = name;
                        if (shortRange == "ON" || shortRange == "on" || shortRange == "On" || shortRange == "oN")
                            blip.IsShortRange = true;
                        else if (shortRange == "OFF" || shortRange == "off" || shortRange == "oFF" || shortRange == "ofF" || shortRange == "OFf" || shortRange == "OfF" || shortRange == "Off" || shortRange == "oFf")
                            blip.IsShortRange = false;
                        blip.Scale = size;
                        if (color != -1)
                            blip.Color = (BlipColor)color;
                        if (flashState == "ON" || flashState == "on" || flashState == "On" || flashState == "oN")
                            blip.IsFlashing = true;
                        else if (flashState == "OFF" || flashState == "off" || flashState == "oFF" || flashState == "ofF" || flashState == "OFf" || flashState == "OfF" || flashState == "Off" || flashState == "oFf")
                            blip.IsFlashing = false;
                        if (flashInterval != -1 && flashInterval > 0)
                            blip.FlashInterval = flashInterval;
                        blips.Add(blip);
                        Notification.PostTicker("Added blip: " + name, true);
                    }
                    catch (Exception ex)
                    {
                        NotifyAndLogError(ex, $"Error processing {section.SectionName}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            NotifyAndLogError(ex, "Error in LoadBlipsFromIni method");
        }
    }

    private void ClearAllBlips()
    {
        foreach (Blip blip in blips)
        {
            if (blip != null && blip.Exists())
                blip.Delete();
        }
        blips.Clear();
    }

    private void NotifyAndLogError(Exception ex, string contextInfo = null)
    {
        string methodName = ex.TargetSite?.Name ?? "Unknown Method";
        string innerMessage = ex.InnerException?.Message != null ? $"\nInner Exception: {ex.InnerException.Message}" : string.Empty;
        string message = $"~r~Error in {methodName}: {ex.Message}{innerMessage}";

        if (!string.IsNullOrEmpty(contextInfo))
        {
            message += $"\nContext: {contextInfo}";
        }

        Notification.PostTicker(message, true);

        lock (fileLock) 
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(LogFilePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (StreamWriter logWriter = new StreamWriter(LogFilePath, append: true))
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
                Notification.PostTicker($"Directory not found: {Path.GetDirectoryName(LogFilePath)}", true);
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