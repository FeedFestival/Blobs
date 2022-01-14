using System.Collections;
using System.Collections.Generic;
using Assets.BlobPopClassic.Blobs;
using Assets.BlobPopClassic.BlobsService;
using Assets.BlobPopClassic.DataModels;
using Assets.HeadStart.Core;
using Assets.HeadStart.Core.SFX;
using Assets.HeadStart.Features.Dialog;
using Assets.Scripts.utils;
using UnityEngine;

namespace Assets.BlobPopClassic
{
    public class BlobPopGame : GameBase
    {
        private static BlobPopGame _instance;
        public static BlobPopGame _ { get { return _instance; } }
        void Awake()
        {
            _instance = this;
        }

        [Header("Game Prefabs")]    // TODO: move this
        public GameObject Blob; // TODO: move this
        public GameObject NewBlob;  // TODO: move this

        [Header("Settings")]
        public int BlobsPerRow;
        public Vector2 StartPos;
        public float Spacing;
        public List<Blob> Blobs;
        public int _increment = -1;
        public bool FirstLevel = true;
        private IEnumerator _tryDestroyNeighbors;
        [HideInInspector]
        public List<int> Affected;
        private List<int> _toDestroy;
        private List<int> _verified;
        private List<int> _checked;
        private Dictionary<int, BlobPointInfo> _blobsByColor;
        private int? _descendTweenId;
        public Collider2D LeftWall;
        public Collider2D RightWall;
        public PolygonCollider2D EndGameCollider;
        [Header("Systems")]
        public BlobPopEffects EffectsPool;
        public DificultyService DificultyService;
        public ClassicPointsController ClassicPointsController;

        //--------------------------- GAME CONSTANTS ------------------------
        //---------------------------
        [HideInInspector]
        public readonly int CEILD_ID = 0;
        public readonly float WALL_STICK_LIMIT = 4.44f;
        public readonly float NEIGHBOR_TEST_DISTANCE = 0.437f;
        public readonly float NEIGHBOR_PROXIMITY = 1.3f;
        public readonly float GAME_OVER_OFFSET_Y = 3.95f;
        public readonly int MIN_NEIGHBOR_COUNT_TO_DESTROY = 2;
        public readonly float BLOB_FORCE_PUSH_ANIM_TIME = 0.15f;
        public readonly float BLOB_ELASTIC_BACK_ANIM_TIME = 0.5f;
        private readonly float WAIT_DESCEND_BLOBS = 0.15f;

        public override void PreStartGame()
        {
            __.Transition.Do(Transition.END);
            
            EffectsPool.Init();
            EffectsPool.GenerateParticleControllers();

            DificultyService.Init();
            ClassicPointsController.Init();

            Main._.Game.StartGame();
        }

        public override void StartGame()
        {
            base.StartGame();

            CheckCoreSession();
            PlayBackgroundMusic();
            BlobFactory._.Init();
            (Main._.Game.Player as BlobPopPlayer).MakePlayableBlobs();
            GenerateBlobLevel();
            ActivateEndGame(false);
        }

        internal void OnGameOver()
        {
            PauseGame();
            DialogOptions dialogOptions = new DialogOptions()
            {
                Title = "Failed",
                Info = ""
            };
            __.Dialog.Show(dialogOptions);
        }

        void PlayBackgroundMusic()
        {
            MusicOpts mOpts = new MusicOpts("GameMusic1", 0.09f);
            mOpts.FadeInSeconds = 30f;
            __.SFX.PlayBackgroundMusic(mOpts);
        }

        private void CheckCoreSession()
        {
            bool hasCoreSession = CoreSession._ != null;
            if (hasCoreSession == false)
            {
                SessionOpts sessionOpts = new SessionOpts()
                {
                    HighScoreType = HighScoreType.RANKED,
                    User = Main._.Game.DeviceUser()
                };
                CoreIoC.IoCDependencyResolver.CreateSession(sessionOpts);
            }
        }

