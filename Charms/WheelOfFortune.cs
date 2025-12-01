using HutongGames.PlayMaker.Actions;
using LostArcana.ReflectionExtensions;

namespace LostArcana
{
    internal class WheelOfFortune : Charm
    {
        #region Basic charm stuff
        public static readonly WheelOfFortune instance = new();

        // Set basic charm data
        public override string Sprite => "WheelOfFortune.png";
        public override string Name => "Wheel Of Fortune";
        public override string Description => "The Wheel Of Fortune";
        public override int DefaultCost => 2;

        private WheelOfFortune() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.WheelOfFortune;
        #endregion

        // Declare Variables
        private bool NailArtHitEnemy;
        private bool CycloneActive;

        public override void Hook()
        {
            // Workaround to make this charm work for cyclone slash, because the game doesn't have a "damages_enemy" fsm for cyclone slash
            On.HeroController.StartCyclone += StartCyclone;
            On.HeroController.EndCyclone += EndCyclone;
            On.HealthManager.TakeDamage += HitEnemy;
        }

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("Knight", "Nail Arts", NailArtFinish),
            ("Great Slash", "damages_enemy", AllowWheelRecharge),
            ("Dash Slash", "damages_enemy", AllowWheelRecharge)
        };

        private void StartCyclone(On.HeroController.orig_StartCyclone orig, HeroController self)
        {
            orig(self);

            CycloneActive = true;
        }

        private void EndCyclone(On.HeroController.orig_EndCyclone orig, HeroController self)
        {
            orig(self);

            CycloneActive = false;
        }

        private void HitEnemy(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);

            if (Equipped() &&
                hitInstance.AttackType == AttackTypes.Nail &&
                CycloneActive)
            {
                NailArtHitEnemy = true;
            }
        }

        private void NailArtFinish(PlayMakerFSM fsm)
        {
            var regaincontrol = fsm.GetState("Regain Control");

            // When regaining control of the knight after a nail art, it checks if the Fool charm is equipped, if the nail art hit an enemy and if the player is holding the attack button
            regaincontrol.AddAction(() => {
                NailArtHitEnemy = false;

                if (!Equipped() ||
                !NailArtHitEnemy ||
                !InputHandler.Instance.inputActions.attack.IsPressed)
                {
                    return;
                }

                ReflectionHelper.SetField(HeroController.instance, "nailChargeTimer", HeroController.instance.GetFieldValue<float>("nailChargeTime"));
                HeroController.instance.cState.nailCharging = true;
                Modding.Logger.Log("Wheel Recharge");
            });
        }

        private void AllowWheelRecharge(PlayMakerFSM fsm)
        {
            var collider = (fsm.GetState("Idle")?.Actions[2] as Trigger2dEvent)?.storeCollider;

            fsm.GetState("Send Event").PrependAction(() => {
                // Checks if the collider hit by the nail art belongs to an enemy
                if (collider.Value.gameObject.GetComponent<HealthManager>()) NailArtHitEnemy = true;
            });
        }
    }
}