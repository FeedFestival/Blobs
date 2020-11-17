using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

        _inFlightIndex = 0;
        FirstBounceBlob.transform.position = BlobFlightPositions[_inFlightIndex].Pos;
        ShootAnimated();

        BlobInMotion = true;
        if (Game._.Level<LevelRandomRanked>().debugLvl._shooting)
        {
            Debug.Log("SHOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOT \n __________________________________________");
        }
    }

    public void ShootAnimated()
    {
        _inFlightIndex++;
        if (_inFlightIndex >= BlobFlightPositions.Count)
        {
            return;
        }
        BlobFlightPositions[_inFlightIndex].distanceToPrevious
            = Vector2.Distance(BlobFlightPositions[_inFlightIndex - 1].Pos, BlobFlightPositions[_inFlightIndex].Pos);
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

        _flightTweenId = LeanTween.move(FirstBounceBlob.gameObject, blobFLight.Pos, blobFLight.time).id;
        LeanTween.descr(_flightTweenId.Value).setEase(LeanTweenType.linear);
        LeanTween.descr(_flightTweenId.Value).setOnComplete(() =>
        {
            if (BlobFlightPositions.Count == 2)
            {
                Debug.Log("Straitgh Route");
            }
            else
            {
                Debug.Log("BlobFlightPositions.Count: " + BlobFlightPositions.Count);
                if (_inFlightIndex == BlobFlightPositions.Count - 2)
                {
                    Debug.Log("[Penultimum Route] _inFlightIndex: " + _inFlightIndex + " BlobFlightPositions.Count: " + BlobFlightPositions.Count);
                }
            }
            ShootAnimated();
        });
    }

    public void BlobHitSticky(BlobHitStickyInfo blobHitStickyInfo)
    {
        if (IsGameOver(blobHitStickyInfo.blobY))
        {
            UIController._.DialogController.ShowDialog(true, GameplayState.Failed);
            return;
        }
        if (MakingBlob) // ?
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
        BlobFlightPositions = new List<BlobFLight>() { new BlobFLight(origin) };

        RaycastHit2D hit = FakeShootBlob(origin: origin, towards: targetPos);
        OnHitSomething(hit, origin);

        Game._.Level<LevelRandomRanked>().DisableWalls(DisableWallOp.Both, false);

        Shoot();
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
            // else if (hit.transform.tag == "StickySurface")
            // {

            // }
            else if (hit.transform.tag == "StickySurface" || hit.transform.tag == "Blob")
            {
                Vector2 newOrigin = (Vector2)hit.centroid;
                if (Game._.Level<LevelRandomRanked>().debugLvl._shooting)
                {
                    Debug.Log("newOrigin: " + newOrigin);
                    CenteroidBlob.position = newOrigin;
                }
                BlobFlightPositions.Add(new BlobFLight(newOrigin));
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            Shoot();
        }
    }
}
