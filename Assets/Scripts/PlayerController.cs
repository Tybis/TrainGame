/*
Author: Christian Mullins
Date: 06/15/21
Summary: Script that handles input that effects Player movement.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float moveSpeed;
    public bool isIdleState { get; private set; }
    private Camera _camera;

    private Coroutine _moveCoroutine;

    private void Start() {
        _camera = Camera.main;
        isIdleState = true;
    }

    private void Update() {
        if (!Input.GetMouseButtonDown(0)) return;

        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_camera.transform.position, ray.direction, out var hit)) {
            switch (hit.transform.tag) {
                case "Ground":
                    if (!isIdleState)
                        StopCoroutine(_moveCoroutine);
                    _moveCoroutine = StartCoroutine(MoveTo(hit.point));
                    break;
                default: break;
            }
        }
    }

    public IEnumerator MoveTo(Vector3 newPosition) {
        newPosition.y = transform.position.y;
        var direction = Vector3.Normalize(newPosition - transform.position);
        while (!Physics.CheckSphere(newPosition + direction + Vector3.up, 0.5f)) {
            isIdleState = false;
            transform.position += direction * moveSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        isIdleState = true;
    }
}