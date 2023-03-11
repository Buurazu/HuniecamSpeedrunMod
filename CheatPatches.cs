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
    public class CheatPatches
    {
        public static int countF10 = 0;
        public static GirlDefinition F10girl;
        public static string lastCheatText = "";
        public static void Update()
        {
            if (!HCSMod.cheatsEnabled) return;

            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (Game.Manager.State == GameState.TITLE)
                {
                    foreach (GirlDefinition girl in Game.Data.Girls.GetAll())
                    {
                        WardrobeGirlSaveData w = Game.Persistence.AddWardrobeGirl(girl);
                        w.hairstyleCount = 5;
                        w.outfitCount = 5;
                    }
                    lastCheatText = "All Outfits Unlocked!";
                    Game.Persistence.saveData.tokens += 144;
                    Game.Persistence.saveData.totalTokens += 144;
                    Game.Persistence.SaveGame();
                    UiTitleScreen component = GameObject.Find("TitleScreen").GetComponent<UiTitleScreen>();
                    component.wardrobeButton.Enable(true);
                }
                else
                {
                    Game.Events.Trigger(new NotificationEvent("Money Given!", false));
                    Game.Manager.Player.cash += 100000;
                }
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                if (Game.Manager.State == GameState.TITLE)
                {
                    
                }
                else
                {
                    Game.Events.Trigger(new NotificationEvent("Fans Given!", false));
                    foreach (FetishDefinition f in Game.Data.Fetishes.GetAll())
                    {
                        Game.Manager.Player.AddFetishFans(f, 1000);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Minus))
            {
                int mult = 1;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    mult = 24;
                }

                Game.Manager.Player.clockMinutesElapsed -= 60 * mult;
            }
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                int mult = 1;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    mult = 24;
                }

                Game.Manager.Player.clockMinutesElapsed += 60*mult;
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {
                if (Game.Manager.State == GameState.TITLE)
                {
                    countF10++;
                    if (countF10 == 1)
                    {
                        F10girl = Game.Data.Girls.Get(new System.Random().Next(18) + 1);
                        Game.Manager.Audio.PlayGirlVoice(F10girl, F10girl.voiceRecruit);
                        lastCheatText = "Press F10 two more times to erase all data!";
                    }
                    else if (countF10 == 2)
                    {
                        Game.Manager.Audio.PlayGirlVoice(F10girl, F10girl.voiceStart);
                        lastCheatText = "Press F10 one more time to erase all data!";
                    }
                    else if (countF10 == 3)
                    {
                        Game.Manager.Audio.PlayGirlVoice(F10girl, F10girl.voiceFinish);
                        countF10 = 0;
                        Game.Persistence.saveData.ResetData();
                        AccessTools.Field(typeof(GamePersistence), "_wardrobe")?.SetValue(Game.Persistence, null);
                        Game.Persistence.SaveGame();
                        UiTitleScreen component = GameObject.Find("TitleScreen").GetComponent<UiTitleScreen>();
                        component.continueButton.Disable(true);
                        component.wardrobeButton.Disable(true);
                        lastCheatText = "Data Erased!";
                    }
                }
            }
        }

        // show a notification at 12pm every day
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UiTopPanel), "OnHourTicked")]
        public static void CheatNotification()
        {
            if (Game.Manager.Clock.MilitaryHour(-1) == 12)
                Game.Events.Trigger(new NotificationEvent("CHEATS ARE ENABLED"));
        }
        // show a notification at the start of a run
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UiTopPanel), "Init")]
        public static void CheatNotification2()
        {
            Game.Events.Trigger(new NotificationEvent("CHEATS ARE ENABLED"));
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UiTopPanel), "RefreshClock")]
        public static void DisplayExactMinute(UnityEngine.UI.Text ____textFieldTime)
        {
            string text = Game.Manager.Clock.Hour(-1).ToString() + ":" +
                StringUtils.FormatIntWithDigitCount(Mathf.CeilToInt((float)(Game.Manager.Clock.Minute(-1))), 2) +
                " " + Game.Manager.Clock.Meridiem(-1).ToUpper();
            ____textFieldTime.text = text;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UiCityMapLocation), "OnButtonPressed")]
        public static void FastClicking(ref float ____resourceCollectTimestamp)
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                ____resourceCollectTimestamp = 0;

            }
        }


    }
}
