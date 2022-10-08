using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Gem : ScriptableObject
{
    public int ID;
    public GemType Type;
    public Sprite Sprite;
    public int BaseValue;
    public float Multiplier;
}