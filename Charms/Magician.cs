namespace LostArcana
{
    internal class Magician : Charm
    {
        #region Basic charm stuff
        public static readonly Magician instance = new();

        // Set basic charm data
        public override string Sprite => "Magician.png";
        public override string Name => "Magician";
        public override string Description => "The Magician";
        public override int DefaultCost => 2;

        private Magician() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Magician;
        #endregion

        // Declare Variables
        private readonly float MultiplierUpperBounds = 2f;
        private readonly float MultiplierLowerBounds = 1f;
        private const int DefaultMaxSoul = 198;
        private float SpellDamageMultiplier;

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
            Action spellCastingModifier = () => {
                if (!Equipped()) return;

                int spellCost = fsm.FsmVariables.GetFsmInt("MP Cost").Value;
                // Sets the spell damage multiplier based on how much soul you have when casting
                SpellDamageMultiplier = (PlayerData.instance.MPCharge + PlayerData.instance.MPReserve - spellCost) * (MultiplierUpperBounds - MultiplierLowerBounds) / (DefaultMaxSoul - spellCost) + MultiplierLowerBounds;
                Modding.Logger.Log("Cost: " + spellCost + " - Multiplier: " + SpellDamageMultiplier);
            };

            fsm.GetState("Can Cast?").PrependAction(spellCastingModifier);
            fsm.GetState("Can Cast? QC").PrependAction(spellCastingModifier);
        }

        private void HitEnemy(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (Equipped() &&
                hitInstance.AttackType == AttackTypes.Spell)
            {
                hitInstance.DamageDealt = Mathf.RoundToInt(hitInstance.DamageDealt * SpellDamageMultiplier);
                Modding.Logger.Log(hitInstance.DamageDealt);
            }

            orig(self, hitInstance);
        }
    }
}