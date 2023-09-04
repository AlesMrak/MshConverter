using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Globalization;
using Scanning;

using Library3d.Math;
using Library3d.Nodes;


namespace Library3d.Mesh
{
    public partial class MeshObject : NodeObject
    {
        public void LoadObj(string fileName, Node root)
        {
            //if (root == null) return;

            CultureInfo ci = new CultureInfo("en-us");
            StreamReader sr = File.OpenText(fileName);


            //MeshData mesh = new MeshData();

            Material material = null;
            Group group = null;
            string line = string.Empty;

            List<MeshData> meshes = new List<MeshData>();
            int vertex_index=0;
            int normal_index=0;
            int texture_index=0;
            int face_index=0;
            int material_index = 0;
            int last_vertex_index=0;
            int last_texture_index = 0;
            int last_face_index=0;

            int[] max_vert = new int[3]  { -99999999,-99999999,-99999999};
            int[] min_vert = new int[3] { 99999999, 99999999, 99999999 }; ;

            Node meshNode = new Node();
            //root.AddNode(meshNode);
            if (root != null)
                root.AddNode(meshNode);
            //meshNode.Name = Path.GetFileNameWithoutExtension(fileName);
            meshNode.Name = Path.GetFileName(fileName);
            
            char[] split = { ' ', '\t' };
            do
            {
                string [] strings = null;
                line = sr.ReadLine();
                line = line.Trim().ToLower();
                strings = line.Split(split);

                bool addnewMat = true;

                //if (line.StartsWith("mtllib") || line.StartsWith("usemtllib") || line.StartsWith("usemtl"))
                if (line.StartsWith("usemtllib") || line.StartsWith("usemtl"))
                {
                    if (mesh != null)
                    {
                        if (material != null && material.Name != strings[1])
                        {
                            material_index = mesh.AddMaterial(material);

                        }
                        else if(material!=null)
                        {
                            addnewMat = false;
                        }
                        if (group != null )
                        {
                            if (addnewMat == true)
                            {
                                mesh.AddGroup(group, max_vert[0] - min_vert[0] + 1, face_index);


                                group = null;


                                last_vertex_index += vertex_index;
                                last_texture_index += texture_index;
                                vertex_index = 0;
                                normal_index = 0;
                                texture_index = 0;
                                face_index = 0;

                                max_vert = new int[3] { -99999999, -99999999, -99999999 };
                                min_vert = new int[3] { 99999999, 99999999, 99999999 }; ;
                            }
                        }
                    }
                    if (addnewMat == true)
                    {
                        material = new Material();
                        material.Name = strings[1];

                        material_index = mesh.AddMaterial(material);

                        group = new Group();
                        group.Id = material_index;
                    }
                    //group.VertexStart = last_vertex_index;
                    //group.FaceStart = last_face_index;

                }
                else if(line.StartsWith("g "))
                {
                    if (line.Contains("x_head")==true)
                    {
                        int ddx = 1;
                        ddx++;
                    }
                    if (mesh != null)
                    {
                        if (group != null)
                        {
                            mesh.AddGroup(group, max_vert[0] - min_vert[0] + 1, face_index);
                        }

                        material = null; 

                        material_index = 0;
                        mesh.FindType();
                        mesh.Normalize();
                        meshes.Add(mesh);
                        face_index = 0;
                        vertex_index = 0;
                        normal_index = 0;
                        texture_index = 0;
                        max_vert = new int[3] { -99999999, -99999999, -99999999 };
                        min_vert = new int[3] { 99999999, 99999999, 99999999 }; ;

                        mesh.FindType();
                    }

                    mesh = new MeshData();

                    if (strings.Length > 1)
                    {
                        for (int j = 1; j < strings.Length; j++)
                        {
                            if (j != 1) mesh.Name += " ";
                            mesh.Name += strings[j];
                        }
                    }
                    if (mesh.Name != string.Empty)
                    {
                        material_index = 0;
                        group = null;

                        mesh.Type = Library3d.Nodes.eNodeObjectType.e3dMesh;
                        

                    }
                    

                }
                else if (line.StartsWith("v "))
                {
                    JVector pos = new JVector();
                    int posv =0;
                    for(int i=1;i<strings.Length;i++)
                    {
                        if(strings[i]!=string.Empty)
                        {
                            pos.Set(posv, Convert.ToSingle(strings[i]));
                            posv++;
                            if(posv>3) break;
                        }
                    }

                    if (mesh != null)
                    {
                        //mesh.Positions.Add(pos);
                        /**/
                        Vertex v = new Vertex();
                        v.pos = pos;
                        mesh.Vertexs.Add(v);
                        vertex_index++;
                        /**/
                    }
                }
                else if (line.StartsWith("vt"))
                {
                    JVector uv = new JVector();

                    int posv =0;
                    for (int i = 1; i < strings.Length; i++)
                    {
                        if (strings[i] != string.Empty)
                        {
                            uv.Set(posv, Convert.ToSingle(strings[i]));
                            posv++;
                            if (posv > 2) break;
                        }
                    }

                    if (mesh != null)
                    {
                        UV uvs = new UV();
                        uvs.u = uv.x;
                        uvs.v = uv.y;
                        mesh.UVs.Add(uvs);
                    }

                    texture_index++;
                }
                else if (line.StartsWith("f "))
                {
                    
                    if (strings.Length >= 4)
                    {
                        
                        Face f = new Face();
                        f.separateIndex = true;
                        char [] splitf = {'\\','/'};

                        int c=0;
                        for (int i = 1; i < strings.Length; i++)
                        {
                            int vindex = 0;
                            int nindex = 0;
                            int tindex = 0;

                            string []findex = strings[i].Split(splitf);
                            if (findex[0] == string.Empty) continue;
                            int cout = 0;

                            for (int ii = 0; ii < findex.Length; ii++)
                            {
                                if (findex[ii] == string.Empty) continue;
                                if (cout == 0)
                                {
                                    vindex = Convert.ToInt32(findex[ii]);
                                    f.v(c, vindex -1);
                                    cout++;

                                }
                                else if (cout ==1)
                                {
                                    tindex = Convert.ToInt32(findex[ii]);
                                    f.t(c, tindex-1);
                                    cout++;
                                }
                                else  if (cout == 2)
                                {
                                    nindex = Convert.ToInt32(findex[ii]);
                                    f.n(c, nindex-1);
                                    cout++;
                                }

                                if (vindex != nindex && nindex>0)
                                {
                                    int nosupport = 1;
                                    nosupport++;

                                    if(mesh!=null)
                                    {
                                        if (nindex - 1 >= mesh.Vertexs.Count)
                                        {
                                            throw new Exception("This OBJ file has invalid data, index of normal face is greater than writen normlas count");
                                        }
                                        mesh.Vertexs[nindex-1].pos = mesh.Vertexs[vindex-1].pos;
                                    }
                                }

                            }
                            c++;
                        }
                        if (mesh != null)
                        {
                            for (int mm = 0; mm < 3; mm++)
                            {
                                if (min_vert[0] > f.v(mm))
                                    min_vert[0] = f.v(mm);
                                if (max_vert[0] < f.v(mm))
                                    max_vert[0] = f.v(mm);
                            }

                            mesh.Faces.Add(f);
                            face_index++;
                        }
                    }
                }
                else if (line.StartsWith("vn"))
                {
                    JVector n = new JVector();
                    int posv = 0;
                    for (int i = 1; i < strings.Length; i++)
                    {
                        if (strings[i] != string.Empty)
                        {
                            n.Set(posv, Convert.ToSingle(strings[i]));
                            posv++;
                            if (posv > 3) break;
                        }
                    }
                    
                    if (mesh != null)
                    {
                        //mesh.Normals.Add(n);
                        if (mesh.Vertexs.Count <= normal_index)
                        {
                            Vertex v = new Vertex();
                            v.n = n;
                            mesh.Vertexs.Add(v);
                        }
                        else
                        {
                            mesh.Vertexs[normal_index].n = n;
                        }
                    }
                    normal_index++;
                }
            }
            while (sr.EndOfStream == false);


            if (mesh != null)
            {
                if (material != null)
                {
                    material_index = mesh.AddMaterial(material);
                }
                if (group != null)
                {
                    group.Id = material_index;
                    mesh.AddGroup(group, max_vert[0] - min_vert[0]+1, face_index);
                }
                //mesh.Normalize2();
                mesh.Normalize();
                meshes.Add(mesh);
            }

            if (meshNode != null)
            {
                foreach (MeshData m in meshes)
                {
                    m.FindType();
                    meshNode.AddObject(m);
                }
            }
        }
    }
}
