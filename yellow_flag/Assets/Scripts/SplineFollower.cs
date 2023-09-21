using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] RacingLines racingLines;
    Spline optimal;

    [SerializeField] float speed = 1f;

    float distancePercentage = 0f;

    float splineLength;


    // Start is called before the first frame update
    /* void Start() {
        SplineContainer splineContainer = racingLines.GetComponent<SplineContainer>();
        optimal = splineContainer.Splines[0];

        splineLength = optimal.CalculateLength();
    }

    // Update is called once per frame
    void Update() {
        distancePercentage += speed * Time.deltaTime / splineLength;
        
        Vector3 currentPosition = optimal.EvaluatePosition(distancePercentage);
        transform.position = currentPosition;

        if (distancePercentage > 1f) {
            distancePercentage = 0f;
        }

        Vector3 nextPosition = optimal.EvaluatePosition(distancePercentage + 0.05f);
        Vector3 direction = nextPosition - currentPosition;
        transform.rotation = Quaternion.LookRotation(direction, transform.up);

    } */
}
