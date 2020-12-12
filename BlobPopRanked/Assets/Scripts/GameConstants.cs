using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameConstants
{
    public static readonly int[] Eases = {
            (int)LeanTweenType.easeInOutBack,
            (int)LeanTweenType.easeOutElastic,
            (int)LeanTweenType.easeOutBounce
        };
}

public static class TAG
{
    public const string Blob = "Blob";
    public const string StickySurface = "StickySurface";
}

public static class LAYER
{
    public const string Blob = "Blob";
    public const string BlobProjectile = "BlobProjectile";
    public const string EndGame = "EndGame";
}

public static class ANIMATE
{
    public const string JustPlay = "JustPlay";
    // public const string Travel = "Travel";
    public const string StartTravel = "StartTravel";
    public const string StopTravel = "StopTravel";
}

public static class ANIMATION
{
    public const string Stretch = "BlobStretch_a_";
}

public static class ANIM_PARAM
{
    public const string StretchMultiplier = "StretchMultiplier";
    public const string StartTravelMultiplier = "StartTravelMultiplier";
    public const string TravelMultiplier = "TravelMultiplier";
}

public enum BlobAnim
{
    Idle, Stretch, Travel
}