using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour
{
    private Rigidbody2D _rb;

    void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        gameObject.layer = LayerMask.NameToLayer("BlobProjectile");
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // sometimes this doesn't register 
        if (col.transform.tag == "Blob" || col.transform.tag == "StickySurface")
        {
            OnCollision(col.transform.tag == "Blob"
                ? col.gameObject.GetComponent<Blob>() : null);
        }
    }

    public void OnCollision(Blob otherBlob = null)
    {
        BlobHitStickyInfo blobHitStickyInfo = new BlobHitStickyInfo(transform.position.y, GetComponent<Blob>());
        blobHitStickyInfo.otherBlob = otherBlob;

        // THIS IS WHERE THE BOUNCE FROM REFLECT HAPPENED AND NOT ANYMORE !!!
        //      blobHitStickyInfo.ReflectDir = GetNormalizedDirection(col.contacts[0].normal);
        // ABOVE !!!

        Game._.Player.BlobHitSticky(blobHitStickyInfo);
        Destroy(_rb);
        Destroy(this);
    }
}
