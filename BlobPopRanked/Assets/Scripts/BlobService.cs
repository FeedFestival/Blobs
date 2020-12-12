using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public static class BlobService
    {
        public static bool AreClose(Transform blobT, Transform otherBlobT, ref float distance, bool proximity = false)
        {
            distance = Vector2.Distance(
                        new Vector2(blobT.position.x, blobT.position.y),
                        new Vector2(otherBlobT.position.x, otherBlobT.position.y)
                    );
            if (proximity)
            {
                return distance < HiddenSettings._.NeighborProximity;
            }
            return distance < HiddenSettings._.NeighborTestDistance;
        }

        public static List<Blob> FindBlobsInProximity(List<Blob> blobs, Blob blob)
        {
            return blobs
                .Where(b =>
                {
                    float distance = 0;
                    bool inProximity = BlobService.AreClose(blob.transform, b.transform, ref distance, proximity: true);
                    if (Game._.Level<LevelRandomRanked>().debugLvl._proximity)
                    {
                        Debug.Log("blob" + blob.Id + " and proximityBlob" + b.Id + " distance: " + distance +
                            "(min: " + HiddenSettings._.NeighborProximity + ") inProximity: " + inProximity);
                    }
                    return inProximity;
                }).ToList();
        }
    }
}
