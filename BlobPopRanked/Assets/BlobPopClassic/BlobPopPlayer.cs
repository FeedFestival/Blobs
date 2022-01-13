using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Assets.Scripts.utils;
using System.Linq;
using UniRx;
using System;
using Assets.HeadStart.Core.Player;
using Assets.HeadStart.Core;
using Assets.BlobPopClassic.Blobs;
using Assets.BlobPopClassic.DataModels;
using Assets.BlobPopClassic.BlobPopColor;

namespace Assets.BlobPopClassic
{
    public class BlobPopPlayer : Player
    {
        public SwitchSettings SwitchSettings;
        public Transform Target;
        public List<BlobFLight> BlobFlightPositions;
        public BlobProjectile FirstProjectile;
        public BlobProjectile SecondProjectile;
        public bool BlobInMotion;
        public bool MakingBlob;
        public bool IsDragging;
        public PredictionManager PredictionManager;
        private int _stopAfter;
        private int _inFlightIndex;
        private bool _firstAndOnly;
        private bool _last;
        private IEnumerator _performSecondCheck;
        private IEnumerator _performLastCheck;
        private BlobFLight _lastBlobFlight;
        public int _lastBlobFlightBlobId;
        public Vector2 LastDir;
        private int _layerMask;
        private BlobHitStickyInfo _blobHitStickyInfo;
        IObserver<int> _pointerUpObserver;
        bool _isSwitchInProgress;
        public SwitchController SwitchController;

        void Start()
        {
            _layerMask = __utils.CreateLayerMask(aExclude: true, LayerMask.NameToLayer(LAYER.BlobProjectile), LayerMask.NameToLayer(LAYER.EndGame));
            var pointerUpDebounced = Observable.Create<int>(observer =>
            {
                _pointerUpObserver = observer;
                return Disposable.Empty;
            });

            pointerUpDebounced
                .Throttle(TimeSpan.FromSeconds(0.03f))
                .Subscribe((int n) =>
                {
                    TryShooting();
                });

            __.Event.On(Evt.STOP_SHOOTING, (object obj) =>
            {
                _isSwitchInProgress = (obj as SwitchBlobEvent).IsSwitchInProgress;
            });

            SwitchController.Init(this);
        }

        public void PointerDrag(BaseEventData baseEventData)
        {
            if (_isSwitchInProgress)
            {
                return;
            }
            if (BlobInMotion || FirstProjectile == null)
            {
                IsDragging = false;
                return;
            }
            IsDragging = true;

            FirstProjectile.GetComponent<CircleCollider2D>().enabled = false;
            SecondProjectile.GetComponent<CircleCollider2D>().enabled = false;

            Vector3 pointerDataPos = (baseEventData as PointerEventData).position;
            Vector3 p = Camera.main.ScreenToWorldPoint(new Vector3(pointerDataPos.x, pointerDataPos.y));
            Vector2 targetPos = new Vector3(p.x, p.y, 0);

            _stopAfter = 0;
            Vector2 origin = FirstProjectile.transform.position;
            BlobFlightPositions = new List<BlobFLight>();
            PrepShot(origin, targetPos);

            PredictionManager.ShowPrediction(FirstProjectile.transform.position, BlobFlightPositions.Select(bf => (Vector3)bf.Pos).ToArray());
        }

        public void PointerUp(BaseEventData baseEventData)
        {
            if (IsDragging)
            {
                _pointerUpObserver.OnNext(1);
            }
        }

        void TryShooting()
        {
            if (IsDragging)
            {
                IsDragging = false;
            }
            PredictionManager.Reset();

            if (_isSwitchInProgress)
            {
                return;
            }

            if (BlobInMotion || FirstProjectile == null)
            {
                return;
            }

            __.Event.Emit(Evt.ACTIVATE_POINTER_AREA, false);

            Shoot();
        }

