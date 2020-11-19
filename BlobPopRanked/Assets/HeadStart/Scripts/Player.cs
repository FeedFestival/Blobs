using System;
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
    public Bounce FirstBounceBlob;
    public Bounce SecondBounceBlob;
    public Vector2 SecondBlobPos;
    public bool BlobInMotion;
    public bool MakingBlob;
    public bool IsInPointArea;
    public float? SmallestBlobY;
    private float _radius = 0.24f;
    private int _stopAfter;
    private int? _flightTweenId;
    private int _inFlightIndex;
    private bool _firstAndOnly;
    private bool _last;
    private IEnumerator _performSecondCheck;
    private IEnumerator _performLastCheck;
    private BlobFLight _lastBlobFlight;
    private int _lastBlobFlightBlobId;
    private Vector2 _lastDir;

    internal void MakeBlob(bool firstLevel = false)
    {
        Timer._.Debounce(() =>
        {
            if (firstLevel)
            {
                FirstBounceBlob = GetRandomBlob().GetComponent<Bounce>();
                SecondBounceBlob = GetRandomBlob().GetComponent<Bounce>();
            }
            else
            {
                FirstBounceBlob = SecondBounceBlob;
                SecondBounceBlob = GetRandomBlob().GetComponent<Bounce>();
            }

            FirstBounceBlob.transform.position = StartPos;
            FirstBounceBlob.GetComponent<Blob>().SetId();
            _lastBlobFlightBlobId = FirstBounceBlob.GetComponent<Blob>().Id;

            SecondBounceBlob.transform.position = SecondBlobPos;
            MakingBlob = false;
        }, 0.2f);
    }

    public void Shoot()
    {
        if (BlobInMotion || FirstBounceBlob == null)
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
        FirstBounceBlob.GetComponent<CircleCollider2D>().enabled = true;
        SecondBounceBlob.GetComponent<CircleCollider2D>().enabled = true;

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
                Vector2.Distance(FirstBounceBlob.transform.position, BlobFlightPositions[_inFlightIndex].Pos);
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
            if (FirstBounceBlob == null)
            {
                Debug.Log("We probably <b>HIT SOMETHING</b> on the way.");
                EndAnimatedShot();
                return;
            }
            _lastDir = (blobFLight.Pos - (Vector2)FirstBounceBlob.transform.position).normalized;
            if (_performSecondCheck != null)
            {
                StopCoroutine(_performSecondCheck);
                _performSecondCheck = null;
            }
            _performSecondCheck = CalculateNewPossibleHit(blobFLight);
            StartCoroutine(_performSecondCheck);
        }

        _flightTweenId = LeanTween.move(FirstBounceBlob.gameObject, blobFLight.Pos, blobFLight.time).id;
        LeanTween.descr(_flightTweenId.Value).setEase(LeanTweenType.linear);
        LeanTween.descr(_flightTweenId.Value).setOnComplete(() =>
        {
            ShootAnimated();
        });
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

        if (FirstBounceBlob != null)
        {
            FirstBounceBlob.transform.GetComponent<CircleCollider2D>().enabled = false;
            Vector2 origin = FirstBounceBlob.transform.position;
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
                        Debug.Log("Route kept: newHitBlob[" + newHitBlob.Id + "] is the <b>SAME</b>, we expected blobFlight.blob[" + blobFlight.Blob.Id + "]");
                    }
                }
                else
                {
                    Debug.Log("Need another <b>ROUTE</b>: " + hit.transform.gameObject.name + " was found where we expected blobFlight.blob[" + blobFlight.Blob.Id + "]");
                    OnHitSomething(hit, origin);
                }
            }
            FirstBounceBlob.transform.GetComponent<CircleCollider2D>().enabled = true;
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

        bool areStickedToAfterAll = _lastBlobFlight.Blob.StickedTo.Contains(_lastBlobFlightBlobId);
        if (areStickedToAfterAll == false)
        {
            if (FirstBounceBlob != null)
            {
                Debug.Log("We didn't <b>HIT ANYTHING</b> _lastBlobFlight.BlobId: " + _lastBlobFlightBlobId + "_lastDir: " + _lastDir);
                Vector2 origin = FirstBounceBlob.transform.position;
                Vector2 targetPos = (Vector2)FirstBounceBlob.transform.position + _lastDir;
                BlobInMotion = false;
                FirstBounceBlob.GetComponent<CircleCollider2D>().enabled = false;
                BlobFlightPositions = new List<BlobFLight>();
                PrepShot(origin, targetPos);
                Shoot();
            }
        }
        StopCoroutine(_performLastCheck);
        _performLastCheck = null;
    }

    public void BlobHitSticky(BlobHitStickyInfo blobHitStickyInfo)
    {
        if (IsGameOver(blobHitStickyInfo.blobY))
        {
            UIController._.DialogController.ShowDialog(true, GameplayState.Failed);
            return;
        }
        if (MakingBlob)
        {
            return;
        }

        blobHitStickyInfo.blob.transform.SetParent(Game._.Level<LevelRandomRanked>().BlobsParentT);

        blobHitStickyInfo.blob.SetPosition(blobHitStickyInfo.blob.transform.position, createdInRow: false);

        blobHitStickyInfo.blob.AnimateElasticSettle(blobHitStickyInfo);

        blobHitStickyInfo.blob.CheckSurroundings(blobHitStickyInfo.otherBlob);

        Game._.Level<LevelRandomRanked>().Blobs.Add(blobHitStickyInfo.blob);
        // ! important to try to destroy AFTER adding
        Game._.Level<LevelRandomRanked>().TryDestroyNeighbors(blobHitStickyInfo.blob);

        FirstBounceBlob = null;
        MakeBlob();
        MakingBlob = true;
        BlobInMotion = false;
    }

    private GameObject GetRandomBlob()
    {
        var go = HiddenSettings._.GetAnInstantiated(
            Game._.Level<LevelRandomRanked>().debugLvl._debugBlobs ? PrefabBank._.DebugNewBlob : PrefabBank._.NewBlob
        );
        go.GetComponent<Blob>().SetColor(
            Game._.Level<LevelRandomRanked>().DificultyService.GetColorByDificulty()
        );
        return go;
    }

    public bool IsGameOver(float blobY)
    {
        float calculatedBlobY = blobY + HiddenSettings._.GameOverOffsetY;

        if (SmallestBlobY.HasValue == false)
        {
            SmallestBlobY = 10;
        }

        if (calculatedBlobY >= SmallestBlobY)
        {
            return false;
        }

        SmallestBlobY = calculatedBlobY;
        if (Game._.Level<LevelRandomRanked>().debugLvl._gameOver)
        {
            Debug.Log(SmallestBlobY.Value);
        }
        return blobY < StartPos.y || SmallestBlobY.Value < 0;
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

        if (BlobInMotion || FirstBounceBlob == null)
        {
            return;
        }

        FirstBounceBlob.GetComponent<CircleCollider2D>().enabled = false;
        SecondBounceBlob.GetComponent<CircleCollider2D>().enabled = false;

        _stopAfter = 0;

        Vector3 pointerDataPos = (baseEventData as PointerEventData).position;
        Vector3 p = Camera.main.ScreenToWorldPoint(new Vector3(pointerDataPos.x, pointerDataPos.y));
        Vector2 targetPos = new Vector3(p.x, p.y, 0);

        if (Game._.Level<LevelRandomRanked>().debugLvl._shooting)
        {
            Target.position = targetPos;
        }

        Vector2 origin = FirstBounceBlob.transform.position;
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
        return Physics2D.CircleCast(origin, _radius, direction);
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
