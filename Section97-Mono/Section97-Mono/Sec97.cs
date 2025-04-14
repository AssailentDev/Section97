using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MelonLoader;
using ScheduleOne;
using ScheduleOne.Clothing;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Responses;
using ScheduleOne.PlayerScripts;
using ScheduleOne.VoiceOver;
using Section97_Mono.Crimes;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Section97_Mono
{
    public class Sec97 : MelonMod
    {
        public static List<GameObject> RobbableDisplayBenches = new List<GameObject>();
        public static List<GameObject> DisplayBench0Items = new List<GameObject>();
        public static List<GameObject> DisplayBench1Items = new List<GameObject>();
        public static List<GameObject> DisplayBench2Items = new List<GameObject>();
        public static bool CanRobPawnShopBench0 = true;
        public static bool CanRobPawnShopBench1 = true;
        public static bool CanRobPawnShopBench2 = true;
        public static List<GameObject> RobbableRegisters = new List<GameObject>();
        
        private MelonPreferences_Category _preferenceCategory;
        
        public override void OnInitializeMelon()
        {
            _preferenceCategory = MelonPreferences.CreateCategory("Section97");
            _preferenceCategory.CreateEntry("GasMart_minimum_robbery_money", 300,
                "Minimum Money From Robbery",
                "The minimum amount of $$$ you can get per gas mart robbery");
            _preferenceCategory.CreateEntry("GasMart_maximum_robbery_money", 700,
                "Maximum Money From Robbery",
                "The maximum amount of $$$ you can get per gas mart robbery");
            _preferenceCategory.CreateEntry("PawnStore_minimum_robbery_money", 500,
                "Minimum Money From Pawn Store Robbery", 
                "The minimum amount of $$$ you can get per Pawn Store Robbery");
            _preferenceCategory.CreateEntry("PawnStore_maximum_robbery_money", 1200,
                "Maximum Money From Pawn Store Robbery", 
                "The maximum amount of $$$ you can get per Pawn Store Robbery");
            _preferenceCategory.CreateEntry("Casino_minimum_robbery_money", 700,
                "Minimum Money From Casino Robbery", 
                "The minimum amount of $$$ you can get per Casino Robbery");
            _preferenceCategory.CreateEntry("Casino_maximum_robbery_money", 1500,
                "Maximum Money From Casino Robbery", 
                "The maximum amount of $$$ you can get per Casino Robbery");
            _preferenceCategory.CreateEntry("TacoTicklers_minimum_robbery_money", 300,
                "Minimum Money From TacoTicklers Robbery", 
                "The minimum amount of $$$ you can get per TacoTicklers Robbery");
            _preferenceCategory.CreateEntry("TacoTicklers_maximum_robbery_money", 600,
                "Maximum Money From TacoTicklers Robbery", 
                "The maximum amount of $$$ you can get per TacoTicklers Robbery");
            _preferenceCategory.CreateEntry("PawnStore_SmashNGrab_minimum_robbery_money", 200,
                "Minimum Money from the Pawn Store smash n grab.",
                "The minimum amount of $$$ from the Pawn Store Smash N Grab (Baseball bat robbery)");
            _preferenceCategory.CreateEntry("PawnStore_SmashNGrab_maximum_robbery_money", 600,
                "Maximum Money from the Pawn Store smash n grab.",
                "The maximum amount of $$$ from the Pawn Store Smash N Grab (Baseball bat robbery)");
        }

        public override void OnDeinitializeMelon()
        {
            _preferenceCategory.SaveToFile();
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            if (sceneName != "Main") return;
            RobbableRegisters.Clear();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName != "Main") return;
            foreach (GameObject gameObject in Object.FindObjectsOfType<GameObject>())
            {
                if (gameObject.name == "cash register")
                {
                    RobbableRegisters.Add(gameObject);
                } else if (gameObject.name.ToLower().Contains("clothing rack"))
                {
                    // try
                    // {
                        bool shirt = gameObject.name.EndsWith("shirts");
                        bool pants = gameObject.name.EndsWith("pants");
                        if (shirt || pants)
                        {
                            Transform child = gameObject.transform.Find("clothing rack");
                            if (child == null) continue;
                            Transform child1 = child.Find("Rack");
                            if (child == null) continue;
                            InteractableObject interactable;
                            interactable = child1.gameObject.AddComponent<InteractableObject>();
                            interactable.SetMessage(shirt ? "Shoplift Shirt" : "Shoplift Pants");
                            interactable.MaxInteractionRange = 2.5f;
                            void FuncThatCallsFunc() => ShopliftRobbery(shirt, pants);
                            interactable.onInteractEnd.AddListener(FuncThatCallsFunc);
                        }
                    // }
                    // catch
                    // {
                        // ignored
                    // }
                } else if (gameObject.name.Contains("Display Bench"))
                {
                    RobbableDisplayBenches.Add(gameObject);
                    if (gameObject.name.Contains("(2)")) // From Display Bench (2)
                    {
                        // Display Bench 2
                        DisplayBench2Items.Add(gameObject.transform.Find("Square tub").gameObject);
                        DisplayBench2Items.Add(gameObject.transform.Find("Square tub (1)").gameObject);
                        DisplayBench2Items.Add(gameObject.transform.parent.Find("Saucepan").gameObject);
                        DisplayBench2Items.Add(gameObject.transform.parent.Find("Display cabinet (2)").Find("Champagne Bottle").gameObject);
                        // Display Bench 1
                        DisplayBench1Items.Add(gameObject.transform.Find("Gold bar").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("Gold bar (1)").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("Gold bar (2)").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("Gold bar (3)").gameObject);
                    } else if (gameObject.name == "Display Bench") // From Display Bench
                    {
                        // Display Bench 1
                        DisplayBench1Items.Add(gameObject.transform.Find("Flashlight").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("45 Bullet").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("45 Bullet (1)").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("45 Bullet (2)").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("45 Bullet (3)").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("45 Bullet (4)").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("45 Bullet (5)").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("45 Bullet (6)").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("Funnel").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("SmallBox").gameObject);
                        DisplayBench1Items.Add(gameObject.transform.Find("SmallBox (1)").gameObject);
                        // Display Bench
                        DisplayBench0Items.Add(gameObject.transform.Find("Mortar").gameObject);
                        DisplayBench0Items.Add(gameObject.transform.Find("Frying Pan").gameObject);
                        DisplayBench0Items.Add(gameObject.transform.parent.Find("Display cabinet (2)").Find("Digital Alarm").gameObject);
                        DisplayBench0Items.Add(gameObject.transform.parent.Find("Display cabinet (2)").Find("Wall Clock").gameObject);
                        DisplayBench0Items.Add(gameObject.transform.parent.Find("Display cabinet (2)").Find("Biohazard Box").gameObject);
                    }
                }
            }
        }

        public void ShopliftRobbery(bool shirt, bool pants)
        {
            if (Singleton<Registry>.Instance == null) return;
            List<ClothingDefinition> clothingDefinitionList = new List<ClothingDefinition>();
            foreach (KeyValuePair<int, Registry.ItemRegister> keyValuePair in Singleton<Registry>.Instance
                         .ItemDictionary)
            {
                if (keyValuePair.value != null && keyValuePair.value.Definition != null &&
                    keyValuePair.value.Definition.Category == EItemCategory.Clothing)
                {
                    int hash = Registry.GetHash(keyValuePair.value.Definition.ID);

                    if (Singleton<Registry>.Instance.ItemDictionary.ContainsKey(hash))
                    {
                        Registry.ItemRegister itemRegister = Singleton<Registry>.Instance.ItemDictionary[hash];
                        if (itemRegister != null && itemRegister.Definition != null)
                        {
                            ClothingDefinition clothingDefinition = (ClothingDefinition)itemRegister.Definition;
                            if (clothingDefinition != null && (shirt && clothingDefinition.Slot == EClothingSlot.Top ||
                                                               pants && clothingDefinition.Slot ==
                                                               EClothingSlot.Bottom))
                            {
                                clothingDefinitionList.Add(clothingDefinition);
                            }
                        }
                    }
                }
            }

            ClothingInstance clothingInstance = (ClothingInstance) clothingDefinitionList[Random.Range(0, clothingDefinitionList.Count)].GetDefaultInstance();
            foreach (HotbarSlot hotbarSlot in PlayerSingleton<PlayerInventory>.Instance.hotbarSlots)
            {
                if (hotbarSlot.GetCapacityForItem(clothingInstance) == 0) continue;
                ClothingInstance clothingInstance2 = (ClothingInstance) clothingInstance.GetCopy(1); 
                EClothingColor colour = AllColors[Random.Range(0, AllColors.Count - 1)]; 
                clothingInstance2.Color = colour; 
                hotbarSlot.AddItem(clothingInstance2); 
                foreach (NPC npc in Object.FindObjectsOfType<NPC>()) 
                { 
                    if (npc != null && npc.IsConscious && npc.awareness && npc.awareness.VisionCone && 
                        npc.awareness.VisionCone.sightedPlayers != null && 
                        npc.awareness.VisionCone.sightedPlayers.Contains(Player.Local) && npc.dialogueHandler != null && 
                        npc.actions != null)
                    { 
                        npc.actions.SetCallPoliceBehaviourCrime(new Shoplifting()); 
                        npc.actions.CallPolice_Networked(Player.Local); 
                        npc.SetPanicked(); 
                        npc.dialogueHandler.PlayReaction("panic_start", 5f, true); 
                        if (npc.VoiceOverEmitter == null) 
                        { 
                            continue;
                        }

                        npc.VoiceOverEmitter.Play(EVOLineType.Scared);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HotbarSlot), "Equip")]
        static class PawnstoreRobberyPatch
        {
            public static bool Prefix(ref HotbarSlot __instance)
            {
                try
                {
                    if (__instance.ItemInstance.GetItemData().ID != "baseballbat") return true;
                    foreach (GameObject gameObject in RobbableDisplayBenches)
                    {
                        if (gameObject.name == "Display Bench" && !CanRobPawnShopBench0)
                        {
                            return true;
                        } else if (gameObject.name == "Display Bench (1)" && !CanRobPawnShopBench1)
                        {
                            return true;
                        } else if (gameObject.name == "DisplayBench(2)" && !CanRobPawnShopBench2)
                        {
                            return true;
                        }

                        InteractableObject interactable;
                        if (gameObject.GetComponent<InteractableObject>() != null)
                        {
                            interactable = gameObject.GetComponent<InteractableObject>();
                            interactable.enabled = true;
                        }
                        else
                        {
                            interactable = gameObject.AddComponent<InteractableObject>();
                            interactable.message = "Smash";
                            interactable.MaxInteractionRange = 4f;
                            interactable.Priority = 10;
                            Transform position = interactable.transform;
                            position.position += position.up * 1.2f;
                            interactable.displayLocationPoint = position;
                            void FuncThatCallsFunc() => StartSmashing(gameObject);
                            interactable.onInteractEnd.AddListener(FuncThatCallsFunc);
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                return true;
            }

            static async void StartSmashing(GameObject gameObject)
            {
                ((Equippable_MeleeWeapon)PlayerSingleton<PlayerInventory>.Instance.equippedSlot.Equippable).Release();
                InteractableObject interactable = gameObject.GetComponent<InteractableObject>();
                interactable.enabled = false;
                List<GameObject> benchItems = new List<GameObject>();
                if (gameObject.name == "Display Bench") { benchItems = DisplayBench0Items; CanRobPawnShopBench0 = false; }
                else if (gameObject.name == "Display Bench (1)") { benchItems = DisplayBench1Items; CanRobPawnShopBench1 = false; }
                else if (gameObject.name == "Display Bench (2)") { benchItems = DisplayBench2Items; CanRobPawnShopBench2 = false; }

                foreach (GameObject item in benchItems)
                {
                    item.active = false;
                }

                int robMoney = Random.Range(
                    MelonPreferences.GetEntryValue<int>("Section97", "PawnStore_SmashNGrab_minimum_robbery_money"),
                    MelonPreferences.GetEntryValue<int>("Section97", "PawnStore_SmashNGrab_maximum_robbery_money"));
                Type typeFromHandle = typeof(MoneyManager);
                FieldInfo? field = typeFromHandle.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
                if (field != null)
                {
                    MoneyManager moneyManager = (MoneyManager) field.GetValue(null);
                    if (moneyManager != null) moneyManager.ChangeCashBalance(robMoney, true, true);
                }
                else
                {
                    MoneyManager moneyManager = Object.FindObjectOfType<MoneyManager>();
                    if (moneyManager != null) moneyManager.ChangeCashBalance(robMoney, true, true);
                }

                foreach (NPC npc in Object.FindObjectsOfType<NPC>())
                {
                    if (npc != null && npc.IsConscious && npc.awareness && npc.awareness.VisionCone &&
                        npc.awareness.VisionCone.sightedPlayers != null &&
                        npc.awareness.VisionCone.sightedPlayers.Contains(Player.Local) && npc.dialogueHandler != null &&
                        npc.actions != null)
                    {
                        npc.actions.SetCallPoliceBehaviourCrime(new SmashNGrab());
                        npc.actions.CallPolice_Networked(Player.Local);
                        npc.SetPanicked();
                        npc.dialogueHandler.PlayReaction("panic_start", 5f, true);
                        if (npc.VoiceOverEmitter == null) continue;
                        npc.VoiceOverEmitter.Play(EVOLineType.Scared);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(5));

                if (gameObject.name == "Display Bench") { CanRobPawnShopBench0 = true; }
                else if (gameObject.name == "Display Bench (1)") { CanRobPawnShopBench1 = true; }
                else if (gameObject.name == "Display Bench (2)") { CanRobPawnShopBench2 = true; }

                foreach (GameObject item in benchItems)
                {
                    item.active = true;
                }
            }
        }

        [HarmonyPatch(typeof(NPCResponses_Civilian), "RespondToAimedAt", new Type[] { typeof(Player) })]
        static class RobberyPatch
        {
            public static bool Prefix(ref NPCResponses_Civilian __instance, ref Player __0)
            {
                NPCScheduleManager npcsm = __instance.gameObject.transform.parent.Find("Schedule")
                    .GetComponent<NPCScheduleManager>();
                if ((npcsm.ActiveAction.name.Contains("Gas mart") || npcsm.ActiveAction.name.Contains("Gas station")) &&
                    npcsm.ActiveAction.name.Contains("worker"))
                {
                    __instance.actions.Cower();
                    StartRobberyRegisters(0);
                    return false;
                }
                
                if (npcsm.ActiveAction.name.Contains("Location-based action (Bar stand point) (3:00 PM - 5:00 AM)") &&
                    __instance.gameObject.transform.parent.name.Contains("Philip"))
                {
                    __instance.actions.Cower();
                    StartRobberyRegisters(2);
                    return false;
                }

                if (npcsm.ActiveAction.name.Contains("Cashier Stand Point"))
                {
                    __instance.actions.Cower();
                    StartRobberyRegisters(3);
                    return false;
                }

                return true;
            }
        }

        public static async void StartRobberyRegisters(int x, [CallerMemberName] string callerName = "")
        {
            foreach (GameObject gameObject in RobbableRegisters)
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
                    interactable.message = "Rob";
                    interactable.MaxInteractionRange = 15f;
                    interactable.Priority = 10;
                    void FuncThatCallsFunc() => startRobbery(x);
                    interactable.onInteractEnd.AddListener(FuncThatCallsFunc);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10));

            foreach (GameObject gameObject in RobbableRegisters)
            {
                InteractableObject interactable = gameObject.GetComponent<InteractableObject>();
                interactable.enabled = false;
            }
        }

        static void startRobbery(int x, [CallerMemberName] string callerName = "")
        {
            if (Player.Local == null) return;
            int robMoney = 0;
            if (x == 0) // Gas Marts
            {
                robMoney = Random.Range(
                    MelonPreferences.GetEntryValue<int>("Section97", "GasMart_minimum_robbery_money"),
                    MelonPreferences.GetEntryValue<int>("Section97", "GasMart_maximum_robbery_money"));
            } else if (x == 1)
            {
                // robMoney = (int)UnityEngine.Random.Range(
                // MelonPreferences.GetEntryValue<int>("Section97", "PawnStore_minimum_robbery_money"),
                // MelonPreferences.GetEntryValue<int>("Section97", "PawnStore_maximum_robbery_money"));
            } else if (x == 2)
            {
                robMoney = Random.Range(
                    MelonPreferences.GetEntryValue<int>("Section97", "Casino_minimum_robbery_money"),
                    MelonPreferences.GetEntryValue<int>("Section97", "Casino_maximum_robbery_money"));
            } else if (x == 3)
            {
                robMoney = Random.Range(
                    MelonPreferences.GetEntryValue<int>("Section97", "TacoTicklers_minimum_robbery_money"),
                    MelonPreferences.GetEntryValue<int>("Section97", "TacoTicklers_maximum_robbery_money"));
            }
            
            Type typeFromHandle = typeof(MoneyManager);
            FieldInfo? field = typeFromHandle.GetField("Instance", BindingFlags.Static | BindingFlags.Public);

            if (field != null)
            {
                MoneyManager moneyManager = (MoneyManager) field.GetValue(null);
                if (moneyManager != null) moneyManager.ChangeCashBalance(robMoney, true, true);
            }
            else
            {
                MoneyManager moneyManager = Object.FindObjectOfType<MoneyManager>();
                if (moneyManager != null) moneyManager.ChangeCashBalance(robMoney, true, true);
            }
            
            foreach (NPC npc in Object.FindObjectsOfType<NPC>())
            {
                if (npc != null && npc.IsConscious && npc.awareness && npc.awareness.VisionCone &&
                    npc.awareness.VisionCone.sightedPlayers != null &&
                    npc.awareness.VisionCone.sightedPlayers.Contains(Player.Local) && npc.dialogueHandler != null &&
                    npc.actions != null)
                {
                    npc.actions.SetCallPoliceBehaviourCrime(new ArmedRobbery());
                    npc.actions.CallPolice_Networked(Player.Local);
                    npc.SetPanicked();
                    npc.dialogueHandler.PlayReaction("panic_start", 5f, true);
                    if (npc.VoiceOverEmitter == null) continue;
                    npc.VoiceOverEmitter.Play(EVOLineType.Scared);
                }
            }
            
            foreach (GameObject gameObject in RobbableRegisters)
            {
                InteractableObject interactable = gameObject.GetComponent<InteractableObject>();
                interactable.enabled = false;
            }
        }
        
        

        private static readonly List<EClothingColor> AllColors =
        [
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
        ];
    }
}