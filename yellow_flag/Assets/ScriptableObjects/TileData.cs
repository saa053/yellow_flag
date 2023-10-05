using UnityEngine;
using UnityEngine.Tilemaps;

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

    public int[] exits; // An exit is a hexagon side, where the road on a track tile will exit. Exit 0 is on top and then it moves clockwise.

    /*  Range (0, 60, 120, 180, 240, 300)
        Defined by putting identical tiles in a hexagon formation in the tilemap.
        Turn Tiles and Straight Tiles: 0 is the top tile. 60 is the next clockwise and so on.
        Hairpin Tiles: 0 is the bottom right tile. 60 is the next clockwise and so on.
    */
    public float rotationAngle;

    public Type type;

    public Vector2 checkpoint; // Racing line knot position (local Vector3 pos)
}
