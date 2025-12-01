namespace LostArcana
{
    internal class Tower : Charm
    {
        #region Basic charm stuff
        public static readonly Tower instance = new();

        // Set basic charm data
        public override string Sprite => "Tower.png";
        public override string Name => "Tower";
        public override string Description => "The Tower";
        public override int DefaultCost => 2;

        private Tower() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Tower;
        #endregion

        // Declare Variables
        private int ExtraLivesAmount = 1;
        private int ExtraLivesHad;

        private GameObject BlastObject;
        private float BlastRadius = 15f;
        private int DamageAmount = 80;
        private bool ShouldDoBlast;

        private bool OvercharmedFix;

        public override void Hook()
        {
            ModHooks.AfterTakeDamageHook += DoExtraLife;
            On.GameManager.FreezeMoment_float_float_float_float += DoBlast;
            ModHooks.SetPlayerBoolHook += OnBench;
        }

        private int DoExtraLife(int hazardType, int damageAmount)
        {
            if (!Equipped() &&
                ExtraLivesHad >= ExtraLivesAmount)
            {
                return damageAmount;
            }

            // Check if you would die if you're overcharmed
            if (PlayerData.instance.overcharmed &&
                damageAmount * 2 >= PlayerData.instance.health + PlayerData.instance.healthBlue)
            {
                PlayerData.instance.overcharmed = false;
                OvercharmedFix = true;

                ExtraLifeFunc(out damageAmount);
            }
            else if (damageAmount >= PlayerData.instance.health + PlayerData.instance.healthBlue)
            {
                ExtraLifeFunc(out damageAmount);
            }

            return damageAmount;

            void ExtraLifeFunc(out int damageAmount)
            {
                damageAmount = PlayerData.instance.health + PlayerData.instance.healthBlue - 1;
                ExtraLivesHad++;
                ShouldDoBlast = true;
            }
        }

        private IEnumerator DoBlast(On.GameManager.orig_FreezeMoment_float_float_float_float orig, GameManager self, float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
        {
            if (!(Equipped() && ShouldDoBlast))
            {
                yield return orig(self, rampDownTime, waitTime, rampUpTime, targetSpeed);
                yield break;
            }
            
            ShouldDoBlast = false;

            // Create object that carries the damaging stuff
            BlastObject = new GameObject();
            BlastObject.transform.parent = self.transform;
            BlastObject.transform.position = HeroController.instance.transform.position;
            BlastObject.SetActive(true);

            // Create and activate collider that detects all enemies that should take damage
            CircleCollider2D BlastCollider = BlastObject.AddComponent<CircleCollider2D>();
            BlastCollider.radius = BlastRadius;
            BlastCollider.isTrigger = true;

            // Create component that damages the enemies selected by the collider
            BlastObject.AddComponent<DamageEnemies>();
            DamageEnemies _DamageEnemies = BlastObject.GetComponent<DamageEnemies>();
            _DamageEnemies.damageDealt = DamageAmount;
            _DamageEnemies.attackType = AttackTypes.Generic;
            _DamageEnemies.ignoreInvuln = true;
            _DamageEnemies.enabled = true;

            Modding.Logger.Log("Towercollider Created");

            yield return orig(self, rampDownTime, waitTime, rampUpTime, targetSpeed);

            if (OvercharmedFix)
            {
                PlayerData.instance.overcharmed = true;
                OvercharmedFix = false;
            }

            yield return null;
            UnityEngine.Object.Destroy(BlastObject);
            Modding.Logger.Log("Towercollider Destroyed");
        }

        private bool OnBench(string name, bool orig)
        {
            if (name == "atBench" &&
                PlayerData.instance.atBench)
            {
                ExtraLivesHad = 0;
            }

            return orig;
        }
    }
}