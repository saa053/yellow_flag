using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Tilemaps;

public class RacingLines : MonoBehaviour
{
    [SerializeField] TileManager tileManager;
    [SerializeField] TrackBuilder track;

    private void Start() {
        SplineContainer splineContainer = GetComponent<SplineContainer>();

        if (!track.buildCompleted) {
            Debug.Log("ERROR: Racing line failed. Track not completed!");
            return;
        }


        // ----- TMP ----
        List<Vector3> optimalCheckpoints = new List<Vector3>();
        foreach (Vector3Int tilePos in track.trackTilesPos) {
            optimalCheckpoints.Add(tileManager.CellToWorld(tilePos));
        }
        // --------------


        CreateRacingLine(splineContainer.Splines[0], optimalCheckpoints);
        //CreateRacingLine(splineContainer.Splines[1], insideCheckpoints);
        //CreateRacingLine(splineContainer.Splines[2], outsideCheckpoints);

        foreach (Spline spline in splineContainer.Splines) {
            spline.SetTangentMode(TangentMode.AutoSmooth);
            spline.Closed = true;
        }
    }

    // Creates a spline racing line based on a list of checkpoints
    private void CreateRacingLine(Spline spline, List<Vector3> checkpoints) {
        foreach (Vector3 checkpoint in checkpoints) {      
                  
            BezierKnot knot = new BezierKnot(new Vector3(checkpoint.x, 0, checkpoint.y), 0, 0);
            spline.Add(knot);
        }
    }
}
