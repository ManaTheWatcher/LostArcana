namespace LostArcana
{
    internal class Death : Charm
    {
        #region Basic charm stuff
        public static readonly Death instance = new();

        // Set basic charm data
        public override string Sprite => "Death.png";
        public override string Name => "Death";
        public override string Description => "Death";
        public override int DefaultCost => 2;

        private Death() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Death;
        #endregion

        // Declare Variables
        private bool IsRunning;
        private float RunSpeed = 13.5f;
        private float moveDirection;
        private Rigidbody2D KnightRb2D;

        public override void Hook()
        {
            On.HeroController.Dash += EnableRun;
            On.HeroController.Move += DoRun;
            ModHooks.HeroUpdateHook += RunCheck;
            On.HeroController.Start += FindRigidbody2D;
        }

        private void EnableRun(On.HeroController.orig_Dash orig, HeroController self)
        {
            orig(self);

            if (!Equipped()) return;
            moveDirection = HeroController.instance.move_input;
            IsRunning = true;
        }

        private void DoRun(On.HeroController.orig_Move orig, HeroController self, float move_direction)
        {
            orig(self, move_direction);

            if (!IsRunning) return;
            KnightRb2D.velocity = new Vector2(move_direction * RunSpeed, KnightRb2D.velocity.y);
        }

        private void RunCheck()
        {
            if (IsRunning &&
                (!InputHandler.Instance.inputActions.dash ||
                moveDirection != HeroController.instance.move_input ||
                HeroController.instance.cState.touchingWall))
            {
                IsRunning = false;
                Modding.Logger.Log("Running Off");
            }
        }

        private void FindRigidbody2D(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            KnightRb2D = HeroController.instance.GetComponent<Rigidbody2D>();
        }
    }
}