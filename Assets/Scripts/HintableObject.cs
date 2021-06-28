/*
Author: Christian Mullins
Date: 6/22/21
Summary
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintableObject : MonoBehaviour {
    public const int HINT_LAYER = 6;

    [Range(1f, 5f)]
    public float lightIntensity = 1f;
    [Range(0.1f, 3f)]
    public float fadeInSpeed = 0.5f;
    [Range(0f, 4f)]
    public float mouseHoverTime = 2f;
    public bool isActive { get; private set; }

    private Light _camLight;
    private int _startLayer;
    private Coroutine _hintCo;

    void Start() {
        _camLight = Camera.main.GetComponent<Light>();
        _startLayer = gameObject.layer;
        _hintCo = null;
        isActive = false;
    }

    /// <summary>
    /// Only set hint if the object is NOT active.
    /// </summary>
    public void SetHint() {
        if (!isActive) {
            isActive = true;
            _hintCo = StartCoroutine(_FadeHint());
        }
    }

    /// <summary>
    /// Returns object ot it's normal state before hinting began.
    /// </summary>
    public void ResetObject() {
        if (isActive)
            StopCoroutine(_hintCo);
        gameObject.layer = _startLayer;
        _hintCo = null;
        _camLight.intensity = 0f;
        isActive = false;   
    }

    /// <summary>
    /// Animate the hint in scene gradually, store coroutine so that it can
    /// be canceled later in ResetObject().
    /// </summary>
    private IEnumerator _FadeHint() {
        yield return new WaitForSeconds(mouseHoverTime);
        gameObject.layer = HINT_LAYER;
        _camLight.intensity = 0f;
        float incrementor = fadeInSpeed * Time.deltaTime;
        do {
            yield return new WaitForEndOfFrame();
            _camLight.intensity += incrementor;
        } while (_camLight.intensity < lightIntensity);
        _camLight.intensity = lightIntensity;
    }

}
