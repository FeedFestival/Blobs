using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGame : MonoBehaviour
{
    
    void OnCollisionEnter2D(Collision2D col)
    {
        if (Game._.GameOver)
        {
            return;
        }
        if (col.transform.tag == "Blob")
        {
            Game._.OnGameOver();
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (Game._.GameOver)
        {
            return;
        }
        if (col.transform.tag == "Blob")
        {
            Game._.OnGameOver();
        }
    }
}
