using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabBank : MonoBehaviour
{
    private static PrefabBank _prefabBank;
    public static PrefabBank _ { get { return _prefabBank; } }

    public GameObject GamePrefab;
    public GameObject MusicManagerPrefab;
    public GameObject ColorBankPrefab;
    public GameObject TimerPrefab;
    public GameObject Blob;
    public GameObject NewBlob;
    public GameObject BlobDebugInfoPrefab;

    [Header("Particle Systems")]
    public GameObject SmallHitParticle;
    public GameObject ExplosionParticle;
    public GameObject PointText;

    void Awake()
    {
        _prefabBank = this;
        DontDestroyOnLoad(this.gameObject);
    }
}
