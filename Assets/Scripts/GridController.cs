﻿using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

namespace Grid
{
    public class GridController
    {
        public GridData GridData;
        public CellData.SectorData SectorData { get { return GridData.SectorData; } }

        public Vector3 Origin = Vector3.zero;
        public Vector3Int Size;
        public Vector3 ResolutionCorrection;

        public GridVisualizer GridVisualizer;
        public LayerController LayerCtrl;

        Cell[,,] CellsMatrix;

        public GridController() { }

        public void Init(GridVisualizer _gridVisualizer, LayerController _layerCtrl)
        {
            GridVisualizer = _gridVisualizer;
            LayerCtrl = _layerCtrl;
            GridData = new GridData();
        }

        #region API
        public bool DoesGridExist()
        {
            if (CellsMatrix != null)
                return true;
            else
                return false;
        }

        public void CreateNewGrid(bool autoLinkCells = true)
        {
            //Reset operation
            ClearGrid();
            //New grid creation
            CreateGrid();
            //Linking process
            if (autoLinkCells)
            {
                for (int i = 0; i < LayerCtrl.GetNumberOfLayers(); i++)
                {
                    LinkCells(LayerCtrl.GetLayerAtIndex(i));
                }
            }
        }

        public void ClearGrid()
        {
            CellsMatrix = null;
        }

        public void Load(string _gridDataPath)
        {
            LoadFromJSON(_gridDataPath);
        }

        public void Save(string _name)
        {
            SaveCurrent(_name);
        }

        /// <summary>
        ///Chiama la funzione Link alla cella selezionata passando la cella su cui si trova il cursore
        /// </summary>
        public void LinkCells(Cell startingCell, Cell endingCell, bool mutualLink = false)
        {
            //startingCell.Link(this.GetCellFromPosition(InputAdapter_Tester.PointerPosition), LayerCtrl.GetLayerAtIndex(0));
            startingCell.Link(endingCell.GridCoordinates, LayerCtrl.GetLayerAtIndex(0));

            if (mutualLink)
                endingCell.Link(startingCell.GridCoordinates, LayerCtrl.GetLayerAtIndex(0));
        }

        public void UnlinkCells(Cell startingCell, Cell endingCell)
        {
            startingCell.UnLink(endingCell.GridCoordinates, LayerCtrl.GetLayerAtIndex(0));
            endingCell.UnLink(startingCell.GridCoordinates, LayerCtrl.GetLayerAtIndex(0));
        }

        #region Getter
        public Cell GetCentralCell()
        {
            return this.GetCellFromPosition(Origin);
        }

        /// <summary>
        /// Return the list of not null cell in Grid
        /// </summary>
        /// <returns></returns>
        public List<Cell> GetListOfCells()
        {
            List<Cell> cellsList = new List<Cell>();

            if(CellsMatrix != null)
            {
                for (int i = 0; i < CellsMatrix.GetLength(0); i++)
                    for (int j = 0; j < CellsMatrix.GetLength(1); j++)
                        for (int k = 0; k < CellsMatrix.GetLength(2); k++)
                            if (CellsMatrix[i, j, k] != null)
                                cellsList.Add(CellsMatrix[i, j, k]);
            }

            return cellsList;
        }

        /// <summary>
        /// Return the Matrix of the grid
        /// </summary>
        /// <returns></returns>
        public Cell[,,] GetCellsMatrix()
        {
            return CellsMatrix;
        }
        #endregion
        #endregion

        #region GridData Management
        void LoadFromJSON(string _jsonGridDataPath)
        {
            string _jsonGridData = File.ReadAllText(_jsonGridDataPath);
            GridData _newGridData = JsonUtility.FromJson<GridData>(_jsonGridData);

            if (_jsonGridData == null)
            {
                Debug.LogWarning("GridController -- No data to load !");
                return;
            }

            GridData = _newGridData;
            Size = GridData.Size;
            Origin = GridData.Origin;
            ResolutionCorrection = GridData.ResolutionCorrection;

            CellsMatrix = GridData.CellsMatrix;
        }

