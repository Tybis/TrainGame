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

    /* player caused actions */
    private Coroutine _moveCoroutine;
    private HintableObject _curHintObj;
    //use Dictionary<Coroutine> to handle various coroutine outputs

    private void Start() {
        _camera = Camera.main;
        isIdleState = true;
        _curHintObj = null;
    }

    //handle all player mouse input here
    private void Update() {
        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(_camera.transform.position, ray.direction, out hit)) {
            if (hit.transform.gameObject.layer == HintableObject.HINT_LAYER) {
                if (_curHintObj == null || _curHintObj != hit.transform.GetComponent<HintableObject>()) {
                    _curHintObj = hit.transform.GetComponent<HintableObject>();
                    _curHintObj.SetHint();
                }
                goto skipReset;
            }
        }

        //reset values unless "goto" jump is called
        if (_curHintObj != null) { 
            _curHintObj.ResetObject();
            _curHintObj = null;
        }

        skipReset: {}
        if (!Input.GetMouseButtonDown(0) || hit.Equals(null)) return;

        switch (hit.transform?.tag) {
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

    /// <summary>
    /// Public coroutine for player movement.
    /// </summary>
    /// <param name="newPosition">Vector3 of desired destination.</param>
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

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Door")) {
            other.GetComponent<TrainDoor>().GoToNextCar();
        }
    }

}