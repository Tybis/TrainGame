/*
TrainDoor.cs
Author: Christian Mullins
Date: 6/17/21
Summary: Script object to handle door transitions.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using AsyncOperation = UnityEngine.AsyncOperation;

public class TrainDoor : MonoBehaviour {
    public string nextCarScene => _nextCarScene;
    [SerializeField]
    private string _nextCarScene;

    public void GoToNextCar(in GameObject curPlayer) {
        if (String.IsNullOrEmpty(_nextCarScene)) return;
        GetComponent<BoxCollider>().enabled = false;
        curPlayer.GetComponent<Rigidbody>().isKinematic = true;
        StartCoroutine(_MigrateScene(curPlayer));
    }

    private IEnumerator _MigrateScene(GameObject curPlayer) {
        // gather prefabs that will move to the new scene
        Camera curCam = curPlayer.GetComponent<PlayerController>().camera;
        var sE = curCam.GetComponent<SceneEffects>();
        yield return StartCoroutine(sE.StartFadeTransition());
        var loadOp = SceneManager.LoadSceneAsync(_nextCarScene, LoadSceneMode.Additive);
        yield return new WaitUntil(() => loadOp.isDone);
        var newScene = SceneManager.GetSceneByName(_nextCarScene);
        SceneManager.SetActiveScene(newScene);
        yield return new WaitUntil(() => curPlayer.scene.Equals(newScene));
        // find new doors
        var doors = new List<GameObject>(GameObject.FindGameObjectsWithTag("Door"));
        doors = doors.FindAll(d => d.scene.Equals(newScene));
        var nextDoor = doors.Find(d => d.GetComponent<TrainDoor>().nextCarScene == gameObject.scene.name);
        var farDoor = doors.Find(d => d != nextDoor && d.scene == nextDoor.scene);
        // get spawn location
        Vector3 spawnPos = Vector3.MoveTowards(
            nextDoor.transform.position, 
            farDoor.transform.position, 2.25f);
        spawnPos.y = curPlayer.transform.position.y;
        curPlayer.transform.position = spawnPos;
        // unload old scene
        var unloadOp = SceneManager.UnloadSceneAsync(gameObject.scene);
        StartCoroutine(sE.EndFadeTransition(unloadOp));
        unloadOp.completed += delegate {
            curPlayer.GetComponent<Collider>().enabled = true;
            curPlayer.GetComponent<Rigidbody>().isKinematic = false;
        };
    }
}
