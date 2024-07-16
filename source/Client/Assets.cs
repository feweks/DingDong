using Raylib_cs;

namespace Dong.Client;

class Assets
{
    private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    private static Dictionary<string, Font> fonts = new Dictionary<string, Font>();
    private static Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();

    public static Texture2D GetTexture(string path)
    {
        if (textures.ContainsKey(path)) return textures[path];

        Texture2D tex = Raylib.LoadTexture(path);
        textures.Add(path, tex);
        return tex;
    }

    public static Font GetFont(string path)
    {
        if (fonts.ContainsKey(path)) return fonts[path];

        Font fnt = Raylib.LoadFontEx(path, 102, null, 400);
        fonts.Add(path, fnt);
        return fnt;
    }

    public static Sound GetSound(string path)
    {
        if (sounds.ContainsKey(path)) return sounds[path];

        Sound sfx = Raylib.LoadSound(path);
        sounds.Add(path, sfx);
        return sfx;
    }
}