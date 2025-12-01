namespace LostArcana
{
    internal class Empress : Charm
    {
        #region Basic charm stuff
        public static readonly Empress instance = new();

        // Set basic charm data
        public override string Sprite => "Empress.png";
        public override string Name => "Empress";
        public override string Description => "The Empress";
        public override int DefaultCost => 1;

        private Empress() { }

        // Connect charm to settings
        public override CharmSettings Settings(SaveSettings s) => s.Empress;
        #endregion

        // Declare variables
        private Dictionary<String, Vector2[]> DefPoints = new Dictionary<String, Vector2[]>();
        private Dictionary<String, Vector2[]> EmpressPoints = new Dictionary<String, Vector2[]>();

        private float ExtensionRange = 1.4f;

        public override void Hook()
        {
            ModHooks.SavegameLoadHook += ResetDicts;
            On.NailSlash.Awake += AwakeNailSlash;
            On.NailSlash.StartSlash += ChangeCollider;
        }

        private void ResetDicts(int obj)
        {
            DefPoints.Clear();
            EmpressPoints.Clear();
        }

        private void AwakeNailSlash(On.NailSlash.orig_Awake orig, NailSlash self)
        {
            orig(self);

            // Get the original collider and add its vertices to a dictionary
            DefPoints.Add(self.name, self.GetComponent<PolygonCollider2D>().points);
            Vector3 localScale = self.GetComponent<PolygonCollider2D>().transform.localScale;

            // Create a custom collider for all nail slash types
            switch (self.name)
            {
                case "WallSlash":
                    AddEmpressPoints([
                        DefPoints[self.name][0],
                        DefPoints[self.name][1],
                        new Vector2(-2f, 0.05f),
                        new Vector2(-2f + (ExtensionRange / localScale.x), -0.05f),
                        new Vector2(-2f, -0.15f),
                        DefPoints[self.name][3],
                        DefPoints[self.name][4],
                        DefPoints[self.name][5]
                    ]);
                    break;

                case "DownSlash":
                    AddEmpressPoints([
                        DefPoints[self.name][0],
                        DefPoints[self.name][1],
                        DefPoints[self.name][2],
                        new Vector2(0.15f, -1.75f),
                        new Vector2(0f, -(1.75f + (ExtensionRange / localScale.y))),
                        new Vector2(-0.15f, -1.75f),
                        DefPoints[self.name][3],
                        DefPoints[self.name][4],
                        DefPoints[self.name][5]
                    ]);
                    break;

                case "AltSlash":
                    AddEmpressPoints([
                        DefPoints[self.name][0],
                        DefPoints[self.name][1],
                        new Vector2(-2.65f, 0.05f),
                        new Vector2(-(2.65f + (ExtensionRange / localScale.x)), -0.05f),
                        new Vector2(-2.65f, -0.15f),
                        DefPoints[self.name][3],
                        DefPoints[self.name][4],
                        DefPoints[self.name][5]
                    ]);
                    break;

                case "Slash":
                    AddEmpressPoints([
                        DefPoints[self.name][0],
                        DefPoints[self.name][1],
                        new Vector2(-2f, 0.05f),
                        new Vector2(-(2f + (ExtensionRange / localScale.x)), -0.05f),
                        new Vector2(-2f, -0.15f),
                        DefPoints[self.name][3],
                        DefPoints[self.name][4],
                        DefPoints[self.name][5]
                    ]);
                    break;

                case "UpSlash":
                    AddEmpressPoints([
                        DefPoints[self.name][0],
                        new Vector2(-0.15f, 1.5f),
                        new Vector2(0f, 1.5f + (ExtensionRange / localScale.y)),
                        new Vector2(0.15f, 1.5f),
                        DefPoints[self.name][1],
                        DefPoints[self.name][2],
                        DefPoints[self.name][3],
                        DefPoints[self.name][4],
                        DefPoints[self.name][5]
                    ]);
                    break;
            }

            void AddEmpressPoints(Vector2[] points)
            {
                EmpressPoints.Add(self.name, points);
            }
        }

        private void ChangeCollider(On.NailSlash.orig_StartSlash orig, NailSlash self)
        {
            PolygonCollider2D nailCollider = self.GetComponent<PolygonCollider2D>();

            // Set the nailslash collider to either the default collider or the custom one created
            if (Equipped())
            {
                nailCollider.points = EmpressPoints[self.name];
                nailCollider.SetPath(0, EmpressPoints[self.name]);
            }
            else
            {
                nailCollider.points = DefPoints[self.name];
                nailCollider.SetPath(0, DefPoints[self.name]);
            }

            orig(self);
        }
    }
}