using System.Collections;
using System.Collections.Generic;
using Assets.BlobPopClassic;
using UnityEngine;

public class EndGame : MonoBehaviour
{

    void OnCollisionEnter2D(Collision2D col)
    {
        if (Main._.Game == null)
        {
            return;
        }

        if (Main._.Game.IsGamePaused())
        {
            return;
        }
        if (col.transform.tag == TAG.Blob)
        {
            (Main._.Game as BlobPopGame).OnGameOver();
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (Main._.Game == null || Main._.Game.IsGamePaused())
        {
            return;
        }
        if (col.transform.tag == TAG.Blob)
        {
            (Main._.Game as BlobPopGame).OnGameOver();
        }
    }
}
