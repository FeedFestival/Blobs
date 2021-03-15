using System;
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
    public AfterLoading AfterLoading;
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

        UIController._.InitMainMenu(LevelController.LevelType == LevelType.MainMenu);
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
        AfterLoading = AfterLoading.RestartLevel;
        LevelToLoad = LevelController.LevelName;
        LoadScene("Loading");
    }

    public void LoadWaitedLevel()
    {
        switch (AfterLoading)
        {
            case AfterLoading.RestartLevel:
                LoadScene(LevelToLoad);
                break;
            case AfterLoading.GoToGame:
            default:
                LoadScene("Game");
                break;
        }
        AfterLoading = AfterLoading.Nothing;
    }

    public void LoadFirstLevel()
    {
        AfterLoading = AfterLoading.GoToGame;
        UIController._.LoadingController.TransitionOverlay(show: false, instant: false, () =>
        {
            LoadScene("Loading");
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

public enum AfterLoading
{
    Nothing, RestartLevel, GoToGame
}
