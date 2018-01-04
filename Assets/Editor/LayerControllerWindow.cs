﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Grid
{
    public class LayerControllerWindow
    {
        LayerController layerCtrl;
        Texture addLayerTexture;
        Texture removeLayerTexture;

        string newLayerName;
        bool newLayerEditable;
        Color newLayerGizmosColor;

        Vector2 scrollPosition;

        public LayerControllerWindow(LayerController _layerCtrl)
        {
            layerCtrl = _layerCtrl;
            addLayerTexture = (Texture)EditorGUIUtility.Load("Plus.png");
            removeLayerTexture = (Texture)EditorGUIUtility.Load("Minus.png");
        }

        public void Show()
        {
            GUILayout.BeginVertical("Box");
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Layer", EditorStyles.boldLabel);
            GUILayout.Space(10);
            GUILayout.Label("Is Editable", EditorStyles.boldLabel);
            GUILayout.Label("Gizmos Color", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            for (int i = 0; i < layerCtrl.GetNumberOfLayers(); i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (layerCtrl.GetLayerAtIndex(i).Name == "Base")
                    GUI.enabled = false;

                layerCtrl.GetLayerAtIndex(i).Name = EditorGUILayout.TextField(layerCtrl.GetLayerAtIndex(i).Name);
                layerCtrl.GetLayerAtIndex(i).IsEditable = EditorGUILayout.Toggle(layerCtrl.GetLayerAtIndex(i).IsEditable);
                layerCtrl.GetLayerAtIndex(i).HandlesColor = EditorGUILayout.ColorField(layerCtrl.GetLayerAtIndex(i).HandlesColor);

                if (GUILayout.Button(removeLayerTexture, GUILayout.Height(18), GUILayout.Width(20)))
                {
                    layerCtrl.RemoveLayer(layerCtrl.GetLayerAtIndex(i));
                    continue;
                }

                if (layerCtrl.GetLayerAtIndex(i).Name == "Base")
                    GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(2);
            GUILayout.Label("New Layer", EditorStyles.boldLabel);
            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            newLayerName = EditorGUILayout.TextField(newLayerName);
            newLayerEditable = EditorGUILayout.Toggle(newLayerEditable);
            newLayerGizmosColor = EditorGUILayout.ColorField(newLayerGizmosColor);

            if (newLayerGizmosColor.a == 0)
                newLayerGizmosColor.a = 100;

            if (GUILayout.Button(addLayerTexture, GUILayout.Height(17), GUILayout.Width(20)))
            {
                if (newLayerName == string.Empty)
                {
                    Debug.LogWarning("Can't add a Layer without a name !");
                    return;
                }
                layerCtrl.AddLayer(newLayerName, newLayerEditable, newLayerGizmosColor);
                newLayerName = string.Empty;
                newLayerEditable = false;
                newLayerGizmosColor = Color.black;
                newLayerGizmosColor.a = 100;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}