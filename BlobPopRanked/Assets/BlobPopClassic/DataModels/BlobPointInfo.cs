using System.Collections.Generic;
using Assets.BlobPopClassic.BlobPopColor;
using UnityEngine;

namespace Assets.BlobPopClassic.DataModels
{
    public class BlobPointInfo
    {
        public List<int> BlobsIds;
        public List<Vector2> BlobsPositions;
        public int Points;
        public BlobPointInfo() { }
    }

    public class ScreenPointBlob : BlobPointInfo
    {
        public BlobColor BlobColor;

        public ScreenPointBlob() { }

        public ScreenPointBlob(BlobPointInfo blobPointInfo)
        {
            BlobsIds = blobPointInfo.BlobsIds;
            BlobsPositions = blobPointInfo.BlobsPositions;
            Points =blobPointInfo.Points;
        }
        public ScreenPointBlob(BlobColor blobColor, BlobPointInfo blobPointInfo)
        {
            BlobsIds = blobPointInfo.BlobsIds;
            BlobsPositions = blobPointInfo.BlobsPositions;
            Points =blobPointInfo.Points;
            CalculateColorPoints(blobColor);
        }

        public void CalculateColorPoints(BlobColor blobColor)
        {
            BlobColor = blobColor;

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
            Points = points * BlobColorService.GetBlobColorPoints(blobColor);
        }
    }
}