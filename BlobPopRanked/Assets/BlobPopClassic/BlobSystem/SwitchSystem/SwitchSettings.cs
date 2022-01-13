using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "SwitchSettings", menuName = "BlobGame/SwitchSettings")]
public class SwitchSettings : ScriptableObject
{
    public Vector3 ShootableBlobPosition;
    public Vector3 SwitchableBlobPosition;

    [Header("TweenTime")]
    public float MakeShootableBlobDebounceTime;
    public float SwitchTime;
}
