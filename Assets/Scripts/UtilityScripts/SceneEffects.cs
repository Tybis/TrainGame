using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AsyncOperation = UnityEngine.AsyncOperation;

public class SceneEffects : MonoBehaviour {
    [SerializeField]
    [Range(0.5f, 3f)]
    private float _fadeTimer = 1f;
    private GameObject _canvasGO;

    public IEnumerator StartFadeTransition() {
        //create new canvas with an image that fills the entire screen
        _canvasGO = new GameObject("BlackoutCanvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = Camera.main;
        var blackOutImage = new GameObject("BlackoutImage").AddComponent<Image>();
        blackOutImage.transform.SetParent(_canvasGO.transform);
        var rectTrans = blackOutImage.GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;
        rectTrans.anchorMax = new Vector2(Screen.width, Screen.height);
        rectTrans.anchorMin = Vector2.zero;
        //make image black and force alpha to max transparency
        blackOutImage.color = new Color(0f, 0f, 0f, 0f);
        //fade image in
        yield return StartCoroutine(_Fade(blackOutImage, true));
    }
    
    public IEnumerator EndFadeTransition(AsyncOperation unloadOp) {
        if (_canvasGO == null)
            Debug.LogError("StartFadeTransition wasn't called prior, no canvas found.");
        //wait for old scene to be unloaded
        
        yield return new WaitUntil(() => unloadOp.isDone); // caught here

        //wait until image is done fading
        yield return StartCoroutine(_Fade(_canvasGO.GetComponentInChildren<Image>(), false));
        //destroy objects once await is finished
        GameObject.Destroy(_canvasGO);
    }

    private IEnumerator _Fade(Image image, bool active) {
        float endAlpha = (active) ? 1f : 0f;
        //TODO: alter the alphaDelta to be a more fitting value once this gets running
        float alphaDelta = (active) ? Time.deltaTime : -Time.deltaTime;
        //alphaDelta *= (1f / _fadeTimer);
        //print("ferp: " + (Time.deltaTime / 1f).ToString());
        do {
            var alphaChange = image.color;
            alphaChange.a = Mathf.Clamp(alphaDelta + alphaChange.a, 0f, 1f);
            yield return new WaitForEndOfFrame();
            image.color = alphaChange;
        } while (image.color.a != endAlpha);
        //total time spent in coroutine should equal _fadeTimer.
    }
}