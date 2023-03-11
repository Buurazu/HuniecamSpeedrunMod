using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HuniecamSpeedrunMod
{
    public class InputPatches
    {
        public static List<KeyCode> mouseKeyboardKeys = new List<KeyCode>();
        public static List<KeyCode> mashKeyboardKeys = new List<KeyCode>();
        public static List<KeyCode> mashScrollUpKeys = new List<KeyCode>();
        public static List<KeyCode> mashScrollDownKeys = new List<KeyCode>();
        public static bool dontFakeScrollwheel = false;

        public static float GetMouseWheel()
        {
            dontFakeScrollwheel = true;
            float result = Input.GetAxis("Mouse ScrollWheel");
            dontFakeScrollwheel = false;
            return result;
        }

        public static bool IsMouseKeyDown()
        {
            if (HCSMod.MouseWheelEnabled.Value && GetMouseWheel() != 0) return true;

            //Mash keys only check for GetKey here
            for (int i = 0; i < mashKeyboardKeys.Count; i++)
            {
                if (Input.GetKey(mashKeyboardKeys[i])) return true;
            }

            for (int i = 0; i < mouseKeyboardKeys.Count; i++)
            {
                if (Input.GetKeyDown(mouseKeyboardKeys[i])) return true;
            }

            return false;
        }

        public static bool IsMouseKeyUp()
        {
            if (HCSMod.MouseWheelEnabled.Value && GetMouseWheel() != 0) return true;

            //Mash keys only check for GetKey here
            for (int i = 0; i < mashKeyboardKeys.Count; i++)
            {
                if (Input.GetKey(mashKeyboardKeys[i])) return true;
            }

            for (int i = 0; i < mouseKeyboardKeys.Count; i++)
            {
                if (Input.GetKeyUp(mouseKeyboardKeys[i])) return true;
            }

            return false;
        }

        public static bool IsMouseKey()
        {
            if (HCSMod.MouseWheelEnabled.Value && GetMouseWheel() != 0) return true;

            for (int i = 0; i < mashKeyboardKeys.Count; i++)
            {
                if (Input.GetKey(mashKeyboardKeys[i])) return true;
            }

            for (int i = 0; i < mouseKeyboardKeys.Count; i++)
            {
                if (Input.GetKey(mouseKeyboardKeys[i])) return true;
            }

            return false;
        }


        //it's that easy
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Input), "GetMouseButtonUp")]
        public static void NoWay(Input __instance, int button, ref bool __result)
        {
            if (button == 0 && __result != true) __result = IsMouseKeyUp();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Input), "GetMouseButtonDown")]
        public static void NoWay2(Input __instance, int button, ref bool __result)
        {
            if (button == 0 && __result != true) __result = IsMouseKeyDown();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Input), "GetMouseButton")]
        public static void NoWay3(Input __instance, int button, ref bool __result)
        {
            if (button == 0 && __result != true) __result = IsMouseKey();
        }

        //check for our scroll wheel mash key
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Input), "GetAxis")]
        public static void NoWay4(Input __instance, string axisName, ref float __result)
        {
            if (dontFakeScrollwheel) return;
            if (axisName == "Mouse ScrollWheel" && __result == 0) {
                for (int i = 0; i < mashScrollUpKeys.Count; i++)
                {
                    if (Input.GetKey(mashScrollUpKeys[i])) __result = 0.2f;
                }
                for (int i = 0; i < mashScrollDownKeys.Count; i++)
                {
                    if (Input.GetKey(mashScrollDownKeys[i])) __result = -0.2f;
                }
            }
        }

    }
}
