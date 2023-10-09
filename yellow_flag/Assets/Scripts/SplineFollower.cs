using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] TrackBuilder trackBuilder;
    [SerializeField] float speed;
    [SerializeField] float startingPoint;
    [SerializeField] float turnThreshold;
    [SerializeField] float imageRotation;

    [SerializeField] TrackBuilder.RacingLine currentIndex;
    [SerializeField] TrackBuilder.RacingLine nextIndex;
    [SerializeField] float switchOffset;
    [SerializeField] float maxAngle;
    [SerializeField] float maxLength;
    [SerializeField] float minLength;

    SplineContainer splineContainer;
    Spline currentLine;
    Spline nextLine;
    Spline switchLine;

    float distance;
    float targetDistance;
    bool init = false;

    void Start() {
        StartCoroutine(InitializeAfterBuild());

        splineContainer = trackBuilder.GetComponent<SplineContainer>();

        // Error handling
        int index = transform.GetSiblingIndex() + 3;
        if (index >= splineContainer.Splines.Count){
            Debug.Log("ERROR: There are more cars than splines (No more switchLine splines left to use)");
            return;
        }

        switchLine = splineContainer.Splines[transform.GetSiblingIndex() + 3];

        distance = startingPoint;

        currentLine = splineContainer.Splines[(int)currentIndex];
        nextLine = currentLine;

        init = true;
    }

    void Update() {
        if (!init)
            return;

        if (distance > 1f) {
            distance = 0f;
        }

        if (currentLine == switchLine && distance > 0.5f){
            currentLine = nextLine;
            switchLine.Clear();
            distance = targetDistance;

            currentIndex = nextIndex;
        }

        Move(currentLine);


        nextLine = splineContainer.Splines[(int)nextIndex];
        if (currentLine != nextLine && currentLine != switchLine){
            SwitchLine();
        }
    }

    void Move(Spline spline) {
        distance += speed * Time.deltaTime / spline.GetLength();
        Vector3 currentPosition = spline.EvaluatePosition(distance);
        transform.position = new Vector3(currentPosition.x, currentPosition.y, 0);

        Vector3 nextPosition = spline.EvaluatePosition(distance + turnThreshold);
        Vector3 direction = nextPosition - currentPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle + imageRotation);
    }

    void BreakAndAccelerate() {
        // Find distancePerentage of apexKnot
        //float distance = splineContainer.Splines[0].ConvertIndexUnit(trackBuilder.apexKnots[0], PathIndexUnit.Knot, PathIndexUnit.Distance);
        //float knotT = distance / splineLength;
    }

    void SwitchLine() {
/*         if (transform.position too close apex) {
            return;
        } */

        Vector3 nextPosition = currentLine.EvaluatePosition(distance + turnThreshold);
        Vector2 direction = nextPosition - transform.position;
        Vector2 direction2 = direction;
        int failSafe = 0;
        targetDistance = distance;
        
        for (Vector3 targetPos = nextLine.EvaluatePosition(distance); Vector2.Distance(targetPos, transform.position) < maxLength; targetPos = nextLine.EvaluatePosition(targetDistance)) {
            direction2 = targetPos - transform.position;

            float angle = Vector3.Angle(direction, direction2);
            float sign = Mathf.Sign(Vector3.Dot(Vector3.Cross(direction, direction2), Vector3.up));
            angle *= sign;

            if (angle < maxAngle && Vector2.Distance(targetPos, transform.position) >= minLength) {
                Debug.Log("Angle: " + angle + " = SWITCHING" + " | Length: " + Vector2.Distance(targetPos, transform.position));
                CreateSwitchLine();
                return;
            }

            Debug.Log("Angle: " + angle + " = Adding offset" + " | Length: " + Vector2.Distance(targetPos, transform.position));
            targetDistance = targetDistance + switchOffset;
            failSafe++;

            if (failSafe > 20) {
                nextLine = currentLine;
                nextIndex = currentIndex;
                return;
            }
        }

        nextLine = currentLine;
        nextIndex = currentIndex;

        Debug.Log("Aborting switch");
    }

    void CreateSwitchLine() {
        float currentLength = currentLine.GetLength();
        float nextLength = nextLine.GetLength();


        Vector3 knot1Pos = currentLine.EvaluatePosition(distance);
        BezierKnot knot1 = new BezierKnot(new Vector3(knot1Pos.x, knot1Pos.y), 0, 0, Quaternion.Euler(-90, 0, 0));
        
        Vector3 knot5Pos = nextLine.EvaluatePosition(targetDistance);
        BezierKnot knot5 = new BezierKnot(new Vector3(knot5Pos.x, knot5Pos.y), 0, 0, Quaternion.Euler(-90, 0, 0));

        Vector3 knot3Pos = Vector3.Lerp(transform.position, knot5Pos, 0.5f);
        BezierKnot knot3 = new BezierKnot(new Vector3(knot3Pos.x, knot3Pos.y), 0, 0, Quaternion.Euler(-90, 0, 0));

        Vector3 lerpPos = currentLine.EvaluatePosition(distance + switchOffset / 5);
        Vector3 knot2Pos = Vector3.Lerp(lerpPos, knot3Pos, 0.5f);
        BezierKnot knot2 = new BezierKnot(new Vector3(knot2Pos.x, knot2Pos.y), 0, 0, Quaternion.Euler(-90, 0, 0));

        lerpPos = nextLine.EvaluatePosition(targetDistance - switchOffset / 5);
        Vector3 knot4Pos = Vector3.Lerp(lerpPos, knot3Pos, 0.5f);
        BezierKnot knot4 = new BezierKnot(new Vector3(knot4Pos.x, knot4Pos.y), 0, 0, Quaternion.Euler(-90, 0, 0));

        lerpPos = nextLine.EvaluatePosition(targetDistance - switchOffset / 6);
        Vector3 knot6Pos = Vector3.Lerp(lerpPos, knot3Pos, 0.1f);
        BezierKnot knot6 = new BezierKnot(new Vector3(knot6Pos.x, knot6Pos.y), 0, 0, Quaternion.Euler(-90, 0, 0));

        lerpPos = currentLine.EvaluatePosition(distance + switchOffset / 6);
        Vector3 knot7Pos = Vector3.Lerp(lerpPos, knot3Pos, 0.1f);
        BezierKnot knot7 = new BezierKnot(new Vector3(knot7Pos.x, knot7Pos.y), 0, 0, Quaternion.Euler(-90, 0, 0));

        switchLine.Add(knot1, TangentMode.Linear);
        switchLine.Add(knot7, TangentMode.AutoSmooth);
        switchLine.Add(knot2, TangentMode.AutoSmooth);
        switchLine.Add(knot3, TangentMode.AutoSmooth);
        switchLine.Add(knot4, TangentMode.AutoSmooth);
        switchLine.Add(knot6, TangentMode.AutoSmooth);
        switchLine.Add(knot5, TangentMode.Linear);

        distance = 0f;
        currentLine = switchLine;
    }

    private IEnumerator InitializeAfterBuild() {
    while (!trackBuilder.buildCompleted)
        yield return null;
    }

    // Calculate the intersection point between two lines
    Vector3 CalculateIntersectionPoint(Vector3 p1, Vector3 d1, Vector3 p2, Vector3 d2)
    {
        Vector3 line1 = p1 - p2;
        float cross = Vector3.Cross(d1, d2).magnitude;

        if (cross < 1e-5f) // Lines are parallel
        {
            return Vector3.zero; // No intersection point
        }

        float t1 = Vector3.Cross(line1, d2).magnitude / cross;
        return p1 + d1 * t1;
    }
}
