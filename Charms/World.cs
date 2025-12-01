namespace LostArcana
{
    internal class World : Charm
    {
        #region Basic charm stuff
        public static readonly World instance = new();

        // Set basic charm data
        public override string Sprite => "World.png";
        public override string Name => "World";
        public override string Description => "The World";
        public override int DefaultCost => 1;

        private World() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.World;
        #endregion

        // Declare Variables
        private readonly int MaxBlueHealthAdded = 1;

        public override List<(string, string, Action<PlayMakerFSM>)> FsmEdits => new()
        {
            ("Knight", "Spell Control", OnFocusFinish)
        };

        private void OnFocusFinish(PlayMakerFSM fsm)
        {
            fsm.GetState("Focus Heal")?.AddAction(() =>
            {
                if (Equipped() &&
                    PlayerData.instance.healthBlue < MaxBlueHealthAdded)
                {
                    EventRegister.SendEvent("ADD BLUE HEALTH");
                }
            });
        }
    }
}