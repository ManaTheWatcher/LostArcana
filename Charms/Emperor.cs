namespace LostArcana
{
    internal class Emperor : Charm
    {
        #region Basic charm stuff
        public static readonly Emperor instance = new();

        // Set basic charm data
        public override string Sprite => "Emperor.png";
        public override string Name => "Emperor";
        public override string Description => "The Emperor";
        public override int DefaultCost => 2;

        private Emperor() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Emperor;
        #endregion

        // Declare variables
        private bool HasBoost;
        private float DamageMultiplier = 4f;


        public override void Hook()
        {
            ModHooks.GetPlayerIntHook += DamageBoost;
            On.HealthManager.TakeDamage += HitEnemy;
            On.PlayerData.UnequipCharm += UnequipResetBoost;
        }

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("Knight", "Spell Control", DetectFocusFinish)
        };

        private void DetectFocusFinish(PlayMakerFSM fsm)
        {
            fsm.GetState("Focus Heal")?.AddAction(() => {
                if (!Equipped()) return;
                HasBoost = true;
                UpdateNailDamage();
            });
        }

        private int DamageBoost(string intName, int damage)
        {
            if (Equipped() &&
                intName == "nailDamage" &&
                HasBoost)
                damage = (int)Math.Round(damage * DamageMultiplier);

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