using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.LevelService;
using Assets.Scripts.utils;
using Assets.Scripts;

public class ClasicLv : MonoBehaviour, ILevel
{
    private static ClasicLv _clasicLv;
    public static ClasicLv _ { get { return _clasicLv; } }
    public ClasicLvDebug __debug__;
    //
    public int BlobsPerRow;
    public Vector2 StartPos;
    public float Spacing;
    public List<Blob> Blobs;
    public int _increment = -1;
    public bool FirstLevel = true;
    public delegate void AndRunCallback();
    private IEnumerator _tryDestroyNeighbors;
    [HideInInspector]
    public List<int> Affected;
    private List<int> _toDestroy;
    private List<int> _verified;
    private List<int> _checked;
    [HideInInspector]
    public DificultyService DificultyService;
    public Transform BlobsParentT;
    public ClasicColorManager ClasicColorManager;
    private Dictionary<int, BlobPointInfo> _blobsByColor;
    private int? _descendTweenId;
    public Collider2D LeftWall;
    public Collider2D RightWall;
    public PolygonCollider2D EndGameCollider;

    void Awake()
    {
        _clasicLv = this;
    }

    void Start()
    {
        DificultyService = GetComponent<DificultyService>();
        DificultyService.Init(this);
    }

    void ILevel.StartLevel()
    {
        MusicOpts mOpts = new MusicOpts("GameMusic1", 0.09f);
        mOpts.FadeInSeconds = 30f;
        MusicManager._.PlayBackgroundMusic(mOpts);
        RetrieveUser();
        Game._.Player.MakeBlob(firstLevel: FirstLevel);
        GenerateBlobLevel();
        ActivateEndGame(false);
    }

    private void RetrieveUser()
    {
        if (Game._.PlayingUser == null)
        {
            Game._.PlayingUser = Game._.User;
        }
        Debug.Log("Starting Level.... [" + Game._.PlayingUser.Name + "]");
    }

    public void DescendBlobs()
    {
        if (Blobs == null)
        {
            return;
        }

        var newPos = new Vector3(BlobsParentT.position.x, BlobsParentT.position.y - Spacing, BlobsParentT.position.z);
        if (_descendTweenId.HasValue)
        {
            LeanTween.cancel(_descendTweenId.Value);
            _descendTweenId = null;
        }
        _descendTweenId = LeanTween.move(BlobsParentT.gameObject,
            newPos,
            HiddenSettings._.BlobExplodeAnimationLength
            ).id;
        LeanTween.descr(_descendTweenId.Value).setEase(LeanTweenType.linear);
        LeanTween.descr(_descendTweenId.Value).setOnComplete(() => { _descendTweenId = null; });


        foreach (Blob blob in Blobs)
        {
            blob.Descend();
        }
        if (__debug__._blobKilling)
        {
            foreach (Blob blob in __debug__.DeadBlobs)
            {
                blob.Descend();
            }
        }
    }

    public void AddAnotherBlobLevel()
    {
        CalculateDificulty();

        if (__debug__._blobGen)
        {
            __debug__.WhenFinishedAddingDescend = true;
        }
        else
        {
            GenerateBlobLevel(alreadyCalculateDificulty: true);
        }
    }

    public void GenerateBlobLevel(bool alreadyCalculateDificulty = false)
    {
        if (__debug__._blobGen) { return; }

        CalculateDificulty(alreadyCalculateDificulty);
        MakeBlobLevel();
        OnFinishedMakingBlobLevel();
    }