        public void DescendBlobs()
        {
            if (Blobs == null)
            {
                return;
            }

            var blobsParent = BlobFactory._.BlobsParent();
            var newPos = new Vector3(blobsParent.position.x, blobsParent.position.y - Spacing, blobsParent.position.z);
            if (_descendTweenId.HasValue)
            {
                LeanTween.cancel(_descendTweenId.Value);
                _descendTweenId = null;
            }
            _descendTweenId = LeanTween.move(blobsParent.gameObject,
                newPos,
                WAIT_DESCEND_BLOBS
                ).id;
            LeanTween.descr(_descendTweenId.Value).setEase(LeanTweenType.linear);
            LeanTween.descr(_descendTweenId.Value).setOnComplete(() => { _descendTweenId = null; });


            foreach (Blob blob in Blobs)
            {
                blob.Descend();
            }
        }

        public void AddAnotherBlobLevel()
        {
            CalculateDificulty();
            GenerateBlobLevel(alreadyCalculateDificulty: true);
        }

        public void GenerateBlobLevel(bool alreadyCalculateDificulty = false)
        {
            CalculateDificulty(alreadyCalculateDificulty);
            MakeBlobLevel();
            OnFinishedMakingBlobLevel();
        }

        private void CalculateDificulty(bool alreadyCalculateDificulty = false)
        {
            if (alreadyCalculateDificulty == false)
            {
                DificultyService.CalculateDificulty();
            }
        }

        private void MakeBlobLevel()
        {
            for (var i = 0; i < BlobsPerRow; i++)
            {
                MakeABlob(i);
            }
        }

        private void OnFinishedMakingBlobLevel()
        {
            if (FirstLevel)
            {
                EndFirstLevel();
                return;
            }
            if (FirstLevel == false)
            {
                DescendBlobs();
                ActivateEndGame();
            }
        }

        private void EndFirstLevel()
        {
            FirstLevel = false;
            StartPos = new Vector2(StartPos.x, StartPos.y + Spacing);
        }

        private void MakeABlob(int? i = null)
        {
            if (i.HasValue == false)
            {
                i = _increment;
            }

            Blob blob = BlobFactory._.GetAvailableBlob() as Blob;

            Vector3 pos;
            if (i == 0)
            {
                pos = new Vector3(
                    StartPos.x,
                    StartPos.y, 0);
            }
            else
            {
                pos = new Vector3(
                    Blobs[Blobs.Count - 1].Pos.x + Spacing,
                    StartPos.y, 0);
            }
            blob.transform.position = pos;
            (blob as IPoolObject).Show();

            blob.SetId();
            blob.Init();
            blob.SetPosition(pos);
            blob.StickedTo.Add(BlobPopGame._.CEILD_ID);

            blob.BlobReveries.StopStrechAnim();
            blob.BlobReveries.SetColor(DificultyService.GetColorByDificulty());
            blob.CalculateNeighbors(Blobs);

            Blobs.Add(blob);
        }

        internal Blob GetBlobById(int bId)
        {
            return Blobs[Blobs.FindIndex(b => b.Bid == bId)];
        }

        public void TryDestroyNeighbors(Blob blob)
        {
            if (blob.HasAnyNeighbors && blob.CanDestroyNeighbors)
            {
                _tryDestroyNeighbors = DestroyNeighbors(blob);
                StartCoroutine(_tryDestroyNeighbors);
            }
            else
            {
                AfterDestroy();
            }
        }

        private IEnumerator DestroyNeighbors(Blob blob)
        {
            _toDestroy = new List<int>();
            Affected = new List<int>();
            FindNeighborsToDestroy(blob);
            if (_blobsByColor == null)
            {
                _blobsByColor = new Dictionary<int, BlobPointInfo>();
            }

            yield return new WaitForEndOfFrame();

            DestroyBlobs();
            CheckAffected();

            yield return new WaitForSeconds(1.3f);

            ClassicPointsController.Calculate(ref _blobsByColor);
            AfterDestroy();
        }

        private void AfterDestroy()
        {
            DificultyService.CheckIfAddingNewRow();

            if (_tryDestroyNeighbors != null)
            {
                StopCoroutine(_tryDestroyNeighbors);
                _tryDestroyNeighbors = null;
            }
        }

        public void FindNeighborsToDestroy(Blob blob)
        {
            _toDestroy.Add(blob.Bid);
            foreach (int bId in blob.Neighbors)
            {
                if (_toDestroy.Contains(bId))
                {
                    continue;
                }
                FindNeighborsToDestroy(GetBlobById(bId));
            }
        }

