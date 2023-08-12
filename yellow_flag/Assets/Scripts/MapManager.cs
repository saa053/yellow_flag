using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Tilemap map;
    [SerializeField] private List<TileData> tileDatas;

    private Dictionary<TileBase, TileData> dataFromTiles;

    private List<Vector3Int> trackTiles = new List<Vector3Int>(); // Gizmos
    public float circleRadius = 0.5f; // Gizmos
    public float delay = 0.2f; // Gizmos


    private void Awake() {
        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach (var tileData in tileDatas) {
            foreach (var tile in tileData.tiles) {
                dataFromTiles.Add(tile, tileData);
            }
        }

        Vector3Int startTilePos = LocateStartTile();
        if (startTilePos == new Vector3Int(-1, -1, -1))
            return;

        StartCoroutine(BuildTrack(startTilePos)); // Gizmos
        // BuildTrack(startTilePos)
    }
    
    void Update() {
    
    }

    // Gizmos
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        foreach (Vector3Int pos in trackTiles) {
            Gizmos.DrawSphere(map.CellToWorld(pos), circleRadius);
        }
    }

    Vector3Int LocateStartTile() {
        BoundsInt bounds = map.cellBounds;
        TileBase[] allTiles = map.GetTilesBlock(bounds);

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

    private IEnumerator BuildTrack(Vector3Int startTilePos) {
        Vector3Int currentPos = startTilePos;
        TileBase currentTile = map.GetTile(startTilePos);
        int exit = dataFromTiles[currentTile].exits[1];
        int entrance;

        do {
            trackTiles.Add(currentPos); // Gizmos
            switch (exit) {
                case 0:
                    currentPos.x += 1;
                    break;
                case 1:
                    if (currentPos.y % 2 != 0) {
                        currentPos.x += 1;
                    }
                    currentPos.y += 1;
                    break;
                case 2:
                    if (currentPos.y % 2 == 0) {
                        currentPos.x -= 1;
                    }
                    currentPos.y += 1;
                    break;
                case 3:
                    currentPos.x -= 1;
                    break;
                case 4:
                    if (currentPos.y % 2 == 0) {
                        currentPos.x -= 1;
                    }
                    currentPos.y -= 1;
                    break;
                case 5:
                    if (currentPos.y % 2 != 0) {
                        currentPos.x += 1;
                    }
                    currentPos.y -= 1;
                    break;
                default:
                    break;
            }

            entrance = (exit + 3) % 6;
            currentTile = map.GetTile(currentPos);
            if (!currentTile) {
                Debug.Log("Track not completed!");
            }
            exit = dataFromTiles[currentTile].exits[0];
            if (entrance == exit) {
                exit = dataFromTiles[currentTile].exits[1];
            }

            yield return new WaitForSeconds(delay); // Gizmos
        } while (!dataFromTiles[currentTile].isStart);
    }
}
