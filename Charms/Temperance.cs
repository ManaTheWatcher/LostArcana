using MonoMod.Cil;

namespace LostArcana
{
    internal class Temperance : Charm
    {
        #region Basic charm stuff
        public static readonly Temperance instance = new();

        // Set basic charm data
        public override string Sprite => "Temperance.png";
        public override string Name => "Temperance";
        public override string Description => "Temperance";
        public override int DefaultCost => 3;

        private Temperance() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Temperance;
        #endregion

        // Declare Variables
        private static int MaxAirDash = 3;
        private static int MaxDoubleJump = 3;

        private int AirDashCount;
        private int DoubleJumpCount;

        public override void Hook()
        {
            AddResetHooks();

            On.HeroController.HeroDash += AllowExtraAirDash;
            On.HeroController.DoDoubleJump += AllowExtraDoubleJump;
        }

        // I got most of the code from https://github.com/flibber-hk/HollowKnight.SkillUpgrades
        // This is the file where I got it from: https://github.com/flibber-hk/HollowKnight.SkillUpgrades/blob/main/SkillUpgrades/Skills/TripleJump.cs

        #region Air dash stuff
        private void AllowExtraAirDash(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            orig(self);

            if (!Equipped()) return;

            bool shouldAirDash = !self.cState.onGround && !self.cState.inAcid;
            if (self.cState.onGround ||
                self.cState.inAcid)
            {
                return;
            }

            AirDashCount++;
            if (AirDashCount < MaxAirDash) GameManager.instance.StartCoroutine(RefreshDashInAir());
        }

        private IEnumerator RefreshDashInAir()
        {
            yield return new WaitUntil(() => !InputHandler.Instance.inputActions.dash.IsPressed || AirDashCount == 0);
            if (AirDashCount != 0) ReflectionHelper.SetField(HeroController.instance, "airDashed", false);
        }
        #endregion

        #region Double jump stuff
        private void AllowExtraDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            if (!Equipped())
            {
                orig(self);
                return;
            }

            if (DoubleJumpCount > 0)
            {
                // Deactivates the wing prefabs.
                // This fixes the issue where, if you do another double jump before the wing prefab is gone, a new instance of the wing prefab is not created.
                self.dJumpWingsPrefab.SetActive(false);
                self.dJumpFlashPrefab.SetActive(false);
            }

            DoubleJumpCount++;
            if (DoubleJumpCount < MaxDoubleJump) GameManager.instance.StartCoroutine(RefreshDoubleJumpInAir());

            orig(self);
        }

        private IEnumerator RefreshDoubleJumpInAir()
        {
            yield return new WaitForSeconds(0.2f);
            // Resets the private doubleJumped bool in Herocontroller
            yield return new WaitUntil(() => !InputHandler.Instance.inputActions.jump.IsPressed || DoubleJumpCount == 0);
            if (DoubleJumpCount != 0) ReflectionHelper.SetField(HeroController.instance, "doubleJumped", false);
        }
        #endregion

        #region Restore air dash and double jump
        // Declare More Variables
        private readonly List<ILHook> _hooked = new List<ILHook>();
        private readonly string[] CoroHooks = new string[]
        {
            "<EnterScene>",
            "<HazardRespawn>",
            "<Respawn>"
        };

        private void AddResetHooks()
        {
            // These are all the places where the game refreshes the player's air dash and double jump
            IL.HeroController.BackOnGround += ResetAirDashAndDoubleJump;
            IL.HeroController.Bounce += ResetAirDashAndDoubleJump;
            IL.HeroController.BounceHigh += ResetAirDashAndDoubleJump;
            IL.HeroController.DoWallJump += ResetAirDashAndDoubleJump;
            IL.HeroController.EnterSceneDreamGate += ResetAirDashAndDoubleJump;
            IL.HeroController.ExitAcid += ResetAirDashAndDoubleJump;
            IL.HeroController.LookForInput += ResetAirDashAndDoubleJump;
            IL.HeroController.RegainControl += ResetAirDashAndDoubleJump;
            IL.HeroController.ResetAirMoves += ResetAirDashAndDoubleJump;
            IL.HeroController.ShroomBounce += ResetAirDashAndDoubleJump;

            // The code below adds a custom ILHook.
            // This is needed because the orig_Update function is a private function and we can't just access it the easy way.
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (string nested in CoroHooks)
            {
                Type nestedType = typeof(HeroController).GetNestedTypes(flags).First(x => x.Name.Contains(nested));

                _hooked.Add(new ILHook(nestedType.GetMethod("MoveNext", flags), ResetAirDashAndDoubleJump));
            }

            _hooked.Add(new ILHook(typeof(HeroController).GetMethod("orig_Update", flags), ResetAirDashAndDoubleJump));
        }

        private void ResetAirDashAndDoubleJump(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            InsertResetCode("airDashed", () => AirDashCount = 0);
            InsertResetCode("doubleJumped", () => DoubleJumpCount = 0);

            void InsertResetCode(string varName, Action resetCounter)
            {
                // Inserts some code of my choosing into a specific place within the method
                while (cursor.TryGotoNext
                (
                    MoveType.After,
                    i => i.MatchLdcI4(0),
                    i => i.MatchStfld<HeroController>(varName)
                ))
                {
                    cursor.EmitDelegate<Action>(resetCounter);
                }
            }
        }
        #endregion
    }
}