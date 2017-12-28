﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grid;

public class MovementController : MonoBehaviour {

    public int PointsOfMovement = 6;
    public GridController TESTController;
    /// <summary>
    /// Calcola le celle in cui è possibile muoversi
    /// </summary>
    /// <returns>Le celle raggiungibili dalla pedina</returns>
    public List<Cell> EvaluateMovementPown()
    {
        List<Cell> possibleMovement = new List<Cell>();
        List<INode> tempList = new List<INode>();

        Cell startCell = TESTController.GetCellFromPosition(transform.position);
        if (PointsOfMovement >= 0)
        {
            possibleMovement.Add(startCell);
        }
        else
            return null;

        for (int i = 0; i < PointsOfMovement; i++)
        {
            for (int j = 0; j < possibleMovement.Count; j++)
            {
                tempList.AddRange(possibleMovement[j].GetNeighbourgs(TESTController.LayerMng.Layers[0]));
            }

            for (int k = 0; k < tempList.Count; k++)
            {
                if(!possibleMovement.Contains(tempList[k] as Cell))
                {
                    possibleMovement.Add(tempList[k] as Cell);
                }
            }
        }

        return possibleMovement;
    }

    public void Drag(Vector3 _position)
    {
        transform.position = _position;
    }

    public void Drop(Vector3 _position)
    {
        transform.position = _position;
    }
}
