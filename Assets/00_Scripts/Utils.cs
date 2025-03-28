using UnityEngine;
using UnityEngine.U2D;

public class Utils : MonoBehaviour
{
    public static SpriteAtlas atlas = Resources.Load<SpriteAtlas>("Atlas");
    public static Sprite GetAtlas(string name)
    {
        return atlas.GetSprite(name);
    }
}
