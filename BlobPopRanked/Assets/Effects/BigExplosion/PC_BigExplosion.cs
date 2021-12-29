using System;
using UniRx;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class PC_BigExplosion : ParticleController
{
    [Header("Big Explosion")]
    public SpriteRenderer ExplosionBubbleT;
    public Animator PreExplosionAnmtor;
    public BigExplosion_SO CurrentExplosion;
    [Header("Explosion Colors SO")]
    public BigExplosion_SO RedBigExplosion;
    public BigExplosion_SO BlueBigExplosion;
    public BigExplosion_SO YellowBigExplosion;
    public BigExplosion_SO GreenBigExplosion;
    public BigExplosion_SO BrownBigExplosion;
    public BigExplosion_SO PinkBigExplosion;
    public BigExplosion_SO WhiteBigExplosion;
    public BigExplosion_SO BlackBigExplosion;
    const int _explosionIndx = 0;
    const int _debrisIndx = 1;
    private MinMaxGradient explosionGradient;
    private MinMaxGradient debrisGradient;
    private const float _preExplosionAnmL = 3f;
    private const float _preExplosionAnmS = 2.9f;
    [HideInInspector]
    public float PreExplosionLengh = _preExplosionAnmL / _preExplosionAnmS;
    private IDisposable _copyPosSub;

    public void SetColor(BlobColor blobColor)
    {
        switch (blobColor)
        {
            case BlobColor.BLACK:
                CurrentExplosion = BlackBigExplosion;
                ExplosionBubbleT.color = Color.yellow;
                break;
            case BlobColor.WHITE:
                CurrentExplosion = WhiteBigExplosion;
                ExplosionBubbleT.color = Color.black;
                break;
            case BlobColor.PINK:
                CurrentExplosion = PinkBigExplosion;
                ExplosionBubbleT.color = Color.white;
                break;
            case BlobColor.BROWN:
                CurrentExplosion = BrownBigExplosion;
                ExplosionBubbleT.color = CurrentExplosion.MaxExplosion;
                break;
            case BlobColor.GREEN:
                CurrentExplosion = GreenBigExplosion;
                ExplosionBubbleT.color = Color.white;
                break;
            case BlobColor.YELLOW:
                CurrentExplosion = YellowBigExplosion;
                ExplosionBubbleT.color = Color.white;
                break;
            case BlobColor.BLUE:
                CurrentExplosion = BlueBigExplosion;
                ExplosionBubbleT.color = Color.white;
                break;
            case BlobColor.RED:
            default:
                CurrentExplosion = RedBigExplosion;
                ExplosionBubbleT.color = Color.white;
                break;
        }

        changeColor();
    }

    public void CopyPosition(Transform blobT)
    {
        _copyPosSub = blobT.ObserveEveryValueChanged(t => t.position)
            .Subscribe(pos =>
            {
                transform.position = pos;
            });
    }

    public override void Play()
    {
        play();
    }

    public override void Play(Vector2 point)
    {
        transform.position = new Vector3(point.x, point.y, 0);
        play();
    }

    private void play()
    {
        float fromScale = 0.1709931f;
        float toScale = 0.3709931f;
        Vector3 bubbleScale = new Vector3(toScale, toScale, toScale);

        ExplosionBubbleT.gameObject.SetActive(true);
        ExplosionBubbleT.transform.localScale = new Vector3(fromScale, fromScale, fromScale);
        LeanTween.scale(ExplosionBubbleT.gameObject, bubbleScale, PreExplosionLengh);
        PreExplosionAnmtor.gameObject.SetActive(true);

        Timer._.InternalWait(() =>
        {
            InternalPlay();
            _copyPosSub.Dispose();
            ExplosionBubbleT.gameObject.SetActive(false);
            PreExplosionAnmtor.gameObject.SetActive(false);
        }, PreExplosionLengh);
    }

    private void changeColor()
    {
        explosionGradient = new MinMaxGradient(CurrentExplosion.MinExplosion, CurrentExplosion.MaxExplosion);
        debrisGradient = new MinMaxGradient(CurrentExplosion.MinDebris, CurrentExplosion.MaxDebris);

        PreExplosionAnmtor.gameObject.GetComponent<SpriteRenderer>().color = CurrentExplosion.MinDebris;

        MainModule main = ParticleSystems[_explosionIndx].main;
        main.startColor = explosionGradient;

        main = ParticleSystems[_debrisIndx].main;
        main.startColor = debrisGradient;
    }
}
