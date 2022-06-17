using System;
using UnityEngine;

namespace NavigationSystem.GridSystem
{
    public class HexNode : MonoBehaviour
    {
        private Transform nodeTransform;
        public bool isWalkable;
        public HexCoord coord;

        public Vector3 WorldPosition => nodeTransform.position;
        private void Start()
        {
            nodeTransform = transform;
        }
    }
}