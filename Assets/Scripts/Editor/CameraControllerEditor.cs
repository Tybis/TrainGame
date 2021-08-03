/*
CameraControllerEditor.cs
Author: Christian Mullins
Date: 6/19/2021
Summary: Editor stuff for CameraController.
*/
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraController))]
public class CameraControllerEditor : Editor {
    private CameraController _thisTarget;

    private void OnEnable() {
        _thisTarget = (CameraController)target;
    }


    public override void OnInspectorGUI() {
        serializedObject.Update();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Rotate Left"))
            _thisTarget.SnapRotateCamera("left");
        if (GUILayout.Button("Rotate Right"))
            _thisTarget.SnapRotateCamera("right");
        GUILayout.EndHorizontal();
        base.OnInspectorGUI();

        if (GUI.changed) {
            _thisTarget.RefreshTrackingValues();
        }
        if (serializedObject.hasModifiedProperties) {
            serializedObject.ApplyModifiedProperties();
        }
    }
}