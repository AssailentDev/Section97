using MelonLoader;
using HarmonyLib;
using Il2CppScheduleOne.NPCs.Responses;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.Interaction;
using MelonLoader.Preferences;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppScheduleOne;
using Il2CppScheduleOne.Clothing;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Law;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.VoiceOver;
using Il2CppSystem.Runtime.CompilerServices;
using Section97.Crimes;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using Random = System.Random;
using Il2CppScheduleOne.TV;
using Il2CppSystem;
using Action = Il2CppSystem.Action;
using Array = Il2CppSystem.Array;
using Console = Il2CppScheduleOne.Console;
using Enum = Il2CppSystem.Enum;
using Exception = System.Exception;
using KeyValuePair = Il2CppSystem.Collections.Generic.KeyValuePair;
using TimeSpan = System.TimeSpan;
using Type = System.Type;

namespace Section97
{
    public class Sec97 : MelonMod

    {
    public static List<GameObject> robbableRegisters = new List<GameObject>();
    public static List<GameObject> stolenItems = new List<GameObject>();

    private MelonPreferences_Category preferenceCategory = (MelonPreferences_Category)null;
    private MelonPreferences_Entry<int> minimumMoney = (MelonPreferences_Entry<int>)null;
    private MelonPreferences_Entry<int> maximumMoney = (MelonPreferences_Entry<int>)null;

    public override void OnInitializeMelon()
    {
        this.preferenceCategory = MelonPreferences.CreateCategory("Section97");
        this.minimumMoney = this.preferenceCategory.CreateEntry<int>("GasMart_minimum_robbery_money", 300,
            "Minimum Money From Robbery",
            "The minimum amount of $$$ you can get per gas mart robbery", false, false, (ValueValidator)null, (string)null);
        this.maximumMoney = this.preferenceCategory.CreateEntry<int>("GasMart_maximum_robbery_money", 700,
            "Maximum Money From Robbery",
            "The maximum amount of $$$ you can get per gas mart robbery", false, false, (ValueValidator)null, (string)null);
        preferenceCategory.CreateEntry<int>("PawnStore_minimum_robbery_money", 500,
            "Minimum Money From Pawn Store Robbery", "The minimum amount of $$$ you can get per Pawn Store Robbery");
        preferenceCategory.CreateEntry<int>("PawnStore_maximum_robbery_money", 1200,
            "Maximum Money From Pawn Store Robbery", "The maximum amount of $$$ you can get per Pawn Store Robbery");
        preferenceCategory.CreateEntry<int>("Casino_minimum_robbery_money", 700,
            "Minimum Money From Casino Robbery", "The minimum amount of $$$ you can get per Casino Robbery");
        preferenceCategory.CreateEntry<int>("Casino_maximum_robbery_money", 1500,
            "Maximum Money From Casino Robbery", "The maximum amount of $$$ you can get per Casino Robbery");
        preferenceCategory.CreateEntry<int>("TacoTicklers_minimum_robbery_money", 300,
            "Minimum Money From TacoTicklers Robbery", "The minimum amount of $$$ you can get per TacoTicklers Robbery");
        preferenceCategory.CreateEntry<int>("TacoTicklers_maximum_robbery_money", 600,
            "Maximum Money From TacoTicklers Robbery", "The maximum amount of $$$ you can get per TacoTicklers Robbery");
        }

    public override void OnDeinitializeMelon()
    {
        this.preferenceCategory.SaveToFile();
    }

