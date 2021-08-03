/*
PlayerController.cs
Author: Christian Mullins
Date: 06/15/21
Summary: Script that handles input that effects Player movement.
*/
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {
    public float moveSpeed;
    public bool isIdleState    { get; private set; }
    public bool isMovingScenes { get; private set; }
    public new Camera camera;

    /* player caused actions */
    private Coroutine _moveCoroutine;
    private HintableObject _curHintObj;
    
    private CameraController _camController;
    private SceneEffects _sceneFX;
    private EventSystem _eventSys;
    
    private void Start() {
        _eventSys = EventSystem.current;
        isMovingScenes = false;
        isIdleState = true;
        _curHintObj = null;
        isMovingScenes = false;
        _camController = camera.GetComponent<CameraController>();
        _sceneFX = GetComponentInChildren<SceneEffects>();

        SceneManager.activeSceneChanged += delegate(Scene oldScene, Scene newScene) {
            if (this == null) return;
            isMovingScenes = true;
            if (gameObject.scene == newScene) {
                Destroy(camera.gameObject);
                Destroy(gameObject);
            }
            if (gameObject.scene == oldScene) {
                SceneManager.MoveGameObjectToScene(gameObject, newScene);
                SceneManager.MoveGameObjectToScene(camera.gameObject, newScene);
                isMovingScenes = false;
            }
        };
    }

    //handle all player mouse input here
    private void Update() {

        if (camera == null) return; // avoid error on scene change
        bool resetObj = true;
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(camera.transform.position, ray.direction, out hit)) {
            if (hit.transform.gameObject.layer == HintableObject.HINT_LAYER) {
                if (_curHintObj == null || _curHintObj != hit.transform.GetComponent<HintableObject>()) {
                    _curHintObj = hit.transform.GetComponent<HintableObject>();
                    _curHintObj.SetHint();
                }
                resetObj = false;
            }
        }

        //reset values unless "goto" jump is called
        if (_curHintObj != null && resetObj) { 
            _curHintObj.ResetObject();
            _curHintObj = null;
        }

        if (!Input.GetMouseButtonDown(0) ||_eventSys.IsPointerOverGameObject()) return;

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
            transform.Translate(movement);
            yield return new WaitForEndOfFrame();
            if (isMovingScenes)
                break;
        }
        isIdleState = true;
    }

    /// <summary>
    /// Public function enabling camera snapping with effects.
    /// </summary>
    /// <param name="direction">"right" or "left"</param>
    public void RotateCamera(string direction) {
        StartCoroutine(_sceneFX.FadeUntil(delegate {
            _camController.SnapRotateCamera(direction);
        }));
    }

    public void InteractWith(GameObject interacting) {
        //placeholder for future implementation
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Door") && !isMovingScenes) {
            other.GetComponent<TrainDoor>().GoToNextCar(gameObject);
            GetComponent<CapsuleCollider>().enabled = false;
            isMovingScenes = true;
        }
    }
}