using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelRandomRankedDebug : MonoBehaviour
{
    public LevelRandomRanked LevelRandomRanked;
    [SerializeField]
    public LevelDebugState LevelDebugState;
    public Text TextHelper;
    [Header("Debug Console")]
    public bool DebugDificulty;
    public bool DebugStickingTo;
    public bool DebugRemoveSticky;
    public bool DebugProximity;
    public bool DebugGameOver;
    [Header("Debug Settings")]
    public bool DebugBlobGeneration;
    public bool DebugNewLevel;
    public bool NeighborDebug;
    public bool NoFiring;
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
                if (DebugBlobGeneration == false)
                {
                    return;
                }
                LevelRandomRanked.GenerateBlobLevel();
            }
            else if (LevelDebugState == LevelDebugState.CreatingBlobsIncremental)
            {
                if (DebugBlobGeneration == false)
                {
                    return;
                }
                LevelRandomRanked.CreateBlob_Debug();
            }
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            if (LevelDebugState == LevelDebugState.CreatingBlobsIncremental)
            {
                if (DebugNewLevel == false)
                {
                    return;
                }
                ChangeDebugState(LevelDebugState.AddNewRow);
            }
            else if (LevelDebugState == LevelDebugState.AddNewRow)
            {
                if (DebugNewLevel == false)
                {
                    return;
                }
                LevelRandomRanked._increment = -1;
                LevelRandomRanked.AddAnotherBlobLevel();
                ChangeDebugState(LevelDebugState.CreatingBlobsIncremental);
            }
        }
    }

    public void ChangeDebugState(LevelDebugState levelDebugState)
    {
        if (DebugBlobGeneration || DebugNewLevel)
        {
            LevelDebugState = levelDebugState;
            switch (LevelDebugState)
            {
                case LevelDebugState.DebugSelecting:
                    TextHelper.text = "State [ " + LevelDebugState.DebugSelecting.ToString() + " ]" +
                    "\n - Press A to Enter Add Blobs Incremental [State]" +
                    "\n - Press C to Cancel";
                    break;
                case LevelDebugState.CreatingBlobsIncremental:
                    TextHelper.text = "State [ " + LevelDebugState.CreatingBlobsIncremental.ToString() + " ]" +
                    (DebugBlobGeneration ? "\n - Press A to add blob" : "") +
                    (DebugNewLevel ? "\n - Press W to switch to Add New Row [State]" : "") +
                    "\n - Press C to Cancel";
                    break;
                case LevelDebugState.AddNewRow:
                    TextHelper.text = "State [ " + LevelDebugState.CreatingBlobsIncremental.ToString() + " ]" +
                    "\n - Press W to add new row" +
                    "\n - Press C to Cancel";
                    LevelRandomRanked.ResetIncrement_Debug();
                    break;
            }
        } else {
            TextHelper.text = "";
        }
    }
}

public enum LevelDebugState
{
    DebugSelecting,
    CreatingBlobsIncremental,
    AddNewRow
}
