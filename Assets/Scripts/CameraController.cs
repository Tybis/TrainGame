/*
CameraController.cs
Author: Christian Mullins
Date: 6/19/2021
Summary: Controls movement of the camera
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class CameraController : MonoBehaviour {
    public Transform target;
    [Tooltip("Only effective if KeepDistanceFromTarget is checked(true).")]
    [Range(5f, 15f)]
    public float trackingDistance;
    [Range(10f, 30f)]
    public float rotationSpeed;

    //constraint bools
    [Header("Constraints")]
    //public bool freezeHorizontalAxis;
    public bool keepDistanceFromTarget;
    
    private bool _isRotating = false;
    private Camera _myCam;
    //constraining components
    private LookAtConstraint _lookAtConstraint;
    private PositionConstraint _positionConstraint;

    private Vector3 _rightRotateTarget => target.position + (transform.right * trackingDistance);
    private Vector3 _leftRotateTarget => target.position - (transform.right * trackingDistance);
    private Direction currentSide { get {
        return (_isRotating) ? Direction.NULL : _GetLongestAxis(transform.position - target.position);
    } }

    private enum Direction {
        PosX = 1, NegX = -1, PosY = 2, NegY = -2, PosZ = 3, NegZ = -3, NULL = 0
    };

    private void Start() {
        _positionConstraint = gameObject.AddComponent<PositionConstraint>();
        _lookAtConstraint = gameObject.AddComponent<LookAtConstraint>();
        ConstraintSource newSource = new ConstraintSource();
        newSource.weight = 1;
        newSource.sourceTransform = target;
        _lookAtConstraint.AddSource(newSource);
        _lookAtConstraint.constraintActive = true;
        _positionConstraint.AddSource(newSource);
        _positionConstraint.translationOffset = -transform.forward * trackingDistance;
        if (keepDistanceFromTarget)
            _positionConstraint.translationAxis = Axis.Z | Axis.X;
        else
            _positionConstraint.translationAxis = _GetAxisFrom(transform.right);
        _positionConstraint.constraintActive = true;
    }

    private void Update() {
        if (_isRotating) return;

        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            StartCoroutine(RotateCamera("left"));
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            StartCoroutine(RotateCamera("right"));
        #endif
    }

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
        _SetSourceConstraintWeights(1f / _lookAtConstraint.sourceCount);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="focalPoint"></param>
    public void DeleteFocalPoint(Transform focalPoint) {
        for (int i = 0; i < _lookAtConstraint.sourceCount; ++i)
            if (_lookAtConstraint.GetSource(i).sourceTransform.Equals(focalPoint))
                _lookAtConstraint.RemoveSource(i);
        _SetSourceConstraintWeights(1f / _lookAtConstraint.sourceCount);
    }

    /// <summary>
    /// Animated version of snapping to a given position
    /// </summary>
    /// <param name="directionStr">String describing direction (left or right)</param>
    public IEnumerator RotateCamera(string directionStr) {
        Vector3 targetPosition;
        if (directionStr.Equals("right", StringComparison.InvariantCultureIgnoreCase))
            targetPosition = _rightRotateTarget;
        else if (directionStr.Equals("left", StringComparison.InvariantCultureIgnoreCase))
            targetPosition = _leftRotateTarget;
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

        if (!keepDistanceFromTarget) {
            var axis = _positionConstraint.translationAxis;
            axis = (axis == Axis.X) ? Axis.Z : Axis.X;
            _positionConstraint.translationAxis = axis;
        }
        _positionConstraint.translationOffset = transform.position - target.position;
        _positionConstraint.constraintActive = true;
        _isRotating = false;
    }

    /// <summary>
    /// Use in Editor instead of the Coroutine as they're not supported
    /// in Editor.
    /// </summary>
    /// <param name="directionStr">String describing direction (left or right)</param>
    public void SnapRotateCamera(string directionStr) {
        Vector3 targetPosition;
        if (directionStr.Equals("right", StringComparison.InvariantCultureIgnoreCase))
            targetPosition = _rightRotateTarget;
        else if (directionStr.Equals("left", StringComparison.InvariantCultureIgnoreCase))
            targetPosition = _leftRotateTarget;
        else
            throw new ArgumentException("{} is not a valid direction.", directionStr);
        transform.position = targetPosition;
        transform.LookAt(target.position, transform.up);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newWeight"></param>
    private void _SetSourceConstraintWeights(in float newWeight) {
        for (int i = 0; i < _lookAtConstraint.sourceCount; ++i) {
            var sourceI = _lookAtConstraint.GetSource(i);
            sourceI.weight = newWeight;
            _lookAtConstraint.SetSource(i, sourceI);
        }
    }

    /// <summary>
    /// Take in Vector3 and output relative Axis enum.
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
}