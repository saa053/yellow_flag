using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileData : ScriptableObject
{
    public TileBase[] tiles;

    public bool isStart;

    public int[] exits;

}
