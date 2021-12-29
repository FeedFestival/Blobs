using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.utils;
using UnityEditor;
using UnityEngine;

public class BlobReveries : MonoBehaviour
{
    public SpriteRenderer Sprite;
    public BlobColor BlobColor;
    public List<StickingGlue> StickingGlues;
    public Animator BlobStretchAnimator;
    public SpriteRenderer TravelSprite;
    public SpriteRenderer AuraSprite;
    public Animator TravelAnimator;
    public Transform TravelPivot;
    public TrailRenderer TrailRenderer;
    public GameObject WallBlob;
    [Header("Animations")]
    public AnimationClip StretchClip;
    public AnimationClip StartTravelClip;
    private int? _forcePushTweenId;
    private int? _reflectTweenId;
    private Vector2 _initialPos;
    private Vector2 _reflectToPos;
    private AfterAnim _afterAnimCallback;
    private IEnumerator _playEID;
    private MusicOpts _blobHitSound;
    private MusicOpts _preBlobExplodeSound;
    private MusicOpts _blobExplodeSound;

    internal void SetColor(BlobColor blobColor, bool instant = true)
    {
        BlobColor = blobColor;
        bool isWall = ClasicLv._.DificultyService.BrownIsWall
            && BlobColor == BlobColor.BROWN;

        if (instant)
        {
            Sprite.color = BlobColorService.GetColorByBlobColor(BlobColor);
            if (isWall)
            {
                WallBlob.SetActive(true);
            }
            else
            {
                WallBlob.SetActive(false);
            }

            if (TrailRenderer != null)
            {
                TrailRenderer.startColor = BlobColorService.GetColorByBlobColor(BlobColor);
                TrailRenderer.endColor = BlobColorService.GetColorByBlobColor(BlobColor);
            }
        }

        if (BlobStretchAnimator != null)
        {
            BlobStretchAnimator.keepAnimatorControllerStateOnDisable = true;
        }

        if (TravelSprite != null)
        {
            TravelSprite.color = BlobColorService.GetColorByBlobColor(BlobColor);
            TravelSprite.gameObject.SetActive(false);
        }

        if (AuraSprite != null)
        {
            AuraSprite.color = BlobColorService.GetBlobAuraColor(blobColor);
        }
    }

    public void AnimateElasticSettle(BlobHitStickyInfo blobHitStickyInfo)
    {

        if (_reflectTweenId.HasValue)
        {
            LeanTween.cancel(_reflectTweenId.Value);
            _reflectTweenId = null;
        }

        bool hitSomethingElseThenABlob = blobHitStickyInfo.otherBlob == null;
        if (hitSomethingElseThenABlob)
        {
            _initialPos = new Vector2(transform.localPosition.x, HiddenSettings._.WallStickLimit - BlobFactory._.BlobsParent().position.y);
        }
        else
        {
            Vector2 dirFromOtherBlobToThisOne = (transform.localPosition - blobHitStickyInfo.otherBlob.transform.localPosition).normalized;

            Ray ray = new Ray(blobHitStickyInfo.otherBlob.transform.localPosition, dirFromOtherBlobToThisOne);
            Vector2 pos = ray.GetPoint(ClasicLv._.Spacing);
            _initialPos = pos;
        }

        _reflectToPos = blobHitStickyInfo.GetReflectPos((Vector2)transform.localPosition);

        _reflectTweenId = LeanTween.moveLocal(gameObject,
            _reflectToPos,
            HiddenSettings._.BlobForcePushAnimL
            ).id;
        LeanTween.descr(_reflectTweenId.Value).setEase(LeanTweenType.easeOutExpo);
        LeanTween.descr(_reflectTweenId.Value).setOnComplete(() =>
        {
            ElasticBack();
        });
    }

    internal void PlayAnim(Anim anim, bool hitSticky = false)
    {
        _afterAnimCallback = anim.AfterAnim;

        switch (anim.BlobAnim)
        {
            case BlobAnim.Stretch:

                Sprite.gameObject.SetActive(true);
                Sprite.transform.eulerAngles = anim.RotE;
                if (TravelSprite != null)
                {
                    TravelSprite.gameObject.SetActive(false);
                }

                BlobStretchAnimator.SetFloat(ANIM_PARAM.StretchMultiplier,
                        hitSticky ? HiddenSettings._.StretchSpeedSticky : HiddenSettings._.StretchSpeed);
                BlobStretchAnimator.SetBool(ANIMATE.JustPlay, anim.Play);

                if (_playEID != null)
                {
                    StopCoroutine(_playEID);
                    _playEID = null;
                }
                _playEID = PlayStretchAnim(anim.Time, () =>
                {
                    BlobStretchAnimator.SetBool(ANIMATE.JustPlay, !anim.Play);
                    if (_afterAnimCallback != null)
                    {
                        _afterAnimCallback();
                    }
                });
                StartCoroutine(_playEID);

                break;
            case BlobAnim.Travel:

                Sprite.gameObject.SetActive(false);
                TravelSprite.gameObject.SetActive(true);
                TravelPivot.localEulerAngles = anim.RotE;

                TravelAnimator.SetBool(ANIMATE.StartTravel, anim.Play);

                TravelAnimator.SetFloat(ANIM_PARAM.StartTravelMultiplier, HiddenSettings._.StartTravelSpeed);
                TravelAnimator.SetFloat(ANIM_PARAM.TravelMultiplier, HiddenSettings._.TravelSpeed);

                break;
            case BlobAnim.Idle:
            default:

                Sprite.gameObject.SetActive(true);
                TravelSprite.gameObject.SetActive(false);
                break;
        }
    }