        public void Shoot()
        {
            if (BlobInMotion || FirstProjectile == null)
            {
                return;
            }

            FirstProjectile.GetComponent<CircleCollider2D>().enabled = true;
            SecondProjectile.GetComponent<CircleCollider2D>().enabled = true;

            _firstAndOnly = BlobFlightPositions.Count == 1;
            _last = false;
            _inFlightIndex = -1;

            ShootAnimated();

            BlobInMotion = true;
            BlobPopClassic._.DificultyService.CalculatePlayTime(start: true);
            BlobPopClassic._.ActivateEndGame(false);
        }

        public void ShootAnimated()
        {
            _inFlightIndex++;
            if (_inFlightIndex >= BlobFlightPositions.Count)
            {
                EndAnimatedShot();
                return;
            }

            if (_firstAndOnly == false)
            {
                _last = _inFlightIndex == BlobFlightPositions.Count - 1;
            }

            BlobFLight blobFLight = BlobFlightPositions[_inFlightIndex];
            if (_inFlightIndex == 0)
            {
                blobFLight.distanceToPrevious =
                    Vector2.Distance(FirstProjectile.transform.position, blobFLight.Pos);
            }
            else
            {
                blobFLight.distanceToPrevious =
                    Vector2.Distance(BlobFlightPositions[_inFlightIndex - 1].Pos, blobFLight.Pos);
            }
            blobFLight.time = 0.09f * blobFLight.distanceToPrevious;

            if (_firstAndOnly || _last)
            {
                if (FirstProjectile == null)
                {
                    Debug.Log("We probably <b>HIT SOMETHING</b> on the way.");
                    EndAnimatedShot();
                    return;
                }
                LastDir = (blobFLight.Pos - (Vector2)FirstProjectile.transform.position).normalized;
                DoSecondCheck(blobFLight);
            }

            FirstProjectile.PlayFlight(blobFLight, ShootAnimated);
        }

        private void DoSecondCheck(BlobFLight blobFLight)
        {
            if (_performSecondCheck != null)
            {
                StopCoroutine(_performSecondCheck);
                _performSecondCheck = null;
            }
            _performSecondCheck = CalculateNewPossibleHit(blobFLight);
            StartCoroutine(_performSecondCheck);
        }

        private void EndAnimatedShot()
        {
            // BUG: in some circumstances this.BlobFlightPositions is null
            _lastBlobFlight = BlobFlightPositions[BlobFlightPositions.Count - 1];
            if (_performLastCheck != null)
            {
                StopCoroutine(_performLastCheck);
                _performLastCheck = null;
            }
            _performLastCheck = CheckIfWeActuallyHit();
            StartCoroutine(_performLastCheck);
        }

        private IEnumerator CalculateNewPossibleHit(BlobFLight blobFlight)
        {
            float cutTime = __percent.Find(10, blobFlight.time);
            float timeToWait = blobFlight.time - cutTime;

            yield return new WaitForSeconds(timeToWait);

            if (FirstProjectile != null)
            {
                FirstProjectile.transform.GetComponent<CircleCollider2D>().enabled = false;
                Vector2 origin = FirstProjectile.transform.position;
                RaycastHit2D hit = FakeShootBlob(origin: origin, towards: blobFlight.Pos);
                if (hit)
                {
                    if (hit.transform.tag == TAG.Blob)
                    {
                        Blob newHitBlob = hit.transform.GetComponent<Blob>();
                        if (newHitBlob == null || blobFlight == null ||blobFlight.Blob == null)
                        {
                            // BUG: this happens when a bunch of blobs explode and at the same time
                            // - we fire another blob towards it and there is no blob behind but the top wall
                            // - then while we are in air another blob row descends
                            // - and in here we hit a new blob
                            Debug.Log("newHitBlob: " + __debug.ToJsonString(newHitBlob));
                            Debug.Log("blobFlight: " + __debug.ToJsonString(blobFlight));
                            Debug.Log("newHitBlob: " + __debug.ToJsonString(newHitBlob));
                            Debug.Log("Null Reference Exception... why? Suspect -> " + hit.transform.gameObject.name);
                        }
                        if (newHitBlob.Bid != blobFlight.Blob.Bid)
                        {
                            Debug.Log("Need another <b>ROUTE</b>: newHitBlob[" + newHitBlob.Bid + "] was found where we expected blobFlight.blob[" + blobFlight.Blob.Bid + "]");
                            FakeHitBlob(hit);
                        }
                    }
                    else if (hit.transform.tag == TAG.StickySurface)
                    {
                        // Debug.Log("<b>StickySurface</b>");
                    }
                    else
                    {
                        Debug.Log("Need another <b>ROUTE</b>: " + hit.transform.gameObject.name + " was found where we expected blobFlight.blob[" + blobFlight.Blob.Bid + "]");
                        OnHitSomething(hit, origin);
                    }
                }
                FirstProjectile.transform.GetComponent<CircleCollider2D>().enabled = true;
                StopCoroutine(_performSecondCheck);
                _performSecondCheck = null;
            }
            else
            {
                Debug.Log("<b>EVERYTHING</b> went well I asume?");
            }
        }

