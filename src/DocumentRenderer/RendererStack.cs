using System.Collections.Generic;

namespace PrintRenderer
{



    public class RendererStack<T> where T: class, IRenderer
    {
        private List<T> _Renderers;
        private int _Index;

        public bool MoreContentAvailable
        {
            get
            {
                var r = GetNext();
                return (r != null) && r.MoreContentAvailable;
            }
        }

        public IEnumerable<T>GetAll()
        {
            foreach (var r in _Renderers)
                yield return r;
        }

        public IEnumerable<T>GetRemaining()
        {
            for (int i = _Index; i < _Renderers.Count; ++i)
                if (_Renderers[i].MoreContentAvailable)
                    yield return _Renderers[i];
        }

        public RendererStack()
        {
            _Renderers = new List<T>();
            _Index = 0;
        }

        public void Add(T r)
        {
            _Renderers.Add(r);
        }

        public T GetNext()
        {
            while (_Index < _Renderers.Count)
            {
                T r = _Renderers[_Index];
                if (r.MoreContentAvailable)
                {
                    return r;
                }
                _Index++;
            }
            return null;
        }

        private IEnumerable<T> Renderers()
        {
            if (_Renderers.Count == 0)
            {
                yield break;
            }

            T r;
            while ((r = GetNext()) != null)
            {
                yield return r;
            }
        }
    }
}
