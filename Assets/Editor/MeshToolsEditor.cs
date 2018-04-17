﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshTools))]
public class MeshToolsEditor : Editor
{
    bool ShowNoiseAndSmoothing = true;
    GUIStyle sectionHeader;

    public void SetStyles()
    {
        // Section Header style
        sectionHeader = new GUIStyle(UnityEditor.EditorStyles.foldout);
        sectionHeader.fontStyle = FontStyle.Bold;
        sectionHeader.imagePosition = ImagePosition.TextOnly;
        sectionHeader.margin = new RectOffset(50, 5, 5, 10);
        sectionHeader.padding = new RectOffset(160, 0, 0, 0);
        sectionHeader.border = new RectOffset(5, 5, 5, 5);
        sectionHeader.clipping = TextClipping.Overflow;
    }

    public void SetSectionHeaderStyle() { 
}

    public override void OnInspectorGUI()
    {
        SetStyles();

        base.OnInspectorGUI();
        EditorGUILayout.HelpBox("A set of tools allow for customising and distorting a mesh. This script can only be placed on a GameObject that contains a MeshFilter and a MeshRenderer.", MessageType.None);

        EditorGUILayout.Space();

        var meshTools = (MeshTools) target;
        var noiseAndSmoothing = meshTools.Noise;

        GUI.color = new Color(247, 182, 130);
        ShowNoiseAndSmoothing = EditorGUILayout.Foldout(ShowNoiseAndSmoothing, "Noise and Smoothing", true, sectionHeader);

        GUI.color = new Color(236, 205, 136);
        if (ShowNoiseAndSmoothing)
        {
            noiseAndSmoothing.ContinuousUpdate = GUILayout.Toggle(noiseAndSmoothing.ContinuousUpdate, "Continuous Update");
            noiseAndSmoothing.OffsetTriangles = GUILayout.Toggle(noiseAndSmoothing.OffsetTriangles, "Offset Triangles");

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Noise Intensity", EditorStyles.label, GUILayout.Width(120));
            noiseAndSmoothing.NoiseIntensity = GUILayout.HorizontalSlider(noiseAndSmoothing.NoiseIntensity, -0.0005f, 0.0005f);
            noiseAndSmoothing.NoiseIntensity = EditorGUILayout.FloatField(noiseAndSmoothing.NoiseIntensity, EditorStyles.numberField, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Smoothing Multiplier", EditorStyles.label, GUILayout.Width(120));
            noiseAndSmoothing.SmoothingTimes = EditorGUILayout.IntSlider(noiseAndSmoothing.SmoothingTimes, 0, 10);
            EditorGUILayout.EndHorizontal();

        }

        GUI.color = Color.clear;
    }
}