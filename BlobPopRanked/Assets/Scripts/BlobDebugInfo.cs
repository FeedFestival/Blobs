using Assets.HeadStart.Core;
using Assets.Scripts;
using Assets.Scripts.utils;
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
        Color color = BlobColorService.GetColorByBlobColor(blobColor);
        color.a = transparency;
        spriteColor.color = color;
        color = __gameColor.GetColor("#FFFFFF");
        color.a = transparency;
        IdText.color = color;
        color = __gameColor.GetColor("#FFFFFF");
        color.a = transparency;
        NeighborText.color = color;
    }
}
