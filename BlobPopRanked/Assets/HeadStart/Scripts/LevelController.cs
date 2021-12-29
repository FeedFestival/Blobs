#pragma warning disable 0414 // private field assigned but not used.
using UnityEngine;
using Assets.Scripts.LevelService;
using System.Collections;

public class LevelController : MonoBehaviour
{
    public static readonly string _version = "1.0.2 (Updated - added ILevel, added EffectsParentT)";
    public bool DebugThis;
    public LevelType LevelType;
    public string LevelName;
    [SerializeField]
    public GameplayState GameplayState;
    public GameObject LevelGo;
    public ILevel Level;
    IEnumerator _preStartGameCo;
    IEnumerator _startGameCo;

    public void Init()
    {
        _preStartGameCo = PreStartGame();
        StartCoroutine(_preStartGameCo);
    }

    IEnumerator PreStartGame()
    {
        Debug.Log("Level - Pre Start Game");

        yield return new WaitForSeconds(0.1f);

        EffectsPool._.GenerateParticleControllers();
        UIController._.PointsController.GeneratePoints();

        _startGameCo = StartGame();
        StartCoroutine(_startGameCo);
    }

    IEnumerator StartGame()
    {
        StopCoroutine(_preStartGameCo);
        _preStartGameCo = null;
        Debug.Log("Level - Start Game");

        yield return new WaitForSeconds(0.1f);

        Level = LevelGo.GetComponent<ILevel>();
        Level.StartLevel();

        StopCoroutine(_startGameCo);
        _startGameCo = null;
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