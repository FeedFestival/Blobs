using Assets.Scripts;
using UnityEngine;

public class StickingGlue : MonoBehaviour
{
    private SpriteRenderer Sprite;
    public int StickedTo;

    public void SetStickedTo(Blob stickedTo, BlobColor blobColor)
    {
        StickedTo = stickedTo.Id;
        gameObject.SetActive(true);

        Vector3 difference = stickedTo.Pos - transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);

        if (Sprite == null) {
            Sprite = GetComponent<SpriteRenderer>();
        }
        Sprite.color = BlobColorService.GetLinkColor(blobColor, stickedTo.BlobReveries.BlobColor);
    }

    public void Unstick()
    {
        StickedTo = 0;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
