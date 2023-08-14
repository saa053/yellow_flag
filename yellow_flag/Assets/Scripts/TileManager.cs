using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class TileManager : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private List<TileData> tileDatas;

    private Dictionary<TileBase, TileData> dataFromTiles;

    private void Awake() {

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

                bool isStart = dataFromTiles[tile].isStart;

                if (isStart)
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

    // Returns true if the tile is a track tile
    public bool isTrack(Vector3Int tilePos) {
        TileBase tile = tilemap.GetTile(tilePos);
        return dataFromTiles[tile].isTrack;
    }

    // Returns true if the tile is a start tile
    public bool isStartTile(Vector3Int tilePos) {
        TileBase tile = tilemap.GetTile(tilePos);
        return dataFromTiles[tile].isStart;
    }

    // Translate tilemap position to world position
    public UnityEngine.Vector3 CellToWorld(Vector3Int tilePos) {
        return tilemap.CellToWorld(tilePos);
    }

    // Translate world position to tilemap position
    public UnityEngine.Vector3 WorldToCell(UnityEngine.Vector3 pos) {
        return tilemap.WorldToCell(pos);
    }
}

