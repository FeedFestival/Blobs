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
    [Header("BlobPopGame")]
    public User User;
    public User PlayingUser;
    public HighScoreType HighScoreType;
    private string LevelToLoad;
    IEnumerator _waitForLevelLoad;
    [Header("Game Prefabs")]
    public GameObject Blob;
    public GameObject NewBlob;
    public GameObject BlobDebugInfoPrefab;

    [Header("Particle Systems")]
    public GameObject PointText;

    public override void PreStartGame()
    {
        __.Transition.Do(Transition.END);
        ClasicLv._.Init();
    }

    public override void StartGame()
    {
        base.StartGame();

        ClasicLv._.StartLevel();
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

        // TODO: remake this
        int points = 999;
        // int points = (int)Math.Ceiling(UIController._.ScreenPoints.Points);
        WeekDetails week = __data.GetWeekDetails();

        if (HighScoreType == HighScoreType.RANKED)
        {
            TryUpdateLatestWeekScore(week, points);
        }
        UpdateHighScore(week, points);
    }

    private void TryUpdateLatestWeekScore(WeekDetails week, int points)
    {
        WeekScore weekScore = DataService.GetHighestWeekScore(week.Id);
        if (weekScore == null)
        {
            weekScore = new WeekScore()
            {
                Id = week.Id,
                Points = points,
                Year = week.Year,
                Week = week.Nr,
                UserId = User.Id
            };
            DataService.AddWeekHighScore(weekScore);
        }
        else
        {
            if (weekScore.Points >= points)
            {
                Debug.Log("weekScore: " + weekScore.Points);
            }
            else
            {
                weekScore.Points = points;
                DataService.UpdateWeekHighScore(weekScore);
            }
        }
    }

    private void UpdateHighScore(WeekDetails week, int points)
    {
        HighScore highScore = new HighScore()
        {
            Points = points,
            Type = HighScoreType,
            WeekId = week.Id,
            UserId = PlayingUser.Id,
            UserName = PlayingUser.Name
        };
        DataService.AddHighScore(highScore);
    }

    IEnumerator WaitForLevelLoad()
    {
        // float loadingWait = _game != null
        //     && _game.AfterLoading == AfterLoading.RestartLevel ? 0.5f : 2f;
        // Debug.Log("loadingWait: " + loadingWait);
        yield return new WaitForSeconds(0.5f);

        // _game.LoadWaitedLevel();
        // StopCoroutine(_waitForLevelLoad);
        // _waitForLevelLoad = null;
    }
}

public enum AfterLoading
{
    Nothing, RestartLevel, GoToGame
}
