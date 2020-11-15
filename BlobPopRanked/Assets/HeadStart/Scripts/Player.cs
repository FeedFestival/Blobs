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
    public Bounce FirstBounceBlob;
    public Bounce SecondBounceBlob;
    public Vector2 SecondBlobPos;
    public bool BlobInMotion;
    public bool MakingBlob;
    public bool IsInPointArea;
    public float? SmallestBlobY;

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
            Game._.Player.MakingBlob = false;
        }, 0.2f);
    }

    public void Shoot(Vector3 point)
    {
        if (BlobInMotion || FirstBounceBlob == null)
        {
            return;
        }

        Game._.Player.Target.position = point;
        FirstBounceBlob.Shoot();
        BlobInMotion = true;
        if (Game._.Level<LevelRandomRanked>().debugLvl._shooting)
        {
            Debug.Log("SHOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOT \n __________________________________________");
        }
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

        blobHitStickyInfo.blob.AnimateElasticSettle(blobHitStickyInfo);

        blobHitStickyInfo.blob.SetPosition(blobHitStickyInfo.blob.transform.position, createdInRow: false);

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
        var go = HiddenSettings._.GetAnInstantiated(PrefabBank._.NewBlob);
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
        PointerEventData pointerData = baseEventData as PointerEventData;
        Vector3 p = Camera.main.ScreenToWorldPoint(new Vector3(pointerData.position.x, pointerData.position.y));
        Game._.Level<LevelRandomRanked>().CastRayToWorld(new Vector3(p.x, p.y, 0));
    }
}
