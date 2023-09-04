using System;
using System.Collections.Generic;
using System.Text;

using Library3d.Math;
using Library3d.Mesh;
using Library3d.Interface;

namespace Library3d.Nodes
{
    public enum eNodeObjectType
    {
        eNone,
        e3dMesh,
        e3dMeshLod,
        eMshFile,
        eHimFile,
        e3dMeshCollision,
        eMshHook,
        e3dMshShadow,
        eNode,
    }
    public class NodeObject
    {
        public string Name = string.Empty;
        public Node ParentNode = null;
        public eNodeObjectType Type = eNodeObjectType.eNone;
        public bool Enabled = true;
        public bool Visible = true;
        public JMatrix ObjectMatrix = null;

        public IRenderObject RenderObject = null;

    }

    public class Node : NodeObject
    {
        public JMatrix Global;
        public JMatrix Local;

        public NodeManager Scene = null;

        public List<Node> Children;
        public List<NodeObject> Objects;

        public Node()
        {
            Init();
        }

        public Node(string name)
        {
            Name = name;
            Init();
        }

        protected void Init()
        {
            Global = new JMatrix();
            Local = new JMatrix();
            Children = new List<Node>();
            Objects = new List<NodeObject>();
        }

        public void AddObject(NodeObject o)
        {
            if (o.ParentNode != null)
            {
                o.ParentNode.RemoveObject(o);
            }
            o.ParentNode = this;
            Objects.Add(o);
        }

        public void RemoveObject(NodeObject o)
        {
            
            o.ParentNode = null;
            Objects.Remove(o);

        }
        public void AddNode(Node n)
        {
            if (n.ParentNode != null)
            {
                n.ParentNode.RemoveNode(n);
            }
            Children.Add(n);
            n.ParentNode = this;
            n.Scene = this.Scene;
            if (this.Scene != null)
            {
                this.Scene.AddNode(n);
            }
        }
        public void RemoveNode(Node n)
        {
            Children.Remove(n);
            n.ParentNode = null;
            if (this.Scene != null)
            {
                this.Scene.RemoveNode(n);
            }
        }

        public void Update_Position()
        {
            _CalculateGlobalPosition(this);
        }

        public void _CalculateGlobalPosition(Node node)
        {
            if (node.ParentNode != null)
                node.Global = node.Local * node.ParentNode.Global;
            else
                node.Global = node.Local;

            foreach (Node child in node.Children)
            {
                _CalculateGlobalPosition(child);
            }
        }
        public MeshData ToEmptyMesh()
        {
            MeshData m = new MeshData();
            m.Name = "["+this.Name+"]";
            //m.Name = this.Name;
            m.OverrideLocalMatrix = this.Local;

            Vertex v = new Vertex();
            v.pos = new JVector(0);
            m.Vertexs.Add(v);
            m.Vertexs.Add(v);
            m.Vertexs.Add(v);

            Face n = new Face();
            n.v1 = 0;
            n.v2 = 1;
            n.v3 = 2;
            m.Faces.Add(n);

            m.Type = eNodeObjectType.eNode;
            return m;
        }

        public override string ToString()
        {
            string str = string.Empty;

            str += string.Format("[Local Matrix]\r\n");
            str += this.Local.ToString() +"\r\n";
            str += string.Format("[Global Matrix]\r\n");
            str += this.Global.ToString() + "\r\n";
            return str;
        }

        public void Clear()
        {
            this.Objects.Clear();
            this.Children.Clear();
        }


        public NodeObject FindObject(string name)
        {
            NodeObject no = null;
            foreach (NodeObject o in this.Objects)
            {
                //if (o.Name.Trim().Equal(sname.Trim(),)) return o;
                if (o.Name.Trim().Equals(name.Trim(), StringComparison.CurrentCultureIgnoreCase) == true) return o;
            }
            foreach (Node n in this.Children)
            {
                no = n.FindObject(name);
                if (no != null) return no;
            }

            return null;
        }

    }

    public class NodeManager
    {
        public Node Root = new Node("Scene");
        public List<Node> Nodes = new List<Node>();
        public void AddNode(Node n)
        {
            Nodes.Add(n);
        }
        public void RemoveNode(Node n)
        {
            Nodes.Remove(n);
        }

        public void Clear()
        {
            if (Root != null) Root.Clear();
            Nodes.Clear();
        }

    }
}
