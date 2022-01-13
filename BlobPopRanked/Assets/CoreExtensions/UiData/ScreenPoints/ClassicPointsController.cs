using System;
using System.Collections.Generic;
using Assets.HeadStart.CoreUi;
using Assets.ScreenPoints;
using Assets.Scripts;
using UnityEngine;

public class ClassicPointsController : MonoBehaviour
{
    public Transform PointsTextT;
    public ScreenPointsSubject ScreenPointsFloatingPointsSubject;
    public ScreenPointsSubject ScreenPointsTotalSubject;

    internal void Init()
    {
        ScreenPointsFloatingPointsSubject = new ScreenPointsSubject(ScreenPointsObsType.FloatingPoints);
        __ui.Register(UiDependency.ScreenPoints, ScreenPointsFloatingPointsSubject);

        ScreenPointsTotalSubject = new ScreenPointsSubject(ScreenPointsObsType.TotalText);
        __ui.Register(UiDependency.ScreenPoints, ScreenPointsTotalSubject);
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
        ScreenPointsFloatingPointsSubject.Set(screenPointsBlobs);
        blobsByColor.Clear();
    }

    internal void UpdatePoints(int points)
    {
        Debug.Log("points: " + points);
        ScreenPointsTotalSubject.SetPoints(points);
    }
}
