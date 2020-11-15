using System;
using UnityEngine;

public class BlobHitStickyInfo
{

    public float blobY;
    public Blob blob;
    public Blob otherBlob;
    public Vector3 ReflectDir;

    public BlobHitStickyInfo()
    {

    }

    public BlobHitStickyInfo(float y, Blob blob)
    {
        this.blobY = y;
        this.blob = blob;
    }
}

public class BlobFLight
{
    public float distanceToPrevious;
    public float time;
    public Vector2 Pos;

    public BlobFLight() { }

    public BlobFLight(Vector2 pos)
    {
        this.Pos = pos;
    }
}

