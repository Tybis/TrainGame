/*
Author: Christian Mullins
Date: 06/15/21
Summary: Script that handles input that effects Player movement.
*/
using System.Collections;
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
                //future implementations below
                case "Object":
                    break;
                case "NPC":
                    break;
                default: break;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newPosition"></param>
    /// <returns></returns>
    public IEnumerator MoveTo(Vector3 newPosition) {
        newPosition.y = transform.position.y;
        var direction = Vector3.Normalize(newPosition - transform.position);
        float travelDistance = 0f;
        float distanceToTravel = Vector3.Distance(newPosition, transform.position);
        while (travelDistance <= distanceToTravel) {
            isIdleState = false;
            var movement = direction * moveSpeed * Time.deltaTime;
            travelDistance += movement.magnitude;
            transform.position += movement;
            yield return new WaitForEndOfFrame();
        }
        isIdleState = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Door")) {
            other.GetComponent<TrainDoor>().GoToNextCar();
        }
    }

}