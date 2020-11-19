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
    public int Points;
    public Transform BlobsParentT;

    private int? _descendTweenId;
    public Collider2D LeftWall;
    public Collider2D RightWall;

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
            debugLvl._debugBlobs ? PrefabBank._.DebugBlob : PrefabBank._.Blob
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
        go.transform.SetParent(BlobsParentT);
        var blob = go.GetComponent<Blob>();
        blob.SetPosition(pos, true);
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
        if (_toDestroy == null || _toDestroy.Count == 0)
        {
            return;
        }

        foreach (int id in _toDestroy)
        {
            Blob oneBlobFromToDestroy = Blobs.Find(b => b.Id == id);
            int colorIndex = (int)oneBlobFromToDestroy.BlobColor;
            DificultyService.ChangeColorNumbers(colorIndex, false);
        }

        int points = CalculatePoints(_toDestroy.Count);
        // Debug.Log("points: " + points + "");
        Points += points;
        UIController._.UiDataController.UpdateText(Points, UiDataType.Point);

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

    private int CalculatePoints(int count)
    {
        if (count <= 3)
        {
            return count;
        }
        int rest = 0;
        if (count > 3)
        {
            rest = count - 3;
        }
        return (3 + (rest * 2));
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

    public bool CanFireBlob(Vector3? point = null)
    {
        if (debugLvl._noFiring)
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

    public Color GetColorByBlobColor(BlobColor blobColor)
    {
        return ColorBank._.GetColorByName(ColorName(blobColor));
    }

    internal Color GetLinkColor(BlobColor fromColor, BlobColor toColor)
    {
        Color color;
        // Debug.Log("fromColor: " + fromColor + " toColor: " + toColor);
        if (fromColor == BlobColor.RED && toColor == BlobColor.RED)
        {
            color = ColorBank._.Pink_Dark_Night_Shadz;
        }
        else if (fromColor == BlobColor.BLUE && toColor == BlobColor.BLUE)
        {
            color = ColorBank._.Blue_San_Marino;
        }
        else if (fromColor == BlobColor.YELLOW && toColor == BlobColor.YELLOW)
        {
            color = ColorBank._.Yellow_Gold_Sand;
        }
        else if (fromColor == BlobColor.GREEN && toColor == BlobColor.GREEN)
        {
            color = ColorBank._.Green_Aqua_Forest;
        }
        else if (fromColor == BlobColor.BROWN && toColor == BlobColor.BROWN)
        {
            color = ColorBank._.Brown_Ferra;
        }

        //
        else if (fromColor == BlobColor.BLUE && toColor == BlobColor.RED
            || fromColor == BlobColor.RED && toColor == BlobColor.BLUE)
        {
            color = ColorBank._.Purple_Studio;
        }
        else if (fromColor == BlobColor.BLUE && toColor == BlobColor.YELLOW ||
            fromColor == BlobColor.YELLOW && toColor == BlobColor.BLUE)
        {
            color = ColorBank._.Green_Yellow;
        }
        else if (fromColor == BlobColor.BLUE && toColor == BlobColor.GREEN ||
            fromColor == BlobColor.GREEN && toColor == BlobColor.BLUE)
        {
            color = ColorBank._.Green_Blue_Wedgewood;
        }
        else if (fromColor == BlobColor.BLUE && toColor == BlobColor.BROWN ||
            fromColor == BlobColor.BROWN && toColor == BlobColor.BLUE)
        {
            color = ColorBank._.Purple_Salt_Box;
        }
        else if (fromColor == BlobColor.RED && toColor == BlobColor.YELLOW
            || fromColor == BlobColor.YELLOW && toColor == BlobColor.RED)
        {
            color = ColorBank._.Orange_Burnt_Sienna;
        }
        else if (fromColor == BlobColor.RED && toColor == BlobColor.GREEN
            || fromColor == BlobColor.GREEN && toColor == BlobColor.RED)
        {
            color = ColorBank._.Green_Red_Xanadu;
        }
        else if (fromColor == BlobColor.RED && toColor == BlobColor.BROWN
            || fromColor == BlobColor.BROWN && toColor == BlobColor.RED)
        {
            color = ColorBank._.Brown_Buccaneer;
        }
        else if (fromColor == BlobColor.YELLOW && toColor == BlobColor.GREEN
                    || fromColor == BlobColor.GREEN && toColor == BlobColor.YELLOW)
        {
            color = ColorBank._.Green_Wild_Willow;
        }
        else if (fromColor == BlobColor.YELLOW && toColor == BlobColor.BROWN
                    || fromColor == BlobColor.BROWN && toColor == BlobColor.YELLOW)
        {
            color = ColorBank._.Brown_Muddy_Waters;
        }
        else if (fromColor == BlobColor.GREEN && toColor == BlobColor.BROWN
                    || fromColor == BlobColor.BROWN && toColor == BlobColor.GREEN)
        {
            color = ColorBank._.Brown_Flint;
        }
        //
        else
        {
            color = HiddenSettings._.TransparentColor;
        }
        color.a = 0.75f;
        return color;
    }

    public string ColorName(BlobColor clobColor)
    {
        switch (clobColor)
        {
            case BlobColor.BLUE:
                return "Blue_Cornflower";
            case BlobColor.YELLOW:
                return "Orange_Koromiko";
            case BlobColor.GREEN:
                return "Green_Ocean";
            case BlobColor.BROWN:
                return "Brown_Ferra";
            case BlobColor.RED:
            default:
                return "Red_Torch";

        }
    }
}

public enum DisableWallOp
{
    Both, LeftInverse, RightInverse, JustLeft, JustRight
}

public enum BlobColor
{
    RED, BLUE, YELLOW, GREEN, BROWN
}
