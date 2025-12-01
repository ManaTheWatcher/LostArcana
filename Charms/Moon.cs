namespace LostArcana
{
    internal class Moon : Charm
    {
        #region Basic charm stuff
        public static readonly Moon instance = new();

        // Set basic charm data
        public override string Sprite => "Moon.png";
        public override string Name => "Moon";
        public override string Description => "The Moon";
        public override int DefaultCost => 1;

        private Moon() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Moon;
        #endregion

        // Declare Variables
        public bool HasMoonBuff;
        private float MoonBuffSpeedMultiplier = 1.35f;

        private GameObject ColliderObject;
        private float ColliderRadius = 10.5f;

        private List<Collider2D> ColliderEnemyList;

        public override void Hook()
        {
            On.HeroController.Start += AddColliderOnAwake;
            On.PlayerData.EquipCharm += CreateColliderOnEquip;
            On.PlayerData.UnequipCharm += DestroyColliderOnUnequip;
            On.HeroController.Move += GiveBoost;
            On.HeroController.EnterScene += ResetDict;
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

            CircleCollider2D MoonCollider = ColliderObject.AddComponent<CircleCollider2D>();
            MoonCollider.radius = ColliderRadius;
            MoonCollider.isTrigger = true;

            ColliderEnemyList = ColliderObject.AddComponent<MoonColliderBehavior>().ColliderEnemyList;

        }

        private void GiveBoost(On.HeroController.orig_Move orig, HeroController self, float move_direction)
        {
            if (HasMoonBuff) move_direction *= MoonBuffSpeedMultiplier;

            orig(self, move_direction);
        }

        private IEnumerator ResetDict(On.HeroController.orig_EnterScene orig, HeroController self, TransitionPoint enterGate, float delayBeforeEnter)
        {
            if (Equipped())
            {
                ColliderEnemyList.Clear();
                HasMoonBuff = false;
            }

            return orig(self, enterGate, delayBeforeEnter);
        }
    }

    public class MoonColliderBehavior : MonoBehaviour
    {
        public List<Collider2D> ColliderEnemyList = new();

        private void OnTriggerEnter2D (Collider2D otherCollider)
        {
            // Check if the collider that entered belongs to an enemy
            if (otherCollider.GetComponent<HealthManager>())
            {
                ColliderEnemyList.Add(otherCollider);
                if (ColliderEnemyList.Count > 0)
                {
                    Moon.instance.HasMoonBuff = true;
                    Modding.Logger.Log("Enable Moonbuff");
                }
            }

        }

        private void OnTriggerExit2D (Collider2D otherCollider)
        {
            if (ColliderEnemyList.Remove(otherCollider) &&
                ColliderEnemyList.Count <= 0)
            {
                Moon.instance.HasMoonBuff = false;
                Modding.Logger.Log("Disable Moonbuff");
            }
        }
    }
}