using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void AfterAnim();
public delegate void OnFlightComplete();

public class Anim
{
    public BlobAnim BlobAnim;
    public bool Play;
    public AfterAnim AfterAnim;
    public float Time;
    public Vector3 RotE;

    public Anim() { }

    public Anim(BlobAnim blobAnim, AfterAnim afterAnim)
    {
        BlobAnim = blobAnim;
        AfterAnim = afterAnim;
    }

    public Anim(BlobAnim blobAnim, float time, bool play = true)
    {
        BlobAnim = blobAnim;
        Play = play;
        Time = time;
    }
}
