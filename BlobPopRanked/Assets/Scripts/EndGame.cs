using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGame : MonoBehaviour
{
    public bool GameEnded;
    void OnCollisionEnter2D(Collision2D col)
    {
        if (GameEnded)
        {
            return;
        }
        if (col.transform.tag == "Blob")
        {
            Game._.GameOver();
            GameEnded = true;
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (GameEnded)
        {
            return;
        }
        if (col.transform.tag == "Blob")
        {
            Game._.GameOver();
            GameEnded = true;
        }
    }
}
