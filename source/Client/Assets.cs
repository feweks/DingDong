using Raylib_cs;

namespace Dong.Client;

class Assets
{
    private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    private static Dictionary<string, Font> fonts = new Dictionary<string, Font>();

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
}