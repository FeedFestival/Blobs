using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenSettings : MonoBehaviour
{
    static private HiddenSettings _hiddenSettings;
    static public HiddenSettings _ { get { return _hiddenSettings; } }

    void Awake()
    {
        _hiddenSettings = this;
        DontDestroyOnLoad(this.gameObject);
    }

    internal float GetTime(float normalTime)
    {
        return InstantDebug ? 0f : normalTime;
    }

    public GameObject GetAnInstantiated(GameObject prefab)
    {
        return Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
    }

    [Header("Default")]
    public Vector2Int ActualScreenSize;

    [Header("Debug Settings")]
    public bool InstantDebug;

    [Header("Game Configuration")]
    public int CeilId = 0;
    public float WallStickLimit = 4.44f;
    public float NeighborTestDistance = 0.55f;
    public float NeighborProximity = 0.6f;
    public int MinNeighborCountToDestroy = 2;

    [Header("ANIMATIONS")]
    public float BlobKillAnimationLength = 0.3f;
    public float BlobExplodeAnimationLength = 0.3f;

    [Header("Colors")]
    public GameObject PrefabBankPrefab;
    public Color MineShaftLightColor;
    public Color MineShaft;
    public Color CodGrayColor;
    public Color TransparentColor;
    public Color OrangeColor;
    public Color PomegranateColor;  // RED
    public Color AlizarinCrimsonColor;
    public Color VioletLightColor;
    public Color MediumPurple;
    public Color VioletDarkColor;
    public Color RoyalBlueLightColor;
    public Color RoyalBlue;
    public Color RoyalBlueDarkColor;
    public Color Conifer;
    public Color AtlantisColor;
    public Color AtlantisDarkColor;
    public Color Gorse;
    public Color Candlelight;
    public Color Amber;
    public Color YellowColor;
    public Color LightBlueColor;
    public Color White;
    public Color WhiteTransparent;

    public Color GetColorByBlobColor(BlobColor blobColor)
    {
        switch (blobColor)
        {
            case BlobColor.AtlantisColor:
                return AtlantisColor;
            case BlobColor.RoyalBlue:
                return RoyalBlue;
            case BlobColor.Candlelight:
                return Candlelight;
            case BlobColor.MediumPurple:
                return MediumPurple;
            case BlobColor.PomegranateColor:
            default:
                return PomegranateColor;

        }
    }
}
