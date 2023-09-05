
using System;
using UnityEngine;

[Serializable]
public class Cell
{
    public enum Type
    {
        Invalid,
        Empty,
        Mine,
        Number
    }

    public Vector2Int position;
    public Type type;
    public int number;
    public bool revealed;
    public bool flagged;
    public bool exploded;
}

