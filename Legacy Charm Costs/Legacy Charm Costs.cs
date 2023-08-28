using Modding;

namespace LegacyCharmCosts{
    public class GlobalSettingsClass{
        public bool permanentChanges = false;
        public bool legacyQS = true;
        public bool legacyFN = true;
    }

    public class LegacyCharmCosts : Mod, ITogglableMod, IMenuMod, IGlobalSettings<GlobalSettingsClass>{
        new public string GetName() => "Legacy Charm Costs";
        public override string GetVersion() => "1.0.2.2";
        public static GlobalSettingsClass GS {get; set;} = new GlobalSettingsClass();
        public bool ToggleButtonInsideMenu => true;
        public int NotchAmount = 0;
        public override void Initialize(){
            ModHooks.GetPlayerIntHook += GetInt;
            ModHooks.GetPlayerBoolHook += GetBool;
            On.HeroController.Awake += HCAwake;
        }

        public void OnLoadGlobal(GlobalSettingsClass s) => GS = s;
        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry){
            IMenuMod.MenuEntry toggleMod = new IMenuMod.MenuEntry(toggleButtonEntry!.Value.Name, toggleButtonEntry.Value.Values, "Toggles the Legacy Charm Costs mod", toggleButtonEntry.Value.Saver, toggleButtonEntry.Value.Loader);
            return new List<IMenuMod.MenuEntry> {
                toggleMod,
                new IMenuMod.MenuEntry {
                    Name = "Permanent Changes",
                    Description = "Should the changes be saved to the opened files?",
                    Values = new string[] {
                        "No",
                        "Yes",
                    },
                    Saver = opt => (GS.permanentChanges, PlayerData.instance.charmCost_32, PlayerData.instance.charmCost_11) = opt switch {
                        0 => (false,QSSavedCost(false,GS.legacyQS),FNSavedCost(false, GS.legacyFN)),
                        1 => (true,QSSavedCost(true, GS.legacyQS),FNSavedCost(true, GS.legacyFN)),
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => GS.permanentChanges switch {
                        false => 0,
                        true => 1,
                    }
                },
                new IMenuMod.MenuEntry {
                    Name = "Legacy QuickSlash",
                    Description = "Should QuickSlash notch cost be reverted to 2?",
                    Values = new string[] {
                        "No",
                        "Yes",
                    },
                    Saver = opt => (GS.legacyQS, PlayerData.instance.charmCost_32) = opt switch {
                        0 => (false,QSSavedCost(GS.permanentChanges,false)),
                        1 => (true,QSSavedCost(GS.permanentChanges,true)),
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => GS.legacyQS switch {
                        false => 0,
                        true => 1,
                    }
                },
                new IMenuMod.MenuEntry {
                    Name = "Legacy Flukenest",
                    Description = "Should Flukenest notch cost be reverted to 2?",
                    Values = new string[] {
                        "No",
                        "Yes",
                    },
                    Saver = opt => (GS.legacyFN, PlayerData.instance.charmCost_11) = opt switch {
                        0 => (false,FNSavedCost(GS.permanentChanges,false)),
                        1 => (true,FNSavedCost(GS.permanentChanges,true)),
                        _ => throw new InvalidOperationException()
                    },
                    Loader = () => GS.legacyFN switch {
                        false => 0,
                        true => 1,
                    }
                }
            };
        }

        private int GetInt(string name, int orig){
            if (name == "charmCost_32"){
                if (GS.legacyQS){
                    orig = 2;
                }
                PlayerData.instance.charmCost_32 = QSSavedCost(GS.permanentChanges,GS.legacyQS);
            Log("Ensuring in-game QuickSlash cost matches with the settings");
            }

            else if(name == "charmCost_11"){
                if (GS.legacyFN){
                    orig = 2;
                }
                PlayerData.instance.charmCost_11 = FNSavedCost(GS.permanentChanges,GS.legacyFN);
            Log("Ensuring in-game Flukenest cost matches with the settings");
            }
            else if(name == "charmSlots"){
                NotchAmount = orig;
            }
            return orig;
        }
        private bool GetBool(string name, bool orig){
            if (name == "overcharmed"){
                Log($"Checking for overcharmed, saw {NotchAmount} notches");
                if (NotchAmount - PlayerData.instance.charmSlotsFilled < 0){
                    orig = true;
                }
                else {
                    orig = false;
                }
            }
            return orig;
        }

        public int QSSavedCost(bool permachanges, bool legaQS){
            if (legaQS && permachanges){
                Log("Saved Quickslash cost: 2");
                return 2;
            }
            else{
                Log("Saved Quickslash cost: 3");
                return 3;
            }
        }
        public int FNSavedCost(bool permachanges, bool legaFN){
            if (legaFN && permachanges){
                Log("Saved Flukenest cost: 2");
                return 2;
            }
            else{
                Log("Saved Flukenest cost: 3");
                return 3;
            }
        }

        private void HCAwake(On.HeroController.orig_Awake orig, HeroController self){
            PlayerData.instance.charmCost_32 = QSSavedCost(GS.permanentChanges,GS.legacyQS);
            PlayerData.instance.charmCost_11 = FNSavedCost(GS.permanentChanges,GS.legacyFN);
            orig(self);
        }

        public GlobalSettingsClass OnSaveGlobal(){
    	    return GS;
        }
        
        public void Unload(){
            ModHooks.GetPlayerIntHook -= GetInt;
            ModHooks.GetPlayerBoolHook -= GetBool;
            On.HeroController.Awake -= HCAwake;
        }
    }
}