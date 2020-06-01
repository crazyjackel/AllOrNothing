using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Harmony;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using ShinyShoe;

namespace AllOrNothing
{
    /// <summary>
    /// Static Class that allows getting the Value of Private Variables
    /// </summary>
    public static class ReflectionMethods
    {
        public static object GetInstanceField<T>(T instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = typeof(T).GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
        public static T1 GetInstanceField<T1, T2>(T2 instance, string fieldName)
        {
            return (T1)GetInstanceField<T2>(instance, fieldName);
        }
    }

    // Credit to Rawsome, Stable Infery for the base of this method.es 
    [BepInPlugin("io.github.crazyjackel.AON", "All or Nothing", "1.0")]
    [BepInProcess("MonsterTrain.exe")]
    [BepInProcess("MtLinkHandler.exe")]
    public class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            var harmony = new Harmony("io.github.crazyjackel.AON");
            harmony.PatchAll();
            Console.WriteLine("Starting All or Nothing 1.0");
        }

    }

    [HarmonyPatch(typeof(CardDraftScreen), "ApplyDraft")]
    class CardDraftScreen_ApplyDraft_Patch
    {
        static void Postfix(CardDraftScreen __instance, IDraftableUI draftedItem)
        {
            //Get SaveManager and DraftItems
            SaveManager save = ReflectionMethods.GetInstanceField<SaveManager, CardDraftScreen>(__instance,"saveManager");
            List<IDraftableUI> draftItems = ReflectionMethods.GetInstanceField<List<IDraftableUI>, CardDraftScreen>(__instance, "draftItems");

            //Safety First
            if (save != null)
            {
                foreach (IDraftableUI uI in draftItems)
                {
                    if (uI != draftedItem)
                    {
                        CardState cardState = (uI as CardUI).GetCardState();
                        CardData cardData = save.GetAllGameData().FindCardData(cardState.GetCardDataID());
                        CardStateModifiers cardStateModifiers = cardState.GetCardStateModifiers();
                        save.AddCardToDeck(cardData, cardStateModifiers, true, true, false, true, true);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(SaveManager), "AddCardToDeck")]
    class SaveManager_AddCardToDeck_Patch
    {
        static void Postfix(CardData cardData)
        {
            //Uncomment this in order to log each card you grab
            //Console.WriteLine(cardData.name);
        }
    }

    // Credit to Rawsome, Stable Infery for this one, too: a quick and dirty patch to disable the multiplayer button.
    [HarmonyPatch(typeof(MainMenuScreen), "CollectMenuButtons")]
    class MainMenuScreen_CollectMenuButtons_Patch
    {
        static void Postfix(ref GameUISelectableButton ___multiplayerButton, ref List<GameUISelectableButton> ___menuButtons)
        {
            ___menuButtons.Remove(___multiplayerButton);
            ___multiplayerButton.enabled = false;
        }
    }
}
