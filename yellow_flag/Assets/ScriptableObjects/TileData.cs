using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Checkpoint {
    public Vector3 position;
    public bool isApex;

    public Checkpoint(Vector3 pos, bool val) {
        position = pos;
        isApex = val;
    }
}

public enum Type {
    other,
    straight,
    turn,
    hairpin,
    start,
}

[CreateAssetMenu]
public class TileData : ScriptableObject {
    public TileBase[] tiles;

    public bool isStart;

    public bool isTrack;

    public bool isTurn;

    public int[] exits; // An exit is a hexagon side, where the road on a track tile will exit. Exit 0 is on top and then it moves clockwise.

    /*  Range (0, 60, 120, 180, 240, 300)
        Defined by putting identical tiles in a hexagon formation in the tilemap.
        Turn Tiles and Straight Tiles: 0 is the top tile. 60 is the next clockwise and so on.
        Hairpin Tiles: 0 is the bottom right tile. 60 is the next clockwise and so on.
    */
    public float rotationAngle;

    public Type type;

    public Checkpoint[] checkpoints; // Racing line knot position (local Vector3 pos) and an apex boolean
}
