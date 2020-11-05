using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.LevelService;
using Assets.Scripts.utils;

public class LevelRandomRanked : MonoBehaviour, ILevel
{
    public LevelRandomRankedDebug debugLvl;
    public int BlobsPerRow;
    public Vector2 StartPos;
    public float Spacing;
    public List<Blob> Blobs;
    private float distance = 4.5f;
    public int _increment = -1;
    public bool FirstLevel = true;
    public List<int> LevelIncreseThreshhold;
    public delegate void AndRunCallback();
    private IEnumerator _tryDestroyNeighbors;
    [HideInInspector]
    public List<int> Affected;
    private List<int> _toDestroy;
    private List<int> _verified;
    private List<int> _checked;
    public DificultyService DificultyService;

    void Start()
    {
        DificultyService = GetComponent<DificultyService>();
        DificultyService.Init(this);
    }

    public void StartLevel()
    {
        Debug.Log("Starting Level....");

        Game._.Player.MakeBlob(firstLevel: FirstLevel);

        if (debugLvl._blobGen == false)
        {
            GenerateBlobLevel();
        }
    }

    public void DescendBlobs()
    {
        if (Blobs == null)
        {
            return;
        }
        foreach (Blob blob in Blobs)
        {
            blob.Descend();
        }
        if (debugLvl._blobKilling)
        {
            foreach (Blob blob in debugLvl.DeadBlobs)
            {
                blob.Descend();
            }
        }
    }

    public void AddAnotherBlobLevel()
    {
        DificultyService.CalculateDificulty();
        DificultyService.CalculateDificultySeed();

        if (debugLvl._blobGen)
        {
            debugLvl.WhenFinishedAddingDescend = true;
        }
        else
        {
            GenerateBlobLevel();
        }
    }

    public void GenerateBlobLevel()
    {
        DificultyService.CalculateDificulty();
        DificultyService.CalculateDificultySeed();

        if (debugLvl._blobGen == false)
        {
            for (var i = 0; i < BlobsPerRow; i++)
            {
                MakeABlob(i);
            }
            OnFinishedMakingBlobLevel();
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
        }
        // Debug.Log("-----------------------------------------------------");
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
            debugLvl.ChangeDebugState(LevelDebugState.AddNewRow);
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
        if (debugLvl.WhenFinishedAddingDescend)
        {
            debugLvl.WhenFinishedAddingDescend = false;
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
        GameObject go = HiddenSettings._.GetAnInstantiated(
            debugLvl._neighborsProcess ? PrefabBank._.DebugBlob : PrefabBank._.Blob
        );
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
        var blob = go.GetComponent<Blob>();
        blob.SetPosition(pos);
        blob.SetColor(DificultyService.GetColorByDificulty());
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
        DificultyService.CheckIfAddingNewRow();
    }

    private IEnumerator DestroyNeighbors(Blob blob)
    {
        yield return new WaitForSeconds(HiddenSettings._.BlobKillAnimationLength);

        _toDestroy = new List<int>();
        Affected = new List<int>();
        FindNeighborsToDestroy(blob);

        DestroyBlobs();
        CheckAffected();

        StopCoroutine(_tryDestroyNeighbors);
        _tryDestroyNeighbors = null;
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
        _toDestroy.Reverse();
        foreach (int bId in _toDestroy)
        {
            int index = Blobs.FindIndex(b => b.Id == bId);
            Blobs[index].Kill();
            if (debugLvl._blobKilling)
            {
                debugLvl.DeadBlobs.Add(Blobs[index]);
            }
            Blobs.RemoveAt(index);
        }

        if (debugLvl._destroyProcess)
        {
            Debug.Log(utils.DebugList(_toDestroy, "_toDestroy"));
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
            if (debugLvl._destroyProcess)
            {
                Debug.Log("Affected not found.");
            }
            return;
        }

        _toDestroy = new List<int>();
        _verified = new List<int>();

        if (debugLvl._destroyProcess)
        {
            Debug.Log(utils.DebugList(Affected, "Affected"));
        }

        foreach (int id in Affected)
        {
            var index = Blobs.FindIndex(b => b.Id == id);
            var blob = Blobs[index];
            if (blob.StickedTo.Count == 0)
            {
                if (debugLvl._destroyProcess)
                {
                    Debug.Log("blob" + blob.Id + " is isolated so it get's destroyed.");
                }
                _toDestroy.Add(blob.Id);
            }
            else
            {
                if (debugLvl._destroyProcess)
                {
                    Debug.Log("blob" + blob.Id + " - checking To See If is touching ceil");
                }
                _checked = new List<int>();
                if (isTouchingCeil(blob) == false)
                {
                    utils.AddIfNone(blob.Id, ref _toDestroy);
                }
            }
        }

        Affected = new List<int>();
        DestroyBlobs(affectedCheck: true);
    }

    private bool isTouchingCeil(Blob blob)
    {
        utils.AddIfNone(blob.Id, ref _checked,
            debugAdd: debugLvl._destroyProcess ? "------------- blob" + blob.Id + " added to " + utils.DebugList(_checked, "_checked") : null);
        if (blob.StickedTo.Contains(HiddenSettings._.CeilId))
        {
            if (debugLvl._destroyProcess)
            {
                Debug.Log("------------- blob" + blob.Id + " touches ceiling.");
            }
            utils.AddIfNone(blob.Id, ref _verified,
                debugAdd: debugLvl._destroyProcess ? "------------- blob" + blob.Id + " added to _verified." : null);
            return true;
        }
        if (debugLvl._destroyProcess)
        {
            Debug.Log("------------- " + utils.DebugList(blob.StickedTo, "blob" + blob.Id + ".StickedTo"));
        }
        foreach (var blobId in blob.StickedTo)
        {
            if (_checked.Contains(blobId))
            {
                if (debugLvl._destroyProcess)
                {
                    Debug.Log("------------- " + utils.DebugList(_checked, "_checked") +
                        " contains blob" + blobId + " so we are not checking him.");
                }
                continue;
            }
            if (_verified.Contains(blobId))
            {
                if (debugLvl._destroyProcess)
                {
                    Debug.Log("------------- _verified contains blob" + blobId + " so it's touching Ceil.");
                }
                return true;
            }
            var stickedBlob = GetBlobById(blobId);
            if (debugLvl._destroyProcess)
            {
                Debug.Log("------------- blob" + stickedBlob.Id + " - checking To See If is touching ceil");
            }
            bool isTouching = isTouchingCeil(stickedBlob);
            if (isTouching)
            {
                if (debugLvl._destroyProcess)
                {
                    Debug.Log("------------- one of it's sticking blobs(" + stickedBlob.Id + ") touches ceil");
                }
                utils.AddIfNone(blob.Id, ref _verified,
                    debugAdd: debugLvl._destroyProcess ? "------------- blob" + blob.Id + " added to _verified." : null);
                return true;
            }
        }
        if (debugLvl._destroyProcess)
        {
            Debug.Log("------------- blob" + blob.Id + " doesn't touch ceiling.");
        }
        return false;
    }

    public void CastRayToWorld()
    {
        if (debugLvl._noFiring)
        {
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 point = ray.origin + (ray.direction * distance);
        Game._.Player.Target.position = point;
        Game._.Player.Shoot(point);
    }
}

public enum BlobColor
{
    PomegranateColor,   // RED
    AtlantisColor,       // GREEN
    RoyalBlue,           // BLUE
    Candlelight,        // YELLOW
    MediumPurple        // VIOLET
}