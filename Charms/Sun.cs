using LostArcana.ReflectionExtensions;

namespace LostArcana
{
    internal class Sun : Charm
    {
        #region Basic charm stuff
        public static readonly Sun instance = new();

        // Set basic charm data
        public override string Sprite => "Sun.png";
        public override string Name => "Sun";
        public override string Description => "The Sun";
        public override int DefaultCost => 2;

        private Sun() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Sun;
        #endregion

        // Declare variables
        private bool HasBoost = false;
        private float BoostDuration = 8f;
        private float AttackCooldownMultiplier = 0.7f;

        public override void Hook()
        {
            On.EnemyDreamnailReaction.RecieveDreamImpact += StartActivateBoost;
            On.HeroController.Attack += ReduceAttackCooldown;
        }

        private void ReduceAttackCooldown(On.HeroController.orig_Attack orig, HeroController self, AttackDirection attackDir)
        {
            orig(self, attackDir);

            if (!HasBoost) return;

            SetAttackCooldown("attack_cooldown");
            SetAttackCooldown("attackDuration");

            void SetAttackCooldown(string varName)
            {
                ReflectionHelper.SetField(HeroController.instance, varName, HeroController.instance.GetFieldValue<float>(varName) * AttackCooldownMultiplier);
            }
        }

        private void StartActivateBoost(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
        {
            orig(self);

            if (Equipped()) HeroController.instance.StartCoroutine(RunTimer());
        }

        private IEnumerator RunTimer()
        {
            HasBoost = true;
            yield return new WaitForSeconds(BoostDuration);
            HasBoost = false;
        }
    }
}