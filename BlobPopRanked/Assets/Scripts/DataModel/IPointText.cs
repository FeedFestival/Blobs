using UnityEngine;
using UnityEngine.UI;

public interface IPointText : IPoolObject
{
    void Init(int id, Text text);
    void ChangeValue(int points, BlobColor blobColor);
    void ShowOnScreen(Vector3 ballsCenterPosition, BlobHitStickyInfo blobHitStickyInfo);
}