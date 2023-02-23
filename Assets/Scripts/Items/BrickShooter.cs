using System;
using UnityEngine;

namespace Items
{
    public class BrickShooter : BrickBase
    {
        [SerializeField] private Transform _shootPoint;


        public Vector3 GetShootPoint()
        {
            return _shootPoint.position;
        }
    }
}