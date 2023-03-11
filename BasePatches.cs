using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using UnityEngine;

namespace HuniecamSpeedrunMod
{
    public class BasePatches
    {
        public static BepInEx.Logging.ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("BasePatches");

        public static UnityEngine.UI.Text newText;
        public static int searchForMe;
        public static int ASLDayNum;
        public static int ASLFansNum;
        public static int ASLSaveFile;

        //for the autosplitter
        public static void InitSearchForMe()
        {
            searchForMe = 123456789;
            ASLDayNum = 1;
            ASLFansNum = 0;
            ASLSaveFile = -1;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UiTopPanel), "Update")]
        public static void CheckFans(TextRollBehavior ____textRollFans)
        {
            searchForMe = 123456789;
            ASLFansNum = (int)AccessTools.Field(typeof(TextRollBehavior), "_tempValue").GetValue(____textRollFans);
            ASLDayNum = Game.Manager.Clock.CalendarDay(-1, false);
            ASLSaveFile = Game.Persistence.activeSaveFileIndex;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UiLoadWindowFile), "RestartThisFile")]
        public static void NewSession()
        {
            ASLSaveFile = Game.Persistence.activeSaveFileIndex;
            ASLDayNum = 1;
            ASLFansNum = 0;
            if (!HCSMod.cheatsEnabled)
                searchForMe = 111;
        }


        //draw mod info on title screen using high score text dupe
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UiTitleScreen), "Start")]
        public static void ModInfo(UiTitleScreen __instance)
        {
            newText = UnityEngine.Object.Instantiate(__instance.highscoreValue);
            newText.transform.SetParent(__instance.transform);
            if (Game.Persistence.saveData.highScore > 0) newText.transform.SetParent(__instance.highscoreCanvasGroup.transform);
            newText.text = "";
            newText.transform.position = new Vector3(10, 1075);
            newText.fontSize = 24;
            newText.alignment = TextAnchor.UpperLeft;

            CheatPatches.lastCheatText = "Cheat Mode Enabled!";
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UiTitleScreen), "Update")]
        public static void ModInfoUpdate(UiTitleScreen __instance)
        {
            //add mod version and settings info to the corner
            string version = "V1.0";
            if (HCSMod.GameVersion() == HCSMod.V102) version = "V1.0.2";

            string newText1 = "Speedrun Mod V" + HCSMod.PluginVersion + " (Game " + version + ")";
            string newText2 = "";
            if (HCSMod.VsyncEnabled.Value)
                newText2 += "Vsync On (" + Screen.currentResolution.refreshRate + ")";
            else
                newText2 += HCSMod.FramerateCap.Value + " FPS Lock";

            if (HCSMod.cheatsEnabled) newText2 += "\n" + CheatPatches.lastCheatText;
            newText.text = newText1 + "\n" + newText2;
        }

        /*
        public static int framecounter = 0;
        public static int lastsec = 0;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ClockManager), "Update")]
        public static void OverflowClockPre(float ____gameMinuteTimestamp, ref float __state)
        {
            if (Game.Manager.Lifetime(true) - ____gameMinuteTimestamp >= 0.1f)
            {
                __state = Game.Manager.Lifetime(true) - ____gameMinuteTimestamp - 0.1f;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClockManager), "Update")]
        public static void OverflowClockPost(ref float ____gameMinuteTimestamp, ref float __state)
        {

            //____gameMinuteTimestamp -= __state;
        }
        */

    }
}
