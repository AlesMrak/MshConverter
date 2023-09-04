using System;
using System.Collections.Generic;
using System.Text;


using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using Library3d.Math;
using Library3d.Mesh;
using Library3d.Nodes;
using Library3d.Error;

namespace Library3dDx9
{
    public class Utility3d
    {

        public static Matrix ToMatrix(JMatrix mat)
        {
            //mat = mat.Transpose(mat);
            Matrix m = new Matrix();
            m.M11 = mat.m(0, 0);
            m.M12 = mat.m(0, 1);
            m.M13 = mat.m(0, 2);
            m.M14 = mat.m(0, 3);
            m.M21 = mat.m(1, 0);
            m.M22 = mat.m(1, 1);
            m.M23 = mat.m(1, 2);
            m.M24 = mat.m(1, 3);
            m.M31 = mat.m(2, 0);
            m.M32 = mat.m(2, 1);
            m.M33 = mat.m(2, 2);
            m.M34 = mat.m(2, 3);
            m.M41 = mat.m(3, 0);
            m.M42 = mat.m(3, 1);
            m.M43 = mat.m(3, 2);
            m.M44 = mat.m(3, 3);

            //m.Transpose(m);

            return m;
        }

        public static List<Mesh> Get3dMeshes(Node root, Device d3dDevice)
        {
            List<Mesh> list = new List<Mesh>();

            foreach (Node child in root.Children)
            {
                _Get3dMeshes(child, ref list, d3dDevice);
            }

            return list;
        }

        private static void _Get3dMeshes(Node root, ref List<Mesh> meshes, Device d3dDevice)
        {
            foreach (NodeObject o in root.Objects)
            {
                if (o.GetType() == typeof(Library3d.Mesh.MeshData))
                {
                    Library3d.Mesh.MeshData ms = (Library3d.Mesh.MeshData)o;

                    Mesh msh = ConvertMeshData2Data(ms, d3dDevice);
                    if (msh != null)
                    {
                        meshes.Add(msh);
                    }

                }
            }
            foreach (Node child in root.Children)
            {
                _Get3dMeshes(child, ref meshes, d3dDevice);
            }
        }

        public static Mesh ConvertMeshData2Data(Library3d.Mesh.MeshData meshData, Device d3dDevice)
        {
            if (meshData.Faces.Count == 0) return null;

            try
            {
                SortedDictionary<int, Group> sortDict = new SortedDictionary<int, Group>();
                foreach (Group g in meshData.Groups)
                {
                    sortDict.Add(g.FaceStart, g);
                }

                Mesh mesh = null;

                if (meshData.Faces.Count == 0 || meshData.Vertexs.Count == 0) return null;

                if (meshData.UVs.Count > 0)
                {
                    mesh = new Mesh(meshData.Faces.Count, meshData.Vertexs.Count, MeshFlags.VbManaged | MeshFlags.IbManaged | MeshFlags.Use32Bit
                         , VertexFormats.Position | VertexFormats.Normal | VertexFormats.Texture1,
                         d3dDevice);
                }
                else
                {
                    mesh = new Mesh(meshData.Faces.Count, meshData.Vertexs.Count, MeshFlags.VbManaged | MeshFlags.IbManaged | MeshFlags.Use32Bit
                         , VertexFormats.Position | VertexFormats.Normal,
                         d3dDevice);
                }


                GraphicsStream vertStream = mesh.LockVertexBuffer(LockFlags.None);

                int index = 0;
                foreach (Vertex v in meshData.Vertexs)
                {
                    UV uv = null;
                    if (meshData.UVs.Count > 0 && meshData.UVs.Count >= meshData.Vertexs.Count)
                        uv = meshData.UVs[index];
                    else if (index > meshData.UVs.Count && meshData.UVs.Count > 0)
                    {
                        int ddd = 1;
                    }
                    if (uv == null)
                        vertStream.Write(new CustomVertex.PositionNormal(v.pos.x, v.pos.y, v.pos.z, v.n.x, v.n.y, v.n.z));
                    else
                        vertStream.Write(new CustomVertex.PositionNormalTextured(v.pos.x, v.pos.y, v.pos.z, v.n.x, v.n.y, v.n.z, uv.u, uv.v));
                    index++;
                }
                mesh.UnlockVertexBuffer();


                GraphicsStream indexStream = mesh.LockIndexBuffer(LockFlags.None);
                uint[] indices = new uint[meshData.Faces.Count * 3];

                int add = 0;
                index = 0;
                for (int i = 0; i < meshData.Faces.Count; i++)
                {
                    if (sortDict.ContainsKey(i) == true)
                    {
                        add = sortDict[i].VertexStart;
                    }
                    Face f = meshData.Faces[i];
                    indices[i * 3 + 0] = (uint)(f.v1 + add);
                    indices[i * 3 + 1] = (uint)(f.v2 + add);
                    indices[i * 3 + 2] = (uint)(f.v3 + add);

                }
                indexStream.Position = 0;
                indexStream.Write(indices);
                mesh.UnlockIndexBuffer();
                AttributeRange[] atable = null;

                if (meshData.Groups.Count > 0)
                {
                    index = 0;
                    atable = new AttributeRange[meshData.Groups.Count];
                    //atable = new AttributeRange[3];

                    if (meshData.Groups.Count > 3)
                    {
                        int d = 1;
                    }

                    foreach (Group g in meshData.Groups)
                    {
                        atable[index].AttributeId = index;
                        atable[index].FaceCount = g.FaceCount;
                        atable[index].FaceStart = g.FaceStart;
                        atable[index].VertexCount = g.VertexCount;
                        atable[index].VertexStart = g.VertexStart;
                        index++;
                        //if (index > 2) break;
                    }
                }
                else
                {
                    atable = new AttributeRange[1];
                    atable[0].AttributeId = 0;
                    atable[0].FaceCount = meshData.Faces.Count;
                    atable[0].FaceStart = 0;
                    atable[0].VertexCount = meshData.Vertexs.Count;
                    atable[0].VertexStart = 0;
                }
                if (atable != null)
                    mesh.SetAttributeTable(atable);

                //int[] adjacency = new int[meshData.Faces.Count * 3];
                //mesh.GenerateAdjacency(0.1f, adjacency);
                return mesh;
            }
            catch (Exception e)
            {
#warning TODO Exception handling
                int d = 1; d++;
                ErrorMessages.Global.AddErrorMessage(string.Format("Converting mesh {0} to directX9 => {1}\r\n",meshData.Name, e.Message),0);
            }
            return null;
            
        }
    }
}
