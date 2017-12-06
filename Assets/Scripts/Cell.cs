﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BGEditor.NodeSystem
{
    public class Cell : ISector, ILink
    {
        CellData cellData;

        public CellData GetCellData()
        {
            return cellData;
        }

        public void SetPosition(Vector3 _position)
        {
            cellData.NodeData.Position = _position;
        }

        public float GetRadius()
        {
            return cellData.SectorData.Radius;
        }        

        public Cell(CellData _data)
        {
            cellData = _data;
        }

        public Vector3 GetPosition()
        {
            return cellData.NodeData.Position;
        }

        #region ISector

        public Vector3 GetCenter()
        {
            return GetPosition(); // da rivedere
        }

        public bool IsInside()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region ILink

        public List<INode> GetNeighbourgs()
        {
            return cellData.LinkData.LinkedNodes;
        }

        public void Link(INode _node)
        {
            cellData.LinkData.LinkedNodes.Add(_node);
        }

        public void UnLink(INode _node)
        {
            throw new System.NotImplementedException();
        }

        public void UnLink(ILink _link)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}