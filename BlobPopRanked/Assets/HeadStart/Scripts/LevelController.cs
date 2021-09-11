#pragma warning disable 0414 // private field assigned but not used.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.LevelService;
using Assets.Scripts.utils;
using System.Linq;

public class LevelController : MonoBehaviour
{
    public static readonly string _version = "1.0.2 (Updated - added ILevel, added EffectsParentT)";
    public bool DebugThis;
    public LevelType LevelType;
    public string LevelName;
    [SerializeField]
    public GameplayState GameplayState;
    public GameObject LevelGo;
    public EffectsPool EffectsPool;
    public ILevel Level;

    public void Init()
    {
        Debug.Log("LevelType: " + LevelType);
        if (LevelType == LevelType.TheGame)
        {
            PreStartGame();
            StartGame();
        }
    }

    public void PreStartGame()
    {
        Debug.Log("Level - Pre Start Game");
        EffectsPool.GenerateParticleControllers();
        UIController._.PointsController.GeneratePoints();
    }

    public void StartGame()
    {
        Debug.Log("Level - Start Game");
        Level = LevelGo.GetComponent<ILevel>();
        Level.StartLevel();
    }

    public void Restart()
    {
        Game._.Restart();
    }
}

public enum GameplayState
{
    Starting, DuringPlay, Failed, Finished
}

public enum LevelType
{
    MainMenu, Loading, TheGame
}