    public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
    {
        if (!(sceneName == "Main")) return;
        Sec97.robbableRegisters.Clear();
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (!(sceneName == "Main")) return;
        foreach (GameObject gameObject in Object.FindObjectsOfType<GameObject>())
        {
            if (gameObject.name == "cash register")
            {
                Sec97.robbableRegisters.Add(gameObject);
            } else if (gameObject.name.ToLower().Contains("clothing rack"))
            {
                try
                {
                    bool Shirt = gameObject.name.EndsWith("shirts");
                    bool Pants = gameObject.name.EndsWith("pants");
                    if (Shirt || Pants)
                    {
                        Transform child = gameObject.transform.FindChild("clothing rack");
                        if (child != null)
                        {
                            Transform child1 = child.FindChild("Rack");
                            if (child1 != null)
                            {
                                InteractableObject interactable =
                                    child1.gameObject.AddComponent<InteractableObject>();
                                interactable.message = Shirt ? "Shoplift Shirt" : "Shoplift Pants";
                                interactable.MaxInteractionRange = 2.5f;
                                void funcThatCallsFunc() => shopliftRobbery(Shirt, Pants);
                                interactable.onInteractEnd.AddListener((UnityAction)funcThatCallsFunc);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    continue;
                }
            }
        }
    }

    public void shopliftRobbery(bool Shirt, bool Pants)
    {
        if (Singleton<Registry>.Instance == null) return;
        List<ClothingDefinition> clothingDefinitionList = new List<ClothingDefinition>();
        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<int, Registry.ItemRegister> keyValuePair in Singleton<Registry>.Instance.ItemDictionary) {
            if (keyValuePair.value != null && keyValuePair.value.Definition != null &&
                keyValuePair.value.Definition.Category == (EItemCategory.Clothing))
            {
                int hash = Registry.GetHash(keyValuePair.value.Definition.ID);
                if (Singleton<Registry>.Instance.ItemDictionary.ContainsKey(hash))
                {
                    Registry.ItemRegister itemRegister = Singleton<Registry>.Instance.ItemDictionary[hash];
                    if (itemRegister != null && itemRegister.Definition != null)
                    {
                        ClothingDefinition clothingDefinition =
                            ((Il2CppObjectBase)itemRegister.Definition).Cast<ClothingDefinition>();
                        if (clothingDefinition != null && (Shirt && clothingDefinition.Slot == EClothingSlot.Top ||
                                                           Pants && clothingDefinition.Slot == EClothingSlot.Bottom))
                        {
                            clothingDefinitionList.Add(clothingDefinition);
                        }
                    }
                }
            }
        }

        ClothingInstance clothingInstance =
            ((Il2CppObjectBase)((ItemDefinition)clothingDefinitionList[
                UnityEngine.Random.Range(0, clothingDefinitionList.Count - 1)]).GetDefaultInstance(1))
            .Cast<ClothingInstance>();
        if (clothingInstance == null) return;
        bool flag = false;
        foreach (HotbarSlot hotbarSlot in PlayerSingleton<PlayerInventory>.Instance.hotbarSlots)
        {
            if (((ItemSlot)hotbarSlot).GetCapacityForItem((ItemInstance)clothingInstance) != 0)
            {
                ClothingInstance clothingInstance2 = ((Il2CppObjectBase)((ItemInstance)clothingInstance).GetCopy(1))
                    .Cast<ClothingInstance>();
                EClothingColor colour = AllColors[UnityEngine.Random.Range(0, AllColors.Count)];
                clothingInstance2.Color = colour;
                ((ItemSlot) hotbarSlot).AddItem((ItemInstance) clothingInstance2, false);
                using (IEnumerator<NPC> enumerator =
                       Object.FindObjectsOfType<NPC>().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        NPC current = enumerator.Current;
                        if (current != null && current.IsConscious && current.awareness &&
                            current.awareness.VisionCone &&
                            current.awareness.VisionCone.sightedPlayers.Contains(Player.Local) &&
                            current.actions != null)
                        {
                            current.actions.SetCallPoliceBehaviourCrime((Crime) new Shoplifting());
                            current.actions.CallPolice_Networked(Player.Local);
                            if (current.VoiceOverEmitter != null)
                            {
                                current.VoiceOverEmitter.Play(EVOLineType.Alerted);
                            }
                        }
                    }

                    break;
                }
            }
        }
    }

    private static readonly List<EClothingColor> AllColors = new List<EClothingColor>
    {
        EClothingColor.White,
        EClothingColor.LightGrey,
        EClothingColor.DarkGrey,
        EClothingColor.Charcoal,
        EClothingColor.Black,
        EClothingColor.LightRed,
        EClothingColor.Red,
        EClothingColor.Crimson,
        EClothingColor.Orange,
        EClothingColor.Tan,
        EClothingColor.Brown,
        EClothingColor.Coral,
        EClothingColor.Beige,
        EClothingColor.Yellow,
        EClothingColor.Lime,
        EClothingColor.LightGreen,
        EClothingColor.DarkGreen,
        EClothingColor.Cyan,
        EClothingColor.SkyBlue,
        EClothingColor.Blue,
        EClothingColor.DeepBlue,
        EClothingColor.Navy,
        EClothingColor.DeepPurple,
        EClothingColor.Purple,
        EClothingColor.Magenta,
        EClothingColor.BrightPink,
        EClothingColor.HotPink
    };

    [HarmonyPatch(typeof(NPCResponses_Civilian), "RespondToAimedAt", new Type[] { typeof(Player) })]
    static class RobberyPatch
    {
        public static bool Prefix(ref NPCResponses_Civilian __instance, ref Player __0)
        {
            try
            {
                NPCScheduleManager npcsm = __instance.gameObject.transform.parent.Find("Schedule")
                    .GetComponent<NPCScheduleManager>();
                if ((npcsm.ActiveAction.name.Contains("Gas mart") || npcsm.ActiveAction.name.Contains("Gas station")) &&
                    npcsm.ActiveAction.name.Contains("worker"))
                {
                    __instance.actions.Cower();
                    startRobberyRegisters(0);
                    return false;
                }

                // if ((npcsm.ActiveAction.name.Contains("Location-based dialogue (Standpoint) (6:15 AM - 6:05 PM)") &&
                     // __instance.gameObject.transform.parent.name.Contains("Mick")))
                // {
                    // __instance.actions.Cower();
                    // startRobberyRegisters(1);
                    // return false;
                // }

                if (npcsm.ActiveAction.name.Contains("Location-based action (Bar stand point) (3:00 PM - 5:00 AM)") &&
                    __instance.gameObject.transform.parent.name.Contains("Philip"))
                {
                    __instance.actions.Cower();
                    startRobberyRegisters(2);
                    return false;
                }

                if (npcsm.ActiveAction.name.Contains("Cashier Stand Point"))
                {
                    __instance.actions.Cower();
                    startRobberyRegisters(3);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        static void startRobbery(int x)
        {
            if (Player.Local == null)
            {
                return;
            }

            bool flag = false;
            HotbarSlot equippedSlot = PlayerSingleton<PlayerInventory>.instance.equippedSlot;
            if (equippedSlot != null && ((ItemSlot)equippedSlot).ItemInstance != null &&
                ((ItemSlot)equippedSlot).ItemInstance.definition != null)
            {
                if (((ItemSlot)equippedSlot).ItemInstance.Equippable.TryCast<Equippable_RangedWeapon>())
                    flag = ((ItemSlot)equippedSlot).ItemInstance.Equippable.Cast<Equippable_RangedWeapon>();
            }
            Console.ChangeCashCommand changeCashCommand = new Console.ChangeCashCommand();
            Il2CppSystem.Collections.Generic.List<string> List1 =
            new Il2CppSystem.Collections.Generic.List<string>();
            int robMoney = 0;
            if (x == 0) // Gas Marts
            {
                robMoney = (int)UnityEngine.Random.Range(
                    MelonPreferences.GetEntryValue<int>("Section97", "GasMart_minimum_robbery_money"),
                    MelonPreferences.GetEntryValue<int>("Section97", "GasMart_maximum_robbery_money"));
            } else if (x == 1)
            {
                robMoney = (int)UnityEngine.Random.Range(
                MelonPreferences.GetEntryValue<int>("Section97", "PawnStore_minimum_robbery_money"),
                MelonPreferences.GetEntryValue<int>("Section97", "PawnStore_maximum_robbery_money"));
            } else if (x == 2)
            {
                robMoney = (int)UnityEngine.Random.Range(
                    MelonPreferences.GetEntryValue<int>("Section97", "Casino_minimum_robbery_money"),
                    MelonPreferences.GetEntryValue<int>("Section97", "Casino_maximum_robbery_money"));
            } else if (x == 3)
            {
                robMoney = (int)UnityEngine.Random.Range(
                    MelonPreferences.GetEntryValue<int>("Section97", "TacoTicklers_minimum_robbery_money"),
                    MelonPreferences.GetEntryValue<int>("Section97", "TacoTicklers_maximum_robbery_money"));
            }

            List1.Add(robMoney.ToString()); 
            MelonLogger.Msg(MelonPreferences.GetEntryValue<int>("Section97", "minimum_robbery_money")); 
            MelonLogger.Msg(MelonPreferences.GetEntryValue<int>("Section97", "_robbery_money")); 
            MelonLogger.Msg(robMoney); 
            ((Console.ConsoleCommand) changeCashCommand).Execute(List1);

            foreach (NPC npc in Object.FindObjectsOfType<NPC>())
            {
                if (npc != null && npc.IsConscious && npc.awareness && npc.awareness.VisionCone &&
                    npc.awareness.VisionCone.sightedPlayers != null &&
                    npc.awareness.VisionCone.sightedPlayers.Contains(Player.Local) && npc.dialogueHandler != null &&
                    npc.actions != null)
                {
                    npc.actions.SetCallPoliceBehaviourCrime((Crime)new ArmedRobbery());
                    npc.actions.CallPolice_Networked(Player.Local);
                    npc.SetPanicked();
                    npc.dialogueHandler.PlayReaction("panic_start", 5f, true);
                    if (npc.VoiceOverEmitter != null)
                    {
                        break;
                    }

                    npc.VoiceOverEmitter.Play(EVOLineType.Scared);
                    break;
                }
            }

            foreach (GameObject gameObject in robbableRegisters)
            {
                InteractableObject interactable = gameObject.GetComponent<InteractableObject>();
                interactable.enabled = false;
            }
            
            return;
        }

        public static async void startRobberyRegisters(int x)
        {
            foreach (GameObject gameObject in robbableRegisters)
            {
                InteractableObject interactable;
                if (gameObject.GetComponent<InteractableObject>() != null)
                {
                    interactable = gameObject.GetComponent<InteractableObject>();
                    interactable.enabled = true;
                }
                else
                {
                    interactable = gameObject.AddComponent<InteractableObject>();
                }
                interactable.message = "Rob";
                interactable.MaxInteractionRange = 15f;
                interactable.Priority = 10;
                void funcThatCallsFunc() => startRobbery(x);
                interactable.onInteractEnd.AddListener((UnityAction) funcThatCallsFunc);
            }

            await Task.Delay(TimeSpan.FromSeconds(10));

            foreach (GameObject gameObject in robbableRegisters)
            {
                InteractableObject interactable = gameObject.GetComponent<InteractableObject>();
                interactable.enabled = false;
            }
        }
    }
    }
}