    private void CalculateDificulty(bool alreadyCalculateDificulty = false)
    {
        if (alreadyCalculateDificulty == false)
        {
            DificultyService.CalculateDificulty();
            DificultyService.CalculateDificultySeed();
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

    public void CreateBlob_Debug()
    {
        IncrementAndRun_Debug(BlobsPerRow, () => { MakeABlob(); });
    }

    public void IncrementAndRun_Debug(int length, AndRunCallback andRun)
    {
        bool reachedMax = (length - 2) < _increment;
        if (reachedMax)
        {
            __debug__.ChangeDebugState(LevelDebugState.AddNewRow);
            return;
        }
        else
        {
            _increment++;
            andRun();
        }
    }

    public void ResetIncrement_Debug()
    {
        if (__debug__.WhenFinishedAddingDescend)
        {
            __debug__.WhenFinishedAddingDescend = false;
            OnFinishedMakingBlobLevel();
        }
        else
        {
            if (FirstLevel)
            {
                EndFirstLevel();
                return;
            }
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
        GameObject go = HiddenSettings._.GetAnInstantiated(PrefabBank._.Blob);
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
        go.transform.SetParent(BlobsParentT);
        var blob = go.GetComponent<Blob>();
        blob.SetPosition(pos, true);
        blob.BlobReveries.SetColor(DificultyService.GetColorByDificulty());
        blob.CalculateNeighbors(Blobs);
        Blobs.Add(blob);
    }

    internal Blob GetBlobById(int bId)
    {
        return Blobs[Blobs.FindIndex(b => b.Id == bId)];
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
        yield return new WaitForSeconds(HiddenSettings._.BlobKillAnimationLength);

        _toDestroy = new List<int>();
        Affected = new List<int>();
        FindNeighborsToDestroy(blob);

        if (_blobsByColor == null)
        {
            _blobsByColor = new Dictionary<int, BlobPointInfo>();
        }

        DestroyBlobs();
        CheckAffected();

        CalculatePoints();

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
        _toDestroy.Add(blob.Id);
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
            int index = Blobs.FindIndex(b => b.Id == bId);
            int colorIndex = (int)Blobs[index].BlobReveries.BlobColor;

            if (_blobsByColor.ContainsKey(colorIndex) == false)
            {
                BlobPointInfo blobPointInfo = new BlobPointInfo();
                blobPointInfo.BlobsIds = new List<int> { Blobs[index].Id };
                blobPointInfo.BlobsPositions = new List<Vector2> { Blobs[index].transform.position };

                _blobsByColor.Add(colorIndex, blobPointInfo);
            }
            else
            {
                _blobsByColor[colorIndex].BlobsIds.Add(Blobs[index].Id);
                _blobsByColor[colorIndex].BlobsPositions.Add(Blobs[index].transform.position);
            }

            DificultyService.ChangeColorNumbers(colorIndex, false);
            Blobs[index].Kill();
            if (__debug__._blobKilling)
            {
                __debug__.DeadBlobs.Add(Blobs[index]);
            }
            Blobs.RemoveAt(index);
        }

        if (__debug__._destroyProcess)
        {
            Debug.Log(__debug.DebugList(_toDestroy, "_toDestroy"));
        }

        if (affectedCheck)
        {
            CheckAffected();
        }
    }

    private void CalculatePoints()
    {
        foreach (KeyValuePair<int, BlobPointInfo> kvp in _blobsByColor)
        {
            BlobColor blobColor = (BlobColor)kvp.Key;
            if (blobColor == BlobColor.BROWN) {
                continue;
            }
            
            BlobPointInfo blobPointInfo = kvp.Value;
            blobPointInfo.CalculateColorPoints(blobColor);
            // Debug.Log(
            //     "Color: " + blobColor.ToString()
            //     + " (count: " + kvp.Value.BlobsIds.Count
            //     + ") -> points: " + blobPointInfo.Points
            // );
            UIController._.PointsController.ShowPoints(blobColor, blobPointInfo);
        }
        _blobsByColor.Clear();
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
            if (__debug__._destroyProcess)
            {
                Debug.Log("Affected not found.");
            }
            return;
        }

        _toDestroy = new List<int>();
        _verified = new List<int>();

        if (__debug__._destroyProcess)
        {
            Debug.Log(__debug.DebugList(Affected, "Affected"));
        }

        foreach (int id in Affected)
        {
            var index = Blobs.FindIndex(b => b.Id == id);
            var blob = Blobs[index];
            if (blob.StickedTo.Count == 0)
            {
                if (__debug__._destroyProcess)
                {
                    Debug.Log("blob" + blob.Id + " is isolated so it get's destroyed.");
                }
                _toDestroy.Add(blob.Id);
            }
            else
            {
                if (__debug__._destroyProcess)
                {
                    Debug.Log("blob" + blob.Id + " - checking To See If is touching ceil");
                }
                _checked = new List<int>();
                if (isTouchingCeil(blob) == false)
                {
                    __utils.AddIfNone(blob.Id, ref _toDestroy);
                }
            }
        }

        Affected = new List<int>();
        DestroyBlobs(affectedCheck: true);
    }

    private bool isTouchingCeil(Blob blob)
    {
        __utils.AddIfNone(blob.Id, ref _checked,
            debugAdd: __debug__._destroyProcess ? "------------- blob" + blob.Id + " added to " + __debug.DebugList(_checked, "_checked") : null);
        if (blob.StickedTo.Contains(HiddenSettings._.CeilId))
        {
            if (__debug__._destroyProcess)
            {
                Debug.Log("------------- blob" + blob.Id + " touches ceiling.");
            }
            __utils.AddIfNone(blob.Id, ref _verified,
                debugAdd: __debug__._destroyProcess ? "------------- blob" + blob.Id + " added to _verified." : null);
            return true;
        }
        if (__debug__._destroyProcess)
        {
            Debug.Log("------------- " + __debug.DebugList(blob.StickedTo, "blob" + blob.Id + ".StickedTo"));
        }
        foreach (var blobId in blob.StickedTo)
        {
            if (_checked.Contains(blobId))
            {
                if (__debug__._destroyProcess)
                {
                    Debug.Log("------------- " + __debug.DebugList(_checked, "_checked") +
                        " contains blob" + blobId + " so we are not checking him.");
                }
                continue;
            }
            if (_verified.Contains(blobId))
            {
                if (__debug__._destroyProcess)
                {
                    Debug.Log("------------- _verified contains blob" + blobId + " so it's touching Ceil.");
                }
                return true;
            }
            var stickedBlob = GetBlobById(blobId);
            if (__debug__._destroyProcess)
            {
                Debug.Log("------------- blob" + stickedBlob.Id + " - checking To See If is touching ceil");
            }
            bool isTouching = isTouchingCeil(stickedBlob);
            if (isTouching)
            {
                if (__debug__._destroyProcess)
                {
                    Debug.Log("------------- one of it's sticking blobs(" + stickedBlob.Id + ") touches ceil");
                }
                __utils.AddIfNone(blob.Id, ref _verified,
                    debugAdd: __debug__._destroyProcess ? "------------- blob" + blob.Id + " added to _verified." : null);
                return true;
            }
        }
        if (__debug__._destroyProcess)
        {
            Debug.Log("------------- blob" + blob.Id + " doesn't touch ceiling.");
        }
        return false;
    }

    public bool CanFireBlob(Vector3? point = null)
    {
        if (__debug__._noFiring)
        {
            return false;
        }
        if (point.HasValue)
        {
            Game._.Player.Target.position = point.Value;
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
        // Debug.Log(LeftWall.gameObject.name + " - " + LeftWall.enabled);
        // Debug.Log(RightWall.gameObject.name + " - " + RightWall.enabled);
    }

    public void ActivateEndGame(bool activate = true)
    {
        if (activate == false)
        {
            EndGameCollider.gameObject.SetActive(activate);
            return;
        }
        Timer._.InternalWait(() =>
        {
            EndGameCollider.gameObject.SetActive(activate);
        }, HiddenSettings._.BlobForcePushAnimL + HiddenSettings._.BlobElasticBackAnimL + 1f);
    }
}

public enum DisableWallOp
{
    Both, LeftInverse, RightInverse, JustLeft, JustRight
}
