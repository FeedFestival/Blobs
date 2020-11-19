using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.utils;
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
            bool isBlob = col.transform.tag == "Blob";
            ContactPoint2D surfaceContact = col.GetContact(0);
            OnCollision(surfaceContact: surfaceContact, isBlob ? col.gameObject.GetComponent<Blob>() : null);
        }
    }

    public void OnCollision(ContactPoint2D surfaceContact, Blob otherBlob = null)
    {
        BlobHitStickyInfo blobHitStickyInfo = new BlobHitStickyInfo(transform.position.y, GetComponent<Blob>());
        blobHitStickyInfo.otherBlob = otherBlob;

        blobHitStickyInfo.ReflectDir = world2d.GetNormalizedDirection(Game._.Player.LastDir, surfaceContact.normal);

        Game._.Player.BlobHitSticky(blobHitStickyInfo);
        Destroy(_rb);
        Destroy(this);
    }
}
