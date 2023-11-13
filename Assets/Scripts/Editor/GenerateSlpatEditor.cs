using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerateSlpat))]
public class GenerateSlpatEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GenerateSlpat generateSlpat = (GenerateSlpat)target;

        DrawDefaultInspector();

        GUILayout.Space(10);
        if (GUILayout.Button("Generate NeighboringSplatMaps"))
            generateSlpat.GenerateNeighboringSplatMaps();

        GUILayout.Space(10);
        if (GUILayout.Button("Generate SplatMaps"))
            generateSlpat.GenerateSplatMaps();

        GUILayout.Space(10);
        if (GUILayout.Button("Generate TestMap"))
            generateSlpat.GenerateTestMaps();

    }
}
