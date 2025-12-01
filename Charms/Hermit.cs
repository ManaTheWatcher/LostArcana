namespace LostArcana
{
    internal class Hermit : Charm
    {
        #region Basic charm stuff
        public static readonly Hermit instance = new();

        // Set basic charm data
        public override string Sprite => "Hermit.png";
        public override string Name => "Hermit";
        public override string Description => "The Hermit";
        public override int DefaultCost => 1;

        private Hermit() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Hermit;
        #endregion

        // Declare Variables
        private int ExtraSoulHitsAmount;
        private int MaxExtraSoulHits = 2;
        private int ExtraSoul = 11;

        public override void Hook()
        {
            ModHooks.SoulGainHook += OnGainSoulFromNail;
        }

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("Knight", "Nail Arts", NailArtFinish)
        };

        private int OnGainSoulFromNail(int soul)
        {
            if (Equipped() &&
                ExtraSoulHitsAmount > 0)
            {
                soul += ExtraSoul * ExtraSoulHitsAmount;
                ExtraSoulHitsAmount--;
            }

            return soul;
        }

        private void NailArtFinish(PlayMakerFSM fsm)
        {
            AddActionToState("DSlash Start", MaxExtraSoulHits);
            AddActionToState("Flash", MaxExtraSoulHits);
            AddActionToState("Flash 2", MaxExtraSoulHits);
            AddActionToState("Regain Control", 0);

            void AddActionToState(string actionName, int value)
            {
                fsm.GetState(actionName).AddAction(() =>
                {
                    ExtraSoulHitsAmount = value;
                });
            }
        }
    }
}