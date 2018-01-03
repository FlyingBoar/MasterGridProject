﻿using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Grid
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(LayerController), typeof(GridControllerVisualizer))]
    public class GridController : MonoBehaviour
    {
        public NodeNetworkData NetworkData;
        public SectorData SectorData;

        public Vector3Int Size;
        public Vector3 ResolutionCorrection;

        LayerController _layerCtrl;
        public LayerController LayerCtrl
        {
            get
            {
                if (!_layerCtrl)
                    _layerCtrl = GetComponent<LayerController>();

                return _layerCtrl;
            }
        }
        GridControllerVisualizer _gridVisualizer;
        public GridControllerVisualizer GridVisualizer
        {
            get
            {
                if (!_gridVisualizer)
                    _gridVisualizer = GetComponent<GridControllerVisualizer>();

                return _gridVisualizer;
            }
        }

        Cell[,,] CellsMatrix;

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
            if (SectorData.Radius.x == 0)
                SectorData.Radius.x = float.Epsilon;
            if (SectorData.Radius.y == 0)
                SectorData.Radius.y = float.Epsilon;
            if (SectorData.Radius.z == 0)
                SectorData.Radius.z = float.Epsilon;

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

        public void Load()
        {
            if(NetworkData != null)
                LoadFromNetworkData(NetworkData);
        }

        public void SaveCurrent()
        {
            Save(GetListOfCells());
        }

        /// <summary>
        ///Chiama la funzione Link alla cella selezionata passando la cella su cui si trova il cursore
        /// </summary>
        public void LinkCells(Cell startingCell, Cell endingCell, bool mutualLink = false)
        {
            //startingCell.Link(this.GetCellFromPosition(InputAdapter_Tester.PointerPosition), LayerCtrl.GetLayerAtIndex(0));
            startingCell.Link(endingCell, LayerCtrl.GetLayerAtIndex(0));

            if (mutualLink)
                endingCell.Link(startingCell, LayerCtrl.GetLayerAtIndex(0));
        }
        #region Getter
        public Cell GetCentralCell()
        {
            return this.GetCellFromPosition(transform.position);
        }

        public List<Cell> GetGridCorners()
        {
            List<Cell> tempList = new List<Cell>();
            tempList.Add(this.GetCellByCoordinates(0, 0, 0));
            tempList.Add(this.GetCellByCoordinates(Size.x -1, 0, 0));
            tempList.Add(this.GetCellByCoordinates(Size.x -1, Size.y - 1, 0));
            tempList.Add(this.GetCellByCoordinates(Size.x - 1, Size.y - 1, Size.z - 1));
            tempList.Add(this.GetCellByCoordinates(0, Size.y - 1, 0));
            tempList.Add(this.GetCellByCoordinates(0, Size.y - 1, Size.z - 1));
            tempList.Add(this.GetCellByCoordinates(0, 0, Size.z - 1));

            return tempList;
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
        void LoadFromNetworkData(NodeNetworkData _networkData)
        {
            if (_networkData == null)
            {
                Debug.LogWarning("GridController -- No data to load !");
                return;
            }

            ClearGrid();

            CellsMatrix = _networkData.CellsMatrix;

            Size = _networkData.Size;
            SectorData = GetListOfCells().Where(c => c != null).First().GetCellData().SectorData;
        }

        NodeNetworkData Save(List<Cell> _cells)
        {
            NodeNetworkData asset = null;

            List<CellData> cellsData = new List<CellData>();
            foreach (Cell cell in _cells)
            {
                cellsData.Add(cell.GetCellData());
            }

            asset = ScriptableObject.CreateInstance<NodeNetworkData>();
            asset.CellsMatrix = CellsMatrix;
            asset.Size = Size;

            string assetName = "NodeNetworkData.asset";
            asset.name = assetName;
            string completePath = AssetDatabase.GenerateUniqueAssetPath(CheckFolder() + assetName);

            AssetDatabase.CreateAsset(asset, completePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return asset;
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

            Vector3 nodePos = this.GetPositionByCoordinates(i, j, k);

            NodeData nodeD = new NodeData(nodePos);
            LinkData linkD = new LinkData();
            SectorData sectorD = SectorData;

            CellsMatrix[i, j, k] = new Cell(new CellData(nodeD, linkD, sectorD), this, new Vector3Int(i,j,k));
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
                        CellsMatrix[i,j,k].Link(CellsMatrix[i != 0 ? i - 1 : 0, j, k], _layer);
                        if (i < Size.x - 1)
                            CellsMatrix[i,j,k].Link(CellsMatrix[i + 1, j, k], _layer);

                        CellsMatrix[i,j,k].Link(CellsMatrix[i, j != 0 ? j - 1 : 0, k], _layer);
                        if(j < Size.y - 1)
                            CellsMatrix[i,j,k].Link(CellsMatrix[i, j + 1 , k], _layer);

                        CellsMatrix[i,j,k].Link(CellsMatrix[i, j, k != 0 ? k - 1 : 0], _layer);
                        if(k < Size.z - 1)
                            CellsMatrix[i,j,k].Link(CellsMatrix[i, j, k + 1], _layer);
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
                        CellsMatrix[i, j, k].GetCellData().LinkData.RemoveLayeredLink(_layer);
                    }
                }
            }
        }
        #endregion

        Vector3 CalculateOffset()
        {
            Vector3 offset = new Vector3(Size.x * SectorData.Diameter.x, Size.y * SectorData.Diameter.y, Size.z * SectorData.Diameter.z);

            offset /= 2;
            offset -= SectorData.Radius;
            return offset;
        }
    }
}
