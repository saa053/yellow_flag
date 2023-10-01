using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] SplineContainer splineContainer;
    Spline optimal;

    [SerializeField] float speed = 1f;
    [SerializeField] float turnThreshold;

    float distancePercentage = 0f;

    float splineLength;


    // Start is called before the first frame update
    void Start() {
        optimal = splineContainer.Splines[0];

        //splineLength = optimal.CalculateLength();
        splineLength = splineContainer.CalculateLength(0);
    }

    // Update is called once per frame
    void Update() {
        distancePercentage += speed * Time.deltaTime / splineLength;
        
        Vector3 currentPosition = splineContainer.EvaluatePosition(distancePercentage);
        transform.position = new Vector3(currentPosition.x, currentPosition.y, 0);

        if (distancePercentage > 1f) {
            distancePercentage = 0f;
        }

        Vector3 nextPosition = splineContainer.EvaluatePosition(distancePercentage + turnThreshold);
        Vector3 direction = nextPosition - currentPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Set the rotation only along the Z-axis
        transform.rotation = Quaternion.Euler(0, 0, angle);


    }
}
