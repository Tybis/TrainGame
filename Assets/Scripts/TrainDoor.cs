/*
TrainDoor.cs
Author: Christian Mullins
Date: 6/17/21
Summary: Script object to handle door transitions.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainDoor : MonoBehaviour {
    public string nextCarScene => _nextCarScene;
    [SerializeField]
    private string _nextCarScene;

    private void Start() {
        if (!_IsValidScene(_nextCarScene) && !String.IsNullOrEmpty(_nextCarScene)) {
            Debug.LogWarning(_nextCarScene + " was not found to be a valid scene.");
        }
    }

    public void GoToNextCar(in GameObject curPlayer) {
        if (String.IsNullOrEmpty(_nextCarScene)) return;
        GetComponent<BoxCollider>().enabled = false;
        curPlayer.GetComponent<Rigidbody>().isKinematic = true;
        StartCoroutine(_MigrateScene(curPlayer));
    }

    /// <summary>
    /// Move player prefabs over to next scene and orient things correctly for new prefab.
    /// </summary>
    /// <param name="curPlayer">Current Player prefab in use.</param>
    private IEnumerator _MigrateScene(GameObject curPlayer) {
        // gather prefabs that will move to the new scene and begin effects
        Camera curCam = curPlayer.GetComponent<PlayerController>().camera;
        var sEffects = curPlayer.GetComponentInChildren<SceneEffects>();
        yield return StartCoroutine(sEffects.StartFadeTransition());
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
        StartCoroutine(sEffects.EndFadeTransition(unloadOp));
        unloadOp.completed += delegate {
            curPlayer.GetComponent<Collider>().enabled = true;
            curPlayer.GetComponent<Rigidbody>().isKinematic = false;
        };
    }

    /// <summary>
    /// Checks if scene name is contained in Build Settings.
    /// </summary>
    /// <param name="sceneName">Name of scene.</param>
    /// <returns>Bool is the scene in parameter is in Build Settings.</returns>
    private bool _IsValidScene(in string sceneName) {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; ++i) {
            string searchScene = SceneUtility.GetScenePathByBuildIndex(i);
            searchScene = searchScene.TrimEnd(".unity".ToCharArray());
            int lastSlash = searchScene.LastIndexOf('/');
            searchScene = searchScene.Substring(lastSlash + 1);
            
            if (searchScene == sceneName) return true;
        }
        return false;
    }
}
