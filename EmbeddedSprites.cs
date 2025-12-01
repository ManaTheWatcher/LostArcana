namespace LostArcana
{
    internal static class EmbeddedSprite
    {
        private static Dictionary<string, Sprite> Sprites = new();

        public static Sprite Get(string name)
        {
            if (Sprites.TryGetValue(name, out var sprite))
            {
                return sprite;
            }
            sprite = LoadSprite(name);
            Sprites[name] = sprite;
            return sprite;
        }

        private static Sprite LoadSprite(string name)
        {
            var loc = Path.Combine(Path.GetDirectoryName(typeof(EmbeddedSprite).Assembly.Location) + "/Resources", name);
            var imageData = File.ReadAllBytes(loc);
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(tex, imageData, true);
            tex.filterMode = FilterMode.Bilinear;
            if (loc.Contains("LifeBloodCharm.png")) return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 100f / 1.85f);
            else return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
        }
    }
}