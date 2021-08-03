/*
CameraController.cs
Author: Christian Mullins
Date: 6/19/2021
Summary: Controls movement of the camera.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class CameraController : MonoBehaviour {

    public Transform target;
    [Range(5f, 15f)]  [Tooltip("Distance from target when looking from the side car view.")]
    public float sideTracking;
    [Range(1f, 5f)]   [Tooltip("Distance from target when looking from inside hallway.")]
    public float hallwayTracking;
    [Range(-3f, 3f)]  [Tooltip("Distance from level Y-Axis of target.")]
    public float trackingHeight;
    [Range(10f, 30f)] [Tooltip("Speed of animation (if used).")]
    public float rotationSpeed;

    private bool _isRotating = false;
    private Camera _myCam;
    //constraining components
    private LookAtConstraint _lookAtConstraint;
    private PositionConstraint _positionConstraint;

    public float deltaTracking  => (isWalkwayView) ? hallwayTracking : sideTracking;
    public bool isWalkwayView   => currentAxis.Equals(_GetHallwayAxis());
    public Axis currentAxis     => _GetAxisFrom(transform.forward);

    private enum Direction {
        PosX = 1, NegX = -1, PosY = 2, NegY = -2, PosZ = 3, NegZ = -3, NULL = 0
    };

    private void OnEnable() {
        _positionConstraint = GetComponent<PositionConstraint>();
        _lookAtConstraint = GetComponent<LookAtConstraint>();
        var newSource = new ConstraintSource();
        newSource.weight = 1;
        newSource.sourceTransform = target;
        _lookAtConstraint.AddSource(newSource);
        _lookAtConstraint.constraintActive = true;
        _positionConstraint.AddSource(newSource);
        _positionConstraint.translationOffset = -transform.forward * deltaTracking;
        _positionConstraint.constraintActive = true;
        
    }

    // currently all initialization is necessary in Editor so Start isn't necessary yet
    //private void Start() {}

    
    #region LookAtConstraint_Functions
    /// <summary>
    /// 
    /// </summary>
    /// <param name="focalPoint"></param>
    public void AddNewFocalPoint(Transform focalPoint) {
        for (int i = 0; i < _lookAtConstraint.sourceCount; ++i) {
            if (_lookAtConstraint.GetSource(i).sourceTransform.Equals(focalPoint)) {
                Debug.LogWarningFormat("{} is a already a SourceConstraint!", focalPoint.name);
                return;
            }
        }
        var newSource = new ConstraintSource();
        newSource.sourceTransform = focalPoint;
        _lookAtConstraint.AddSource(newSource);
        _SetFocalConstraintWeights (1f / _lookAtConstraint.sourceCount);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="focalPoint"></param>
    public void DeleteFocalPoint(Transform focalPoint) {
        for (int i = 0; i < _lookAtConstraint.sourceCount; ++i)
            if (_lookAtConstraint.GetSource(i).sourceTransform.Equals(focalPoint))
                _lookAtConstraint.RemoveSource(i);
        _SetFocalConstraintWeights(1f / _lookAtConstraint.sourceCount);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newWeight"></param>
    private void _SetFocalConstraintWeights(in float newWeight) {
        for (int i = 0; i < _lookAtConstraint.sourceCount; ++i) {
            var sourceI = _lookAtConstraint.GetSource(i);
            sourceI.weight = newWeight;
            _lookAtConstraint.SetSource(i, sourceI);
        }
    }
    #endregion

    #region PositionConstraint_Functions
    public void AddNewPositionConstraint(Transform newConstraint) {
        for (int i = 0; i < _positionConstraint.sourceCount; ++i) {
            if (_positionConstraint.GetSource(i).sourceTransform.Equals(newConstraint)) {
                Debug.LogWarning(newConstraint.name + " is a already a SourceConstraint!");
            }
        }
        var newSource = new ConstraintSource();
        newSource.sourceTransform = newConstraint;
        _positionConstraint.AddSource(newSource);
        _SetFocalConstraintWeights (1f / _positionConstraint.sourceCount);
    }
    public void DeletePositionConstraint(Transform deleting) {
        for (int i = 0; i < _positionConstraint.sourceCount; ++i) {
            if (_positionConstraint.GetSource(i).sourceTransform.Equals(deleting))
                _positionConstraint.RemoveSource(i);
        }
        _SetPositionalConstraintWeights(1f / _positionConstraint.sourceCount);
    }
    private void _SetTranslationAxis(in bool isSideCarView) {
        //get axis of player and cam
        var myAxis = _GetAxisFrom(transform.forward);
        var otherAxis = _GetAxisFrom(transform.right);
        _positionConstraint.translationAxis = (isSideCarView)? myAxis : otherAxis;
    }
    private void _SetPositionalConstraintWeights(in float newWeight) {
        for (int i = 0; i < _positionConstraint.sourceCount; ++i) {
            var sourceI = _positionConstraint.GetSource(i);
            sourceI.weight = newWeight;
            _positionConstraint.SetSource(i, sourceI);
        }
    }
    #endregion
    
    public void ClearAllConstraints() {
        for (int i = 0; i < _positionConstraint.sourceCount; ++i) {
            _positionConstraint.RemoveSource(i);
        }
        for (int i = 0; i < _lookAtConstraint.sourceCount; ++i) {
            _lookAtConstraint.RemoveSource(i);
        }
    }

    /// <summary>
    /// Animated version of snapping to a given position
    /// </summary>
    /// <param name="directionStr">String describing direction (left or right)</param>
    public IEnumerator RotateCamera(string directionStr) {
        Vector3 targetPosition;
        if (directionStr.Equals("right", StringComparison.InvariantCultureIgnoreCase))
            targetPosition = _GetTargetPosition("right");
        else if (directionStr.Equals("left", StringComparison.InvariantCultureIgnoreCase))
            targetPosition = _GetTargetPosition("left");
        else
            throw new ArgumentException("{} is not a valid direction.", directionStr);

        _isRotating = true;
        var moveVector = (targetPosition - transform.position).normalized;
        float goalDistance = Vector3.Distance(transform.position, targetPosition);
        float traveled = 0f;
        _positionConstraint.constraintActive = false;
        _positionConstraint.locked = false;
        do {
            transform.position += moveVector * rotationSpeed * Time.deltaTime;
            traveled += moveVector.magnitude * rotationSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        } while (traveled < goalDistance);

        //_positionConstraint.translationAxis = _GetAxisFrom(transform.right);
        _positionConstraint.translationOffset = transform.position - target.position;
        _positionConstraint.constraintActive = true;
        _isRotating = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void RefreshTrackingValues() {
        float tracking = (isWalkwayView) ? hallwayTracking : sideTracking;
        transform.position = (target.position - (transform.forward * tracking)) + (transform.up.normalized * trackingHeight);
        //_positionConstraint ??= GetComponent<PositionConstraint>();
        if (_positionConstraint != null) {
            _positionConstraint.translationOffset = transform.position - target.position;
        }
    }

    /// <summary>
    /// Use in Editor instead of the Coroutine as they're not supported
    /// in Editor.
    /// </summary>
    /// <param name="directionStr">String describing direction (left or right)</param>
    public void SnapRotateCamera(string directionStr) {
        if (Application.isPlaying) {
            _positionConstraint.locked = false;
            _positionConstraint.constraintActive = false;
        }

        Vector3 targetPosition;
        if (directionStr.Equals("right", StringComparison.InvariantCultureIgnoreCase))
            targetPosition = _GetTargetPosition("right");
        else if (directionStr.Equals("left", StringComparison.InvariantCultureIgnoreCase))
            targetPosition = _GetTargetPosition("left");
        else
            throw new ArgumentException(directionStr + " is not a valid direction.");
        
        transform.position = targetPosition;
        transform.LookAt(target.position, transform.up);
        if (Application.isPlaying) {
            _positionConstraint.translationOffset = transform.position - target.position;
            _positionConstraint.locked = true;
            _positionConstraint.constraintActive = true;
        }
    }

    private Vector3 _GetTargetPosition(in string dirStr) {
        if (dirStr != "right" && dirStr != "left")
            Debug.LogError(dirStr + " is not a valid string direction.");
        float targetTracking = (!isWalkwayView) ? hallwayTracking : sideTracking;
        var direction = (dirStr == "right") ? transform.right : -transform.right;
        return target.position + (direction * targetTracking);
    }

    #region UtilityFunctions
    /// <summary>
    /// Take in normalized Vector3 and output relative Axis enum.
    /// </summary>
    /// <param name="direction">Vector3 of normalized direction.</param>
    /// <returns>Axis enum</returns>
    private Axis _GetAxisFrom(in Vector3 direction) {
        var vecAxis = transform.right;
        int index = -1;
        float greatestAxis = 0f;
        if (Mathf.Abs(direction[0]) > greatestAxis) {
            index = 0;
            greatestAxis = Mathf.Abs(direction[0]);
        }
        if (Mathf.Abs(direction[2]) > greatestAxis) {
            index = 2;
            greatestAxis = Mathf.Abs(direction[2]);
        }
        return (index == 0) ? Axis.X : Axis.Z;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="checking"></param>
    /// <returns></returns>
    private static Direction _GetLongestAxis(in Vector3 checking) {
        int index = -2;
        float greatestAxis = 0f;
        for (int i = 0; i < 3; ++i) {
            if (Mathf.Abs(checking[i]) > Mathf.Abs(greatestAxis)) {
                index = i;
                greatestAxis = checking[i];
            }
        }
        //adjust index to match enum value
        ++index;
        return (Direction)(index * (Mathf.Abs(greatestAxis)/greatestAxis));
    }

    /// <summary>
    /// Calculate the axis of the hallway based on the door set.
    /// </summary>
    /// <returns>Direction of the Hallway as Direction.</returns>
    private Axis _GetHallwayAxis() {
        var doorList = GameObject.FindGameObjectsWithTag("Door");
        var dirList = new List<Direction>();
        foreach (var door in doorList)
            dirList.Add(_GetLongestAxis(door.transform.forward)); // adjust es no bueno
        //keeps getting called as False (Direction.NULL)
        var outDir = (dirList.TrueForAll(d => d == dirList[0])) ? dirList[0] : Direction.NULL;
        return (Mathf.Abs((int)outDir) == 1) ? Axis.X : Axis.Z;
    }
    #endregion
}