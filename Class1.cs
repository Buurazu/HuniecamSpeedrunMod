using System;
using System.IO;
using System.Linq;
using BepInEx;
using UnityEngine;
using HarmonyLib;
using BepInEx.Configuration;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;

namespace HuniecamSpeedrunMod
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class HCSMod : BaseUnityPlugin
    {
        public const string PluginVersion = "1.1";
        public const string PluginGUID = "HCSSpeedrunMod";
        public const string PluginName = "HunieCam Studio Speedrun Mod";
        public static readonly string PluginBaseDir = Path.GetDirectoryName(typeof(HCSMod).Assembly.Location);

        public static bool cheatsEnabled = false;

        public const int V1 = 0;
        public const int V102 = 1;

        public static ConfigEntry<Boolean> VsyncEnabled { get; private set; }
        public static ConfigEntry<int> FramerateCap { get; private set; }
        public static ConfigEntry<String> MashKeys { get; private set; }
        public static ConfigEntry<String> MouseKeys { get; private set; }
        public static ConfigEntry<Boolean> MouseWheelEnabled { get; private set; }
        public static ConfigEntry<String> MashScrollUp { get; private set; }
        public static ConfigEntry<String> MashScrollDown { get; private set; }
        public static ConfigEntry<KeyboardShortcut> CheatHotkey { get; private set; }

        private void Awake()
        {
            VsyncEnabled = Config.Bind(
                "Settings", nameof(VsyncEnabled),
                false,
                "Enable or disable Vsync. The FPS cap below will only take effect with it disabled");
            FramerateCap = Config.Bind(
                "Settings", nameof(FramerateCap),
                144,
                "Set your framerate cap when Vsync is off.\nValid values on 1.0.2: 60, 120, 144, 170, 240, 300, 360." +
                "\nValid values on 1.0: Anything lower than 360 (to replicate Vsync being off in V1.0, while preventing hardware advantage).\nHigher FPS could help days progress very slightly faster");

            MouseWheelEnabled = Config.Bind(
                "Settings", nameof(MouseWheelEnabled),
                false,
                "Enable or disable the mouse wheel being treated as a click (mash scroll keys won't be counted as a click regardless)");
            MouseKeys = Config.Bind(
                "Settings", nameof(MouseKeys),
                "None",
                "These keys (comma separated) will be treated as a click; set to None to disable\nMouse0-6 work as well; check https://docs.unity3d.com/ScriptReference/KeyCode.html for the list of options");
            MashKeys = Config.Bind(
                "Settings", nameof(MashKeys),
                "Mouse1, Space",
                "These keys (comma separated) will be treated as clicking every frame; set to None to disable\nMouse0-6 work as well; check https://docs.unity3d.com/ScriptReference/KeyCode.html for the list of options");
            MashScrollUp = Config.Bind(
                "Settings", nameof(MashScrollUp),
                "UpArrow, W",
                "These keys (comma separated) will be treated as mouse scroll wheel up every frame (set to None to disable)");
            MashScrollDown = Config.Bind(
                "Settings", nameof(MashScrollDown),
                "DownArrow, S",
                "These keys (comma separated) will be treated as mouse scroll wheel down every frame (set to None to disable)");

            CheatHotkey = Config.Bind(
                "Settings", nameof(CheatHotkey),
                new KeyboardShortcut(KeyCode.C),
                "The hotkey to use for activating Cheat Mode on the title screen (set to None to disable)");
        }

        /*
         * todo: 
         * cheat mode things: money cheat / fan cheat obviously. make the item store instantly usable. fast forward time
         * outfit customization?
        */
        public static void StringToKeyCodes(string keysString, ref List<KeyCode> list)
        {
            string[] keys = keysString.Split(',');
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = keys[i].Trim();
                KeyCode kc = KeyCode.None;
                try
                {
                    kc = (KeyCode)System.Enum.Parse(typeof(KeyCode), keys[i]);
                }
                catch { BasePatches.Logger.LogMessage(keys[i] + " is not a valid keycode name!"); }
                if (kc != KeyCode.None)
                {
                    list.Add(kc);
                }
            }
        }

        public static int GameVersion()
        {
            //Determine the version of the game running by checking the token amount for Diamond Trophy
            if (Game.Data.Trophies.Get(5).tokenReward == 24) return V102;
            else return V1;
        }

        void Start()
        {
            //I can't believe the game doesn't run in background by default
            Application.runInBackground = true;

            if (!VsyncEnabled.Value)
            {
                QualitySettings.vSyncCount = 0;
                if (GameVersion() == V102 && FramerateCap.Value != 60 && FramerateCap.Value != 120 && FramerateCap.Value != 144 &&
                    FramerateCap.Value != 170 && FramerateCap.Value != 240 && FramerateCap.Value != 300 && FramerateCap.Value != 360)
                    FramerateCap.Value = 144;
                Application.targetFrameRate = FramerateCap.Value;
            }
            else
            {
                QualitySettings.vSyncCount = 1;
            }

            Harmony.CreateAndPatchAll(typeof(BasePatches), null); BasePatches.InitSearchForMe();
            Harmony.CreateAndPatchAll(typeof(InputPatches), null);

            StringToKeyCodes(MouseKeys.Value, ref InputPatches.mouseKeyboardKeys);
            StringToKeyCodes(MashKeys.Value, ref InputPatches.mashKeyboardKeys);

            StringToKeyCodes(MashScrollUp.Value, ref InputPatches.mashScrollUpKeys);
            StringToKeyCodes(MashScrollDown.Value, ref InputPatches.mashScrollDownKeys);


        }

        private void Update()
        {
            //BasePatches.Logger.LogMessage(Game.Persistence.activeSaveFileIndex);
            CheatPatches.Update();

            if (Game.Manager.State == GameState.TITLE)
            {
                //check for the Cheat Mode hotkey
                if (cheatsEnabled == false && CheatHotkey.Value.IsDown())
                {
                    GirlDefinition randomGirl = Game.Data.Girls.Get(new System.Random().Next(18) + 1);
                    Game.Manager.Audio.PlayGirlVoice(randomGirl, randomGirl.voiceBlocked);
                    Harmony.CreateAndPatchAll(typeof(CheatPatches), null);
                    cheatsEnabled = true;
                }
            }
        }

    }
}