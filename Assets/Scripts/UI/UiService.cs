using System;
using System.Collections.Generic;

namespace UI
{
    public class UiService
    {
        private readonly Dictionary<Type, object> _views;

        public UiService()
        {
            _views = new Dictionary<Type, object>();
        }

        public void Register<T>(T view)
        {
            _views.Add(typeof(T), view);
        }

        public T Get<T>()
        {
            return (T) _views[typeof(T)];
        }
    }
}