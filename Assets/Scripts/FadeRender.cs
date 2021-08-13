/*
FadeRender.cs
Author: Christian Mullins **and editted from our lovely artist Beren's code
Date: 8/5/2021
Summary: Attach to a GameObject for making materials transparent if the camera
    colliders with a given object.
*/
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FadeRender : MonoBehaviour {
    [Range(0f, 1f)]
    public float fadeVal = 0.5f;

    private Renderer _rend; 
    private Shader _defaultShader;
    private Shader _fadeShader;
    private Color[] _defaultColors;
    private Color[] _fadeColors;
    private int _materialCount;

    private void Start() {
        _rend = GetComponent<Renderer>();

        _materialCount = _rend.materials.Length;
        _defaultColors = new Color[_materialCount];
        _fadeColors = new Color[_materialCount];

        for (int i = 0; i < _materialCount; ++i) {
            var newColor = _rend.materials[i].color;
            _defaultColors[i] = newColor;
            _fadeColors[i] = new Color(newColor.r, newColor.g, newColor.b, fadeVal);
        }
        
        _defaultShader = _rend.material.shader;
        _fadeShader = Shader.Find("Transparent/Diffuse");
    }

    //set
    private void OnTriggerEnter(Collider other) {
        if (other.tag.Equals("MainCamera")) {
            if (_rend.materials.Length > 1) {
                for (int i = 0; i < _materialCount; ++i) {
                    _rend.materials[i].shader = _fadeShader;
                    _rend.materials[i].color = _fadeColors[i];
                }
            } else {
                _rend.material.shader = _fadeShader;
                _rend.material.color = _fadeColors[0];
            }
        }
    }

    //reset
    private void OnTriggerExit(Collider other) {
        if (other.tag.Equals("MainCamera")) {
            for (int i = 0; i < _materialCount; ++i) {
                _rend.materials[i].color = _defaultColors[i];
                _rend.materials[i].shader = _defaultShader;
            }
        }
    }
}
