namespace LostArcana
{
    internal class Lovers : Charm
    {
        #region Basic charm stuff
        public static readonly Lovers instance = new();

        // Set basic charm data
        public override string Sprite => "Lovers.png";
        public override string Name => "Lovers";
        public override string Description => "The Lovers";
        public override int DefaultCost => 1;

        private Lovers() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Lovers;
        #endregion

        //Declare Variables
        private GameObject BlastObject;
        private float BlastRadius = 6f;
        private float BlastLifespan = 0.1f;

        public override void Hook()
        {
            On.HeroController.DoDoubleJump += Blast;
        }

        private void Blast(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            orig(self);

            if (!Equipped()) return;

            // Create object that carries the damaging stuff
            BlastObject = new GameObject();
            BlastObject.transform.parent = self.transform;
            BlastObject.transform.position = HeroController.instance.transform.position;
            BlastObject.SetActive(true);

            // Create and activate collider that detects all enemies that should take damage
            CircleCollider2D blastCollider = BlastObject.AddComponent<CircleCollider2D>();
            blastCollider.radius = BlastRadius;
            blastCollider.isTrigger = true;

            // Create component that damages the enemies selected by the collider
            BlastObject.AddComponent<DamageEnemies>();
            DamageEnemies _DamageEnemies = BlastObject.GetComponent<DamageEnemies>();
            _DamageEnemies.damageDealt = PlayerData.instance.nailDamage;
            _DamageEnemies.attackType = AttackTypes.Generic;
            _DamageEnemies.ignoreInvuln = false;
            _DamageEnemies.enabled = true;

            Modding.Logger.Log("Loverscollider Created");

            HeroController.instance.StartCoroutine(DestroyObject(blastCollider));
        }

        private IEnumerator DestroyObject(CircleCollider2D blastCollider)
        {
            yield return new WaitForSeconds(BlastLifespan);
            UnityEngine.Object.Destroy(BlastObject);
            Modding.Logger.Log("Loverscollider Destroyed");
        }
    }
}