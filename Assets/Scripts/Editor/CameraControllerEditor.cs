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
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Rotate Left"))
            _thisTarget.SnapRotateCamera("left");
        if (GUILayout.Button("Rotate Right"))
            _thisTarget.SnapRotateCamera("right");
        GUILayout.EndHorizontal();
        base.OnInspectorGUI();
        
        if (serializedObject.hasModifiedProperties) {
            //TODO: change position in-scene from variable changes
            //if (_thisTarget.keepDistanceFromTarget && )
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }

    private Vector3 _GetAdjustedPosition(float newTrackingDistance) {
        return _thisTarget.target.position + (-_thisTarget.transform.forward * newTrackingDistance);
    }
}