using LostArcana.ReflectionExtensions;

namespace LostArcana
{
    internal class HangedMan : Charm
    {
        #region Basic charm stuff
        public static readonly HangedMan instance = new();

        // Set basic charm data
        public override string Sprite => "HangedMan.png";
        public override string Name => "Hanged Man";
        public override string Description => "The Hanged Man";
        public override int DefaultCost => 1;

        private HangedMan() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.HangedMan;
        #endregion

        // Declare Variables
        private GameObject ColliderObject;
        private float ColliderRadius = 10.5f;

        private List<HealthManager> EnemyList;

        private float AttackCooldownMultiplierMin = 0.5f;
        private float AttackCooldownMultiplierScale = 0.015f;

        public override void Hook()
        {
            On.HeroController.Start += AddColliderOnAwake;
            On.PlayerData.EquipCharm += CreateColliderOnEquip;
            On.PlayerData.UnequipCharm += DestroyColliderOnUnequip;
            On.HeroController.EnterScene += ResetList;
            On.HeroController.Attack += ReduceAttackCooldown;
        }

        private void AddColliderOnAwake(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            if (Equipped()) CreateCollider();
        }

        private void CreateColliderOnEquip(On.PlayerData.orig_EquipCharm orig, PlayerData self, int charmNum)
        {
            orig(self, charmNum);

            if (charmNum == Num) CreateCollider();
        }

        private void DestroyColliderOnUnequip(On.PlayerData.orig_UnequipCharm orig, PlayerData self, int charmNum)
        {
            orig(self, charmNum);

            if (charmNum == Num) UnityEngine.Object.Destroy(ColliderObject);
        }

        private void CreateCollider()
        {
            // Create game object that holds the collider and attach it to the player
            ColliderObject = new GameObject();
            ColliderObject.transform.position = HeroController.instance.transform.position;
            ColliderObject.transform.parent = HeroController.instance.gameObject.transform;
            ColliderObject.SetActive(true);

            CircleCollider2D _collider = ColliderObject.AddComponent<CircleCollider2D>();
            _collider.radius = ColliderRadius;
            _collider.isTrigger = true;

            EnemyList = ColliderObject.AddComponent<HangedManColliderBehavior>().EnemyList;

        }

        private IEnumerator ResetList(On.HeroController.orig_EnterScene orig, HeroController self, TransitionPoint enterGate, float delayBeforeEnter)
        {
            if (Equipped()) EnemyList.Clear();

            return orig(self, enterGate, delayBeforeEnter);
        }

        private void ReduceAttackCooldown(On.HeroController.orig_Attack orig, HeroController self, AttackDirection attackDir)
        {
            orig(self, attackDir);

            if (!Equipped()) return;

            int totalHealth = 0;
            foreach (HealthManager enemy in EnemyList)
            {
                totalHealth += enemy.hp;
            }

            // The game does attack speed by setting an attack cooldown.
            // To increase the attack speed, I multiply the cooldown by a number between 1 and 0.
            // I wanted the multiplier to follow a radical function so that it would scale nicely with both regular enemies and bosses.
            var attackCooldownMultiplier = (float)(1 - (Mathf.Sqrt(totalHealth + 1) - 1) * AttackCooldownMultiplierScale);
            // The cap is to make sure the attack speed doesn't grow out of hand when massive hp pools are nearby
            if (attackCooldownMultiplier < AttackCooldownMultiplierMin) attackCooldownMultiplier = AttackCooldownMultiplierMin;

            SetAttackCooldown("attack_cooldown");
            SetAttackCooldown("attackDuration");

            void SetAttackCooldown(string varName)
            {
                ReflectionHelper.SetField(HeroController.instance, varName, HeroController.instance.GetFieldValue<float>(varName) * attackCooldownMultiplier);
            }
        }
    }

    public class HangedManColliderBehavior : MonoBehaviour
    {
        public List<HealthManager> EnemyList = new();

        private void OnTriggerEnter2D(Collider2D otherCollider)
        {
            // Check if the collider that entered belongs to an enemy
            var healthManager = otherCollider.GetComponent<HealthManager>();
            if (healthManager != null) EnemyList.Add(healthManager);
        }

        private void OnTriggerExit2D(Collider2D otherCollider)
        {
            EnemyList.Remove(otherCollider.GetComponent<HealthManager>());
        }
    }
}