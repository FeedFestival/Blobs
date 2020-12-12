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
    public int CeilId;
    public float WallStickLimit;
    public float NeighborTestDistance;
    public float NeighborProximity;
    public int MinNeighborCountToDestroy;
    public float GameOverOffsetY;

    [Header("ANIMATIONS")]
    public float BlobKillAnimationLength;
    public float BlobExplodeAnimationLength;
    public float BlobForcePushAnimL;
    public float BlobElasticBackAnimL;
    public float BallStickyReflectDistanceModifier;
    public float StretchSpeed;
    public float StretchSpeedSticky;
    public float StartTravelSpeed;
    public float TravelSpeed;
    public GameObject PrefabBankPrefab;

    [Header("Colors")]
    public Color TransparentColor;
    public Color White;
}