    public void StretchOnCollision(Vector2 normalDir, AfterAnim afterAnimation = null, bool hitSticky = false)
    {
        Anim anim = new Anim(BlobAnim.Stretch, afterAnimation);
        anim.Play = true;
        float stretchTime = StretchClip.length
            / (hitSticky ? HiddenSettings._.StretchSpeedSticky : HiddenSettings._.StretchSpeed);
        anim.Time = stretchTime;
        var pos = new Vector2(transform.position.x, transform.position.y);
        anim.RotE = world2d.LookRotation2D(pos, pos + normalDir, fromFront: true);
        PlayAnim(anim, hitSticky);
    }

    IEnumerator PlayStretchAnim(float time, AfterAnim afterAnim)
    {
        yield return new WaitForSeconds(time);

        afterAnim();
        StopCoroutine(_playEID);
        _playEID = null;
    }

    public void StopStrechAnim()
    {
        if (_playEID != null)
        {
            StopCoroutine(_playEID);
            _playEID = null;
        }
        BlobStretchAnimator.SetBool(ANIMATE.JustPlay, false);
    }

    public void AnimateShockwave(List<Blob> proximityBlobs)
    {
        foreach (Blob proxiBlob in proximityBlobs)
        {
            float distance = Vector2.Distance(
                        new Vector2(transform.position.x, transform.position.y),
                        new Vector2(proxiBlob.transform.position.x, proxiBlob.transform.position.y)
                    );
            Vector2 dir = ((proxiBlob.transform.position - transform.position).normalized * ((1 - distance) + 0.1f)) * 0.2f;

            if (ClasicLv._.DificultyService.BrownIsWall
                && proxiBlob.BlobReveries.BlobColor == BlobColor.BROWN)
            {
                dir = dir * 0.2f;
            }

            proxiBlob.BlobReveries.ForcePush(dir);
        }
    }

    public void ForcePush(Vector2 dir)
    {
        if (_forcePushTweenId.HasValue)
        {
            LeanTween.cancel(_forcePushTweenId.Value);
            _forcePushTweenId = null;
        }
        _initialPos = transform.localPosition;
        _forcePushTweenId = LeanTween.moveLocal(gameObject, _initialPos + dir, HiddenSettings._.BlobForcePushAnimL).id;
        LeanTween.descr(_forcePushTweenId.Value).setEase(LeanTweenType.easeOutExpo);
        LeanTween.descr(_forcePushTweenId.Value).setOnComplete(() =>
        {
            ElasticBack();
        });
    }

    public void ElasticBack(bool worldMove = false)
    {
        if (_forcePushTweenId.HasValue)
        {
            LeanTween.cancel(_forcePushTweenId.Value);
            _forcePushTweenId = null;
        }

        _forcePushTweenId = LeanTween.moveLocal(gameObject, _initialPos, HiddenSettings._.BlobElasticBackAnimL).id;
        LeanTweenType ease = (LeanTweenType)percent.GetRandomFromArray<int>(GameConstants.Eases);
        LeanTween.descr(_forcePushTweenId.Value).setEase(ease);
    }

    internal void RemoveStickingGlue(int id)
    {
        int index = StickingGlues.FindIndex(sG => sG.StickedTo == id);
        if (index >= 0)
        {
            StickingGlues[index].Unstick();
        }
    }

    public void PlayHitEffect(Vector2 point)
    {
        if (_blobHitSound == null)
        {
            _blobHitSound = new MusicOpts("BlobHit", 0.5f, false);
        }
        MusicManager._.PlaySFX(_blobHitSound);
        ParticleController pc = EffectsPool._.GetParticle(ParticleType.SmallHit);
        pc.Play(point);
    }

    public float PlayExplodeEffect(Transform blobT)
    {
        PC_BigExplosion bpc = (EffectsPool._.GetParticle(ParticleType.BigExplosion) as PC_BigExplosion);

        // StartCoroutine(playExplode(bpc, blobT));

        PlayPreExplosionSFX();
        bpc.SetColor(BlobColor);
        bpc.CopyPosition(blobT);
        bpc.Play(blobT.position);

        return bpc.PreExplosionLengh;
    }

    public void PlayPreExplosionSFX()
    {
        if (_preBlobExplodeSound == null)
        {
            _preBlobExplodeSound = new MusicOpts("Pre_Explosion", 0.1f, false);
        }
        MusicManager._.PlaySFX(_preBlobExplodeSound);
    }

    public void PlayExplosionSFX()
    {
        // EditorApplication.isPaused = true;
        if (_blobExplodeSound == null)
        {
            _blobExplodeSound = new MusicOpts("Explosion", 0.8f, false);
        }
        MusicManager._.PlaySFX(_blobExplodeSound);
    }
}
