﻿using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static readonly string _version = "1.0.2";
    private static Game _game;
    public static Game _ { get { return _game; } }
    public LevelController LevelController;
    public Player Player;
    public DataService DataService;
    public User User;
    public bool RestartLevel;
    private string LevelToLoad;
    private int _uniqueId;
    public bool GameOver;

    void Awake()
    {
        _game = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void Init()
    {
        Debug.Log("Game - Init");

        GetUser();
        Debug.Log(User.Name);

        UIController._.Init();

        LevelController.Init();

        UIController._.LoadingController.TransitionOverlay(show: true, instant: false);
    }

    private void GetUser()
    {
        DataService = new DataService();
        DataService.CreateDBIfNotExists();
        User = DataService.GetLastUser();
        if (User == null)
        {
            User = new User()
            {
                Id = 1,
                Name = "no-name-user",
                IsFirstTime = true,
                HasSavedGame = false,
                IsUsingSound = true,
                Language = "en"
            };
            DataService.CreateUser(User);
        }
    }

    public void Restart()
    {
        RestartLevel = true;
        LevelToLoad = LevelController.LevelName;
        LoadScene("Loading");
    }

    public void LoadWaitedLevel()
    {
        if (RestartLevel)
        {
            RestartLevel = false;
            LoadScene(LevelToLoad);
            return;
        }
    }

    public void LoadFirstLevel()
    {
        LevelToLoad = "Game";
        UIController._.LoadingController.TransitionOverlay(show: false, instant: false, () =>
        {
            LoadScene(LevelToLoad);
        });
    }

    private void LoadScene(string level)
    {
        GameOver = false;
        SceneManager.LoadScene(level);
    }

    internal void OnGameOver()
    {
        GameOver = true;
        UIController._.DialogController.ShowDialog(true, GameplayState.Failed);
    }

    public T Level<T>()
    {
        return (T)Convert.ChangeType(LevelController.Level, typeof(T));
    }

    public int GetUniqueId()
    {
        _uniqueId++;
        return _uniqueId;
    }
}
