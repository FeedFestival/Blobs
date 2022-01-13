﻿using System.Collections;
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
                return distance < ClasicLv._.NEIGHBOR_PROXIMITY;
            }
            return distance < ClasicLv._.NEIGHBOR_TEST_DISTANCE;
        }

        public static List<Blob> FindBlobsInProximity(List<Blob> blobs, Blob blob)
        {
            return blobs
                .Where(b =>
                {
                    float distance = 0;
                    bool inProximity = BlobService.AreClose(blob.transform, b.transform, ref distance, proximity: true);
                    if (ClasicLv._.__debug__._proximity)
                    {
                        Debug.Log("blob" + blob.Bid + " and proximityBlob" + b.Bid + " distance: " + distance +
                            "(min: " + ClasicLv._.NEIGHBOR_PROXIMITY + ") inProximity: " + inProximity);
                    }
                    return inProximity;
                }).ToList();
        }

        public static Vector3[] GetLinePrediction(Vector3 start, Vector3[] positions)
        {
            int count = 1;
            count += positions.Length > 3 ? 3 : positions.Length;
            Vector3[] list = new Vector3[count];
            list[0] = new Vector3(start.x, start.y, 2);
            for (var i = 1; i < list.Length; i++)
            {
                list[i] = new Vector3(positions[i - 1].x, positions[i - 1].y, 2);
            }
            return list;
        }
    }
}
