using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

using Library3d.Mesh;
using Library3d.Math;
using Library3d.Nodes;

namespace Library3d.Md5
{
    public class Md5Joint : NodeObject
    {
        public JVector Pos = new JVector();
        public JQuaternion Quart = new JQuaternion();
        public int Id = 0;
        public int ParentJoint = -1;

    }

    public class Md5Saver
    {
        public string FileName = string.Empty;
        public bool LimitNames10 = false;
        public void Save(string filename, Node root,bool [] options)
        {
            if (root == null) return;
            LimitNames10 = options[5];

            FileName = filename;

            StreamWriter rw = File.CreateText(filename);
            if (rw == null) return;

            List<Md5Joint> nodes = new List<Md5Joint>();
            List<MeshData> meshes = new List<MeshData>();

            Md5Joint origin = new Md5Joint();
            origin.Name = "origin";
            origin.Id = 0;
            nodes.Add(origin);
            int jc = 1;
            foreach(Node node in root.Children)
                _TraverseNode(node, origin, ref nodes, ref meshes, ref jc);

            string command = string.Format("mesh {0} -game Doom ", filename);
            /*
            foreach (Md5Joint j in nodes)
                command += string.Format("keep -{0} ", j.Name);
            foreach (MeshData m in meshes)
                command += string.Format("keepmesh -{0} ", m.Name);
            /**/

            rw.WriteLine("MD5Version 10");
            rw.WriteLine("commandline  \"{0}\"", command);
            rw.WriteLine("");
            rw.WriteLine("numJoints\t{0}",nodes.Count);
            rw.WriteLine("numMeshes\t{0}\r\n", meshes.Count);


            rw.WriteLine("joints {");
            foreach (Md5Joint joint in nodes)
            {
                rw.WriteLine("\t \"{0}\" {1} ( {2} ) ( {3} ) \t//{4}", LimitMaxString(joint.Name), joint.ParentJoint, joint.Pos, joint.Quart, joint.Name);
            }
            rw.WriteLine("}\r\n");

            foreach (MeshData mesh in meshes)
            {
                if (mesh.Type == eNodeObjectType.e3dMesh && options[0] == true ||
                    mesh.Type == eNodeObjectType.e3dMeshLod && options[1] == true ||
                    mesh.Type == eNodeObjectType.e3dMshShadow && options[2] == true ||
                    mesh.Type == eNodeObjectType.eMshHook && options[3] == true ||
                    mesh.Type == eNodeObjectType.e3dMeshCollision && options[4] == true
                    )
                {


                    foreach (Group g in mesh.Groups)
                    {
                        rw.WriteLine("mesh {");
                        rw.WriteLine("\t// meshes: {0}", LimitMaxString(mesh.Name));
                        rw.WriteLine("\tshader \"{0}\"\r\n", LimitMaxString(mesh.Materials[g.Id].Name));

                        rw.WriteLine("\tnumverts  {0}", g.VertexCount);

                        int count = 0;

                        for (int i = g.VertexStart; i < g.VertexStart + g.VertexCount; i++)
                        {
                            UV uv = mesh.UVs[i];
                            float u = (uv.u+ MeshObject.Uoffset) * MeshObject.Uscale;
                            float v = (uv.v + MeshObject.Voffset) * MeshObject.Vscale;
                            rw.WriteLine("\tvert {0} ( {1} {2} ) {3} 1", count, v, u, count);
                            count++;
                        }
                        rw.WriteLine("");

                        rw.WriteLine("\tnumtris  {0}", g.FaceCount);
                        count = 0;
                        for (int j = g.FaceStart; j < g.FaceStart + g.FaceCount; j++)
                        {
                            Face f = mesh.Faces[j];
                            rw.WriteLine("\ttri {0} {3} {2} {1}", count, f.v1, f.v2, f.v3);
                            count++;
                        }
                        rw.WriteLine("");

                        rw.WriteLine("\tnumweights  {0}", g.VertexCount);
                        count = 0;
                        for (int k = g.VertexStart; k < g.VertexStart + g.VertexCount; k++)
                        {
                            Vertex v = mesh.Vertexs[k];
                            rw.WriteLine("\tweight {0} {1} 1 ( {2} {3} {4} )", count, mesh.ModelNumber, v.pos.x, v.pos.y, v.pos.z);
                            count++;
                        }
                        rw.WriteLine("");

                        rw.WriteLine("}\r\n");
                    }
                }
            }

            rw.Close();
        }

        public void _TraverseNode(Node root,Md5Joint pJoint,ref List<Md5Joint> nodes, ref List<MeshData> meshes,ref int jc)
        {
            if (root == null) return;

            Md5Joint md5j = new Md5Joint();
            md5j.Name = Path.GetFileNameWithoutExtension(root.Name);
            md5j.Id = jc;
            if (pJoint != null)
            {
                md5j.ParentJoint= pJoint.Id;
            }
            md5j.Pos = root.Global.Pos;
            md5j.Quart = md5j.Quart.FromMatrix(root.Global);

            nodes.Add(md5j);
            

            foreach (NodeObject no in root.Objects)
            {
                if (no.GetType() == typeof(MeshData) && no.Visible == true)
                {
                    MeshData msh = (MeshData)no;
                    msh.ModelNumber = jc;
                    meshes.Add(msh);
                }
            }
            jc++;

            foreach (Node child in root.Children)
            {
                _TraverseNode(child,md5j, ref nodes, ref meshes,ref jc);
            }

        }

        protected string LimitMaxString(string Name)
        {
            return Name;
            if (LimitNames10 == true)
            {
                if (Name.Length > 10)
                {
                    string name = string.Empty;

                    for (int i = 0; i < 10; i++)
                    {
                        name += Name[i];
                    }
                    return name;

                }
            }
            return Name;
        }

    }
}
