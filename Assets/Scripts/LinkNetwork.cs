﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grid
{
    public class LinkNetwork
    {
        LinkNetworkType type;
        List<Link> links = new List<Link>();

        public LinkNetwork(LinkNetworkType _type)
        {
            type = _type;
        }

        public void AddLink(Link _newLink)
        {
            foreach (Link link in links)
            {
                if (link.ID == _newLink.ID)
                    return;
            }

            links.Add(_newLink);
        }
    } 
    
    [System.Serializable]
    public struct Link
    {
        public DirectionID ID;
        public Vector3Int Direction;

        public Link(DirectionID _id, Vector3Int _direction)
        {
            ID = _id;
            Direction = _direction;
        }
    }

    public enum DirectionID
    {
        Forward,
        ForwardRight,
        Right,
        BackwardRight,
        Backward,
        BackwardLeft,
        Left,
        ForwardLeft
    }
}

