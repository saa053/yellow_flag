using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Tilemaps;
using System;


public class RacingLines : MonoBehaviour
{
    [SerializeField] TileManager tileManager;
    [SerializeField] TrackBuilder track;

    SplineContainer splineContainer;

    public List<int> apexKnots = new List<int>();

    public enum RacingLine {
        optimal,
        outside,
        inside,
    }

    void Start() {
        splineContainer = GetComponent<SplineContainer>();

        if (!track.buildCompleted) {
            Debug.Log("ERROR: Racing line failed. Track not completed!");
            return;
        }

        CreateRacingLine((int)RacingLine.optimal, tileManager.GetCheckpoint);
        //CreateRacingLine(splineContainer.Splines[(int)RacingLine.outside], insideCheckpoints);
        //CreateRacingLine(splineContainer.Splines[(int)RacingLine.inside], outsideCheckpoints);

        foreach (Spline spline in splineContainer.Splines) {
            spline.SetTangentMode(TangentMode.AutoSmooth);
            spline.Closed = true;
        }
    }

    // Creates a racing line by filling the spline with Bezier knots at every checkpoint provided
    void CreateRacingLine(int racingLine, Func<Vector3Int, int, Checkpoint> getCheckpointFunction) {
        int knotIndex = 0;

        foreach (Vector3Int tilePos in track.trackTilesPos) {
            // Convert checkpoint positions to world vector3 positions
            Checkpoint checkpoint = getCheckpointFunction(tilePos, racingLine);
            Vector3 center = tileManager.CellToWorld(tilePos);
            Vector3 knotPos = center - checkpoint.position;

            // Create and add knot
            BezierKnot knot = new BezierKnot(new Vector3(knotPos.x, 0, knotPos.y), 0, 0);
            splineContainer.Splines[racingLine].Add(knot);

            // Store index of apex knots in a list
            if (checkpoint.isApex) {
                apexKnots.Add(knotIndex);
            }

            knotIndex++;
        }
    }
}