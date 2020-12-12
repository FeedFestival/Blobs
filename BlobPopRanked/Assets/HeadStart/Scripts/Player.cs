﻿using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.Scripts.utils;

public class Player : MonoBehaviour
{
    public Vector2 StartPos;
    public Transform Target;
    public Transform ReflectDir;
    public Transform CenteroidBlob;
    public Transform Centroid;
    public List<BlobFLight> BlobFlightPositions;
    public BlobProjectile FirstProjectile;
    public BlobProjectile SecondProjectile;
    public Vector2 SecondBlobPos;
    public bool BlobInMotion;
    public bool MakingBlob;
    public bool IsInPointArea;
    private int _stopAfter;
    private int _inFlightIndex;
    private bool _firstAndOnly;
    private bool _last;
    private IEnumerator _performSecondCheck;
    private IEnumerator _performLastCheck;
    private BlobFLight _lastBlobFlight;
    private int _lastBlobFlightBlobId;
    public Vector2 LastDir;
    private int _layerMask;
    private int? _flightTweenId;

    void Start()
    {
        _layerMask = utils.CreateLayerMask(aExclude: true, LayerMask.NameToLayer("BlobProjectile"), LayerMask.NameToLayer("EndGame"));
    }

    internal void MakeBlob(bool firstLevel = false)
    {
        Timer._.Debounce(() =>
        {
            if (firstLevel)
            {
                FirstProjectile = GetRandomBlob().GetComponent<BlobProjectile>();
                SecondProjectile = GetRandomBlob().GetComponent<BlobProjectile>();
            }
            else
            {
                FirstProjectile = SecondProjectile;
                SecondProjectile = GetRandomBlob().GetComponent<BlobProjectile>();
            }

            FirstProjectile.transform.position = StartPos;
            FirstProjectile.GetComponent<Blob>().SetId();
            _lastBlobFlightBlobId = FirstProjectile.GetComponent<Blob>().Id;

            SecondProjectile.transform.position = SecondBlobPos;
            MakingBlob = false;
        }, 0.2f);
    }

    public void Shoot()
    {
        if (BlobInMotion || FirstProjectile == null)
        {
            return;
        }

        if (Game._.Level<LevelRandomRanked>().debugLvl._shooting == false && Target != null)
        {
            Destroy(Target.gameObject);
            Destroy(ReflectDir.gameObject);
            Destroy(CenteroidBlob.gameObject);
            Destroy(Centroid.gameObject);
        }
        FirstProjectile.GetComponent<CircleCollider2D>().enabled = true;
        SecondProjectile.GetComponent<CircleCollider2D>().enabled = true;

        if (Game._.Level<LevelRandomRanked>().debugLvl._shooting)
        {
            Debug.Log("SHOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOT \n ___________________" + BlobFlightPositions.Count + "_______________________");
        }

        _firstAndOnly = BlobFlightPositions.Count == 1;
        _last = false;
        _inFlightIndex = -1;
        ShootAnimated();

        BlobInMotion = true;
    }

    public void ShootAnimated()
    {
        _inFlightIndex++;
        if (_inFlightIndex >= BlobFlightPositions.Count)
        {
            EndAnimatedShot();
            return;
        }

        if (_firstAndOnly == false)
        {
            _last = _inFlightIndex == BlobFlightPositions.Count - 1;
        }

        if (_inFlightIndex == 0)
        {
            BlobFlightPositions[_inFlightIndex].distanceToPrevious =
                Vector2.Distance(FirstProjectile.transform.position, BlobFlightPositions[_inFlightIndex].Pos);
        }
        else
        {
            BlobFlightPositions[_inFlightIndex].distanceToPrevious =
                Vector2.Distance(BlobFlightPositions[_inFlightIndex - 1].Pos, BlobFlightPositions[_inFlightIndex].Pos);
        }
        BlobFlightPositions[_inFlightIndex].time = 0.09f * BlobFlightPositions[_inFlightIndex].distanceToPrevious;
        PlayFlight(BlobFlightPositions[_inFlightIndex]);
    }

    private void PlayFlight(BlobFLight blobFLight)
    {
        if (_flightTweenId.HasValue)
        {
            LeanTween.cancel(_flightTweenId.Value);
            _flightTweenId = null;
        }

        if (_firstAndOnly || _last)
        {
            if (FirstProjectile == null)
            {
                Debug.Log("We probably <b>HIT SOMETHING</b> on the way.");
                EndAnimatedShot();
                return;
            }
            LastDir = (blobFLight.Pos - (Vector2)FirstProjectile.transform.position).normalized;
            DoSecondCheck(blobFLight);
        }

        _flightTweenId = LeanTween.move(FirstProjectile.gameObject, blobFLight.Pos, blobFLight.time).id;
        LeanTween.descr(_flightTweenId.Value).setEase(LeanTweenType.linear);
        LeanTween.descr(_flightTweenId.Value).setOnComplete(() =>
        {
            ShootAnimated();
        });
    }

