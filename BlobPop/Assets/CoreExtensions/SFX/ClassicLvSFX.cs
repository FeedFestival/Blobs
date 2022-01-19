using System.Collections.Generic;
using Assets.HeadStart.Core.SFX;
using UnityEngine;

namespace Assets.HeadStart.Core.SFX
{
    public class ClassicLvSFX : SFXBase
    {
        [Header("GameMusic")]
        public AudioClip GameMusic1;
        [Header("Gameplay Sounds")]
        public AudioClip BlobHit;
        public AudioClip PreExplosion;
        public AudioClip Explosion;
        public AudioClip MoneyTravel;
        public AudioClip MoneyPump;

        public override void Init()
        {
            base.Sounds = new Dictionary<string, MAudio>()
        {
            { "MainMenuMusic", new MAudio() { AudioClip = MainMenuMusic } },
            { "Click", new MAudio() { AudioClip = ClickSound } },
            { "GameMusic1", new MAudio() { AudioClip = GameMusic1 } },
            { "BlobHit", new MAudio() { AudioClip = BlobHit } },
            { "Pre_Explosion", new MAudio() { AudioClip = PreExplosion } },
            { "Explosion", new MAudio() { AudioClip = Explosion } },
            { "MoneyTravel", new MAudio() { AudioClip = MoneyTravel } },
            { "MoneyPump", new MAudio() { AudioClip = MoneyPump } }
        };
        }
    }
}