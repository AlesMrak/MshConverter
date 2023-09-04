using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

using Library3d.Mesh;
using Library3d.Math;

namespace Library3d.Format3ds
{
    public class Group3ds : Group
    {
        public string MaterialName = string.Empty;
        public List<int> Faces = new List<int>();
        public int MinIndex = 9999999;
        public int MaxIndex = -99999999;
    }

    public class Mesh3dsParent
    {
        public string Name;
        public int Flags1=0;
        public int Flags2= 0;
        public int Parent = -1;
    }

    public class Mesh3dsId
    {
        public string Name;
        public int Id = 0;
    }

    public partial class Object3ds
    {

        protected List<Vertex> ReadVertex(ref BinaryReader br)
        {
            List<Vertex> list = new List<Vertex>();
            ushort count = br.ReadUInt16();
            for (int i = 0; i < count; i++)
            {
                float x = br.ReadSingle();
                float y = br.ReadSingle();
                float z = br.ReadSingle();

                Vertex v = new Vertex();
                v.pos = new JVector(x, y, z);
                list.Add(v);
            }
            return list;
        }

        protected List<Face> ReadFaces(ref BinaryReader br)
        {
            List<Face> list = new List<Face>();
            ushort count = br.ReadUInt16();
            for (int i = 0; i < count; i++)
            {
                ushort v1 = br.ReadUInt16();
                ushort v2 = br.ReadUInt16();
                ushort v3 = br.ReadUInt16();
                ushort flags = br.ReadUInt16();


                Face f = new Face();
                f.id = i;
                f.v1 = v1;
                f.v2 = v2;
                f.v3 = v3;

                
                if (f.v1 < 0 || f.v2 < 0 || f.v3 < 0)
                {
                    int d = 1;
                }

                list.Add(f);

            }
            return list;
        }

        protected List<UV> ReadUVs(ref BinaryReader br)
        {
            List<UV> list = new List<UV>();
            ushort count = br.ReadUInt16();
            for (int i = 0; i < count; i++)
            {
                float u = br.ReadSingle();
                float v = br.ReadSingle();

                UV uv = new UV();
                uv.u = u;
                //uv.v = v * -1.0f;
                uv.v = v;

                list.Add(uv);

            }
            return list;
        }


        protected Group3ds ReadGroup(ref BinaryReader br,MeshData mesh)
        {
            Group3ds gr = new Group3ds();

            gr.Name = gr.MaterialName = ReadString(ref br);
            ushort count  = br.ReadUInt16();

            int min = 99999999;
            int max = -99999999;

            for (int i = 0; i < count; i++)
            {
                ushort face = br.ReadUInt16();
                gr.Faces.Add(face);

                if (face < min) min = face;
                if (face > max) max = face;
            }

            gr.FaceCount = max - min + 1;
            gr.FaceStart = min;

            Dictionary<int, int> vertexIds = new Dictionary<int, int>();
            for (int i = 0; i < gr.FaceCount; i++)
            {
                Face c = mesh.Faces[gr.FaceStart + i];
                for (int j = 0; j < 3; j++)
                {
                    if (vertexIds.ContainsKey(c.v(j)) == false)
                        vertexIds.Add(c.v(j), c.v(j));
                }
            }
            gr.VertexCount = vertexIds.Count;
          
            return gr;

        }


        protected List<ushort> ReadSmoothing(ref BinaryReader br, int faces)
        {
            List<ushort> list = new List<ushort>();

            for (int i = 0; i < faces; i++)
            {
                ushort val = (ushort)br.ReadUInt32();
                list.Add(val);
            }
            return list;
        }

       
    }
}
