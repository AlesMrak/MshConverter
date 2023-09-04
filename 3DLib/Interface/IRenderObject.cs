using System;
using System.Collections.Generic;
using System.Text;

using Library3d.Nodes;

namespace Library3d.Interface
{
    public interface IRenderObject : IDisposable
    {
        NodeObject DataObject
        {
            get;
            set;
        }
        bool Visible
        {
            get;
            set;
        }

        bool CanRender
        {
            get;
        }

        void Render(object data);
        void Update(object data);
        void Init(object data);
        void Save(string fileName);

    }
}
