using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 StartPos;
    public Transform Target;
    public Bounce FirstBounceBlob;
    public Bounce SecondBounceBlob;
    public Vector2 SecondBlobPos;
    public bool BlobInMotion;
    public bool MakingBlob;

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

    public void BlobHitSticky(float blobY, Blob blob, Blob otherBlob = null)
    {
        if (IsGameOver(blobY))
        {
            return;
        }
        if (MakingBlob) // ?
        {
            return;
        }

        blob.SetPosition(blob.transform.position, createdInRow: false);

        blob.CheckSurroundings(otherBlob);

        Game._.Level<LevelRandomRanked>().Blobs.Add(blob);
        // ! important to try to destroy AFTER adding
        Game._.Level<LevelRandomRanked>().TryDestroyNeighbors(blob);

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

    private bool IsGameOver(float blobY)
    {
        if (Game._.Level<LevelRandomRanked>().debugLvl._gameOver)
        {
            Debug.Log(blobY);
        }
        return blobY < StartPos.y || blobY < -3.95f;
    }
}
