using System;
using UnityEngine;

public class BlobHitStickyInfo {

    public float blobY;
    public Blob blob;
    public Blob otherBlob;
    public Vector3 ReflectDir;

    public BlobHitStickyInfo() {
        
    }

    public BlobHitStickyInfo(float y, Blob blob)
    {
        this.blobY = y;
        this.blob = blob;
    }
}
