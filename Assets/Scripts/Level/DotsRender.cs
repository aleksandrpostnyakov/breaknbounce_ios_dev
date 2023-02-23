using System.Collections.Generic;
using Config;
using UnityEngine;

namespace Level
{
    public class DotsRender
    {
        private Dot.Pool _pool;
        private GameConfig _gameConfig;
        private List<Dot> _dots;
        private Transform _parent;
        public DotsRender(Dot.Pool pool, Transform parent, GameConfig gameConfig)
        {
            _pool = pool;
            _gameConfig = gameConfig;
            _parent = parent;
            _dots = new List<Dot>();
        }

        public void Draw(List<DotsInfo> dotsInfos)
        {
            var ind = 1;
            foreach (var dotsInfo in dotsInfos)
            {
                ind++;
                for (var i = 1; i <= dotsInfo.Count; i++)
                {
                    var dot = _pool.Spawn();
                    dot.Init(_parent, Vector3.Lerp(dotsInfo.StartPosition, dotsInfo.FinishPosition, (float)i / dotsInfo.Count), _gameConfig.GetDefaultFieldConfig.DotScale);
                    _dots.Add(dot);
                }
            }
        }

        public void Clear()
        {
            foreach (var dot in _dots)
            {
                _pool.Despawn(dot);
            }
            
            _dots.Clear();
        }
        
    }

    public class DotsInfo
    {
        public Vector3 StartPosition;
        public Vector3 FinishPosition;
        public int Count;
    }
}