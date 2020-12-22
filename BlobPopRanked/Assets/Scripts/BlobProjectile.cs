using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.utils;
using UnityEngine;

public class BlobProjectile : MonoBehaviour
{
    public BlobReveries BlobReveries;
    private Rigidbody2D _rb;
    private int? _flightTweenId;

    void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        gameObject.layer = LayerMask.NameToLayer(LAYER.BlobProjectile);
    }

    public void PlayFlight(BlobFLight blobFLight, OnFlightComplete onFlightComplete)
    {
        if (_flightTweenId.HasValue)
        {
            LeanTween.cancel(_flightTweenId.Value);
            _flightTweenId = null;
        }

        Anim anim = new Anim(BlobAnim.Travel, time: blobFLight.time);
        anim.RotE = world2d.LookRotation2D(new Vector2(transform.position.x, transform.position.y), blobFLight.Pos, fromFront: true);
        BlobReveries.PlayAnim(anim);

        bool isWall = blobFLight.Blob == null;
        if (isWall)
        {
            _flightTweenId = LeanTween.move(gameObject, blobFLight.Pos, blobFLight.time).id;
            LeanTween.descr(_flightTweenId.Value).setOnComplete(() =>
            {
                BlobReveries.PlayHitEffect(blobFLight.hitPoint);

                var normalDir = new Vector2(blobFLight.normal.x, 0);
                BlobReveries.StretchOnCollision(normalDir, () =>
                {
                    if (_flightTweenId.HasValue)
                    {
                        LeanTween.cancel(_flightTweenId.Value);
                        _flightTweenId = null;
                    }
                    if (onFlightComplete != null)
                    {
                        onFlightComplete();
                    }
                });
            });
            return;
        }

        _flightTweenId = LeanTween.move(gameObject, blobFLight.Pos, blobFLight.time).id;
        LeanTween.descr(_flightTweenId.Value).setEase(LeanTweenType.linear);
        LeanTween.descr(_flightTweenId.Value).setOnComplete(() =>
        {
            onFlightComplete();
        });
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // sometimes this doesn't register 
        if (col.transform.tag == TAG.Blob || col.transform.tag == TAG.StickySurface)
        {
            bool isBlob = col.transform.tag == TAG.Blob;
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

        BlobReveries.StretchOnCollision(blobHitStickyInfo.ReflectDir, null, hitSticky: true);

        BlobReveries.TrailRenderer.gameObject.SetActive(true);
        Destroy(_rb);
        Destroy(this);
    }
}
