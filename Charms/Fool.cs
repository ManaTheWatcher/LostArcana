using HutongGames.PlayMaker.Actions;
using LostArcana.ReflectionExtensions;

namespace LostArcana
{
    internal class Fool : Charm
    {
        #region Basic charm stuff
        public static readonly Fool instance = new();

        // Set basic charm data
        public override string Sprite => "Fool.png";
        public override string Name => "Fool";
        public override string Description => "The Fool";
        public override int DefaultCost => 1;

        private Fool() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Fool;
        #endregion

        // Declare Variables
        private int DamageWhenEquipped = 50;
        private int DamageWhenUnequipped;

        private bool isInvincible;

        FsmState WallCharge;
        FsmState WallChargeCancel;

        private bool ShouldFlip;

        public override void Hook()
        {
            On.HeroController.TakeDamage += DoInvincibility;

            On.HeroController.CanSuperDash += AllowAirSD;
        }

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("SD Burst", "damages_enemy", IncreaseCDashDamage),
            ("SuperDash Damage", "damages_enemy", IncreaseCDashDamage),
            ("Knight", "Superdash", AddSDFSM)
        };

        private void DoInvincibility(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject damageSource, CollisionSide damageSide, int damageAmount, int hazardType)
        {
            if (Equipped() &&
                isInvincible &&
                damageSource.GetComponent<HealthManager>())
            {
                return;
            }

            orig(self, damageSource, damageSide, damageAmount, hazardType);
        }

        private void IncreaseCDashDamage(PlayMakerFSM fsm)
        {
            DamageWhenUnequipped = fsm.FsmVariables.IntVariables[3].Value;

            // I got this code from https://github.com/BubkisLord/Fyrenest
            // This is the file where I got it from: https://github.com/BubkisLord/Fyrenest/blob/main/Charms/BetterCDash.cs
            var sendEvent = fsm.GetState("Send Event");
            // Guard against the IntCompare action not being there.
            // That sometimes happens, even though the code works.
            // This is only to keep it from flooding modlog with spurious exceptions.
            var damage = (sendEvent?.Actions[0] as IntCompare)?.integer1;
            if (damage != null &&
                sendEvent != null)
            {
                sendEvent.PrependAction(() => {
                    damage.Value = Equipped() ? DamageWhenEquipped : DamageWhenUnequipped;
                });
            }
        }

        private void AddSDFSM(PlayMakerFSM fsm)
        {
            // Variables
            WallCharge = fsm.GetState("Wall Charge");
            WallChargeCancel = fsm.GetState("Charge Cancel Wall");

            #region Invincibility
            AddEnableInvincibilityAction("Left");
            AddEnableInvincibilityAction("Right");
            AddDisableInvincibilityAction("Air Cancel");
            AddDisableInvincibilityAction("Hit Wall");

            void AddEnableInvincibilityAction(string fsmName)
            {
                fsm.GetState(fsmName)?.AddAction(() => {
                    if (Equipped()) isInvincible = true;
                });
            }
            void AddDisableInvincibilityAction(string fsmName)
            {
                fsm.GetState(fsmName)?.AddAction(() => {
                    if (isInvincible) isInvincible = false;
                });
            }
            #endregion

            #region AirCharge
            fsm.GetState("Relinquish Control")?.PrependAction(() => {
                if (Equipped() &&
                    !HeroController.instance.cState.wallSliding &&
                    !HeroController.instance.cState.onGround)
                {
                    ShouldFlip = true;
                }
            });

            // Flips the knight when charging superdash in the air, so you dash in the direction that you're looking
            WallCharge?.PrependAction(() => {
                if (ShouldFlip) HeroController.instance.FlipSprite();
            });

            // Flips the knight back if you cancel a superdash midair
            WallChargeCancel?.AddAction(() => {
                if (Equipped() &&
                    ShouldFlip)
                {
                    HeroController.instance.FlipSprite();
                }
            });

            fsm.GetState("Regain Control")?.AddAction(() => ShouldFlip = false);
            #endregion
        }

        private bool AllowAirSD(On.HeroController.orig_CanSuperDash orig, HeroController self)
        {
            if (!Equipped())
            {
                // Enables everything if the Fool charm isn't equipped
                WallCharge.Actions[7].Enabled = true;
                WallCharge.Actions[8].Enabled = true;
                WallChargeCancel.Actions[7].Enabled = true;

                return orig(self);
            }

            // Disables the checks that makes sure the knight is on a wall
            WallCharge.Actions[7].Enabled = false;
            WallCharge.Actions[8].Enabled = false;

            // Makes the knight not wall slide midair when you cancel superdash
            WallChargeCancel.Actions[7].Enabled = self.cState.wallSliding;

            // Returns the same as the HeroController's CanSuperDash, except whether the player is on the ground on on a wall
            return !GameManager.instance.isPaused &&
                self.hero_state != ActorStates.no_input &&
                !self.cState.dashing &&
                !self.cState.hazardDeath &&
                !self.cState.hazardRespawning &&
                !self.cState.backDashing &&
                (!self.cState.attacking || self.GetFieldValue<float>("attack_time") >= self.ATTACK_RECOVERY_TIME) &&
                !self.cState.slidingLeft &&
                !self.cState.slidingRight &&
                !self.controlReqlinquished &&
                !self.cState.recoilFrozen &&
                !self.cState.recoiling &&
                !self.cState.transitioning &&
                self.playerData.GetBool("hasSuperDash");
        }
    }
}