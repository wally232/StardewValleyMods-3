﻿using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomFixedDialogue
{
    internal class DialoguePatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static string prefix = "CustomFixedDialogue";
        private static string NPCPrefix = "Strings\\StringsFromCSFiles:NPC.cs.";
        private static string extraPrefix = "Data\\ExtraDialogue";
        private static List<string> NPCexceptions = new List<string>() 
        { 
            "3981",
            "3969",
        };

        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }
        public static void LocalizedContentManager_LoadString_Postfix(string path, ref string __result)
        {
            try
            {
                if (path.StartsWith(extraPrefix))
                {
                    __result = $"{prefix}{path.Replace("Data\\ExtraDialogue:", "ExtraDialogue_")}^{__result}";
                    Monitor.Log($"edited dialogue: {__result}");
                }
                else if (path.StartsWith(NPCPrefix) && !NPCexceptions.Contains(path.Substring(NPCPrefix.Length)))
                {
                    __result = $"{prefix}{path.Replace("Strings\\StringsFromCSFiles:", "")}^{__result}";
                    Monitor.Log($"edited dialogue: {__result}");
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(LocalizedContentManager_LoadString_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }


        public static void LocalizedContentManager_LoadString_Postfix2(string path, ref string __result)
        {
            try
            {
                if (path.StartsWith(extraPrefix))
                {
                    __result = $"{prefix}{path.Replace("Data\\ExtraDialogue:", "ExtraDialogue_")}^{__result}";
                }
                else if (path.StartsWith(NPCPrefix) && !NPCexceptions.Contains(path.Substring(NPCPrefix.Length)))
                {
                    __result = $"{prefix}{path.Replace("Strings\\StringsFromCSFiles:", "")}^{__result}";
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(LocalizedContentManager_LoadString_Postfix2)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void Dialogue_parseDialogueString_Prefix(Dialogue __instance, ref string masterString)
        {
            try
            {
                if (masterString.StartsWith(prefix))
                {
                    Dictionary<string, string> dialogueDic = null;
                    try
                    {
                        dialogueDic = Helper.Content.Load<Dictionary<string, string>>($"Characters/Dialogue/{__instance.speaker.Name}", ContentSource.GameContent);
                    }
                    catch 
                    { 
                    }
                    string key = masterString.Substring(prefix.Length).Split('^')[0];
                    if (dialogueDic != null &&  dialogueDic.ContainsKey(key))
                    {
                        Monitor.Log($"{__instance.speaker.Name} has dialogue for {key}", LogLevel.Debug);
                        masterString = dialogueDic[key];
                    }
                    else
                    {
                        masterString = string.Join("^", masterString.Split('^').Skip(1));
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Dialogue_parseDialogueString_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void Dialogue_CTOR_Prefix()
        {
            Monitor.Log($"WORKING NOW", LogLevel.Error);
        }
    }
}