        private IEnumerator CheckIfWeActuallyHit()
        {
            yield return new WaitForSeconds(0.1f);

            bool hitSomethingElseThenABlob = _lastBlobFlight.Blob == null;
            // bool hitSomethingElseThenABlob = true;
            if (hitSomethingElseThenABlob == false)
            {
                bool areStickedToAfterAll = _lastBlobFlight.Blob.StickedTo.Contains(_lastBlobFlightBlobId);
                if (areStickedToAfterAll == false)
                {
                    if (FirstProjectile != null)
                    {
                        Debug.Log("We didn't <b>HIT ANYTHING</b> _lastBlobFlight.BlobId: " + _lastBlobFlightBlobId + "LastDir: " + LastDir);
                        Vector2 origin = FirstProjectile.transform.position;
                        Vector2 targetPos = (Vector2)FirstProjectile.transform.position + LastDir;

                        var isSameProjectile = _lastBlobFlightBlobId == FirstProjectile.GetComponent<Blob>().Bid;
                        Debug.Log("There is a bug here! How to catch? <b>isSameProjectile: " + isSameProjectile + "</b>");

                        FirstProjectile.GetComponent<CircleCollider2D>().enabled = false;
                        BlobFlightPositions = new List<BlobFLight>();
                        PrepShot(origin, targetPos);
                        ReShoot();
                    }
                }
            }
            StopCoroutine(_performLastCheck);
            _performLastCheck = null;
        }

        void ReShoot()
        {
            FirstProjectile.GetComponent<CircleCollider2D>().enabled = true;
            SecondProjectile.GetComponent<CircleCollider2D>().enabled = true;

            _firstAndOnly = BlobFlightPositions.Count == 1;
            _last = false;
            _inFlightIndex = -1;

            ShootAnimated();

            BlobPopClassic._.DificultyService.CalculatePlayTime(start: true);
            BlobPopClassic._.ActivateEndGame(false);
        }

        public void BlobHitSticky(BlobHitStickyInfo blobHitStickyInfo)
        {
            if (MakingBlob || Main._.Game.IsGamePaused())
            {
                return;
            }

            _blobHitStickyInfo = blobHitStickyInfo;

            BlobFactory._.AddNewBlobToPool(_blobHitStickyInfo.blob);
            _blobHitStickyInfo.blob.NewBlobBecomesBlob();
            _blobHitStickyInfo.blob.SetPosition(blobHitStickyInfo.blob.transform.position);

            _blobHitStickyInfo.blob.BlobReveries.AnimateElasticSettle(blobHitStickyInfo);

            _blobHitStickyInfo.blob.CheckSurroundings(blobHitStickyInfo.otherBlob);

            BlobPopClassic._.Blobs.Add(_blobHitStickyInfo.blob);
            // ! important to try to destroy AFTER adding
            BlobPopClassic._.TryDestroyNeighbors(_blobHitStickyInfo.blob);

            FirstProjectile = null;
            MakeSwitchableBlob();
        }

        public BlobHitStickyInfo GetBlobHitStickyInfo()
        {
            return _blobHitStickyInfo;
        }

