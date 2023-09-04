using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Library3d.Math;
using Library3d.Nodes;
using Library3d.Error;

namespace Library3d.Mesh
{
    public class HierNode
    {
        public string   Name= string.Empty;
        public string   ParentName= string.Empty;
        public string   Mesh = string.Empty;
        public JMatrix  Local = new JMatrix();
        public JMatrix  Global = new JMatrix();
        public float VisibilitySphere;
        public MeshObject MeshLoaded = null;

        public List<HierCollisionObject> Collisions = new List<HierCollisionObject>();
        public bool Visible = true;
        public bool Separable = false;
        public int Hierarchy = -1;
        public List<HierNode> Nodes = new List<HierNode>();
    }

    public class HierCollisionObject
    {
        public string   Type = "sphere";
        public JVector  Position = new JVector();
        public float    R = 0;
        public string   Name = string.Empty;
        
    }

    public partial class HierLoader
    {
        public string FileName = string.Empty;
        public Dictionary<string, HierNode> NodesDict = new Dictionary<string, HierNode>();

        public HierNode Root = null;


        public void Load(string filename)
        {
            FileName = filename;

            StreamReader rd = File.OpenText(FileName);

            if (rd == null) return;

            HierNode pnode = null;
            NodesDict.Clear();

            string line = string.Empty;
            do
            {
                line = rd.ReadLine();
                line = line.Trim();

                ///New node
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string name = line.Substring(1, line.Length - 2);
                    if (pnode != null)
                    {
                        if (pnode.ParentName == string.Empty)
                        {
                            Root = pnode;
                        }
                        NodesDict.Add(pnode.Name.ToLower(), pnode);
                    }
                    pnode = new HierNode();
                    pnode.Name = name;
                }
                else
                {
                    char[] chs = { ' ' };
                    string[] cmds = line.Split(chs, StringSplitOptions.RemoveEmptyEntries);
                    if (cmds.Length > 1)
                    {
                        if (cmds[0].Contains("VisibilitySphere"))
                        {
                            pnode.VisibilitySphere = Convert.ToSingle(cmds[1]);
                        }
                        else if(cmds[0].Contains("CollisionObject"))
                        {
                            HierCollisionObject col = new HierCollisionObject();
                            if(cmds.Length==2)
                            {
                                col.Type = "mesh";
                                col.Name = cmds[1];
                                pnode.Collisions.Add(col);
                            }
                            else if(cmds.Length==6)
                            {
                                col.Type = cmds[1];
                                col.R = Convert.ToSingle(cmds[2]);
                                col.Position.x = Convert.ToSingle(cmds[3]);
                                col.Position.y = Convert.ToSingle(cmds[4]);
                                col.Position.z = Convert.ToSingle(cmds[5]);
                                pnode.Collisions.Add(col);
                            }
                        }
                        else if (cmds[0].Contains("Mesh"))
                        {
                            pnode.Mesh = cmds[1];
                        }
                        else if (cmds[0].Contains("Parent"))
                        {
                            pnode.ParentName = cmds[1];
                        }
                        else if (cmds[0].Contains("Hidden"))
                        {
                            pnode.Visible = false;
                        }
                        else if (cmds[0].Contains("Separable"))
                        {
                            pnode.Separable = true;
                        }
                        else if (cmds[0].Contains("Attaching"))
                        {
                            JVector pos = new JVector();

                            pos.x = Convert.ToSingle(cmds[1]);
                            pos.y = Convert.ToSingle(cmds[2]);
                            pos.z = Convert.ToSingle(cmds[3]);

                            pnode.Local.RowX = pos;

                            pos.x = Convert.ToSingle(cmds[4]);
                            pos.y = Convert.ToSingle(cmds[5]);
                            pos.z = Convert.ToSingle(cmds[6]);

                            pnode.Local.RowY = pos;

                            pos.x = Convert.ToSingle(cmds[7]);
                            pos.y = Convert.ToSingle(cmds[8]);
                            pos.z = Convert.ToSingle(cmds[9]);

                            pnode.Local.RowZ = pos;
                            
                            pos.x = Convert.ToSingle(cmds[10]);
                            pos.y = Convert.ToSingle(cmds[11]);
                            pos.z = Convert.ToSingle(cmds[12]);

                            pnode.Local.Pos = pos;

                        }

                    }
                }
            } while (rd.EndOfStream == false);

            if (pnode != null)
            {
                NodesDict.Add(pnode.Name, pnode);
            }

            if (Root == null)
                throw new Exception("No root node!");

            foreach (HierNode node in NodesDict.Values)
            {
                if (node.ParentName != string.Empty)
                {
                    HierNode parent = NodesDict[node.ParentName.ToLower()];
                    parent.Nodes.Add(node);
                }
            }
            foreach (HierNode node in NodesDict.Values)
            {
                if (node.Mesh != string.Empty)
                {
                    node.MeshLoaded = new MeshObject();
                    try
                    {
                        if (node.Mesh.Contains("Flap") == true)
                        {
                            int d = 1;
                        }
                        node.MeshLoaded.LoadMsh(node.Mesh + ".msh");
                    }
                    catch (Exception e)
                    {
                        node.MeshLoaded = new MeshObject();
                        node.MeshLoaded.Name = node.Mesh;
                        node.MeshLoaded.Meshes = new MeshData();
                        node.MeshLoaded.Meshes.Name = node.Mesh;

                        ErrorMessages.Global.AddErrorMessage(string.Format("Failed to load {0} ({1})!\r\n", node.Mesh + ".msh",e.Message), 0);
                        
                    }

                    node.MeshLoaded.Meshes.HierarchyLevel = -1;
                    foreach (MeshData m in node.MeshLoaded.Lods)
                    {
                        m.HierarchyLevel = 0;
                    }
                }
            }
            rd.Close();
            CalcGlobal();

        }

        public void CalcGlobal()
        {
            if (Root == null) return;

            Root.Global = Root.Local;

            _TraverseCalcGlobal(Root);

        }

        public void _TraverseCalcGlobal(HierNode parent)
        {
            JMatrix parentMat = parent.Global;

            foreach (HierNode child in parent.Nodes)
            {
                child.Global = child.Local * parentMat;
                /*
                if (child.MeshLoaded != null)
                {
                    child.MeshLoaded.GlobalPosition = child.Global;
                    child.MeshLoaded.LocalPosition = child.Local;
                }
                /**/
                _TraverseCalcGlobal(child);
            }
        }


        public Node CreateNodes(NodeManager scene)
        {
            if (Root == null) return null;

            Node root = new Node();
            root.Scene = scene;

            scene.Nodes.Add(root);

            _CreateNode(Root, root);

            scene.Root.AddNode(root);
            return root;
        }

        protected void _CreateNode(HierNode hierParent, Node nodeParent)
        {

            nodeParent.Name = hierParent.Name;
            nodeParent.Local = hierParent.Local;
            nodeParent.Global = hierParent.Global;
            /**/
            if (hierParent.MeshLoaded != null)
            {
                //hierParent.MeshLoaded.LocalPosition = hierParent.Local;
                //hierParent.MeshLoaded.GlobalPosition = hierParent.Global;

                hierParent.MeshLoaded.AddToNode(nodeParent);
            }
            foreach (HierNode child in hierParent.Nodes)
            {
                Node childNode = new Node();
                childNode.Scene = nodeParent.Scene;
                _CreateNode(child, childNode);
                nodeParent.AddNode(childNode);
            }
        }
    }
}
