using System.Collections;
using System.Collections.Generic;
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
    }
}
