using Assets.Scripts;
using Assets.Scripts.utils;
using UnityEngine;
using UnityEngine.UI;

public class PointsParticles : MonoBehaviour, IPointPool
{
    private int _id;
    private bool _isUsed;
    private Text _textComponent;
    private int _pointsValue;
    private Vector2 _viewportPosition;
    private Vector2 _worldObjectScreenPosition;
    private RectTransform _rt;
    private int? _reflectTweenId;
    private int? _sizeTweenId;
    private int? _towardsTotalTweenId;
    private BlobColor _currentBlobColor;
    public TrailRenderer Trail;

    int IPointPool.Id
    {
        get => _id;
    }
    bool IPointPool.IsUsed
    {
        get => _isUsed;
    }
    void IPointPool.Init(int id, Text text)
    {
        _id = id;
        _textComponent = text;
        _rt = this.gameObject.GetComponent<RectTransform>();
    }
    void IPointPool.Show(bool show = true) => InternalShow(show);
    public void ChangeValue(int points, BlobColor blobColor)
    {
        _pointsValue = points;
        _textComponent.text = _pointsValue.ToString();
        _currentBlobColor = blobColor;
        _textComponent.color = BlobColorService.GetColorByBlobColor(_currentBlobColor);
    }
    public void ShowOnScreen(Vector3 ballsCenterPosition, BlobHitStickyInfo blobHitStickyInfo)
    {
        world2d.ShowOnScreen(ref _rt, ballsCenterPosition, isAtCenter: false);

        var multiplier = HiddenSettings._.ActualScreenSize.x / 4;
        var reflectDir = new Vector2(blobHitStickyInfo.ReflectDir.x, -1);
        Vector2 reflectToPos = _rt.anchoredPosition + (reflectDir * multiplier);

        float timeDiff = UnityEngine.Random.Range(0.15f, 0.5f);

        AnimateShowNumber(reflectToPos, timeDiff);
        AnimateIncreaseSize(timeDiff);
    }

    private void InternalShow(bool show = true)
    {
        _isUsed = show;
        _textComponent.gameObject.SetActive(show);
        //
        Trail.gameObject.SetActive(false);
    }

    private void AnimateShowNumber(Vector2 reflectToPos, float timeDiff)
    {
        _reflectTweenId = LeanTween.move(_rt,
            reflectToPos,
            timeDiff
            ).id;
        LeanTween.descr(_reflectTweenId.Value).setEase(LeanTweenType.easeOutExpo);
        LeanTween.descr(_reflectTweenId.Value).setOnComplete(() =>
        {
            AnimateAddNumberToTotal();
            ShowPointsTrail();
        });
    }

    private void AnimateIncreaseSize(float timeDiff)
    {
        float smallestSize = HiddenSettings._.ActualScreenSize.x * 0.02f;
        float biggestSize = HiddenSettings._.ActualScreenSize.x * 0.6f;

        _sizeTweenId = LeanTween.value(gameObject,
            smallestSize,
            biggestSize,
            timeDiff
            ).id;
        LeanTween.descr(_sizeTweenId.Value).setEase(LeanTweenType.linear);
        LeanTween.descr(_sizeTweenId.Value).setOnUpdate((float value) =>
        {
            _rt.sizeDelta = new Vector2(value, value);
        });
        LeanTween.descr(_sizeTweenId.Value).setOnComplete(() =>
        {
            _sizeTweenId = null;
            AnimateDecreaseSize(timeDiff);
        });
    }

    private void AnimateDecreaseSize(float timeDiff)
    {
        float smallestSize = HiddenSettings._.ActualScreenSize.x * 0.03f;
        float biggestSize = HiddenSettings._.ActualScreenSize.x * 0.7f;

        _sizeTweenId = LeanTween.value(gameObject,
                biggestSize,
                smallestSize,
                timeDiff
                ).id;
        LeanTween.descr(_sizeTweenId.Value).setEase(LeanTweenType.easeInOutQuint);
        LeanTween.descr(_sizeTweenId.Value).setOnUpdate((float value) =>
        {
            _rt.sizeDelta = new Vector2(value, value);
        });
        LeanTween.descr(_sizeTweenId.Value).setOnComplete(() =>
        {
            _sizeTweenId = null;
        });
    }

    private void AnimateAddNumberToTotal()
    {
        float x = ((HiddenSettings._.ActualScreenSize.x) * 0.12f);
        float y = ((HiddenSettings._.ActualScreenSize.y) * 0.03f);
        Vector2 totalPointsPosition = new Vector2(x, y);

        _towardsTotalTweenId = LeanTween.move(_rt,
            totalPointsPosition,
            HiddenSettings._.TowardTotalAnimL
            ).id;
        LeanTween.descr(_towardsTotalTweenId.Value).setEase(LeanTweenType.easeInOutQuart);
        LeanTween.descr(_towardsTotalTweenId.Value).setOnComplete(() =>
        {
            UIController._.PointsController.UpdatePoints(_pointsValue, _currentBlobColor);
            InternalShow(false);
        });
    }

    void ShowPointsTrail()
    {
        Color color = BlobColorService.GetColorByBlobColor(_currentBlobColor);
        GradientColorKey colorKey = new GradientColorKey(color, 0.5f);
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(0, 0);
        alphaKeys[1] = new GradientAlphaKey(1, 1);
        var colorGradient = new Gradient();
        colorGradient.SetKeys(new GradientColorKey[1] { colorKey }, alphaKeys);

        Trail.colorGradient = colorGradient;
        Trail.gameObject.SetActive(true);
    }
}
