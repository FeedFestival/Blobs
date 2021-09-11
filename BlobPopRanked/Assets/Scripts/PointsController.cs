using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class PointsController : MonoBehaviour
{
    public Transform HolderT;
    List<IPointPool> _pointTexts;
    int _startPointsLength = 2;
    public float Points;
    int? _pointsEnlargeTweenId;
    public float PointsEnlargeDurationS = 0.3f;
    Vector2 _normalSize;
    Vector2 _bigSize;
    int? _pointsColorizeTweenId;
    float _time;
    float _splitAdd;
    float _addPoint;
    IEnumerator __setTimedPoints;

    void Start()
    {
        _normalSize = new Vector2(
            HiddenSettings._.ActualScreenSize.x * 0.3083f,
            HiddenSettings._.ActualScreenSize.y * 0.0265f
        );
        _bigSize = new Vector2(
            HiddenSettings._.ActualScreenSize.x * 0.3083f,
            HiddenSettings._.ActualScreenSize.y * 0.0446f
        );
    }

    public void GeneratePoints()
    {
        if (_pointTexts == null)
        {
            _pointTexts = new List<IPointPool>();
        }
        for (var i = 0; i < _startPointsLength; i++)
        {
            GenerateNewPoint(i);
        }
    }

    public void ShowPoints(BlobColor blobColor, BlobPointInfo blobPointInfo)
    {
        var pointText = GetAvailablePointText();
        (pointText as PointsParticles).ChangeValue(blobPointInfo.Points, blobColor);
        pointText.Show();

        // TODO: get a start position for the particle
        var ballsCenterPos = blobPointInfo.BlobsPositions[0];
        Debug.Log("ballsCenterPos: " + ballsCenterPos);

        var blobHitStickyInfo = Game._.Player.GetBlobHitStickyInfo();
        (pointText as PointsParticles).ShowOnScreen(ballsCenterPos, blobHitStickyInfo);
    }

    public void UpdatePoints(int toAdd, BlobColor blobColor)
    {
        SetupTweenVariables(toAdd);
        SetPoints();
        EnlargePoints();
        ColorizePoints(blobColor);
    }

    void SetupTweenVariables(int toAdd) {
        if (__setTimedPoints != null)
        {
            StopCoroutine(__setTimedPoints);
            __setTimedPoints = null;
            var lastAdditionalPoints = _splitAdd * _addPoint;

            Points += lastAdditionalPoints;
            UIController._.UiDataController.UpdateText((int)Points, UiDataType.Point);
        }
        if (toAdd > 10)
        {
            _splitAdd = 10;
            _addPoint = toAdd / _splitAdd;
        }
        else
        {
            _splitAdd = toAdd;
            _addPoint = 1;
        }
        _time = PointsEnlargeDurationS / _splitAdd;
    }

    void SetPoints()
    {
        if (_splitAdd <= 0)
        {
            FinishedSettingPoints();
            return;
        }
        if (__setTimedPoints != null)
        {
            __setTimedPoints = null;
        }
        __setTimedPoints = SetPointsCo();
        StartCoroutine(__setTimedPoints);
    }

    void FinishedSettingPoints()
    {
        StopCoroutine(__setTimedPoints);
        __setTimedPoints = null;

        EnlargePoints(false);
        ColorizePoints(BlobColor.WHITE);
    }

    IEnumerator SetPointsCo()
    {
        yield return new WaitForSeconds(_time);

        _splitAdd -= 1;
        Points += _addPoint;
        UIController._.UiDataController.UpdateText((int)Points, UiDataType.Point);

        SetPoints();
    }

    void EnlargePoints(bool enlarge = true)
    {
        if (_pointsEnlargeTweenId.HasValue)
        {
            LeanTween.cancel(_pointsEnlargeTweenId.Value);
        }
        var pointsRt = UIController._.UiDataController.PointsText.GetComponent<RectTransform>();
        _pointsEnlargeTweenId = LeanTween.size(pointsRt,
            enlarge == true ? _bigSize : _normalSize,
            PointsEnlargeDurationS
            ).id;
        LeanTween.descr(_pointsEnlargeTweenId.Value).setEase(LeanTweenType.linear);
    }

    private void ColorizePoints(BlobColor blobColor)
    {
        if (_pointsColorizeTweenId.HasValue)
        {
            LeanTween.cancel(_pointsColorizeTweenId.Value);
        }
        Color color = ClasicLv._.ClasicColorManager.GetColorByBlobColor(blobColor);
        var pointsRt = UIController._.UiDataController.PointsText.GetComponent<RectTransform>();
        _pointsColorizeTweenId = LeanTween.colorText(pointsRt,
            color,
            PointsEnlargeDurationS
            ).id;
        LeanTween.descr(_pointsColorizeTweenId.Value).setEase(LeanTweenType.linear);
    }

    IPointPool GetAvailablePointText()
    {
        var pointText = _pointTexts.Find(pT => pT.IsUsed == false);
        if (pointText == null)
        {
            pointText = GenerateNewPoint();
        }
        return pointText;
    }

    IPointPool GenerateNewPoint(int? index = null)
    {
        var pointText = GetNewText(index);
        pointText.Show(false);
        _pointTexts.Add(pointText);
        return pointText;
    }

    IPointPool GetNewText(int? index)
    {
        if (index.HasValue == false)
        {
            index = _pointTexts.Count + 1;
        }
        var go = HiddenSettings._.GetAnInstantiated(PrefabBank._.PointText);
        go.transform.SetParent(HolderT);
        var rect = go.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        var textComponent = go.GetComponent<Text>();
        var pointParticle = go.GetComponent<PointsParticles>() as IPointPool;
        pointParticle.Init(index.Value, textComponent);
        return pointParticle;
    }
}
