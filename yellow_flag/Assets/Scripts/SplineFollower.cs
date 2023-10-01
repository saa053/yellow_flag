using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] SplineContainer splineContainer;
    [SerializeField] float speed = 1f;
    [SerializeField] float startingPoint;
    [SerializeField] float turnThreshold;
    [SerializeField] float imageRotation;

    float distancePercentage = 0f;
    float splineLength;

    private enum RacingLine {
        optimal,
        outside,
        inside,
    }

    void Start() {
        distancePercentage += startingPoint;

        splineLength = splineContainer.CalculateLength((int)RacingLine.optimal);
    }

    void Update() {
        if (splineLength == 0) {
            splineLength = splineContainer.CalculateLength((int)RacingLine.optimal);
        }

        distancePercentage += speed * Time.deltaTime / splineLength;
        Debug.Log(splineLength);
        
        Vector3 currentPosition = splineContainer.EvaluatePosition(distancePercentage);
        transform.position = new Vector3(currentPosition.x, currentPosition.y, (int)RacingLine.optimal);

        if (distancePercentage > 1f) {
            distancePercentage = 0f;
        }

        Vector3 nextPosition = splineContainer.EvaluatePosition(distancePercentage + turnThreshold);
        Vector3 direction = nextPosition - currentPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle + imageRotation);
    }
}
