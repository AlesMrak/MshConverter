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
        int current_lod = 0;
        //MeshData mesh = new MeshData();
        MeshData mesh = null;

        public static string RemoveComment(string str)
        {
            string res = string.Empty;
            string[] sep = { "//" };
            string[] vals = str.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            if (vals.Length > 0)
            {
                res = vals[0];
            }
            return res.Trim() ;
        }


        public bool LoadText(string fileName)
        {
            CultureInfo ci = new CultureInfo("en-us");

            StreamReader sr = File.OpenText(fileName);

            mesh = new MeshData();
            mesh.ParentMeshObject = this;
            mesh.HierarchyLevel = -1;
            mesh.Name = this.Name;

            current_lod = 0;

            MeshCollisionBlock currentBlock = null;
            MeshCollisionFrame currentFrame = null;
            MeshCollisionPart currentPart = null;


            bool readLine = true;
            string line = string.Empty;
            while (sr.EndOfStream == false)
            {
                if (readLine == true)
                    line = sr.ReadLine();
                else readLine = true;

                if (line.Contains("Materials"))
                {
                    GetMesh(line);

                    line = sr.ReadLine();
                    line = GetValue(line);
                    while (line != string.Empty)
                    {
                        Material m = new Material();
                        m.Name = MeshObject.RemoveComment(line.Trim());
                        mesh.Materials.Add(m);
                        
                        line = sr.ReadLine();
                        line = GetValue(line);
                    }

                }
                else if (line.Contains("[LOD]"))
                {
                    int c = 0;
                    // LOD count
                    
                }
                else if (line.Contains("ShVertices_Frame0]"))
                {
                    GetMesh(line);
                    Scanner scanner = new Scanner();
                    object[] targets = new object[6];
                    targets[0] = new Single();
                    targets[1] = new Single();
                    targets[2] = new Single();

                    line = sr.ReadLine();

                    line = GetValue(line);
                    while (line != string.Empty)
                    {
                        Vertex vertex = new Vertex();

                        char[] splt = { ' ', '\t' };
                        string[] tokens = line.Split(splt,StringSplitOptions.RemoveEmptyEntries);
                        ArrayList l = new ArrayList();
                        foreach (string s in tokens)
                        {
                            float f = 0;
                            Single.TryParse(s, (NumberStyles.Float | NumberStyles.AllowThousands), ci, out f);

                            l.Add(f);
                        }

                        vertex.pos.x = (float)l[0];
                        vertex.pos.y = (float)l[1];
                        vertex.pos.z = (float)l[2];

                        mesh.Vertexs.Add(vertex);

                        line = sr.ReadLine();
                        line = GetValue(line);
                    }
                }
                else if (line.Contains("[CoVer"))
                {
                   
                    currentFrame = new MeshCollisionFrame();

                    Scanner scanner = new Scanner();
                    object[] targets = new object[6];
                    targets[0] = new Single();
                    targets[1] = new Single();
                    targets[2] = new Single();

                    line = sr.ReadLine();

                    line = GetValue(line);
                    while (line != string.Empty)
                    {
                        Vertex vertex = new Vertex();

                        char[] splt = { ' ', '\t' };
                        string[] tokens = line.Split(splt,StringSplitOptions.RemoveEmptyEntries);
                        ArrayList l = new ArrayList();
                        foreach (string s in tokens)
                        {
                            float f = 0;
                            Single.TryParse(s, (NumberStyles.Float | NumberStyles.AllowThousands), ci, out f);

                            l.Add(f);
                        }

                        vertex.pos.x = (float)l[0];
                        vertex.pos.y = (float)l[1];
                        vertex.pos.z = (float)l[2];


                        if (currentFrame != null)
                        {
                            currentFrame.Vertexs.Add(vertex);
                        }

                        line = sr.ReadLine();
                        line = GetValue(line);
                    }
                    if (currentFrame != null)
                    {
                        currentPart.CollisionFrames.Add(currentFrame);
                    }


                }
                else if (line.Contains("CoFac_"))
                {
                    Scanner scanner = new Scanner();
                    object[] targets = new object[3];
                    targets[0] = new Int32();
                    targets[1] = new Int32();
                    targets[2] = new Int32();

                    line = sr.ReadLine();
                    line = GetValue(line);
                    while (line != string.Empty)
                    {
                        scanner.Scan(line, "{0} {1} {2}", targets);

                        int v1 = (int)targets[0];
                        int v2 = (int)targets[1];
                        int v3 = (int)targets[2];

                        if (currentFrame != null)
                        {
                            Face f = new Face();
                            f.v1 = v1;
                            f.v2 = v2;
                            f.v3 = v3;
                            currentFrame.Faces.Add(f);
                        }
                        line = sr.ReadLine();
                        line = GetValue(line);
                    }
                }
                else if (line.Contains("CoCommon"))
                {
                    string cleandef = line.Replace("[", string.Empty);
                    cleandef = cleandef.Replace("]", string.Empty);
                    string[] parts = cleandef.Split('_');

                    if (parts.Length == 1)
                    {
                    }
                    else if (parts.Length == 2)
                    {
                        string[] subparts = parts[1].Split('p');

                        if (subparts.Length == 2)
                        {
                            //part
                            if (currentPart != null)
                            {
                                currentBlock.Parts.Add(currentPart);
                            }
                            currentPart = new MeshCollisionPart();
                            currentPart.ParentMeshObject = this;
                        }
                        else if (subparts.Length == 1)
                        {
                            //block
                            if (currentBlock != null)
                            {
                                this.Collisions.Add(currentBlock);
                            }
                            currentBlock = new MeshCollisionBlock();
                        }
                    }

                    line = sr.ReadLine();
                    line = GetValue(line);
                    while (line != string.Empty)
                    {
                        char[] splt = { ' ', '\t' };
                        string[] vals = line.Split(splt,StringSplitOptions.RemoveEmptyEntries);

                        if (vals.Length != 2) break;

                        string name = vals[0];
                        string val = vals[1];

                        if (name == "NBlocks")
                        {

                        }
                        else if (name == "NParts")
                        {
                            if (currentBlock != null)
                                currentBlock.NParts = Convert.ToInt32(val);
                        }
                        else if (name == "ParentBone")
                        {
                            if (currentBlock != null)
                                currentBlock.ParentBone = Convert.ToInt32(val);
                        }
                        else if (name == "Type")
                        {
                            if (currentPart != null)
                                currentPart.Type = val;
                        }
                        else if (name == "NFrames")
                        {
                            if (currentPart != null)
                                currentPart.NFrames = Convert.ToInt32(val);
                        }
                        else if (name == "Name")
                        {
                            if (currentPart != null)
                                currentPart.Name = val;
                        }
                        else if (name!=string.Empty)
                        {
                            if (currentFrame != null)
                            {
                                if(currentFrame.DynamicValues.ContainsKey(name)==false)
                                {
                                    currentFrame.DynamicValues.Add(name, val);
                                }
                            }
                        }

                        line = sr.ReadLine();
                        line = GetValue(line);
                    }

                }
                else if (line.Contains("CoNei_"))
                {

                }
                else if (line.Contains("CoNeiCnt_"))
                {

                }
                else if (line.Contains("[Hooks]"))
                {

                    line = sr.ReadLine();
                    line = GetValue(line);
                    while (line != string.Empty)
                    {
                        char[] splt = { ' ', '\t' };
                        string[] val = line.Split(splt,StringSplitOptions.RemoveEmptyEntries);
                        if (val.Length != 2) break;

                        Hook hook = new Hook();
                        hook.Name = val[0];
                        hook.Value = val[1];
                        this.Hooks.Add(hook);

                        line = sr.ReadLine();
                        line = GetValue(line);
                    }
                }
                else if (line.Contains("Vertices_Frame0]"))
                {
                    GetMesh(line);

                    Scanner scanner = new Scanner();
                    object[] targets = new object[6];
                    targets[0] = new Single();
                    targets[1] = new Single();
                    targets[2] = new Single();
                    targets[3] = new Single();
                    targets[4] = new Single();
                    targets[5] = new Single();

                    line = sr.ReadLine();
                    line = GetValue(line);
                    while (line != string.Empty)
                    {
                        Vertex vertex = new Vertex();
                        char[] splt = { ' ', '\t' };
                        string[] tokens = line.Split(splt,StringSplitOptions.RemoveEmptyEntries);

                        if (tokens.Length != 6) break;

                        ArrayList l = new ArrayList();
                        foreach (string s in tokens)
                        {
                            float f = 0;
                            Single.TryParse(s, (NumberStyles.Float | NumberStyles.AllowThousands), ci, out f);

                            l.Add(f);
                        }

                        vertex.pos.x = (float)l[0];
                        vertex.pos.y = (float)l[1];
                        vertex.pos.z = (float)l[2];

                        vertex.n.x = (float)l[3];
                        vertex.n.y = (float)l[4];
                        vertex.n.z = (float)l[5];

                        mesh.Vertexs.Add(vertex);

                        line = sr.ReadLine();
                        line = GetValue(line);
                    }
                }
                else if (line.Contains("MaterialMapping"))
                {
                    GetMesh(line);
                    Scanner scanner = new Scanner();
                    object[] targets = new object[6];
                    targets[0] = new Single();
                    targets[1] = new Single();

                    line = sr.ReadLine();
                    line = GetValue(line);
                    while (line != string.Empty)
                    {
                        UV m = new UV();

                        char[] splt = { ' ', '\t' };
                        string[] tokens = line.Split(splt,StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.Length != 2) break;

                        ArrayList l = new ArrayList();
                        foreach (string s in tokens)
                        {
                            float f = 0;
                            Single.TryParse(s, (NumberStyles.Float | NumberStyles.AllowThousands), ci, out f);

                            l.Add(f);
                        }

                        m.u = (float)l[0];
                        m.v = (float)l[1];


                        mesh.UVs.Add(m);

                        line = sr.ReadLine();
                        line = GetValue(line);
                    }
                }
                else if (line.Contains("Faces]"))
                {
                    GetMesh(line);

                    Scanner scanner = new Scanner();
                    object[] targets = new object[3];
                    targets[0] = new Int32();
                    targets[1] = new Int32();
                    targets[2] = new Int32();

                    line = sr.ReadLine();
                    line = line.Trim();
                    while (line != string.Empty)
                    {
                        Face m = new Face();
                        scanner.Scan(line, "{0} {1} {2}", targets);

                        if (targets.Length != 3)
                        {
                            continue;
                        }

                        m.v1 = (int)targets[0];
                        m.v2 = (int)targets[1];
                        m.v3 = (int)targets[2];

                        mesh.Faces.Add(m);
                        line = sr.ReadLine();
                        line = GetValue(line);
                    }

                    int d = 1;
                }
                else if (line.Contains("FaceGroups]"))
                {
                    GetMesh(line);
                    Group g = null;

                    line = sr.ReadLine();
                    line = GetValue(line);

                    while (line != string.Empty)
                    {
                        char[] splt = { ' ', '\t' };
                        string[] values = line.Split(splt, StringSplitOptions.RemoveEmptyEntries);
                        if (values.Length >= 5)
                        {
                            if (g != null)
                            {
                                mesh.Groups.Add(g);
                            }
                            g = new Group();

                            g.Id = Convert.ToInt32(values[0]);
                            g.VertexStart = Convert.ToInt32(values[1]);
                            g.VertexCount = Convert.ToInt32(values[2]);
                            g.FaceStart = Convert.ToInt32(values[3]);
                            g.FaceCount = Convert.ToInt32(values[4]);

                        }
                        else if (values.Length == 2)
                        {

                        }
                        else
                        {
                            break;
                        }

                        line = sr.ReadLine();
                        line = GetValue(line);
                    }

                    if (g != null)
                    {
                        mesh.Groups.Add(g);
                    }

                }
                else if (line.Contains("Space_Frame0]"))
                {
                    GetMesh(line);
                }
                else if (line.Contains("HookLoc"))
                {
                    line = sr.ReadLine();
                    line = line.Trim();
                    int count = 0;
                    while (line != string.Empty)
                    {
                        char[] splt = { ' ', '\t' };
                        string[] vals = line.Split(splt, StringSplitOptions.RemoveEmptyEntries);

                        if (vals.Length <12) break;


                        Hook hook = this.Hooks[count];
                        hook.Position = new JMatrix();
                        hook.Position.RowX = new JVector(float.Parse(vals[0]), float.Parse(vals[1]), float.Parse(vals[2]));
                        hook.Position.RowY = new JVector(float.Parse(vals[3]), float.Parse(vals[4]), float.Parse(vals[5]));
                        hook.Position.RowZ = new JVector(float.Parse(vals[6]), float.Parse(vals[7]), float.Parse(vals[8]));
                        hook.Position.Pos = new JVector(float.Parse(vals[9]), float.Parse(vals[10]), float.Parse(vals[11]));
                        this.Hooks[count] = hook;

                        line = sr.ReadLine();
                        line = GetValue(line);

                        count++;
                    }
                }


            }
            sr.Close();

            if (currentBlock != null)
            {
                this.Collisions.Add(currentBlock);
            }
            if (currentPart != null)
            {
                currentBlock.Parts.Add(currentPart);
            }
            if (currentFrame != null)
            {
                //currentPart.CollisionFrames.Add(currentFrame);
            }

            if(mesh.Name.EndsWith(" SH")==true)
            {
                if (current_lod > 0)
                {
                    this.Lods[current_lod-1].Shadow = mesh;
                }
            }
            else
            if (current_lod > 0)
            {
                this.Lods.Add(mesh);
            }
            else if (current_lod == 0)
            {
                this.Meshes = mesh;
            }

            return true;
        }

        protected void GetMesh(string line)
        {
            
            if (line.Contains("ShVertices_Frame0") == true)
            {
                /**/
                int lod_num = 0;
                string[] names = line.Split('_');
                if (names.Length > 2)
                {
                    int end = names[0].Length + 1;
                    string number = names[0].Substring(4, end - 5);
                    lod_num = Convert.ToInt32(number);
                }

                if (mesh != null)
                {
                    if (lod_num == 0)
                    {
                        mesh.Type = eNodeObjectType.e3dMesh;
                        this.Meshes = mesh;
                    }
                    else
                    {
                        mesh.Type = eNodeObjectType.e3dMeshLod;
                        this.Lods.Add(mesh);
                    }
                }
                MeshData parent = mesh;

                mesh = new MeshData();
                mesh.Type = eNodeObjectType.e3dMshShadow;
                mesh.ParentMeshObject = this;
                parent.Shadow = mesh;
                if(lod_num>0)
                    mesh.Name = string.Format("L{0} {1} SH", lod_num, this.Name);
                else
                    mesh.Name = string.Format("{0} SH", this.Name);

                

                mesh.HierarchyLevel = 0;
                /**/
            }
            else if (line.Contains("ShFaces]") == true)
            {

            }
            else if (line.Contains("[LOD") == true && line.Contains("[LOD]") == false)
            {
                int end = line.IndexOf("_") + 1;
                string number = line.Substring(4, end - 5);
                int newLod = Convert.ToInt32(number);
                if (current_lod != newLod)
                {
                    if (current_lod > 0)
                    {
                        if (mesh.Type != eNodeObjectType.e3dMshShadow)
                        {
                            mesh.Type = eNodeObjectType.e3dMeshLod;
                            this.Lods.Add(mesh);
                        }
                    }
                    else
                    {
                        if (mesh.Type != eNodeObjectType.e3dMshShadow)
                        {
                            mesh.Type = eNodeObjectType.e3dMesh;
                            this.Meshes = mesh;
                        }
                    }
                    current_lod = newLod;

                    mesh = new MeshData();

                    mesh.ParentMeshObject = this;
                    mesh.Name = string.Format("L{0}", newLod);
                    mesh.HierarchyLevel = 0;
                }
            }

        }

        protected string GetValue(string line)
        {
            string v = line.Trim();
            if (line.Length >= 2)
            {
                int commStart = line.IndexOf(@"//", 0, 2);
                if (commStart >= 0)
                {
                    v = v.Remove(commStart);
                }
            }
            return v;

        }
    }

}
