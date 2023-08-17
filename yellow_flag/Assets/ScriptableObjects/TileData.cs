using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileData : ScriptableObject {
    public TileBase[] tiles;

    public bool isStart;

    public bool isTrack;

    public int[] exits; // An exit is a hexagon side, where the road on a track tile ends. Exit 0 is on top and then it moves clockwise.

    public Vector3 checkpoint;
}
