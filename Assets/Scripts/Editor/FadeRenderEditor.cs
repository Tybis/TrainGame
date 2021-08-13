/*
FadeRenderEditor.cs
Author: Christian Mullins
Date: 8/5/2021
Summary: Editor class for the FadeRender component.
*/
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FadeRender))]
[CanEditMultipleObjects]
public class FadeRenderEditor : Editor {
    private FadeRender _target;

    private void OnEnable() {
        _target = (FadeRender)target;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        //button for setting global values
        bool applyVal = GUILayout.Button("Apply Global Value");
        if (applyVal) {
            //grabs all scripts in the current scene as Objects
            var targetObjs = GameObject.FindObjectsOfType(typeof(FadeRender));
            foreach (var obj in targetObjs) {
                var saveObj = new SerializedObject(obj);
                saveObj.FindProperty("fadeVal").floatValue = _target.fadeVal;
                saveObj.ApplyModifiedProperties();
            }
        }
        //save changes
        if (serializedObject.hasModifiedProperties) {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
