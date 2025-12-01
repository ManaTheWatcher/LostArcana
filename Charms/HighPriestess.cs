namespace LostArcana
{
    internal class HighPriestess : Charm
    {
        #region Basic charm stuff
        public static readonly HighPriestess instance = new();

        // Set basic charm data
        public override string Sprite => "HighPriestess.png";
        public override string Name => "High Priestess";
        public override string Description => "The High Priestess";
        public override int DefaultCost => 3;

        private HighPriestess() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.HighPriestess;
        #endregion

        // Declare variables
        private int ExtraSoul = 22;

        public override void Hook()
        {
            ModHooks.SoulGainHook += OnGainSoulByNail;
        }

        private int OnGainSoulByNail(int soul)
        {
            if (Equipped() &&
                PlayerData.instance.MPCharge + PlayerData.instance.MPReserve < PlayerData.instance.focusMP_amount)
            {
                soul += ExtraSoul;
            }

            return soul;
        }
    }
}