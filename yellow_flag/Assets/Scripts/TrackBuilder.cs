using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Tilemaps;

public class TrackBuilder : MonoBehaviour
{
    [SerializeField] TileManager tileManager;
    [SerializeField] float spaceBetweenRacingLines;
    SplineContainer splineContainer;

    public List<int> apexKnots = new List<int>();
    public enum RacingLine {
        optimal,
        left,
        right,
    }
    public bool buildCompleted = false;


    void Start() {
        splineContainer = GetComponent<SplineContainer>();
       
        CreateRacingLines();

        foreach (Spline spline in splineContainer.Splines) {
            spline.SetTangentMode(TangentMode.AutoSmooth);
            spline.Closed = true;
        }

        buildCompleted = true;
    }

    // Locates the start tile, loops through track and adds knots to racing line splines
    void CreateRacingLines() {
        Vector3Int currentPos = tileManager.LocateStartTile();
        Type type;

        int entrance = tileManager.GetExits(currentPos)[0];
        int exit = tileManager.GetExits(currentPos)[1];

        int knotIndex = 0;
        do {
            AddKnots(currentPos, entrance, knotIndex);
            knotIndex++;
            
            currentPos = GetNextTrackTile(currentPos, exit);
            type = tileManager.GetType(currentPos);
            if (type == Type.other) {
                Debug.Log("ERROR: Track is unfinished");
                return;
            }

            // Current tile entrance is the opposite side of the last tile exit (hexagons)
            entrance = (exit + 3 ) % 6;
            
            // Find the current tile exit
            exit = tileManager.GetExits(currentPos)[0];
            if (entrance == exit) {
                exit = tileManager.GetExits(currentPos)[1];
            }

        } while (!(tileManager.GetType(currentPos) == Type.start));
    }
    

    Vector3Int GetNextTrackTile(Vector3Int currentPos, int exit) {
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

        return currentPos;
    }


    void AddKnots(Vector3Int tilePos, int entrance, int knotIndex)
    {
        Vector3[] knotPositions = GetKnotPos(tilePos);

        // Creat knots for each checkpoint
        BezierKnot optimalKnot = new BezierKnot(new Vector3(knotPositions[(int)RacingLine.optimal].x, 0, knotPositions[(int)RacingLine.optimal].y), 0, 0);
        BezierKnot leftKnot = new BezierKnot(new Vector3(knotPositions[(int)RacingLine.left].x, 0, knotPositions[(int)RacingLine.left].y), 0, 0);
        BezierKnot rightKnot = new BezierKnot(new Vector3(knotPositions[(int)RacingLine.right].x, 0, knotPositions[(int)RacingLine.right].y), 0, 0);

        // Add optimal knot to spline
        splineContainer.Splines[(int)RacingLine.optimal].Add(optimalKnot);

        // Add left and right knot to correct spline
        float referencePoint = CalculateLeftRight(tilePos, entrance, knotPositions[(int)RacingLine.optimal], knotPositions[(int)RacingLine.left]);
        if (referencePoint > 0) {
            // Left = Left
            splineContainer.Splines[(int)RacingLine.left].Add(leftKnot);
            splineContainer.Splines[(int)RacingLine.right].Add(rightKnot);
        }
        else {
            // Left = Right
            splineContainer.Splines[(int)RacingLine.right].Add(leftKnot);
            splineContainer.Splines[(int)RacingLine.left].Add(rightKnot);
        }

        // Add turn and hairpin knots to apex list
        Type type = tileManager.GetType(tilePos);
        if (type == Type.hairpin || type == Type.turn) {
            apexKnots.Add(knotIndex);
        }
    }


    // Calculates the knot position for all racing lines and returns them in an array
    Vector3[] GetKnotPos(Vector3Int tilePos) {
        Vector3 optimal; 
        Vector3 left;
        Vector3 right;

        // Get checkpoint of tile
        optimal = tileManager.GetCheckpoint(tilePos);

        // Calculate and set left and right checkpoint
        Vector3 vectorToCenterOfTile = optimal - Vector3.zero;
        vectorToCenterOfTile = vectorToCenterOfTile.normalized;
        if (vectorToCenterOfTile == Vector3.zero) {
            left = new Vector3(optimal.x + spaceBetweenRacingLines, optimal.y, optimal.z);
            right = new Vector3(optimal.x - spaceBetweenRacingLines, optimal.y, optimal.z);
        } else {
            left = optimal + spaceBetweenRacingLines * vectorToCenterOfTile;
            right = optimal - spaceBetweenRacingLines * vectorToCenterOfTile;
        }

        // Rotate checkpoints to match tile rotation
        float tileRotation = tileManager.GetRotation(tilePos);
        Quaternion newRotation = Quaternion.Euler(0f, 0f, -tileRotation);
        optimal = newRotation * optimal;
        left = newRotation * left;
        right = newRotation * right;
        
        // Convert checkpoints local pos to world pos
        Vector3 center = tileManager.CellToWorld(tilePos);
        optimal = center - optimal;
        left = center - left;
        right = center - right;

        Vector3[] knotPositions = new Vector3[] {
            optimal,
            left,
            right
        };

        return knotPositions;

    }


    // Calculates left and right of driving direction on the given tile and returns referencePoint
    // If referencePoint is positive, left knot should be placed in left spline
    // else left knot should be placed in right spline
    // Right knot should be placed opposite of left knot
    float CalculateLeftRight(Vector3Int tilePos, int entrance, Vector3 optimal, Vector3 left) {
        // Find middle of entrance side to use as reference
        Vector3 center = tileManager.CellToWorld(tilePos);
        float angle = -60f * entrance;
        Vector3 entranceDirection = Quaternion.Euler(0, 0, angle) * Vector3.up;
        Vector3 sideMidpoint = center + entranceDirection;

        // Calculate cross product
        Vector2 referenceToOptimal = optimal - sideMidpoint;
        Vector2 referenceToleft = left - sideMidpoint;
        float crossProduct = Vector2.Dot(referenceToleft, new Vector2(-referenceToOptimal.y, referenceToOptimal.x));
        
        return crossProduct;
    }
}
