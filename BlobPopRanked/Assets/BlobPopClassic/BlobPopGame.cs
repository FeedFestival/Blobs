using Assets.HeadStart.Core;
using Assets.HeadStart.Features.Dialog;
using UnityEngine;

namespace Assets.BlobPopClassic
{
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
}