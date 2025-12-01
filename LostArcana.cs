global using Modding;
global using GlobalEnums;
global using HutongGames.PlayMaker;
global using SFCore;
global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using UnityEngine;
global using static LostArcana.LostArcana;

namespace LostArcana
{
    public class LostArcana : Mod, IMod, ILocalSettings<SaveSettings>, ITogglableMod
    {
        // Most of this code is either from the Fyrenest or the Transcendence mod
        // Link to the Fyrenest project: https://github.com/BubkisLord/Fyrenest
        // Link to the Transcendence project: https://github.com/dpinela/Transcendence

        #region Basic mod stuff
        public LostArcana() : base("Lost Arcana") { }
        public override string GetVersion() => "0.0.0.1";

        internal static LostArcana instance;

        internal SaveSettings Settings = new();
        public void OnLoadLocal(SaveSettings s) => Settings = s;
        public SaveSettings OnSaveLocal() => Settings;
        #endregion

        /// <summary>
        /// A list of all added charms.
        /// </summary>
        private readonly static List<Charm> Charms = new()
        {
            Fool.instance,
            Magician.instance,
            HighPriestess.instance,
            Empress.instance,
            Emperor.instance,
            Hierophant.instance,
            Lovers.instance,
            Chariot.instance,
            Justice.instance,
            Hermit.instance,
            WheelOfFortune.instance,
            Strength.instance,
            HangedMan.instance,
            Death.instance,
            Temperance.instance,
            Devil.instance,
            Tower.instance,
            Star.instance,
            Moon.instance,
            Sun.instance,
            Judgement.instance,
            World.instance
        };
        public static LostArcana Loadedinstance { get; set; }

        private Dictionary<string, Func<bool, bool>> BoolGetters = new();
        private Dictionary<string, Action<bool>> BoolSetters = new();
        private Dictionary<string, Func<int, int>> IntGetters = new();
        private Dictionary<string, Func<int, int>> IntSetters = new();
        private Dictionary<(string, string), Action<PlayMakerFSM>> FSMEdits = new();

        /// <summary>
        /// Called when the mod is loaded
        /// </summary>
        public override void Initialize()
        {
            Log("Initializing Mod.\nInitializing Part 1...");

            instance = this;

            ModHooks.LanguageGetHook += GetCharmStrings;
            ModHooks.GetPlayerBoolHook += ReadCharmBools;
            ModHooks.SetPlayerBoolHook += WriteCharmBools;
            ModHooks.GetPlayerIntHook += ReadCharmCosts;
            ModHooks.SetPlayerIntHook += WriteCharmCosts;
            On.PlayMakerFSM.OnEnable += EditFSMs;

            ModHooks.AfterSavegameLoadHook += OnLoadSave;
            ModHooks.SavegameLoadHook += ModHooks_SavegameLoadHook;
            On.PlayerData.CountCharms += CountOurCharms;

            Log("Initialization Part 1 Complete.");

            if (LostArcana.Loadedinstance != null) return;
            LostArcana.Loadedinstance = this;

            Log("Initializing Part 2...");

            foreach (Charm charm in Charms)
            {
                var num = CharmHelper.AddSprites(EmbeddedSprite.Get(charm.Sprite))[0];
                charm.Num = num;
                var settings = charm.Settings;

                settings(Settings).Cost = charm.DefaultCost;
                IntGetters[$"charmCost_{num}"] = _ => settings(Settings).Cost;
                IntSetters[$"charmCost_{num}"] = value => settings(Settings).Cost = value;
                AddTextEdit($"CHARM_NAME_{num}", "UI", charm.Name);
                AddTextEdit($"CHARM_DESC_{num}", "UI", () => charm.Description);
                BoolGetters[$"equippedCharm_{num}"] = _ => settings(Settings).Equipped;
                BoolSetters[$"equippedCharm_{num}"] = value => settings(Settings).Equipped = value;
                BoolGetters[$"gotCharm_{num}"] = _ => true;
                BoolGetters[$"newCharm_{num}"] = _ => false;

                charm.Hook();
                charm.Initialize();

                foreach (var edit in charm.FsmEdits)
                {
                    AddFsmEdit(edit.obj, edit.fsm, edit.edit);
                }
            }
            for (var i = 1; i <= 40; i++)
            {
                var num = i; // needed for closure to capture a different copy of the variable each time
                BoolGetters[$"equippedCharm_{num}"] = value => value;
                IntGetters[$"charmCost_{num}"] = value => value;
            }

            if (ModHooks.GetMod("DebugMod") != null)
            {
                DebugModHook.GiveAllCharms(() =>
                {
                    PlayerData.instance.CountCharms();
                });
            }

            Log("Initializing Part 2 Complete.\n\nAll Initializing Complete.");
        }

        private void ModHooks_SavegameLoadHook(int obj)
        {
            PlayerData.instance.CalculateNotchesUsed();
        }

        private void OnLoadSave(SaveGameData obj)
        {
            PlayerData.instance.CalculateNotchesUsed();

        }

        #region Charm Language Replacements
        private string GetCharmStrings(string key, string sheetName, string orig)
        {
            if (TextEdits.TryGetValue((key, sheetName), out var text))
            {
                return text();
            }
            return orig;
        }
        #endregion

        #region Charm Function Stuff
        private readonly Dictionary<(string Key, string Sheet), Func<string>> TextEdits = new();

        internal void AddTextEdit(string key, string sheetName, string text)
        {
            TextEdits.Add((key, sheetName), () => text);
        }

        internal void AddTextEdit(string key, string sheetName, Func<string> text)
        {
            TextEdits.Add((key, sheetName), text);
        }

        private bool ReadCharmBools(string boolName, bool value)
        {
            if (BoolGetters.TryGetValue(boolName, out var f))
            {
                return f(value);
            }
            return value;
        }

        private bool WriteCharmBools(string boolName, bool value)
        {
            if (BoolSetters.TryGetValue(boolName, out var f))
            {
                f(value);
            }
            return value;
        }

        private int ReadCharmCosts(string intName, int value)
        {
            if (IntGetters.TryGetValue(intName, out var cost))
            {
                return cost(value);
            }
            return value;
        }

        private int WriteCharmCosts(string intName, int value)
        {
            if (IntSetters.TryGetValue(intName, out var cost))
            {
                return cost(value);
            }
            return value;
        }

        internal void AddFsmEdit(string objName, string fsmName, Action<PlayMakerFSM> edit)
        {
            var key = (objName, fsmName);
            var newEdit = edit;
            if (FSMEdits.TryGetValue(key, out var orig))
            {
                newEdit = fsm => {
                    orig(fsm);
                    edit(fsm);
                };
            }
            FSMEdits[key] = newEdit;
        }

        private void EditFSMs(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM fsm)
        {
            orig(fsm);
            if (FSMEdits.TryGetValue((fsm.gameObject.name, fsm.FsmName), out var edit))
            {
                edit(fsm);
            }
        }
        #endregion

        private void CountOurCharms(On.PlayerData.orig_CountCharms orig, PlayerData self)
        {
            orig(self);
            self.SetInt("charmsOwned", self.GetInt("charmsOwned") + Charms.Count());
        }

        internal static void UpdateNailDamage()
        {
            static IEnumerator WaitThenUpdate()
            {
                yield return null;
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            }
            GameManager.instance.StartCoroutine(WaitThenUpdate());
        }

        public void Unload()
        {
            foreach (Charm charm in Charms)
            {
                charm.Settings(Settings).Equipped = false;
            }
        }
    }
}