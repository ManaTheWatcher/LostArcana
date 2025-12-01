namespace LostArcana
{
    internal class Judgement : Charm
    {
        #region Basic Charm Stuff
        public static readonly Judgement instance = new();

        // Set basic charm data
        public override string Sprite => "Judgement.png";
        public override string Name => "Judgement";
        public override string Description => "Judgement";
        public override int DefaultCost => 3;

        private Judgement() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Judgement;
        #endregion

        // Declare variables
        private GameObject LastHit;
        private HealthManager MarkedEnemyHealth;
        private int HitCounter;
        private int HitsNeededForSpecial = 3;

        private Coroutine TimerCoroutine;
        private bool TimerIsRunning;
        private float HitTimer = 6f;

        private HitInstance TickDamage;
        private int TickAmount = 6;
        private float TickSpacing = 0.25f;
        private int TickDamageAmount = 1;

        public override void Hook()
        {
            On.HeroController.Start += AwakeHeroController;
            On.HealthManager.TakeDamage += MarkEnemy;
        }

        private void AwakeHeroController(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            TickDamage = new HitInstance();
            TickDamage.AttackType = AttackTypes.Generic;
            TickDamage.DamageDealt = TickDamageAmount;
            TickDamage.Multiplier = 1f;
            TickDamage.IgnoreInvulnerable = true;
        }

        private void MarkEnemy(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);

            if (!(Equipped() &&
                hitInstance.AttackType == AttackTypes.Nail))
            {
                return;
            }

            if (LastHit != self.gameObject)
            {
                HitCounter = 1;
                LastHit = self.gameObject;
                if (TimerIsRunning) HeroController.instance.StopCoroutine(TimerCoroutine);
                TimerCoroutine = HeroController.instance.StartCoroutine(HitTimerCoroutine());
                return;
            }

            HitCounter++;
            if (HitCounter < HitsNeededForSpecial) return;

            Modding.Logger.Log("Special Hit");
            ResetHit();
            MarkedEnemyHealth = self;
            if (TimerIsRunning) HeroController.instance.StopCoroutine(TimerCoroutine);
            HeroController.instance.StartCoroutine(DamageEnemyCoroutine());
        }

        private IEnumerator DamageEnemyCoroutine()
        {
            for (int i = 0; i < TickAmount; i++)
            {
                yield return new WaitForSeconds(TickSpacing);

                MarkedEnemyHealth?.Hit(TickDamage);
            }
        }

        private IEnumerator HitTimerCoroutine()
        {
            TimerIsRunning = true;
            yield return new WaitForSeconds(HitTimer);
            TimerIsRunning = false;
            ResetHit();
        }

        private void ResetHit()
        {
            HitCounter = 0;
            LastHit = null;
        }
    }
}