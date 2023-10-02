using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] RacingLines racingLines;
    [SerializeField] float speed;
    [SerializeField] float startingPoint;
    [SerializeField] float turnThreshold;
    [SerializeField] float imageRotation;

    SplineContainer splineContainer;

    float distancePercentage = 0f;
    float splineLength;

    RacingLines.RacingLine optimal = RacingLines.RacingLine.optimal;
    //RacingLines.RacingLine outside = RacingLines.RacingLine.outside;
    //RacingLines.RacingLine inside = RacingLines.RacingLine.inside;

    void Start() {
        splineContainer = racingLines.GetComponent<SplineContainer>();

        distancePercentage = startingPoint;

        splineLength = splineContainer.CalculateLength((int)optimal);
    }

    void Update() {
        if (splineLength == 0) {
            splineLength = splineContainer.CalculateLength((int)optimal);
            return;
        }

        Move();
    }

    void Move() {
        distancePercentage += speed * Time.deltaTime / splineLength;
        
        Vector3 currentPosition = splineContainer.EvaluatePosition(distancePercentage);
        transform.position = new Vector3(currentPosition.x, currentPosition.y, (int)optimal);

        if (distancePercentage > 1f) {
            distancePercentage = 0f;
        }

        Vector3 nextPosition = splineContainer.EvaluatePosition(distancePercentage + turnThreshold);
        Vector3 direction = nextPosition - currentPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle + imageRotation);
    }

    void BreakAndAccelerate() {
        //float distance = splineContainer.Splines[0].ConvertIndexUnit(racingLines.apexKnots[0], PathIndexUnit.Knot, PathIndexUnit.Distance);
        //float knotT = distance / splineLength;
    }
}
