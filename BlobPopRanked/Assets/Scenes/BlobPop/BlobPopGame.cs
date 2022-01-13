using System;
using System.Collections;
using System.Collections.Generic;
using Assets.HeadStart.Core;
using Assets.HeadStart.Core.Player;
using Assets.HeadStart.Core.SceneService;
using Assets.HeadStart.Features.Dialog;
using Assets.Scripts.utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlobPopGame : GameBase
{
    // [Header("BlobPopGame")]
    // public User User;
    // public User PlayingUser;
    // public HighScoreType HighScoreType;
    // private string LevelToLoad;
    // IEnumerator _waitForLevelLoad;
    [Header("Game Prefabs")]
    public GameObject Blob;
    public GameObject NewBlob;
    public GameObject BlobDebugInfoPrefab;

    [Header("Particle Systems")]
    public GameObject PointText;

    public override void PreStartGame()
    {
        __.Transition.Do(Transition.END);
        BlobPopClassic._.Init();
    }

    public override void StartGame()
    {
        base.StartGame();

        BlobPopClassic._.StartLevel();
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
}
