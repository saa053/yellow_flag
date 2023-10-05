using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class TileManager : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] List<TileData> tileDatas;

    Dictionary<TileBase, TileData> dataFromTiles;

    void Awake() {

        // Put all tile data in a dictionary for easy lookup
        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (var tileData in tileDatas) {
            foreach (var tile in tileData.tiles) {
                dataFromTiles.Add(tile, tileData);
            }
        }
    }

    // Returns the tilemap position of the first start tile it finds
    public Vector3Int LocateStartTile() {
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = bounds.x; x < bounds.xMax; x++) {
            for (int y = bounds.y; y < bounds.yMax; y++) {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                TileBase tile = allTiles[x - bounds.x + (y - bounds.y) * bounds.size.x];
                
                if (tile == null || !dataFromTiles.ContainsKey(tile))
                    continue;

                if (dataFromTiles[tile].type == Type.start)
                    return cellPosition;
            }
        }

        Debug.Log("ERROR: Missing start tile!");

        return new Vector3Int(-1, -1, -1);
    }

    // Returns a tiles exits
    public int[] GetExits(Vector3Int tilePos) {
        TileBase tile = tilemap.GetTile(tilePos);
        return dataFromTiles[tile].exits;
    }

    // Returns the type of the tile at tilePos
    public Type GetType(Vector3Int tilePos) {
        TileBase tile = tilemap.GetTile(tilePos);
        return dataFromTiles[tile].type;
    }

    // Returns the rotation of the tile
    public float GetRotation(Vector3Int tilePos) {
        TileBase tile = tilemap.GetTile(tilePos);
        return dataFromTiles[tile].rotationAngle;
    }

    // Translate tilemap position to world position
    public Vector3 CellToWorld(Vector3Int tilePos) {
        return tilemap.CellToWorld(tilePos);
    }

    // Translate world position to tilemap position
    public Vector3 WorldToCell(Vector3 tilePos) {
        return tilemap.WorldToCell(tilePos);
    }

    public Vector3 GetCheckpoint(Vector3Int tilePos) {
        TileBase tile = tilemap.GetTile(tilePos);
        return dataFromTiles[tile].checkpoint;
    }
}

