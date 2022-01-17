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

        public ScreenPointBlob(BlobColor blobColor, BlobPointInfo blobPointInfo)
        {
            BlobsIds = blobPointInfo.BlobsIds;
            BlobsPositions = blobPointInfo.BlobsPositions;
            CalculateColorPoints(blobColor);
        }

        private void CalculateColorPoints(BlobColor blobColor)
        {
            BlobColor = blobColor;

            if (BlobsIds.Count <= 3)
            {
                Points = BlobsIds.Count;
            }
            int rest = 0;
            if (BlobsIds.Count > 3)
            {
                rest = BlobsIds.Count - 3;
            }
            // TODO: CALCULATE how long it took the user to make the move
            // - and try adding some extra points if he didnt use the Prediction
            // TODO: CALCULATE if user used the Prediction to get passed an obstable
            // - and give him some extra points for that
            Points = (3 + (rest * 2)) * BlobColorService.GetBlobColorPoints(blobColor);
        }
    }

    public class ScreenToiletPaper : ScreenPointBlob
    {
        public ScreenToiletPaper() { }
        public ScreenToiletPaper(BlobPointInfo blobPointInfo)
        {
            BlobsIds = blobPointInfo.BlobsIds;
            BlobColor = BlobColor.BROWN;
            BlobsPositions = blobPointInfo.BlobsPositions;
            CalculateToiletPaperPoints();
        }

        private void CalculateToiletPaperPoints()
        {
            // TODO: do the whole 10% + 5% thing
            Points = 1;
        }
    }
}