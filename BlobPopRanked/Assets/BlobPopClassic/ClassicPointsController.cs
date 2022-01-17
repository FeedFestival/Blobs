using System.Collections.Generic;
using Assets.BlobPopClassic.BlobPopColor;
using Assets.BlobPopClassic.DataModels;
using Assets.CoreExtensions.ScreenData;
using Assets.HeadStart.CoreUi;
using Assets.Scripts.utils;
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
                    ScreenToiletPaper screenToiletPaper = new ScreenToiletPaper(kvp.Value);
                    if (screenToiletPaper.Points > 0)
                    {
                        ScreenDataSubject.SetToiletPaper(screenToiletPaper);
                    }
                    continue;
                }

                ScreenPointBlob screenPointBlob = new ScreenPointBlob(blobColor, kvp.Value);
                screenDataBlobs.Add(screenPointBlob);
            }
            ScreenDataSubject.Set(screenDataBlobs);
            blobsByColor.Clear();
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                BlobPointInfo bpi = new BlobPointInfo()
                {
                    Points = 1,
                    BlobsIds = new List<int> { 13 },
                    BlobsPositions = new List<Vector2> { new Vector2(0.596f, 4.515f) }
                };
                ScreenToiletPaper stp = new ScreenToiletPaper(bpi);
                ScreenDataSubject.SetToiletPaper(stp);
            }
        }
    }
}