        public void DestroyBlobs(bool affectedCheck = false)
        {
            if (_toDestroy == null || _toDestroy.Count == 0)
            {
                return;
            }
            _toDestroy.Reverse();

            foreach (int bId in _toDestroy)
            {
                int index = Blobs.FindIndex(b => b.Bid == bId);
                int colorIndex = (int)Blobs[index].BlobReveries.BlobColor;

                if (_blobsByColor.ContainsKey(colorIndex) == false)
                {
                    BlobPointInfo blobPointInfo = new BlobPointInfo();
                    blobPointInfo.BlobsIds = new List<int> { Blobs[index].Bid };
                    blobPointInfo.BlobsPositions = new List<Vector2> { Blobs[index].transform.position };

                    _blobsByColor.Add(colorIndex, blobPointInfo);
                }
                else
                {
                    _blobsByColor[colorIndex].BlobsIds.Add(Blobs[index].Bid);
                    _blobsByColor[colorIndex].BlobsPositions.Add(Blobs[index].transform.position);
                }

                DificultyService.ChangeColorNumbers(colorIndex, false);
                Blobs[index].Kill();
                Blobs.RemoveAt(index);
            }

            if (affectedCheck)
            {
                CheckAffected();
            }
        }

        public void CheckAffected()
        {
            foreach (int id in _toDestroy)
            {
                if (Affected.Contains(id))
                {
                    int index = Affected.IndexOf(id);
                    Affected.RemoveAt(index);
                }
            }
            if (Affected == null || Affected.Count == 0)
            {
                return;
            }

            _toDestroy = new List<int>();
            _verified = new List<int>();

            foreach (int id in Affected)
            {
                var index = Blobs.FindIndex(b => b.Bid == id);
                var blob = Blobs[index];
                if (blob.StickedTo.Count == 0)
                {
                    _toDestroy.Add(blob.Bid);
                }
                else
                {
                    _checked = new List<int>();
                    if (isTouchingCeil(blob) == false)
                    {
                        __utils.AddIfNone(blob.Bid, ref _toDestroy);
                    }
                }
            }

            Affected = new List<int>();
            DestroyBlobs(affectedCheck: true);
        }

        private bool isTouchingCeil(Blob blob)
        {
            __utils.AddIfNone(blob.Bid, ref _checked);
            if (blob.StickedTo.Contains(BlobPopGame._.CEILD_ID))
            {
                __utils.AddIfNone(blob.Bid, ref _verified);
                return true;
            }
            foreach (var blobId in blob.StickedTo)
            {
                if (_checked.Contains(blobId))
                {
                    continue;
                }
                if (_verified.Contains(blobId))
                {
                    return true;
                }
                var stickedBlob = GetBlobById(blobId);
                bool isTouching = isTouchingCeil(stickedBlob);
                if (isTouching)
                {
                    __utils.AddIfNone(blob.Bid, ref _verified);
                    return true;
                }
            }
            return false;
        }

        public bool CanFireBlob(Vector3? point = null)
        {
            if (point.HasValue)
            {
                (Main._.Game.Player as BlobPopPlayer).Target.position = point.Value;
                return true;
            }
            return false;
        }

        public void DisableWalls(DisableWallOp disableWallOp, bool disable = true)
        {
            switch (disableWallOp)
            {
                case DisableWallOp.LeftInverse:
                    LeftWall.enabled = !disable;
                    RightWall.enabled = !LeftWall.enabled;
                    break;
                case DisableWallOp.RightInverse:
                    RightWall.enabled = !disable;
                    LeftWall.enabled = !RightWall.enabled;
                    break;
                case DisableWallOp.JustLeft:
                    LeftWall.enabled = !disable;
                    break;
                case DisableWallOp.JustRight:
                    RightWall.enabled = !disable;
                    break;
                case DisableWallOp.Both:
                default:
                    LeftWall.enabled = !disable;
                    RightWall.enabled = !disable;
                    break;
            }
        }

        public void ActivateEndGame(bool activate = true)
        {
            if (activate == false)
            {
                EndGameCollider.gameObject.SetActive(activate);
                return;
            }
            __.Time.RxWait(() =>
            {
                EndGameCollider.gameObject.SetActive(activate);
            }, BLOB_FORCE_PUSH_ANIM_TIME + BLOB_ELASTIC_BACK_ANIM_TIME + 1f);
        }
    }

    public enum DisableWallOp
    {
        Both, LeftInverse, RightInverse, JustLeft, JustRight
    }
}
