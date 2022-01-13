using System.Collections.Generic;
using Assets.BlobPopClassic.BlobPopColor;
using Assets.BlobPopClassic.DataModels;
using Assets.HeadStart.CoreUi;
using Assets.ScreenPoints;
using UnityEngine;

namespace Assets.BlobPopClassic
{
    public class ClassicPointsController : MonoBehaviour
    {
        public Transform PointsTextT;
        public ScreenPointsSubject ScreenPointsSubject;

        internal void Init()
        {
            ScreenPointsSubject = new ScreenPointsSubject();
            ScreenPointsSubject.SetPointsWorldPosition(PointsTextT.position);

            __ui.Register(UiDependency.ScreenPoints, ScreenPointsSubject);
        }

        public void Calculate(ref Dictionary<int, BlobPointInfo> blobsByColor)
        {
            List<ScreenPointBlob> screenPointsBlobs = new List<ScreenPointBlob>();
            foreach (KeyValuePair<int, BlobPointInfo> kvp in blobsByColor)
            {
                BlobColor blobColor = (BlobColor)kvp.Key;
                if (blobColor == BlobColor.BROWN)
                {
                    continue;
                }

                ScreenPointBlob screenPointBlob = new ScreenPointBlob(blobColor, kvp.Value);
                screenPointsBlobs.Add(screenPointBlob);
            }
            ScreenPointsSubject.Set(screenPointsBlobs);
            blobsByColor.Clear();
        }

        internal void UpdatePoints(int points)
        {
            ScreenPointsSubject.SetTotalPoints(points);
        }
    }
}
