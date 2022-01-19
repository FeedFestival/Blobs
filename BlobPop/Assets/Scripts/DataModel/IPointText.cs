using Assets.BlobPopClassic.BlobPopColor;
using Assets.BlobPopClassic.DataModels;
using UnityEngine;
using UnityEngine.UI;

public interface IPointText : IPoolObject
{
    void Init(int id, Vector2 totalPointsPos, Vector2Int actualScreenSize, PointTextParticle.OnPointsUpdated onPointsUpdated);
    void ChangeValue(int points, BlobColor blobColor);
    void ShowOnScreen(Vector3 ballsCenterPosition, BlobHitStickyInfo blobHitStickyInfo);
}