namespace LostArcana
{
    internal class Star : Charm
    {
        #region Basic charm stuff
        public static readonly Star instance = new();

        // Set basic charm data
        public override string Sprite => "Star.png";
        public override string Name => "Star";
        public override string Description => "The Star";
        public override int DefaultCost => 3;

        private Star() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Star;
        #endregion

        // Declare variables
        private static int HealAmount = 1;

        public override void Hook()
        {
            On.EnemyDreamnailReaction.RecieveDreamImpact += Heal;
        }

        private void Heal(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
        {
            orig(self);

            if (Equipped()) HeroController.instance.AddHealth(HealAmount);
        }
    }
}