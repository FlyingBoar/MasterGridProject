﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Grid
{
    [System.Serializable]
    public class MasterGrid : EditorWindow
    {
        public static GridController GridCtrl { get; private set; }
        public static GridControllerWindow GridCtrlWindow { get; private set; }

        public static GridVisualizer GridVisualizer { get; private set; }
        public static GridVisualizerWindow GridVisualizerWindow { get; private set; }

        public static GridLayerController LayerCtrl { get; private set; }
        public static GridLayerControllerWindow GridLayerCtrlWindow { get; private set; }

        public static GridScanner GridScanner { get; private set; }
        public static GridScannerWindow GridScannerWindow { get; private set; }

        

        #region Save/Load Variables
        TextAsset fileToLoad;
        [SerializeField]
        string newDataName = "NewGridData";
        #endregion

        [SerializeField]
        static int selectedToolbarItem;
        [SerializeField]
        static List<string> toolbarEntries = new List<string>();

        [MenuItem("Window/Master Grid _%g")]
        static void Init()
        {
            MasterGrid masterGrid = (MasterGrid)GetWindow(typeof(MasterGrid));
            masterGrid.titleContent = new GUIContent("Master Grid");
            //masterGrid.minSize = new Vector2(600, 350);
            masterGrid.Show();

            GridCtrl = new GridController();
            GridVisualizer = new GridVisualizer(GridCtrl);
            LayerCtrl = new GridLayerController(GridCtrl);
            GridScanner = new GridScanner();

            GridCtrl.Init(LayerCtrl);

            GridCtrlWindow = new GridControllerWindow(GridCtrl);
            GridVisualizerWindow = new GridVisualizerWindow(GridVisualizer);
            GridLayerCtrlWindow = new GridLayerControllerWindow(LayerCtrl);
            GridScannerWindow = new GridScannerWindow(GridScanner, GridCtrl);

            if (toolbarEntries.Count == 0)
            {
                toolbarEntries.Add("Grid Controller");
                toolbarEntries.Add("Grid Visualizer");
                toolbarEntries.Add("Layer Controller");
                toolbarEntries.Add("Grid Scanner");
            }

            SceneView.onSceneGUIDelegate += DrawCall;
            SceneView.onSceneGUIDelegate += MouseInteraction;
        }

        private void OnEnable()
        {
            //Workaround
            if (GridCtrl == null)
                Init();
        }

        private void OnGUI()
        {

            #region Save/Load Region

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (fileToLoad == null)
                GUI.enabled = false;
            if (GUILayout.Button("Save", GUILayout.Height(40)))
            {
                GridCtrl.SaveCellMatrixInData();
                DataManager.SaveData(AssetDatabase.GetAssetPath(fileToLoad));
            }
            if (fileToLoad == null)
                GUI.enabled = true;

            if (GUILayout.Button("Save as", GUILayout.Height(40)))
            {
                GridCtrl.SaveCellMatrixInData();
                DataManager.SaveNewData(newDataName);
            }


            EditorGUILayout.BeginVertical();
            GUILayout.Label("Asset Name", EditorStyles.boldLabel);
            newDataName = GUILayout.TextField(newDataName);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Load Grid") && fileToLoad != null)
            {
                DataManager.LoadData(AssetDatabase.GetAssetPath(fileToLoad));
                GridCtrl.ReInitVariables();
            }

            fileToLoad = (TextAsset)EditorGUILayout.ObjectField(fileToLoad, typeof(TextAsset), false);
            if (fileToLoad != null)
            {
                string loadFilePath = AssetDatabase.GetAssetPath(fileToLoad);
                if (!loadFilePath.Contains(".json"))
                {
                    fileToLoad = null;
                    Debug.LogWarning("File format not supported !");
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            #endregion

            selectedToolbarItem = GUILayout.Toolbar(selectedToolbarItem, toolbarEntries.ToArray());
            switch (selectedToolbarItem)
            {
                case 0: // grid controller
                    GridCtrlWindow.Show();
                    break;
                case 1: // grid visualizer    
                    GridVisualizerWindow.Show();
                    break;
                case 2: // layer controller
                    GridLayerCtrlWindow.Show();
                    break;
                case 3: // grid scanner
                    GridScannerWindow.Show();
                    break;
            }
        }

        static void DrawCall(SceneView _sceneView)
        {
            GridVisualizer.DrawHandles();
            SceneView.RepaintAll();
        }

        static void MouseInteraction(SceneView _sceneView)
        {
            if (_sceneView != SceneView.currentDrawingSceneView)
                return;
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                    OnLeftClick();
                else if (Event.current.button == 1)
                    OnRightClick();
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                if (Event.current.button == 0 && Event.current.control)
                    DragSelection();
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                if(Event.current.button == 0)
                    Tools.hidden = false;
            }
        }

        static void DragSelection()
        {
            if (GridCtrl == null)
                return;
            Transform selectedTransf = Selection.activeTransform;
            if (selectedTransf != null)
            {
                Tools.hidden = true;
                Cell cell = GridCtrl.GetCellFromPosition(GridInput.PointerPosition);
                if(cell != null)
                    selectedTransf.position = cell.GetPosition();
                else
                    Tools.hidden = false;
            }
        }

        static void OnLeftClick()
        {
            if (GridCtrlWindow.CurrentMouseAction == GridControllerWindow.MouseActions.Link)
                GridCtrlWindow.LinkSelectedCell();
            else if(GridCtrlWindow.CurrentMouseAction == GridControllerWindow.MouseActions.LinkMutual)
                GridCtrlWindow.LinkSelectedCell(true);
            else if(GridCtrlWindow.CurrentMouseAction == GridControllerWindow.MouseActions.Unlink)
                GridCtrlWindow.UnlinkSelectedCell();
            else
                GridCtrlWindow.SelectCell();
        }

        static void OnRightClick()
        {
            if (GridCtrlWindow.SelectedCell == GridCtrl.GetCellFromPosition(GridInput.PointerPosition))
            {
                GenericMenu menu = new GenericMenu();
                if (GridCtrlWindow.SelectedCell != null)
                {
                    menu.AddItem(new GUIContent("Link Cell"), false, () => { GridCtrlWindow.StartMouseAction(GridControllerWindow.MouseActions.Link); });
                    menu.AddItem(new GUIContent("Mutual Link Cell"), false, () => { GridCtrlWindow.StartMouseAction(GridControllerWindow.MouseActions.LinkMutual); });
                    menu.AddItem(new GUIContent("Unlink Cell"), false, () => { GridCtrlWindow.StartMouseAction(GridControllerWindow.MouseActions.Unlink); });
                }
                menu.ShowAsContext();
            }
            else if (GridCtrlWindow.CurrentMouseAction != GridControllerWindow.MouseActions.None)
            {
                GridCtrlWindow.EndMouseAction();
            }
        }

        private void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= DrawCall;
            SceneView.onSceneGUIDelegate -= MouseInteraction;
        }
    }
}
