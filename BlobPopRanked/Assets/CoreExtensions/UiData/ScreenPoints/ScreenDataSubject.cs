using System.Collections;
using System.Collections.Generic;
using Assets.BlobPopClassic.DataModels;
using Assets.HeadStart.CoreUi;
using UnityEngine;

namespace Assets.CoreExtensions.ScreenData
{
    public class ScreenDataSubject : CoreUiObservedValue
    {
        public List<ScreenPointBlob> ScreenDataBlobs;
        public ScreenToiletPaper ScreenToiletPaper;
        public Vector3 PointsWorldPosition;
        public void Set(List<ScreenPointBlob> screenDataBlobs)
        {
            ScreenDataBlobs = screenDataBlobs;
        }
        public void SetToiletPaper(ScreenToiletPaper screenToiletPaper)
        {
            ScreenToiletPaper = screenToiletPaper;
        }
        public void SetPointsWorldPosition(Vector3 pointsWorldPosition)
        {
            PointsWorldPosition = pointsWorldPosition;
        }
        public void Clear()
        {
            ScreenDataBlobs = null;
        }
    }
}
