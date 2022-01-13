using System.Collections;
using System.Collections.Generic;
using Assets.HeadStart.Core.Player;
using Assets.HeadStart.CoreUi;
using Assets.Scripts;
using Assets.Scripts.utils;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Assets.ScreenPoints
{
    public class ScreenPointsSubject : CoreUiObservedValue
    {
        public List<ScreenPointBlob> ScreenPointsBlobs;
        public int TotalPoints;
        public Vector3 PointsWorldPosition;
        public void Set(List<ScreenPointBlob> screenPointsBlobs)
        {
            ScreenPointsBlobs = screenPointsBlobs;
        }
        public void SetTotalPoints(int points)
        {
            TotalPoints = points;
        }
        public void SetPointsWorldPosition(Vector3 pointsWorldPosition)
        {
            PointsWorldPosition = pointsWorldPosition;
        }
        public void Clear()
        {
            ScreenPointsBlobs = null;
        }
    }

    public class ScreenPoints : MonoBehaviour, IUiDependency
    {
        public Transform _holderT;
        public Text PointsText;
        private RectTransform _pointsRt;
        private Vector2 _totalPointsPos;
        List<IPoolObject> _pointTexts;
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
        private Vector2Int _actualScreenSize;

        void Awake()
        {
            __ui.SetAvailable(UiDependency.ScreenPoints, this);
        }

        public void Register(CoreUiObservedValue observed)
        {
            (observed as ScreenPointsSubject)
                .ObserveEveryValueChanged(x => x.ScreenPointsBlobs)
                .Subscribe(screenPointsBlobs =>
                {
                    if (screenPointsBlobs == null) { return; }
                    foreach (var spb in screenPointsBlobs)
                    {
                        ShowPoints(spb);
                    }
                });

            (observed as ScreenPointsSubject)
                .ObserveEveryValueChanged(x => x.TotalPoints)
                .Subscribe(UpdatePoints);
            (observed as ScreenPointsSubject)
                .ObserveEveryValueChanged(x => x.PointsWorldPosition)
                .Subscribe(pointsWorldPosition =>
                {
                    Init(pointsWorldPosition);
                });
        }

        private void UpdatePoints(int totalPoints)
        {
            PointsText.text = totalPoints.ToString();
        }

        void Init(Vector3 pointsTPos)
        {
            // TODO: re think how this get's initialized
            var canvasSize = Camera.main.GetComponent<CoreCamera>().Canvas.sizeDelta;
            _actualScreenSize = new Vector2Int(
                (int)canvasSize.x,
                (int)canvasSize.y
            );

            _normalSize = new Vector2(
                _actualScreenSize.x * 0.3083f,
                _actualScreenSize.y * 0.0265f
            );
            _bigSize = new Vector2(
                _actualScreenSize.x * 0.3083f,
                _actualScreenSize.y * 0.0446f
            );

            _pointsRt = PointsText.GetComponent<RectTransform>();
            _pointsRt.sizeDelta = _normalSize;
            _totalPointsPos = __world2d.GetWorldObjScreenPos(pointsTPos, _actualScreenSize, isAtCenter: false);
            _pointsRt.anchoredPosition = _totalPointsPos;
            _totalPointsPos = _totalPointsPos + (_normalSize / 2);

            GeneratePoints();
        }

        public void GeneratePoints()
        {
            if (_pointTexts == null)
            {
                _pointTexts = new List<IPoolObject>();
            }
            for (var i = 0; i < _startPointsLength; i++)
            {
                GenerateNewPoint(i);
            }
        }

        public void ShowPoints(ScreenPointBlob screenPointBlob)
        {
            var pointText = GetAvailablePointText() as IPointText;
            pointText.ChangeValue(screenPointBlob.Points, screenPointBlob.BlobColor);
            pointText.Show();

            Vector2 ballsCenterPos = __world2d.GetCenterPosition(screenPointBlob.BlobsPositions);
            var blobHitStickyInfo = (Main._.Game.Player as BlobPopPlayer).GetBlobHitStickyInfo();
            pointText.ShowOnScreen(ballsCenterPos, blobHitStickyInfo);
        }

        public void UpdateScreenPoints(int toAdd, BlobColor blobColor)
        {
            SetupTweenVariables(toAdd);
            SetPoints();
            EnlargePoints();
            ColorizePoints(blobColor);
        }

        void SetupTweenVariables(int toAdd)
        {
            if (__setTimedPoints != null)
            {
                StopCoroutine(__setTimedPoints);
                __setTimedPoints = null;
                var lastAdditionalPoints = _splitAdd * _addPoint;

                Points += lastAdditionalPoints;
                UpdatePoints((int)Points);
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
            UpdatePoints((int)Points);
            SetPoints();
        }

        void EnlargePoints(bool enlarge = true)
        {
            if (_pointsEnlargeTweenId.HasValue)
            {
                LeanTween.cancel(_pointsEnlargeTweenId.Value);
            }

            _pointsEnlargeTweenId = LeanTween.size(_pointsRt,
                enlarge == true ? _bigSize : _normalSize,
                PointsEnlargeDurationS
                ).id;
            LeanTween.descr(_pointsEnlargeTweenId.Value).setEase(LeanTweenType.linear);
        }

        void ColorizePoints(BlobColor blobColor)
        {
            if (_pointsColorizeTweenId.HasValue)
            {
                LeanTween.cancel(_pointsColorizeTweenId.Value);
            }
            Color color = BlobColorService.GetColorByBlobColor(blobColor);

            _pointsColorizeTweenId = LeanTween.colorText(_pointsRt,
                color,
                PointsEnlargeDurationS
                ).id;
            LeanTween.descr(_pointsColorizeTweenId.Value).setEase(LeanTweenType.linear);
        }

        IPoolObject GetAvailablePointText()
        {
            var pointText = _pointTexts.Find(pT => pT.IsUsed == false);
            if (pointText == null)
            {
                pointText = GenerateNewPoint();
            }
            return pointText;
        }

        IPoolObject GenerateNewPoint(int? index = null)
        {
            var pointText = GetNewText(index);
            pointText.Hide();
            _pointTexts.Add(pointText);
            return pointText;
        }

        IPoolObject GetNewText(int? index)
        {
            if (index.HasValue == false)
            {
                index = _pointTexts.Count + 1;
            }
            var prefab = (Main._.Game as BlobPopGame).PointText;
            var go = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            go.transform.SetParent(_holderT);
            var rect = go.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            var textComponent = go.GetComponent<Text>();
            var pointParticle = go.GetComponent<PointTextParticle>() as IPointText;
            pointParticle.Init(index.Value, textComponent, _totalPointsPos, _actualScreenSize, UpdateScreenPoints);
            return pointParticle;
        }
    }
}
