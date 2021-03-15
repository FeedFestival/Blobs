using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button Start;
    public Button HighCharts;
    public Button Options;
    public Button Back;
    public GameObject ActualMainMenuView;
    public GameObject OptionsView;
    public GameObject HighScoreView;

    void Awake()
    {
        SwitchView("Nothing");
    }

    public void Init(bool showMenu = true, bool hasSavedGame = true)
    {
        MusicManager._.PlayBackgroundMusic("MainMenuMusic");
        gameObject.SetActive(showMenu);

        SwitchView("Main");
    }

    public void SwitchView(string view)
    {
        if (MusicManager._ != null)
        {
            MusicManager._.PlaySound("Click");
        }
        switch (view)
        {
            case "Nothing":
                ActualMainMenuView.SetActive(false);
                OptionsView.SetActive(false);
                HighScoreView.SetActive(false);
                Options.gameObject.SetActive(false);
                Back.gameObject.SetActive(false);
                break;
            case "Main":
                ActualMainMenuView.SetActive(true);
                OptionsView.SetActive(false);
                HighScoreView.SetActive(false);
                Options.gameObject.SetActive(true);
                Back.gameObject.SetActive(false);
                break;
            case "Options":
                ActualMainMenuView.SetActive(false);
                OptionsView.SetActive(true);
                HighScoreView.SetActive(false);
                Options.gameObject.SetActive(false);
                Back.gameObject.SetActive(true);
                break;
            case "HighScore":
                ActualMainMenuView.SetActive(false);
                OptionsView.SetActive(false);
                HighScoreView.SetActive(true);
                Options.gameObject.SetActive(false);
                Back.gameObject.SetActive(true);
                break;
            default:
                Game._.LoadFirstLevel();
                break;
        }
    }
}

public enum MenuView
{
    Nothing, Main, Options, HighScore
}
