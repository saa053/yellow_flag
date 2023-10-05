using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] TrackBuilder trackBuilder;
    [SerializeField] float speed;
    [SerializeField] float startingPoint;
    [SerializeField] float turnThreshold;
    [SerializeField] float imageRotation;
    [SerializeField] TrackBuilder.RacingLine racingLine;

    SplineContainer splineContainer;

    float distancePercentage = 0f;
    float splineLength;

    void Start() {
        splineContainer = trackBuilder.GetComponent<SplineContainer>();

        distancePercentage = startingPoint;
    }

    void Update() {
        if (!trackBuilder.buildCompleted)
            return;

        Move((int)racingLine);
    }

    void Move(int racingLine) {
        if (splineLength == 0)
            splineLength = splineContainer.CalculateLength(racingLine);

        distancePercentage += speed * Time.deltaTime / splineLength;
        
        Vector3 currentPosition = splineContainer.EvaluatePosition(distancePercentage);
        transform.position = new Vector3(currentPosition.x, currentPosition.y, racingLine);

        if (distancePercentage > 1f) {
            distancePercentage = 0f;
        }

        Vector3 nextPosition = splineContainer.EvaluatePosition(distancePercentage + turnThreshold);
        Vector3 direction = nextPosition - currentPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle + imageRotation);
    }

    void BreakAndAccelerate() {
        //float distance = splineContainer.Splines[0].ConvertIndexUnit(trackBuilder.apexKnots[0], PathIndexUnit.Knot, PathIndexUnit.Distance);
        //float knotT = distance / splineLength;
    }
}
