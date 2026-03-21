namespace LostArcana
{
    internal class Hierophant : Charm
    {
        #region Basic charm stuff
        public static readonly Hierophant instance = new();

        // Set basic charm data
        public override string Sprite => "Hierophant.png";
        public override string Name => "Hierophant";
        public override string Description => "The Hierophant is a spiritual guide and a teacher. They bring new knowledge to those willing to listen.\n\nTurns the blood of your enemies into Lifeblood.";
        public override int DefaultCost => 1;

        private Hierophant() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Hierophant;
        #endregion

        // Declare variables
        private int HitsNeeded = 10;
        private int HitsCurrent;
        private int MaxBlueHealthAdded = 2;

        public override void Hook()
        {
            On.HealthManager.TakeDamage += HitEnemy;
        }

        private void HitEnemy(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);

            if (!(Equipped() &&
                hitInstance.AttackType == AttackTypes.Nail &&
                PlayerData.instance.healthBlue < MaxBlueHealthAdded))
            {
                return;
            }

            HitsCurrent++;

            if (HitsCurrent < HitsNeeded) return;

            HitsCurrent = 0;
            EventRegister.SendEvent("ADD BLUE HEALTH");
        }
    }
}