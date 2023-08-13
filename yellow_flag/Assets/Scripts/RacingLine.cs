using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Tilemaps;

public class RacingLine : MonoBehaviour
{
    [SerializeField] private Tilemap map;
    [SerializeField] MapManager mapManager;

    public void Start() {
        SplineContainer splineContainer = GetComponent<SplineContainer>();

        //Spline optimal = new Spline(); // Spline 0 (added by default by GameObject)

        // Create optimal racing line
        foreach (Vector3Int tile in mapManager.trackTiles) {

            float x = map.CellToWorld(tile).x;
            float y = map.CellToWorld(tile).y;
            Vector3 pos = new Vector3(x, 0, y);
            
            BezierKnot knot = new BezierKnot(pos, 0, pos);
            splineContainer.Splines[0].Add(knot);
        }

        //Spline outside = new Spline(); // Spline 1
        //splineContainer.AddSpline(outside);

        //TODO: Create outside racing line

        //Spline inside = new Spline(); // Spline 2
        //splineContainer.AddSpline(inside);

        //TODO: Create inside racing line

        foreach (Spline spline in splineContainer.Splines) {
            spline.SetTangentMode(TangentMode.AutoSmooth);
            spline.Closed = true;
        }
    }
}
