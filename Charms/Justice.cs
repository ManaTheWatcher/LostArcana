namespace LostArcana
{
    internal class Justice : Charm
    {
        #region Basic charm stuff
        public static readonly Justice instance = new();

        // Set basic charm data
        public override string Sprite => "Justice.png";
        public override string Name => "Justice";
        public override string Description => "Justice";
        public override int DefaultCost => 3;

        private Justice() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Justice;
        #endregion

        // Declare variables
        private int SoulGainAmount = 33;

        public override void Hook()
        {
            On.HeroController.NailParry += AddSoul;
        }

        private void AddSoul(On.HeroController.orig_NailParry orig, HeroController self)
        {
            orig(self);

            if (!Equipped()) return;

            // I can't find where I stole this from... oops...
            int mpReserve = PlayerData.instance.MPReserve;
            PlayerData.instance.AddMPCharge(SoulGainAmount);
            GameCameras.instance.soulOrbFSM.SendEvent("MP GAIN");
            if (PlayerData.instance.MPReserve != mpReserve) GameManager.instance.soulVessel_fsm.SendEvent("MP RESERVE UP");
        }
    }
}