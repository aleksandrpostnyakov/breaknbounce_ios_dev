using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Level
{
    public class AttackWarning
    {
        private WarningIcon.Pool _pool;
        private Field _field;

        private List<WarningIcon> _icons;

        public AttackWarning(WarningIcon.Pool pool, Field field)
        {
            _pool = pool;
            _field = field;
            _icons = new List<WarningIcon>();
        }

        public void Clear()
        {
            foreach (var icon in _icons)
            {
                _pool.Despawn(icon);
            }
            _icons.Clear();
        }

        public void Add(int index)
        {
            var icon = _pool.Spawn();
            icon.Init(_field.GetWarningsTransform, _field.GetBaseSize() * index, index);
            _icons.Add(icon);
        }

        public void Remove(int index)
        {
            var icon = _icons.FirstOrDefault(i => i.Id == index);
            if (icon == null)
            {
                return;
            }
            
            _pool.Despawn(icon);
            _icons.Remove(icon);
        }
    }
    
}