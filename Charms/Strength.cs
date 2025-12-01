namespace LostArcana
{
    internal class Strength : Charm
    {
        #region Basic charm stuff
        public static readonly Strength instance = new();

        // Set basic charm data
        public override string Sprite => "Strength.png";
        public override string Name => "Strength";
        public override string Description => "Strength";
        public override int DefaultCost => 3;

        private Strength() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Strength;
        #endregion

        // Declare Variables
        private const int DefaultMaxSoul = 99;
        private const float SpellDamageMultiplier = 5f;
        private bool DoDamageMultiplier;

        private int DefaultCastingCost;

        public override void Hook()
        {
            On.HealthManager.TakeDamage += HitEnemy;
        }

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("Knight", "Spell Control", SetSpellDamageMultiplier),
        };

        private void SetSpellDamageMultiplier(PlayMakerFSM fsm)
        {
            var canCast = fsm.GetState("Can Cast?");
            var canCastQC = fsm.GetState("Can Cast? QC");
            var spellEnd = fsm.GetState("Spell End");
            var CastingCost = fsm.FsmVariables.GetFsmInt("MP Cost");

            Action castingModifier = () =>
            {
                if (!Equipped() ||
                PlayerData.instance.MPCharge < DefaultMaxSoul)
                {
                    DoDamageMultiplier = false;
                    return;
                }

                DefaultCastingCost = CastingCost.Value;
                CastingCost.Value = DefaultMaxSoul;
                DoDamageMultiplier = true;
                Modding.Logger.Log("Cost: " + CastingCost.Value);
            };

            canCast.PrependAction(castingModifier);
            canCastQC.PrependAction(castingModifier);
            spellEnd.AddAction(() =>
            {
                if (!DoDamageMultiplier) return;

                CastingCost.Value = DefaultCastingCost;
                DoDamageMultiplier = false;
                Modding.Logger.Log("MP Cost Reset");
            });
        }

        private void HitEnemy(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (Equipped() &&
                hitInstance.AttackType == AttackTypes.Spell &&
                DoDamageMultiplier)
            {
                hitInstance.DamageDealt = Mathf.RoundToInt(hitInstance.DamageDealt * SpellDamageMultiplier);
                Modding.Logger.Log(hitInstance.DamageDealt);
            }

            orig(self, hitInstance);
        }
    }
}