using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new spirit", menuName = "HandmadeSpirit/Spirit", order = 1)]
public class SpiritSO : ScriptableObject
{
    public string spiritName;
    public Sprite image;
    public Color colour;
}