    private void DoSecondCheck(BlobFLight blobFLight)
    {
        if (_performSecondCheck != null)
        {
            StopCoroutine(_performSecondCheck);
            _performSecondCheck = null;
        }
        _performSecondCheck = CalculateNewPossibleHit(blobFLight);
        StartCoroutine(_performSecondCheck);
    }

    private void EndAnimatedShot()
    {
        _lastBlobFlight = BlobFlightPositions[BlobFlightPositions.Count - 1];
        if (_performLastCheck != null)
        {
            StopCoroutine(_performLastCheck);
            _performLastCheck = null;
        }
        _performLastCheck = CheckIfWeActuallyHit();
        StartCoroutine(_performLastCheck);
    }

    private IEnumerator CalculateNewPossibleHit(BlobFLight blobFlight)
    {
        float cutTime = percent.Find(10, blobFlight.time);
        float timeToWait = blobFlight.time - cutTime;

        yield return new WaitForSeconds(timeToWait);

        if (FirstProjectile != null)
        {
            FirstProjectile.transform.GetComponent<CircleCollider2D>().enabled = false;
            Vector2 origin = FirstProjectile.transform.position;
            RaycastHit2D hit = FakeShootBlob(origin: origin, towards: blobFlight.Pos);
            if (hit)
            {
                if (hit.transform.tag == "Blob")
                {
                    Blob newHitBlob = hit.transform.GetComponent<Blob>();
                    if (newHitBlob.Id != blobFlight.Blob.Id)
                    {
                        Debug.Log("Need another <b>ROUTE</b>: newHitBlob[" + newHitBlob.Id + "] was found where we expected blobFlight.blob[" + blobFlight.Blob.Id + "]");
                        FakeHitBlob(hit);
                    }
                    else
                    {
                        // Debug.Log("Route kept: newHitBlob[" + newHitBlob.Id + "] is the <b>SAME</b>, we expected blobFlight.blob[" + blobFlight.Blob.Id + "]");
                    }
                }
                else if (hit.transform.tag == "StickySurface")
                {
                    // Debug.Log("<b>StickySurface</b>");
                }
                else
                {
                    Debug.Log("Need another <b>ROUTE</b>: " + hit.transform.gameObject.name + " was found where we expected blobFlight.blob[" + blobFlight.Blob.Id + "]");
                    OnHitSomething(hit, origin);
                }
            }
            FirstProjectile.transform.GetComponent<CircleCollider2D>().enabled = true;
            StopCoroutine(_performSecondCheck);
            _performSecondCheck = null;
        }
        else
        {
            Debug.Log("<b>EVERYTHING</b> went well I asume?");
        }
    }

    private IEnumerator CheckIfWeActuallyHit()
    {
        yield return new WaitForSeconds(0.1f);

        bool hitSomethingElseThenABlob = _lastBlobFlight.Blob == null;
        if (hitSomethingElseThenABlob == false)
        {
            bool areStickedToAfterAll = _lastBlobFlight.Blob.StickedTo.Contains(_lastBlobFlightBlobId);
            if (areStickedToAfterAll == false)
            {
                if (FirstProjectile != null)
                {
                    Debug.Log("We didn't <b>HIT ANYTHING</b> _lastBlobFlight.BlobId: " + _lastBlobFlightBlobId + "LastDir: " + LastDir);
                    Vector2 origin = FirstProjectile.transform.position;
                    Vector2 targetPos = (Vector2)FirstProjectile.transform.position + LastDir;
                    BlobInMotion = false;
                    FirstProjectile.GetComponent<CircleCollider2D>().enabled = false;
                    BlobFlightPositions = new List<BlobFLight>();
                    PrepShot(origin, targetPos);
                    Shoot();
                }
            }
        }
        StopCoroutine(_performLastCheck);
        _performLastCheck = null;
    }

    public void BlobHitSticky(BlobHitStickyInfo blobHitStickyInfo)
    {
        if (MakingBlob || Game._.GameOver)
        {
            return;
        }

        blobHitStickyInfo.blob.transform.SetParent(Game._.Level<LevelRandomRanked>().BlobsParentT);

        blobHitStickyInfo.blob.SetPosition(blobHitStickyInfo.blob.transform.position, createdInRow: false);

        blobHitStickyInfo.blob.BlobReveries.AnimateElasticSettle(blobHitStickyInfo);

        blobHitStickyInfo.blob.CheckSurroundings(blobHitStickyInfo.otherBlob);

        Game._.Level<LevelRandomRanked>().Blobs.Add(blobHitStickyInfo.blob);
        // ! important to try to destroy AFTER adding
        Game._.Level<LevelRandomRanked>().TryDestroyNeighbors(blobHitStickyInfo.blob);

        FirstProjectile = null;
        MakeBlob();
        MakingBlob = true;
        BlobInMotion = false;
    }

