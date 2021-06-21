/*
NormalInverter.cs
Author: Christian Mullins
Date: 06/15/21
Summary: Utility tool to instantly flip normals with jsut a button.
*/
using UnityEngine;
using UnityEditor;
 
[RequireComponent(typeof(MeshFilter))]
public class NormalInverter : MonoBehaviour {}

#if UNITY_EDITOR
[CustomEditor(typeof(NormalInverter))]
public class NormalInverterEditor : Editor {
    public override void OnInspectorGUI() {
        if (GUILayout.Button("Invert Object's Normals")) {
            var myObject = (NormalInverter)target;
            var filter = myObject.GetComponent<MeshFilter>();
            if (filter != null) {
                Mesh mesh = filter.mesh;
                var normals = mesh.normals;
                for (int i = 0; i < normals.Length; ++i)
                    normals[i] = -normals[i];
                mesh.normals = normals;
 
                for (int i = 0; i < mesh.subMeshCount; ++i) {
                    var triangles = mesh.GetTriangles(i);
                    for (int ii = 0; ii < triangles.Length; ii += 3) {
                        int temp = triangles[ii];
                        triangles[ii] = triangles[ii + 1];
                        triangles[ii + 1] = temp;
                    }
                    mesh.SetTriangles(triangles, i);
                }
            }
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }        
    }
}
#endif