namespace LostArcana
{
    internal class Chariot : Charm
    {
        #region Basic charm stuff
        public static readonly Chariot instance = new();

        // Set basic charm data
        public override string Sprite => "Chariot.png";
        public override string Name => "Chariot";
        public override string Description => "The Chariot";
        public override int DefaultCost => 2;

        private Chariot() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Chariot;
        #endregion

        // Declare variables
        private bool HasBoost = false;
        private float DamageMultiplier = 1.5f;

        public override void Hook()
        {
            On.HeroController.Dash += ActivateDamageBoost;
            ModHooks.GetPlayerIntHook += DamageBoost;
            On.HealthManager.TakeDamage += HitEnemy;
            On.PlayerData.UnequipCharm += UnequipResetBoost;
        }

        private void ActivateDamageBoost(On.HeroController.orig_Dash orig, HeroController self)
        {
            orig(self);

            if (Equipped()) HasBoost = true;

            UpdateNailDamage();
        }

        private int DamageBoost(string intName, int damage)
        {
            if (intName == "nailDamage" &&
                HasBoost)
            {
                damage = Mathf.RoundToInt(damage * DamageMultiplier);
            }

            return damage;
        }

        private void HitEnemy(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);

            if (hitInstance.AttackType == AttackTypes.Nail &&
                HasBoost)
            {
                ResetNailDamage();
            }
        }

        private void UnequipResetBoost(On.PlayerData.orig_UnequipCharm orig, PlayerData self, int charmNum)
        {
            orig(self, charmNum);

            if (charmNum == Num) ResetNailDamage();
        }

        private void ResetNailDamage()
        {
            HasBoost = false;
            UpdateNailDamage();
        }
    }
}