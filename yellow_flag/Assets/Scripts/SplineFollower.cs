using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] TrackBuilder trackBuilder;

    // Movement variables
    [SerializeField] float speed;
    [SerializeField] float startingPoint;
    [SerializeField] float turnThreshold;
    [SerializeField] float imageRotation;
    [SerializeField] float rotationSpeed;
    public float distance; // Percentage travelled of line

    // Brake and accelerate variables
    [SerializeField] float breakPower;
    [SerializeField] float acceleratePower;

    // Variables for handling line switching
    [SerializeField] TrackBuilder.RacingLine currentIndex;
    [SerializeField] TrackBuilder.RacingLine nextIndex;
    [SerializeField] float switchOffset;
    [SerializeField] float maxAngle;
    [SerializeField] float maxLength;
    [SerializeField] float minLength;
    float targetDistance; // Percentage travelled of target line. Used to determine where the car will end up when switching lines

    // Splines
    SplineContainer splineContainer;
    Spline currentLine;
    Spline nextLine;
    Spline switchLine;

    // Variable for checking if track has been initialized
    bool init = false;
    bool isStartPositionSet = false;

    void Start() {
        StartCoroutine(InitializeAfterBuild());

        splineContainer = trackBuilder.GetComponent<SplineContainer>();

        // Check if there are enough splines for the cars
        int index = transform.GetSiblingIndex() + 3;
        if (index >= splineContainer.Splines.Count){
            Debug.Log("ERROR: There are more cars than splines (No more switchLine splines left to use)");
            return;
        }

        distance = startingPoint;

        // Initialize splines
        currentLine = splineContainer.Splines[(int)currentIndex];
        nextLine = currentLine;
        switchLine = splineContainer.Splines[transform.GetSiblingIndex() + 3];

        init = true;
    }

    void Update() {
        if (!init)
            return;
        
        if (!isStartPositionSet)
            setStartPosition(currentLine);

        
        Move(currentLine);

        BreakAndAccelerate(currentLine);

        nextLine = splineContainer.Splines[(int)nextIndex];
        if (currentLine != nextLine){
            HandleSwitching();
        }
    }

    // Moves and updates the cars position to follow the line given
    void Move(Spline spline) {
        // Update position
        distance += speed * Time.deltaTime / spline.GetLength();
        distance %= 1f;
        Vector3 currentPosition = spline.EvaluatePosition(distance);
        transform.position = new Vector3(currentPosition.x, currentPosition.y, 0);

        // Rotate smoothly
        Vector3 nextPosition = spline.EvaluatePosition(distance + turnThreshold);
        Vector3 direction = nextPosition - currentPosition;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + imageRotation;
        float angleChange = Mathf.DeltaAngle(transform.rotation.eulerAngles.z, targetAngle);
        angleChange = Mathf.Clamp(angleChange, -rotationSpeed, rotationSpeed);
        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + angleChange);
    }

    void BreakAndAccelerate(Spline spline) {

        /* for (int i = currentApex; i < trackBuilder.apexes.Count; i++) {
            TrackBuilder.Apex apex = trackBuilder.apexes[i];

            float ukjentVaribel = spline.ConvertIndexUnit(apex.knotIndex, PathIndexUnit.Knot, PathIndexUnit.Distance);
            float apexDistance = ukjentVaribel / spline.GetLength();
            float distanceToBreakPoint = apexDistance - distance;

            // Calculate brakingPoint
        } */


        /* foreach (TrackBuilder.Apex apex in trackBuilder.apexes) {
            float ukjentVaribel = spline.ConvertIndexUnit(apex.knotIndex, PathIndexUnit.Knot, PathIndexUnit.Distance);
            float apexDistance = ukjentVaribel / spline.GetLength();
            float distanceToBreakPoint = apexDistance - distance;

            if (distanceToBreakPoint <= breakPoint && distanceToBreakPoint > 0) {
                speed -= breakPower;
                Debug.Log("BRAKING");
            } else if (distanceToBreakPoint > -breakPoint && distanceToBreakPoint <= 0) {
                speed += acceleratePower;
                Debug.Log("ACCELERATING");
            }
        } */


    }

    // Moves the car smoothly from currentLine to nextLine
    void HandleSwitching() {
        if (currentLine == switchLine) {
            // Check if car is at the end of switchLine (Since spline is set to loop, the end is 0.5f)
            if (distance > 0.5f){
                // Clear switchLine and set to follow the spline switching to
                currentLine = nextLine;
                switchLine.Clear();
                distance = targetDistance;
                currentIndex = nextIndex;
            }
        } else {
            bool res = UpdateTargetDistance();
            if (!res)
                return;

            CreateSwitchLine();
            distance = 0;
            currentLine = switchLine;
        }
    }

    // Calculates the targetDistance for the line switch
    bool UpdateTargetDistance() {
        Vector3 nextPosition = currentLine.EvaluatePosition(distance + turnThreshold);
        Vector2 direction = nextPosition - transform.position;
        Vector2 directionToTarget;

        int failSafe = 0;
        targetDistance = distance;
        
        // Adds an offset to targetDistance until the angle and length the car will have to drive to switch lines is acceptable (maxAngle, minLength). Fails if targetDistance is far away from current position
        for (Vector3 targetPos = nextLine.EvaluatePosition(distance); Vector2.Distance(targetPos, transform.position) < maxLength; targetPos = nextLine.EvaluatePosition(targetDistance)) {
            directionToTarget = targetPos - transform.position;

            // Calculate switch line angle where 0 = direction
            float angle = Vector3.Angle(direction, directionToTarget);
            float sign = Mathf.Sign(Vector3.Dot(Vector3.Cross(direction, directionToTarget), Vector3.up));
            angle *= sign;

            // Switch line if angle and length of switchLine is acceptable
            if (angle < maxAngle && Vector2.Distance(targetPos, transform.position) >= minLength)
                return true;

            // Add offset to target to create a better angle next loop
            targetDistance = targetDistance + switchOffset;

            // Error handling
            failSafe++;
            if (failSafe > 100) {
                nextLine = currentLine;
                nextIndex = currentIndex;
                Debug.Log("SwitchLine stuck in loop");
                return false;
            }
        }

        nextLine = currentLine;
        nextIndex = currentIndex;

        Debug.Log("Can't switch");
        return false;
    }

    // Creates a line from current position to targetDistance on the line its switching to
    void CreateSwitchLine() {
        Vector3 startPos = currentLine.EvaluatePosition(distance);
        Vector3 targetPos = nextLine.EvaluatePosition(targetDistance);
        Vector3 halfwayPos = Vector3.Lerp(transform.position, targetPos, 0.5f);
        
        Vector3 knot1 = Vector3.Lerp(
            currentLine.EvaluatePosition(distance + switchOffset / 3), 
            halfwayPos, 
            0.1f);

        Vector3 knot2 = Vector3.Lerp(
            currentLine.EvaluatePosition(distance + switchOffset / 3), 
            halfwayPos, 
            0.5f);

        Vector3 knot4 = Vector3.Lerp(
            nextLine.EvaluatePosition(targetDistance - switchOffset / 3), 
            halfwayPos, 
            0.5f);

        Vector3 knot5 = Vector3.Lerp(
            nextLine.EvaluatePosition(targetDistance - switchOffset / 3), 
            halfwayPos, 
            0.1f);
        
        AddKnot(startPos, false);
        AddKnot(knot1, true);
        AddKnot(knot2, true);
        AddKnot(halfwayPos, true);
        AddKnot(knot4, true);
        AddKnot(knot5, true);
        AddKnot(targetPos, false);
    }

    // Helper function for creating and adding BezierKnot to switchLine
    void AddKnot(Vector3 pos, bool auto) {
        TangentMode tangentMode;
        if (auto)
            tangentMode = TangentMode.AutoSmooth;
        else {
            tangentMode = TangentMode.Linear;
        }

        switchLine.Add(new BezierKnot(pos, 0, 0, Quaternion.Euler(-90, 0, 0)), tangentMode);
    }

    // Sets the transform to start position and rotation
    void setStartPosition(Spline spline) {
        // Move to grid
        // TODO

        // Rotate
        Vector3 currentPosition = spline.EvaluatePosition(distance);
        Vector3 nextPosition = spline.EvaluatePosition(distance + turnThreshold);
        Vector3 direction = nextPosition - currentPosition;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + imageRotation;
        transform.rotation = Quaternion.Euler(0, 0, targetAngle);

        isStartPositionSet = true;
    }

    // Waits until the track is completed before continuing
    private IEnumerator InitializeAfterBuild() {
    while (!trackBuilder.buildCompleted)
        yield return null;
    }
}