        private void PrepShot(Vector2 origin, Vector2 targetPos)
        {
            RaycastHit2D hit = FakeShootBlob(origin: origin, towards: targetPos);
            OnHitSomething(hit, origin);
            BlobPopClassic._.DisableWalls(DisableWallOp.Both, false);
        }

        private RaycastHit2D FakeShootBlob(Vector2 origin, Vector2 towards)
        {
            var direction = (towards - origin).normalized;
            float radius = FirstProjectile.GetComponent<Blob>().GetRadius();
            return Physics2D.CircleCast(origin, radius, direction, Mathf.Infinity, _layerMask);
        }

        private void OnHitSomething(RaycastHit2D hit, Vector2 origin)
        {
            _stopAfter++;
            if (_stopAfter > 100)
            {
                return;
            }

            if (hit)
            {
                if (hit.transform.tag == "ReflectSurface")
                {
                    if (hit.transform.gameObject.name.Contains("RWall"))
                    {
                        BlobPopClassic._.DisableWalls(DisableWallOp.RightInverse);
                    }
                    else
                    {
                        BlobPopClassic._.DisableWalls(DisableWallOp.LeftInverse);
                    }

                    Vector2 newOrigin = (Vector2)hit.centroid;
                    // Debug.Log("newOrigin: " + newOrigin);
                    BlobFLight blobFLight = new BlobFLight(newOrigin);
                    blobFLight.hitPoint = hit.point;
                    blobFLight.normal = hit.normal;
                    BlobFlightPositions.Add(blobFLight);
                    Vector2 oldDir = ((Vector2)newOrigin - (Vector2)origin).normalized;
                    Vector2 reflectDir = Vector2.Reflect(oldDir, hit.normal.normalized);
                    Vector2 newTowards = (Vector2)newOrigin + reflectDir;

                    RaycastHit2D newHit = FakeShootBlob(origin: newOrigin, towards: newTowards);
                    OnHitSomething(newHit, newOrigin);
                }
                else if (hit.transform.tag == TAG.StickySurface || hit.transform.tag == TAG.Blob)
                {
                    FakeHitBlob(hit);
                }
            }
        }

        private void FakeHitBlob(RaycastHit2D hit)
        {
            Vector2 newOrigin = (Vector2)hit.centroid;
            Blob blob = hit.transform.GetComponent<Blob>();
            BlobFLight blobFLight = new BlobFLight(newOrigin, blob);
            blobFLight.hitPoint = hit.point;
            BlobFlightPositions.Add(blobFLight);
        }

        public BlobFLight GetLastBlobFlight()
        {
            return _lastBlobFlight;
        }

        internal void MakePlayableBlobs()
        {
            __.Time.RxWait(() =>
            {
                FirstProjectile = GetRandomBlob();
                FirstProjectile.transform.position = SwitchSettings.ShootableBlobPosition;
                SetupProjectileBlob(ref FirstProjectile);

                SecondProjectile = GetRandomBlob();
                SecondProjectile.transform.position = SwitchSettings.SwitchableBlobPosition;

            }, SwitchSettings.MakeShootableBlobDebounceTime);
        }

        void MakeSwitchableBlob()
        {
            MakingBlob = true;

            __.Event.Emit(Evt.MAKE_ANOTHER_BLOB);
        }

        public void SetupProjectileBlob(ref BlobProjectile blobProjectile)
        {
            Blob currentBlob = blobProjectile.GetComponent<Blob>();
            PredictionManager.ChangeColor(currentBlob.BlobReveries.BlobColor);
            if (currentBlob.Bid == 0)
            {
                currentBlob.SetId();
            }
            _lastBlobFlightBlobId = currentBlob.Bid;
        }

        public BlobProjectile GetRandomBlob()
        {
            var prefab = (Main._.Game as BlobPopGame).NewBlob;
            var go = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            go.name = "Nblob";
            BlobColor blobColor = BlobPopClassic._.DificultyService.GetColorByDificulty(newBlob: true);
            go.GetComponent<Blob>().BlobReveries.SetColor(blobColor);
            return go.GetComponent<BlobProjectile>();
        }
    }
}
