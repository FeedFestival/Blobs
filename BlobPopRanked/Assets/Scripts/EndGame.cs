using Assets.BlobPopClassic;
using Assets.HeadStart.Core;
using IngameDebugConsole;
using UnityEngine;

public class EndGame : MonoBehaviour
{
    void Start()
    {
        DebugLogConsole.AddCommand(
            "end",
            "Ends the game with a random number of points and Toiler Paper",
            ConsoleEndGame
        );
    }

    void ConsoleEndGame()
    {
        CoreSession._.SessionOpts.Points = Random.Range(800, 1500);
        CoreSession._.SessionOpts.ToiletPaper = Random.Range(0, 5);
        (Main._.Game as BlobPopGame).OnGameOver();
    }

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
