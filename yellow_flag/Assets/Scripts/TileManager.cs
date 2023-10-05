using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public Vector2Int LocateStartTile() {
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = bounds.x; x < bounds.xMax; x++) {
            for (int y = bounds.y; y < bounds.yMax; y++) {
                Vector2Int cellPosition = new Vector2Int(x, y);
                TileBase tile = allTiles[x - bounds.x + (y - bounds.y) * bounds.size.x];
                
                if (tile == null || !dataFromTiles.ContainsKey(tile))
                    continue;

                if (dataFromTiles[tile].type == Type.start)
                    return cellPosition;
            }
        }

        Debug.Log("ERROR: Missing start tile!");

        return new Vector2Int(-1, -1);
    }

    // Returns a tiles exits
    public int[] GetExits(Vector2Int tilePos) {
        TileBase tile = tilemap.GetTile(new Vector3Int(tilePos.x, tilePos.y, 0));
        return dataFromTiles[tile].exits;
    }

    // Returns the type of the tile at tilePos
    public Type GetType(Vector2Int tilePos) {
        TileBase tile = tilemap.GetTile(new Vector3Int(tilePos.x, tilePos.y, 0));
        return dataFromTiles[tile].type;
    }

    // Returns the rotation of the tile
    public float GetRotation(Vector2Int tilePos) {
        TileBase tile = tilemap.GetTile(new Vector3Int(tilePos.x, tilePos.y, 0));
        return dataFromTiles[tile].rotationAngle;
    }

    // Translate tilemap position to world position
    public Vector2 CellToWorld(Vector2Int tilePos) {
        return tilemap.CellToWorld(new Vector3Int(tilePos.x, tilePos.y, 0));
    }

    // Translate world position to tilemap position
    public Vector3Int WorldToCell(Vector2 tilePos) {
        return tilemap.WorldToCell(new Vector3Int((int)tilePos.x, (int)tilePos.y, 0));
    }

    public Vector2 GetCheckpoint(Vector2Int tilePos) {
        TileBase tile = tilemap.GetTile(new Vector3Int(tilePos.x, tilePos.y, 0));
        return dataFromTiles[tile].checkpoint;
    }
}