    private GameObject GetRandomBlob()
    {
        var go = HiddenSettings._.GetAnInstantiated(PrefabBank._.NewBlob);
        go.GetComponent<Blob>().BlobReveries.SetColor(
            Game._.Level<LevelRandomRanked>().DificultyService.GetColorByDificulty()
        );
        return go;
    }

    public void SetIsInPointArea(bool val)
    {
        IsInPointArea = val;
    }

    public void PointerUp(BaseEventData baseEventData)
    {
        if (Game._.Level<LevelRandomRanked>().debugLvl._noFiring)
        {
            return;
        }

        if (BlobInMotion || FirstProjectile == null)
        {
            return;
        }

        FirstProjectile.GetComponent<CircleCollider2D>().enabled = false;
        SecondProjectile.GetComponent<CircleCollider2D>().enabled = false;

        Vector3 pointerDataPos = (baseEventData as PointerEventData).position;
        Vector3 p = Camera.main.ScreenToWorldPoint(new Vector3(pointerDataPos.x, pointerDataPos.y));
        Vector2 targetPos = new Vector3(p.x, p.y, 0);

        if (Game._.Level<LevelRandomRanked>().debugLvl._shooting)
        {
            Target.position = targetPos;
        }


        _stopAfter = 0;
        Vector2 origin = FirstProjectile.transform.position;
        BlobFlightPositions = new List<BlobFLight>();
        PrepShot(origin, targetPos);
        Shoot();
    }

    private void PrepShot(Vector2 origin, Vector2 targetPos)
    {
        RaycastHit2D hit = FakeShootBlob(origin: origin, towards: targetPos);
        OnHitSomething(hit, origin);
        Game._.Level<LevelRandomRanked>().DisableWalls(DisableWallOp.Both, false);
    }

    private RaycastHit2D FakeShootBlob(Vector2 origin, Vector2 towards)
    {
        var direction = (towards - origin).normalized;
        if (Game._.Level<LevelRandomRanked>().debugLvl._shooting)
        {
            Debug.Log("direction: " + direction);
        }
        float radius = FirstProjectile.GetComponent<Blob>().GetRadius();
        return Physics2D.CircleCast(origin, radius, direction, Mathf.Infinity, _layerMask);
    }

    private void OnHitSomething(RaycastHit2D hit, Vector2 origin)
    {
        if (Game._.Level<LevelRandomRanked>().debugLvl._shooting)
        {
            Debug.Log("origin: " + origin);
        }
        _stopAfter++;
        if (_stopAfter > 100)
        {
            return;
        }

        if (hit)
        {
            if (Game._.Level<LevelRandomRanked>().debugLvl._shooting)
            {
                Debug.Log("hit.transform.gameObject.name: " + hit.transform.gameObject.name);
                Debug.Log("hit.transform.tag: " + hit.transform.tag);
            }
            if (hit.transform.tag == "ReflectSurface")
            {
                if (hit.transform.gameObject.name.Contains("RWall"))
                {
                    Game._.Level<LevelRandomRanked>().DisableWalls(DisableWallOp.RightInverse);
                }
                else
                {
                    Game._.Level<LevelRandomRanked>().DisableWalls(DisableWallOp.LeftInverse);
                }

                Vector2 newOrigin = (Vector2)hit.centroid;
                BlobFlightPositions.Add(new BlobFLight(newOrigin));
                Vector2 oldDir = ((Vector2)newOrigin - (Vector2)origin).normalized;
                Vector2 reflectDir = Vector2.Reflect(oldDir, hit.normal.normalized);
                Vector2 newTowards = (Vector2)newOrigin + reflectDir;

                if (Game._.Level<LevelRandomRanked>().debugLvl._shooting)
                {
                    Debug.Log("newOrigin: " + newOrigin);
                    if (_stopAfter == 1)
                    {
                        Centroid.position = newOrigin;
                    }
                    Debug.Log("oldDir: " + oldDir);
                    Debug.Log("reflectDir: " + reflectDir);
                    Debug.Log("newTowards: " + newTowards);
                    if (_stopAfter == 1)
                    {
                        ReflectDir.position = newTowards;
                    }
                }

                RaycastHit2D newHit = FakeShootBlob(origin: newOrigin, towards: newTowards);
                OnHitSomething(newHit, newOrigin);
            }
            else if (hit.transform.tag == "StickySurface" || hit.transform.tag == "Blob")
            {
                FakeHitBlob(hit);
            }
        }
    }

    private void FakeHitBlob(RaycastHit2D hit)
    {
        Vector2 newOrigin = (Vector2)hit.centroid;
        if (Game._.Level<LevelRandomRanked>().debugLvl._shooting)
        {
            Debug.Log("newOrigin: " + newOrigin);
            CenteroidBlob.position = newOrigin;
        }
        Blob blob = hit.transform.GetComponent<Blob>();
        BlobFlightPositions.Add(new BlobFLight(newOrigin, blob));
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            Shoot();
        }
    }
}
