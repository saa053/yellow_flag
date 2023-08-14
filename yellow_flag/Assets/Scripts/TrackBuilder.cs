using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackBuilder : MonoBehaviour
{
    [SerializeField] TileManager tileManager;
    public List<Vector3Int> trackTilesPos = new List<Vector3Int>();
    public bool buildCompleted = false;

    // Gizmos
    [SerializeField] private bool drawTrackWithGizmos;
    [SerializeField] float circleRadius;
    [SerializeField] float delay;

    private void Start() {
        if (drawTrackWithGizmos) {
            StartCoroutine(BuildTrackWithGizmos());
        } else {
            BuildTrack();
        }
    }

    // Creates a list of all tiles that makes up the track
    private void BuildTrack() {
        Vector3Int currentPos = tileManager.LocateStartTile();

        int entrance;
        int exit = tileManager.GetExits(currentPos)[1]; // [1] for direction of track

        do {
            trackTilesPos.Add(currentPos);

            // Moves to the next track tile depending on what the current tiles direction is
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
            
            if (!tileManager.isTrack(currentPos)) {
                Debug.Log("Track not completed!");
                return;
            }

            // Current tile entrance is the opposite side of the last tile exit
            entrance = (exit + 3) % 6;
            
            // Find the current tile exit
            exit = tileManager.GetExits(currentPos)[0];
            if (entrance == exit) {
                exit = tileManager.GetExits(currentPos)[1];
            }

        } while (!tileManager.isStartTile(currentPos));

        buildCompleted = true;
    }

    // Visually draws the track building process for debugging
    private IEnumerator BuildTrackWithGizmos() {
        Vector3Int currentPos = tileManager.LocateStartTile();

        int entrance;
        int exit = tileManager.GetExits(currentPos)[1]; // [1] for direction of track

        do {
            trackTilesPos.Add(currentPos);

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
            
            if (!tileManager.isTrack(currentPos)) {
                Debug.Log("Track not completed!");
                break;
            }

            entrance = (exit + 3) % 6;
            exit = tileManager.GetExits(currentPos)[0];
            
            if (entrance == exit) {
                exit = tileManager.GetExits(currentPos)[1];
            }

            yield return new WaitForSeconds(delay); // Gizmos

        } while (!tileManager.isStartTile(currentPos));

        buildCompleted = true;
    }

    private void OnDrawGizmos() {
        if (!drawTrackWithGizmos)
            return;

        Gizmos.color = Color.red;
        foreach (Vector3Int pos in trackTilesPos) {
            Gizmos.DrawSphere(tileManager.CellToWorld(pos), circleRadius);
        }
    }
}
