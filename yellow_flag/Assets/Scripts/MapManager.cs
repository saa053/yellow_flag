using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Tilemap map;
    [SerializeField] private List<TileData> tileDatas;
    [SerializeField] private bool drawTrackWithGizmos;
    public List<Vector3Int> trackTiles = new List<Vector3Int>();

    private Dictionary<TileBase, TileData> dataFromTiles;

    // Gizmos
    public float circleRadius = 0.5f;
    public float delay = 0.2f;


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

        if (drawTrackWithGizmos) {
            StartCoroutine(BuildTrackWithGizmos(startTilePos));
        } else {
            BuildTrack(startTilePos);
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

    private void BuildTrack(Vector3Int startTilePos) {
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

        } while (!dataFromTiles[currentTile].isStart);
    }

    private IEnumerator BuildTrackWithGizmos(Vector3Int startTilePos) {

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

    private void OnDrawGizmos() {
        if (!drawTrackWithGizmos)
            return;

        Gizmos.color = Color.red;
        foreach (Vector3Int pos in trackTiles) {
            Gizmos.DrawSphere(map.CellToWorld(pos), circleRadius);
        }
    }
}

