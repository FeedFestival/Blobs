using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClasicLvDebug : MonoBehaviour
{
    public ClasicLv ClasicLv;
    [SerializeField]
    public LevelDebugState LevelDebugState;
    public Text TextHelper;
    [Header("Debug Console")]
    public bool _dificulty;
    public bool _colorDistribution;
    public bool _shooting;
    public bool _stickingProcess;
    public bool _proximity;
    public bool _destroyCheck;
    public bool _destroyProcess;
    public bool _gameOver;
    [Header("Debug Settings")]
    public bool _debugBlobs;
    public bool _blobGen;
    public bool _newLevel;
    public bool _neighborsProcess;
    public bool _noFiring;
    public bool _blobKilling;
    public bool _keepScore;
    public List<Blob> DeadBlobs = new List<Blob>();
    [HideInInspector]
    public bool WhenFinishedAddingDescend;

    void Start()
    {
        ChangeDebugState(LevelDebugState.DebugSelecting);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            ChangeDebugState(LevelDebugState.DebugSelecting);
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            if (LevelDebugState == LevelDebugState.DebugSelecting)
            {
                ChangeDebugState(LevelDebugState.CreatingBlobsIncremental);
                if (_blobGen == false)
                {
                    return;
                }
                ClasicLv.GenerateBlobLevel();
            }
            else if (LevelDebugState == LevelDebugState.CreatingBlobsIncremental)
            {
                if (_blobGen == false)
                {
                    return;
                }
                ClasicLv.CreateBlob_Debug();
            }
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            if (LevelDebugState == LevelDebugState.CreatingBlobsIncremental)
            {
                if (_newLevel == false)
                {
                    return;
                }
                ChangeDebugState(LevelDebugState.AddNewRow);
            }
            else if (LevelDebugState == LevelDebugState.AddNewRow)
            {
                if (_newLevel == false)
                {
                    return;
                }
                ClasicLv._increment = -1;
                ClasicLv.AddAnotherBlobLevel();
                ChangeDebugState(LevelDebugState.CreatingBlobsIncremental);
            }
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            ChangeDebugState(LevelDebugState.DebugKeepScore);
        }
    }

    public void ChangeDebugState(LevelDebugState levelDebugState)
    {
        if (_blobGen || _newLevel || _keepScore)
        {
            LevelDebugState = levelDebugState;
            switch (LevelDebugState)
            {
                case LevelDebugState.DebugSelecting:
                    TextHelper.text = "State [ " + LevelDebugState.DebugSelecting.ToString() + " ]" +
                    "\n - Press A to Enter Add Blobs Incremental [State]" +
                    "\n - Press D to Test Keep Score and Finish the Game with random points." +
                    "\n - Press C to Cancel";
                    break;
                case LevelDebugState.CreatingBlobsIncremental:
                    TextHelper.text = "State [ " + LevelDebugState.CreatingBlobsIncremental.ToString() + " ]" +
                    (_blobGen ? "\n - Press A to add blob" : "") +
                    (_newLevel ? "\n - Press W to switch to Add New Row [State]" : "") +
                    "\n - Press C to Cancel";
                    break;
                case LevelDebugState.AddNewRow:
                    TextHelper.text = "State [ " + LevelDebugState.CreatingBlobsIncremental.ToString() + " ]" +
                    "\n - Press W to add new row" +
                    "\n - Press C to Cancel";
                    ClasicLv.ResetIncrement_Debug();
                    break;
                case LevelDebugState.DebugKeepScore:
                    Game._.OnGameOver();
                    break;
                default:
                    TextHelper.text = "";
                    break;
            }
        }
        else
        {
            TextHelper.text = "";
        }
    }
}

public enum LevelDebugState
{
    DebugSelecting,
    CreatingBlobsIncremental,
    AddNewRow,
    DebugKeepScore
}
