/*
Author: Christian Mullins
Date: 06/17/21
Summary: Simple script object to handle door transitions
*/
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainDoor : MonoBehaviour {
    public string nextCarScene => _nextCarScene;
    [SerializeField]
    private string _nextCarScene;

    public void GoToNextCar() {
        if (string.IsNullOrEmpty(_nextCarScene)) return;
        try {
            SceneManager.LoadScene(_nextCarScene, LoadSceneMode.Single);
        } catch (Exception e) {
            throw e;
        }
    }
}
