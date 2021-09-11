using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class BlobPointInfo
    {
        public List<int> BlobsIds;
        public List<Vector2> BlobsPositions;
        public int Points;
        public BlobPointInfo()
        {

        }

        public void CalculateColorPoints(BlobColor blobColor)
        {
            // Refactor this so we take into account the color when adding points
            if (BlobsIds.Count <= 3)
            {
                Points = BlobsIds.Count;
            }
            int rest = 0;
            if (BlobsIds.Count > 3)
            {
                rest = BlobsIds.Count - 3;
            }
            var points = (3 + (rest * 2));
            Points = points * ClasicLv._.DificultyService.GetBlobColorPoints(blobColor);
        }
    }
}