        GridData SaveCurrent(string _name = null)
        {
            GridData newGridData = new GridData();

            newGridData.CellsMatrix = CellsMatrix;
            newGridData.Layers = LayerCtrl.Layers;
            newGridData.Size = Size;
            newGridData.Origin = Origin;
            newGridData.ResolutionCorrection = ResolutionCorrection;
            newGridData.SectorData = SectorData;

            string assetName;
            if (_name == null)
                assetName = "NewGridData.txt";
            else
                assetName = _name + ".txt";

            //newGridData.name = assetName;
            string completePath = AssetDatabase.GenerateUniqueAssetPath(CheckFolder() + assetName);

            string jasonData = JsonUtility.ToJson(newGridData);

            File.WriteAllText(completePath, jasonData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return newGridData;
        }

        string CheckFolder()
        {
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder("Assets/GridData"))
                AssetDatabase.CreateFolder("Assets", "GridData");
#endif

            return "Assets/GridData/";
        }
        #endregion

        #region Grid Creation
        void CreateGrid()
        {
            int maxSize = Size.x >= Size.y ? Size.x : Size.y;
            maxSize = maxSize >= Size.z ? maxSize : Size.z;

            CellsMatrix = new Cell[maxSize, maxSize, maxSize];
            for (int i = 0; i < maxSize; i++)
            {
                for (int j = 0; j < maxSize; j++)
                {
                    for (int k = 0; k < maxSize; k++)
                    {                  
                        CreateCell(i, j, k);
                    }
                }
            }
        }

        void CreateCell(int _i, int _j, int _k)
        {
            int i = _i < Size.x ? _i : 0;
            int j = _j < Size.y ? _j : 0;
            int k = _k < Size.z ? _k : 0;

            Vector3 nodePos = this.GetPositionByCoordinates(new Vector3Int(i, j, k));

            CellsMatrix[i, j, k] = new Cell(new CellData(SectorData, nodePos, LayerCtrl.GetLayerAtIndex(0)), this);
        }

        /// <summary>
        /// Crea i collegamenti alle celle
        /// </summary>
        internal void LinkCells(Layer _layer)
        {
            for (int i = 0; i < CellsMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < CellsMatrix.GetLength(1); j++)
                {
                    for (int k = 0; k < CellsMatrix.GetLength(2); k++)
                    {
                        if (CellsMatrix[i, j, k] == null)
                            continue;

                        //Link of the next and previus cell along all directions
                        CellsMatrix[i,j,k].Link(CellsMatrix[i != 0 ? i - 1 : 0, j, k].GridCoordinates, _layer);
                        if (i < Size.x - 1)
                            CellsMatrix[i,j,k].Link(CellsMatrix[i + 1, j, k].GridCoordinates, _layer);

                        CellsMatrix[i,j,k].Link(CellsMatrix[i, j != 0 ? j - 1 : 0, k].GridCoordinates, _layer);
                        if(j < Size.y - 1)
                            CellsMatrix[i,j,k].Link(CellsMatrix[i, j + 1 , k].GridCoordinates, _layer);

                        CellsMatrix[i,j,k].Link(CellsMatrix[i, j, k != 0 ? k - 1 : 0].GridCoordinates, _layer);
                        if(k < Size.z - 1)
                            CellsMatrix[i,j,k].Link(CellsMatrix[i, j, k + 1].GridCoordinates, _layer);
                    }
                }
            }
        }

        internal void RemoveLinks(Layer _layer)
        {
            for (int i = 0; i < CellsMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < CellsMatrix.GetLength(1); j++)
                {
                    for (int k = 0; k < CellsMatrix.GetLength(2); k++)
                    {
                        if (CellsMatrix[i, j, k] == null)
                            continue;
                        CellsMatrix[i, j, k].UnLinkAll(_layer);
                        CellsMatrix[i, j, k].GetCellData().RemoveLayeredLink(_layer);
                    }
                }
            }
        }
        #endregion
    }
}
