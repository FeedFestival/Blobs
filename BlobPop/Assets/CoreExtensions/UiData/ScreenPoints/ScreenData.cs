using System.Collections;
using System.Collections.Generic;
using Assets.HeadStart.CoreUi;
using Assets.Scripts.utils;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Assets.BlobPopClassic;
using Assets.BlobPopClassic.DataModels;
using Assets.BlobPopClassic.BlobPopColor;
using Assets.HeadStart.Core;

namespace Assets.CoreExtensions.ScreenData
{
    public class ScreenData : MonoBehaviour, IUiDependency
    {
        public float RealPoints;
        public Transform _holderT;
        public Text PointsText;
        public GameObject PointTextPrefab;
        public GameObject PointIconPrefab;
        private RectTransform _pointsRt;
        private Vector2 _totalPointsPos;
        List<IPoolObject> _pointTexts;
        List<IPoolObject> _pointIcons;
        int? _pointsEnlargeTweenId;
        Vector2 _normalSize;
        Vector2 _bigSize;
        int? _pointsColorizeTweenId;
        float _time;
        float _splitAdd;
        float _addPoint;
        IEnumerator __setTimedPoints;
        private Vector2Int _actualScreenSize;
        //--------------------------- GAME CONSTANTS ------------------------
        //---------------------------
        private readonly int START_POINTS_LENGTH = 5;   // 3
        private readonly float POINTS_ENLARGE_DURATION_SECONDS = 0.3f;

        void Awake()
        {
            __ui.SetAvailable(UiDependency.ScreenData, this);
        }

        public void Register(CoreUiObservedValue observed)
        {
            (observed as ScreenDataSubject)
                .ObserveEveryValueChanged(x => x.ScreenDataBlobs)
                .Subscribe(screenDataBlobs =>
                {
                    if (screenDataBlobs == null) { return; }
                    foreach (var spb in screenDataBlobs)
                    {
                        ShowPoints(spb);
                    }
                });

            (observed as ScreenDataSubject)
                .ObserveEveryValueChanged(x => x.ScreenToiletPaper)
                .Subscribe(screenToiletPaper =>
                {
                    if (screenToiletPaper == null) { return; }
                    ShowPoints(screenToiletPaper);
                });
            (observed as ScreenDataSubject)
                .ObserveEveryValueChanged(x => x.PointsWorldPosition)
                .Subscribe(pointsWorldPosition =>
                {
                    Init(pointsWorldPosition);
                });
        }

        private void UpdatePoints()
        {
            PointsText.text = CoreSession._.SessionOpts.Points.ToString();
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
            if (_pointIcons == null)
            {
                _pointIcons = new List<IPoolObject>();
            }
            for (var i = 0; i < START_POINTS_LENGTH; i++)
            {
                GenerateNewPoint(i);
            }
            // one should be enough
            GenerateNewPointIcon(0);
        }

        public void ShowPoints(ScreenPointBlob screenPointBlob)
        {
            IPointText pointText;
            if (screenPointBlob.BlobColor == BlobColor.BROWN)
            {
                // TODO: we want to show the toilet paper next to a number
                pointText = GetAvailablePointIcon() as IPointText;
            }
            else
            {
                pointText = GetAvailablePointText() as IPointText;
            }
            pointText.ChangeValue(screenPointBlob.Points, screenPointBlob.BlobColor);
            pointText.Show();

            Vector2 ballsCenterPos = __world2d.GetCenterPosition(screenPointBlob.BlobsPositions);
            // TODO: ramake this -> dont use specifics or extends ScreenData
            var blobHitStickyInfo = (Main._.Game.Player as BlobPopPlayer).GetBlobHitStickyInfo();
            pointText.ShowOnScreen(ballsCenterPos, blobHitStickyInfo);
        }

        public void UpdateScreenData(int toAdd, BlobColor blobColor)
        {
            SetupTweenVariables(toAdd);
            SetPoints();
            EnlargePoints();
            ColorizePoints(blobColor);
        }

        public void UpdateToiletPaperScreenData(int toAdd, BlobColor blobColor)
        {
            Debug.Log("toAdd: " + toAdd);

            // for now 
            CoreSession._.SessionOpts.ToiletPaper += toAdd;

            // TODO - add this functionality
            // SetupTweenVariables(toAdd);
            // SetPoints();
            // EnlargePoints();
            // ColorizePoints(blobColor);
        }

        void SetupTweenVariables(int toAdd)
        {
            if (__setTimedPoints != null)
            {
                StopCoroutine(__setTimedPoints);
                __setTimedPoints = null;
                var lastAdditionalPoints = _splitAdd * _addPoint;

                RealPoints += lastAdditionalPoints;
                CoreSession._.SessionOpts.Points = Mathf.CeilToInt(RealPoints);
                UpdatePoints();
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
            _time = POINTS_ENLARGE_DURATION_SECONDS / _splitAdd;
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
            RealPoints += _addPoint;
            CoreSession._.SessionOpts.Points = Mathf.CeilToInt(RealPoints);
            UpdatePoints();
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
                POINTS_ENLARGE_DURATION_SECONDS
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
                POINTS_ENLARGE_DURATION_SECONDS
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

        IPoolObject GetAvailablePointIcon()
        {
            var pointIcon = _pointIcons.Find(pT => pT.IsUsed == false);
            if (pointIcon == null)
            {
                pointIcon = GenerateNewPointIcon();
            }
            return pointIcon;
        }

        IPoolObject GenerateNewPoint(int? index = null)
        {
            var pointText = GetNewPointText(PointTextPrefab, index);
            pointText.Hide();
            _pointTexts.Add(pointText);
            return pointText;
        }

        IPoolObject GenerateNewPointIcon(int? index = null)
        {
            var pointIcon = GetNewPointIcon(PointIconPrefab, index);
            pointIcon.Hide();
            _pointIcons.Add(pointIcon);
            return pointIcon;
        }

        IPoolObject GetNewPointText(GameObject prefab, int? index)
        {
            if (index.HasValue == false)
            {
                index = _pointTexts.Count + 1;
            }
            var go = createPointText(prefab, index);
            var pointParticle = go.GetComponent<PointTextParticle>() as IPointText;
            pointParticle.Init(index.Value, _totalPointsPos, _actualScreenSize, UpdateScreenData);
            return pointParticle;
        }

        IPoolObject GetNewPointIcon(GameObject prefab, int? index)
        {
            if (index.HasValue == false)
            {
                index = _pointTexts.Count + 1;
            }
            var go = createPointText(prefab, index);
            var pointParticle = go.GetComponent<PointIconParticle>() as IPointText;
            pointParticle.Init(index.Value, _totalPointsPos, _actualScreenSize, UpdateToiletPaperScreenData);
            return pointParticle;
        }

        private GameObject createPointText(GameObject prefab, int? index)
        {
            var go = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            go.transform.SetParent(_holderT);
            var rect = go.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            return go;
        }

        void OnDestroy()
        {
            __ui.SetUnavailable(UiDependency.ScreenData);
        }
    }
}
