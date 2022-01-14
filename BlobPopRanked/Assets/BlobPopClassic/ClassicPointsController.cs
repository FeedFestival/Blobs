using System.Collections.Generic;
using Assets.BlobPopClassic.BlobPopColor;
using Assets.BlobPopClassic.DataModels;
using Assets.CoreExtensions.ScreenData;
using Assets.HeadStart.CoreUi;
using UnityEngine;

namespace Assets.BlobPopClassic
{
    public class ClassicPointsController : MonoBehaviour
    {
        public Transform PointsTextT;
        public ScreenDataSubject ScreenDataSubject;

        internal void Init()
        {
            ScreenDataSubject = new ScreenDataSubject();
            ScreenDataSubject.SetPointsWorldPosition(PointsTextT.position);

            __ui.Register(UiDependency.ScreenData, ScreenDataSubject);
        }

        public void Calculate(ref Dictionary<int, BlobPointInfo> blobsByColor)
        {
            List<ScreenPointBlob> screenDataBlobs = new List<ScreenPointBlob>();
            foreach (KeyValuePair<int, BlobPointInfo> kvp in blobsByColor)
            {
                BlobColor blobColor = (BlobColor)kvp.Key;
                if (blobColor == BlobColor.BROWN)
                {
                    continue;
                }

                ScreenPointBlob screenPointBlob = new ScreenPointBlob(blobColor, kvp.Value);
                screenDataBlobs.Add(screenPointBlob);
            }
            ScreenDataSubject.Set(screenDataBlobs);
            blobsByColor.Clear();
        }

        internal void UpdatePoints(int points)
        {
            ScreenDataSubject.SetTotalPoints(points);
        }
    }
}
