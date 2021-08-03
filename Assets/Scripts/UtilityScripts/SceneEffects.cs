/*
SceneEffects.cs
Author: Christian Mullins
Date: 7/5/21
Summary: Handles all miscellaneous scene effects for the UI.
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using AsyncOperation = UnityEngine.AsyncOperation;

public class SceneEffects : MonoBehaviour {
    public Image fadeOutImage;
    [SerializeField]
    [Range(0.5f, 3f)]
    private float _fadeTimer = 1f;

    private IEnumerator Start() {
        fadeOutImage.enabled = true;
        yield return StartCoroutine(_Fade(false));
        fadeOutImage.enabled = false;
    }

    /// <summary>
    /// Initialize the image fade in.
    /// </summary>
    public IEnumerator StartFadeTransition() {
        fadeOutImage.enabled = true;
        //make image black and force alpha to max transparency
        fadeOutImage.color = new Color(0f, 0f, 0f, 0f);
        //fade image in
        yield return StartCoroutine(_Fade(true));
    }

    /// <summary>
    /// Initialize image fade out and sync with load operations.
    /// </summary>
    /// <param name="unloadOp">Current unload operation.</param>
    public IEnumerator EndFadeTransition(AsyncOperation unloadOp) {
        yield return new WaitUntil(() => unloadOp.isDone); // caught here??
        //wait until image is done fading
        yield return StartCoroutine(_Fade(false));
        fadeOutImage.enabled = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    public IEnumerator FadeUntil(Action action) {
        yield return StartCoroutine(StartFadeTransition());
        action();
        yield return StartCoroutine(_Fade(false));
        fadeOutImage.enabled = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    private IEnumerator _Fade(bool active) {
        float endAlpha = (active) ? 1f : 0f;
        //TODO: alter the alphaDelta to be a more fitting value once this gets running
        float alphaDelta = (active) ? Time.deltaTime : -Time.deltaTime;
        //alphaDelta *= (1f / _fadeTimer);
        //print("ferp: " + (Time.deltaTime / 1f).ToString());
        do {
            var alphaChange = fadeOutImage.color;
            alphaChange.a = Mathf.Clamp(alphaDelta + alphaChange.a, 0f, 1f);
            yield return new WaitForEndOfFrame();
            fadeOutImage.color = alphaChange;
        } while (fadeOutImage.color.a != endAlpha);
        //total time spent in coroutine should equal _fadeTimer
    }
}