using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Effects", menuName = "Game Effects/BigExplosion", order = 1)]
public class BigExplosion_SO : ScriptableObject
{
    public Color MinExplosion;
    public Color MaxExplosion;
    public Color MinDebris;
    public Color MaxDebris;
}
