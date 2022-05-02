using System.Collections.Generic;
using System.IO;
using UnityEngine;

class CardImageCache
{
    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    private static readonly string[] cardImageFileExtensions = new[] { "png", "jpg" };

    public static (bool, Sprite) GetOrInit(string cardName)
    {
        if (!cache.TryGetValue(cardName, out var sprite))
        {
            sprite = InitCardImage(cardName);
            if (sprite == null)
            {
                return (false, null);
            }

            cache.Add(cardName, sprite);

            Debug.Log("スプライト読み込み");
        }

        return (true, sprite);
    }

    private static Sprite InitCardImage(string cardName)
    {
        foreach (var extension in cardImageFileExtensions)
        {
            var s = InitCardImage(cardName, extension);
            if (s != null)
            {
                return s;
            }
        }

        return null;
    }

    private static Sprite InitCardImage(string cardName, string extension)
    {
        var cardImageFilePath = Path.Combine(Config.CardImagesDirectoryPath, $"{cardName}.{extension}");

        if (!File.Exists(cardImageFilePath))
        {
            return null;
        }

        var bytes = File.ReadAllBytes(cardImageFilePath);
        var texture = new Texture2D(0, 0);
        ImageConversion.LoadImage(texture, bytes);

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }
}
