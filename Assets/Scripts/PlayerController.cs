/*
PlayerController.cs
Author: Christian Mullins
Date: 06/15/21
Summary: Script that handles input that effects Player movement.
*/
using System.Collections;
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
    
    private void Start() {
        isMovingScenes = false;
        isIdleState = true;
        _curHintObj = null;
        isMovingScenes = false;

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
                isMovingScenes = false; //**********
            }
        };
    }

    //handle all player mouse input here
    private void Update() {
        #if UNITY_EDITOR
        if (isIdleState) {
            if (Input.GetKey(KeyCode.LeftArrow))
                StartCoroutine(MoveTo(transform.position - camera.transform.right * moveSpeed));
            else if (Input.GetKey(KeyCode.RightArrow))
                StartCoroutine(MoveTo(transform.position + camera.transform.right * moveSpeed));
        }
        #endif

        if (camera == null) return; // avoid error on scene change

        var ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(camera.transform.position, ray.direction, out hit)) {
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
            if (isMovingScenes)
                break;
        }
        isIdleState = true;
    }

    public void InteractWith(GameObject interacting) {

    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Door") && !isMovingScenes) {
            other.GetComponent<TrainDoor>().GoToNextCar(gameObject);
            GetComponent<CapsuleCollider>().enabled = false;
            isMovingScenes = true;
        }
    }
/*
    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Door"))
            isMovingScenes = false;
    }
*/

}