namespace LostArcana
{
    internal class Devil : Charm
    {
        #region Basic charm stuff
        public static readonly Devil instance = new();

        // Set basic charm data
        public override string Sprite => "Devil.png";
        public override string Name => "Devil";
        public override string Description => "The Devil";
        public override int DefaultCost => 1;

        private Devil() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Devil;
        #endregion

        //Declare Variables
        private DevilShield DevilShield;
        private readonly float ShieldMaxLifespan = 6f;
        private readonly float LifespanIncrease = 1.5f;
        private readonly float ShieldCooldownTime = 8f;
        private bool ShieldOnCooldown = false;

        public override void Hook()
        {
            ModHooks.AfterTakeDamageHook += DoShield;
            On.HealthManager.TakeDamage += EnemyHit;
            ModHooks.SetPlayerBoolHook += OnBench;
            On.HeroController.Start += ActivateDevil;
        }

        /// <summary>
        /// Take no damage when the shield is active, then deactivate the shield
        /// </summary>
        /// <returns></returns>
        private int DoShield(int hazardType, int damageAmount)
        {
            if (!(Equipped() && DevilShield.ShieldActive)) return damageAmount;

            DevilShield.ShieldActive = false;
            DevilShield.DevilTimer = 0f;
            DevilShield.StartCoroutine(ShieldCooldown());
            Modding.Logger.Log("Devil time: " + DevilShield.DevilTimer);
            return 0;
        }

        /// <summary>
        /// Increase the current lifespan of the shield when hitting an enemy
        /// </summary>
        private void EnemyHit(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);

            if (Equipped() &&
                hitInstance.AttackType == AttackTypes.Nail &&
                !ShieldOnCooldown)
            {
                DevilShield.DevilTimer += LifespanIncrease;
                Modding.Logger.Log("Devil time: " + DevilShield.DevilTimer);
            }
        }

        /// <summary>
        /// Resets the shield when you rest at a bench
        /// </summary>
        private bool OnBench(string name, bool orig)
        {
            // Checks if the bool just set was the bool "atBench" and checks if the bool "atBench" is currently true
            if (name == "atBench" &&
                PlayerData.instance.atBench &&
                Equipped())
            {
                DevilShield.DevilTimer = ShieldMaxLifespan;
                ShieldOnCooldown = false;
                Modding.Logger.Log("Devil time: " + DevilShield.DevilTimer);
            }

            return orig;
        }

        /// <summary>
        /// Activate the shield when the game starts
        /// </summary>
        private void ActivateDevil(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            DevilShield = HeroController.instance.gameObject.AddComponent<DevilShield>();
        }

        /// <summary>
        /// Puts the shield on cooldown
        /// </summary>
        private IEnumerator ShieldCooldown()
        {
            ShieldOnCooldown = true;
            yield return new WaitForSeconds(ShieldCooldownTime);
            ShieldOnCooldown = false;
        }
    }

    public class DevilShield : MonoBehaviour
    {
        // Declare variables
        public float DevilTimer = 0f;
        public bool ShieldActive = false;

        private void Update()
        {
            if (!Devil.instance.Equipped()) return;
            if (DevilTimer > 0f)
            {
                DevilTimer -= Time.deltaTime;
            }
            else if (DevilTimer < 0f)
            {
                DevilTimer = 0f;
                ShieldActive = false;
                Modding.Logger.Log("Devil time: " + DevilTimer);
            }

            if (DevilTimer <= 6f) return;
            DevilTimer = 6f;
            ShieldActive = true;
            Modding.Logger.Log("Devil time: " + DevilTimer);
        }
    }
}