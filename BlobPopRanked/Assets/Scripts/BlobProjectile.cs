using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.utils;
using UnityEngine;

public class BlobProjectile : MonoBehaviour
{
    private Rigidbody2D _rb;
    private int? _flightTweenId;

    void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        gameObject.layer = LayerMask.NameToLayer("BlobProjectile");
    }

    // public void PlayFlight(BlobFLight blobFLight)
    // {
    //     if (_flightTweenId.HasValue)
    //     {
    //         LeanTween.cancel(_flightTweenId.Value);
    //         _flightTweenId = null;
    //     }

    //     if (_firstAndOnly || _last)
    //     {
    //         if (FirstProjectile == null)
    //         {
    //             Debug.Log("We probably <b>HIT SOMETHING</b> on the way.");
    //             EndAnimatedShot();
    //             return;
    //         }
    //         LastDir = (blobFLight.Pos - (Vector2)FirstProjectile.transform.position).normalized;
    //         DoSecondCheck(blobFLight);
    //     }

    //     _flightTweenId = LeanTween.move(FirstProjectile.gameObject, blobFLight.Pos, blobFLight.time).id;
    //     LeanTween.descr(_flightTweenId.Value).setEase(LeanTweenType.linear);
    //     LeanTween.descr(_flightTweenId.Value).setOnComplete(() =>
    //     {
    //         ShootAnimated();
    //     });
    // }

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
