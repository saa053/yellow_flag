using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class TrackBuilder : MonoBehaviour
{
    [SerializeField] TileManager tileManager;
    [SerializeField] float spaceBetweenRacingLines;

    [SerializeField] float hairpinCorneringSpeed;
    [SerializeField] float turnCorneringSpeed;
    [SerializeField] float maggotsAndBeckettsSpeed;

    [SerializeField] float longTurnCorneringSpeed;
    SplineContainer splineContainer;

    public struct Apex {
        public int knotIndex;
        public Type type;
        public float rotation;
        public float corneringSpeed;
    }

    public List<Apex> apexes = new List<Apex>();
    // public List<int> apexKnots = new List<int>();
    public enum RacingLine {
        optimal,
        left,
        right
    }
    public bool buildCompleted = false;


    void Start() {
        splineContainer = GetComponent<SplineContainer>();
       
        CreateRacingLines();
        CalculateCorneringSpeeds();


        foreach (Spline spline in splineContainer.Splines) {
            spline.SetTangentMode(TangentMode.AutoSmooth);
            spline.Closed = true;
        }

        buildCompleted = true;
    }

    // Locates the start tile, loops through track and adds knots to racing line splines
    void CreateRacingLines() {
        Vector2Int currentPos = tileManager.LocateStartTile();
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
    

    Vector2Int GetNextTrackTile(Vector2Int currentPos, int exit) {
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


    void AddKnots(Vector2Int tilePos, int entrance, int knotIndex)
    {
        Vector2[] knotPositions = GetKnotPos(tilePos);

        // Creat knots for each checkpoint
        BezierKnot optimalKnot = new BezierKnot(
            new Vector3(knotPositions[(int)RacingLine.optimal].x, knotPositions[(int)RacingLine.optimal].y), 
            0, 
            0, 
            Quaternion.Euler(-90, 0, 0));
        BezierKnot leftKnot = new BezierKnot(
            new Vector3(knotPositions[(int)RacingLine.left].x, knotPositions[(int)RacingLine.left].y), 
            0, 
            0, 
            Quaternion.Euler(-90, 0, 0));
        BezierKnot rightKnot = new BezierKnot(
            new Vector3(knotPositions[(int)RacingLine.right].x, knotPositions[(int)RacingLine.right].y), 
            0, 
            0, 
            Quaternion.Euler(-90, 0, 0));

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
            //apexKnots.Add(knotIndex);

            Apex apex;

            apex.knotIndex = knotIndex;
            apex.type = type;
            apex.rotation = tileManager.GetRotation(tilePos);
            
            if (type == Type.hairpin)
                apex.corneringSpeed = hairpinCorneringSpeed;
            else
                apex.corneringSpeed = turnCorneringSpeed;

            apexes.Add(apex);
        }
    }


    // Calculates the knot position for all racing lines and returns them in an array
    Vector2[] GetKnotPos(Vector2Int tilePos) {
        Vector2 optimal; 
        Vector2 left;
        Vector2 right;

        // Get checkpoint of tile
        optimal = tileManager.GetCheckpoint(tilePos);

        // Calculate and set left and right checkpoint
        Vector2 vectorToCenterOfTile = optimal - Vector2.zero;
        vectorToCenterOfTile = vectorToCenterOfTile.normalized;
        if (vectorToCenterOfTile == Vector2.zero) {
            left = new Vector2(optimal.x + spaceBetweenRacingLines, optimal.y);
            right = new Vector2(optimal.x - spaceBetweenRacingLines, optimal.y);
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
        Vector2 center = tileManager.CellToWorld(tilePos);
        optimal = center - optimal;
        left = center - left;
        right = center - right;

        Vector2[] knotPositions = new Vector2[] {
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
    float CalculateLeftRight(Vector2Int tilePos, int entrance, Vector2 optimal, Vector2 left) {
        // Find middle of entrance side to use as reference
        Vector2 center = tileManager.CellToWorld(tilePos);
        float angle = -60f * entrance;
        Vector2 entranceDirection = Quaternion.Euler(0, 0, angle) * Vector2.up;
        Vector2 sideMidpoint = center + entranceDirection;

        // Calculate cross product
        Vector2 referenceToOptimal = optimal - sideMidpoint;
        Vector2 referenceToleft = left - sideMidpoint;
        float crossProduct = Vector2.Dot(referenceToleft, new Vector2(-referenceToOptimal.y, referenceToOptimal.x));
        
        return crossProduct;
    }


    void CalculateCorneringSpeeds() {
        for (int i = 0; i < apexes.Count; i++) {
            Apex apex = apexes[i];
            if (apex.type != Type.turn)
                continue;


            if (i > 0) {
                Apex lastApex = apexes[i - 1];

                if (lastApex.type == Type.turn && apex.knotIndex - 1 == lastApex.knotIndex) {
                    apex.corneringSpeed = lastApex.corneringSpeed;
                    apexes[i] = apex;
                    continue;
                }
            }

            if (i + 1 < apexes.Count) {
                Apex nextApex = apexes[i + 1];

                if (nextApex.type == Type.turn && apex.knotIndex + 1 == nextApex.knotIndex) {
                    if (Mathf.Abs(apex.rotation - nextApex.rotation) == 180) {
                        apex.corneringSpeed = maggotsAndBeckettsSpeed;
                        apexes[i] = apex;
                    } else {
                        apex.corneringSpeed = longTurnCorneringSpeed;
                        apexes[i] = apex;
                    }
                }
            }
        }
    }
}
