using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private static UIController _uIController;
    public static UIController _ { get { return _uIController; } }
    void Awake()
    {
        _uIController = this;
        UiDataController.gameObject.SetActive(false);
        LoadingController.Init(totalBlackness: true);
    }
    public DialogController DialogController;
    public UiDataController UiDataController;
    public LoadingController LoadingController;
    public MainMenu MainMenu;

    public void Init()
    {
        if (UiDataController != null)
        {
            UiDataController.Init();
        }

        if (DialogController != null)
        {
            DialogController.gameObject.SetActive(true);
            DialogController.ShowDialog(false);
        }

        LoadingController.TransitionOverlay(show: true, instant: false);
    }

    public void InitMainMenu(bool isLevelMainMenu)
    {
        if (MainMenu != null)
        {
            Debug.Log("Init Main Menu");
            if (isLevelMainMenu)
            {
                MainMenu.Init(showMenu: true, hasSavedGame: Game._.User.HasSavedGame);
            }
            else
            {
                Destroy(MainMenu.gameObject);
            }
        }
    }
}
