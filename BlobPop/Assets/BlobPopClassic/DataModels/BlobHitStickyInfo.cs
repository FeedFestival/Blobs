using Assets.BlobPopClassic.Blobs;
using UnityEngine;

namespace Assets.BlobPopClassic.DataModels
{
    public class BlobHitStickyInfo
    {
        public float blobY;
        public Blob blob;
        public Blob otherBlob;
        public Vector3 ReflectDir;
        private readonly float REFLECT_DISTANCE_MODIFIER = 0.33f;

        public BlobHitStickyInfo()
        {

        }

        public BlobHitStickyInfo(float y, Blob blob)
        {
            this.blobY = y;
            this.blob = blob;
        }
        public Vector2 GetReflectPos(Vector2 localPosition)
        {
            return localPosition + ((Vector2)ReflectDir.normalized * REFLECT_DISTANCE_MODIFIER);
        }
    }

    public class BlobFLight
    {
        public float distanceToPrevious;
        public float time;
        public Vector2 Pos;
        public Blob Blob;
        public Vector2 normal;
        public Vector2 hitPoint;

        public BlobFLight() { }

        public BlobFLight(Vector2 pos, Blob blob = null)
        {
            this.Pos = pos;
            if (blob != null)
            {
                Blob = blob;
            }
        }
    }
}
