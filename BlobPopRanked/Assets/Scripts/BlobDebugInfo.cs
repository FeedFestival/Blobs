using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobDebugInfo : MonoBehaviour
{
    public TextMesh IdText;
    public TextMesh NeighborText;

    public void SetId(Transform t, int id)
    {
        transform.SetParent(t);
        transform.localPosition = Vector3.zero;
        IdText.text = id.ToString();
    }

    public void FakeKill(BlobColor blobColor, ref SpriteRenderer spriteColor)
    {
        float transparency = 0.15f;
        Color color = Game._.Level<LevelRandomRanked>().GetColorByBlobColor(blobColor);
        color.a = transparency;
        spriteColor.color = color;
        color = HiddenSettings._.White;
        color.a = transparency;
        IdText.color = color;
        color = HiddenSettings._.White;
        color.a = transparency;
        NeighborText.color = color;
    }
}
