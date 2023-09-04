using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Windows.Forms;

using Library3d.Math;
using Library3d.Nodes;
using Library3d.Interface;
using Library3d.Error;

using Scanning;

namespace Library3d.Mesh
{
    /*
     struct FaceGroups { 
   *word    numVerts 
   *word    numFaces 
   struct Each_FG { 
       *word    GroupID 
       *word    VertStart 
       *word    VertCount 
       *word    FaceStart 
       *word    FaceCount 
       *word    pad0         //must be 0 
   } 
} 
it maybe byte, see Data_Container section! 
/**/

    public class XValue
    {
        public int CountZero;
        public int HeaderVal;
        public int Offset;
    }
    public class MshItem
    {
        public long StartOffset = 0;
        public long EndOffset = 0;
        public long Lenght = 0;
        public int Index = 0;
        public int Number1 = 0;
        public int Number2 = 0;
        public int Number3 = 0;
        public byte NumberOfBytes = 0;
        public byte Signed = 0;
    }

    public class Material : NodeObject
    {
        public List<MaterialLayer> Layers = new List<MaterialLayer>();
        public int tfDoubleSide = 0;
        public int tfShouldSort=0;
        public int tfDropShadow=0;
        public int tfGameTimer=1;
        //Light Params
        public double Ambient=1.0;
        public double Diffuse = 1.0;
        public double Specular = 0.4;
        public double SpecularPow = 32;
        public double Shine = 0.0;

    }

    public class MaterialLayer : NodeObject
    {
        public string TextureFileName = string.Empty;
        public string ClassName;
        public int tfModulate=1;
        public int tfNoWriteZ=1;            
        public int tfMinLinear=1;            
        public int tfMagLinear=1;           
        public int tfBlend=1;
        public int tfMipMap=0;
        public int tfNoDegradation = 1;
    }

    public class UV
    {
        public float u;
        public float v;
    }

    public class Face
    {
        public int group_id;
        public int id;
        public int v1;
        public int v2;
        public int v3;

        public int t1;
        public int t2;
        public int t3;
        public bool separateIndex = false;

        public int n1;
        public int n2;
        public int n3;

        public int v(int index)
        {
            if (index == 0) return v1;
            if (index == 1) return v2;
            if (index == 2) return v3;
            throw new Exception("Error! Out of bounds!");

        }

        public void v(int index,int val)
        {
            if (index == 0) v1 = val;
            else if (index == 1) v2 = val;
            else if (index == 2) v3 = val;
            else throw new Exception("Error! Out of bounds!");

        }

        public void t(int index, int val)
        {
            if (index == 0) t1 = val;
            else if (index == 1) t2 = val;
            else if (index == 2) t3 = val;
            else throw new Exception("Error! Out of bounds!");

        }

        public int t(int index)
        {
            if (index == 0) return t1;
            else if (index == 1) return t2;
            else if (index == 2) return t3;
            else throw new Exception("Error! Out of bounds!");

        }

        public void n(int index, int val)
        {
            if (index == 0) n1 = val;
            else if (index == 1) n2 = val;
            else if (index == 2) n3 = val;
            else throw new Exception("Error! Out of bounds!");

        }

        public int n(int index)
        {
            if (index == 0) return n1;
            else if (index == 1) return n2;
            else if (index == 2) return n3;
            else throw new Exception("Error! Out of bounds!");

        }


    }

    public class Group : NodeObject
    {
        public int Id = -1;
        public int VertexStart = 0;
        public int VertexCount = 0;
        public int FaceStart = 0;
        public int FaceCount = 0;
        public List<Face> Faces = null;
    }

    public class Vertex
    {
        public JVector pos = new JVector();
        public JVector uv = new JVector();
        public JVector n = new JVector();

        public Vertex()
        {

        }
        public Vertex(JVector p)
        {
            pos = p;
        }
    }


    public class MeshCollisionBlock : NodeObject
    {
        public int ParentBone = -1;
        public int NParts = 0;
        public int Id = 0;
                
        public List<MeshCollisionPart> Parts = new List<MeshCollisionPart>();
        public void AddPart(MeshCollisionPart part)
        {
            foreach (MeshCollisionPart p in Parts)
            {
                if (p.Id == part.Id) return;
            }
            Parts.Add(part);
            NParts = Parts.Count;
        }
        public MeshCollisionPart GetPart(int id)
        {
            foreach (MeshCollisionPart p in Parts)
            {
                if (p.Id == id) return p;
            }
            return null;
        }
    }

    public class MeshCollisionPart : NodeObject
    {
        public string Type = "Mesh";
        public int NFrames = 0;
        public MeshObject ParentMeshObject = null;
        public int Id = 0;


        public List<MeshCollisionFrame> CollisionFrames = new List<MeshCollisionFrame>();

        public void AddFrame(MeshCollisionFrame frm)
        {
            CollisionFrames.Add(frm);
            NFrames = CollisionFrames.Count;
        }
    }

    public class MeshCollisionFrame : NodeObject
    {
        public List<Vertex>         Vertexs = new List<Vertex>();
        public List<Face>           Faces = new List<Face>();
        public List<int>            Neigbours = new List<int>();
        public List<int>            NeigboursCounts = new List<int>();
        public int Id = 0;
        public string FName = string.Empty;
        public Dictionary<string, string> DynamicValues = new Dictionary<string, string>();

        public Dictionary<int, FaceIndexNeight> FaceNeigh = new Dictionary<int, FaceIndexNeight>();
        public List<FaceIndexNeight> aFaceNeigh = new List<FaceIndexNeight>();
        public void AddCol(int vertexId, int neighId)
        {
            FaceIndexNeight c = null;
            if (FaceNeigh.ContainsKey(vertexId) == true)
            {
                c = FaceNeigh[vertexId];
            }
            else
            {
                c = new FaceIndexNeight();
                c.FaceId = vertexId;

                if (aFaceNeigh.Count <= vertexId)
                {
                    for (int i = aFaceNeigh.Count; i <= vertexId; i++)
                    {
                        aFaceNeigh.Add(null);
                    }
                }
                FaceNeigh.Add(c.FaceId, c);
                aFaceNeigh[vertexId] = c;

            }
            c.AddNei(neighId);
        }
    }

    public class Hook : NodeObject
    {
        public string Value = string.Empty;
        public JMatrix               Position;
    }

    public class Bone : NodeObject
    {
        public int Id;
        public int ParentId;
    }

    public class MshCommonData
    {
        public int numberOfBones = 0;
        public string FrameType = string.Empty;
        public int numberOfFrames = 0;

    }

    public class FaceIndexNeight
    {
        public int FaceId = -1;
        public Dictionary<int, int> Neig = new Dictionary<int, int>();
        public List<int> aNeig = new List<int>();

        public void AddNei(int nei)
        {
            if (Neig.ContainsKey(nei) == false)
            {
                Neig.Add(nei, nei);
                aNeig.Add(nei);
            }
        }
    }

    public class MeshData : NodeObject
    {
        public bool ShouldNormalize = false;
        public JVector Pivot = null;
        public JMatrix OverrideLocalMatrix = null;
        public int HierarchyLevel = -1;
        public int ModelNumber = 0;
        public int LodDistance = 0;
        public int LodNumber = 0;
        public Node Node = null;

        public MeshObject       ParentMeshObject = null;
        public List<Bone>       Bones = new List<Bone>();

        public List<Material>   Materials = new List<Material>();
        public List<Face>       Faces = new List<Face>();
        public List<Vertex>     Vertexs = new List<Vertex>();
        public List<UV>         UVs = new List<UV>();
        public List<JMatrix> TangentSpace = new List<JMatrix>();
        public List<Group>      Groups = new List<Group>();
        ///3ds debuging
        public JVector KeyPosition=null;
        public JVector KeyRotation = null;
        public float KeyAngle = 0;
        public JVector KeyScale = null;

        /// <summary>
        /// OBJ loading helper storage!!!
        /// </summary>
        public List<JVector>  Normals = new List<JVector>();
        public List<JVector> Positions = new List<JVector>();
        //public List<JVector> Textures = new List<JVector>();

        public MeshData Shadow = null;

        

        

        public MeshCollisionFrame GetCollision()
        {
            MeshCollisionFrame col = new MeshCollisionFrame();

            col.Vertexs = this.Vertexs;
            col.Faces = this.Faces;
            col.Name = this.Name;
            col.Type = eNodeObjectType.e3dMeshCollision;

            for (int faceinx = 0; faceinx < this.Faces.Count; faceinx++)
            {
                Face f = this.Faces[faceinx];

                col.AddCol(f.v1, f.v2);
                col.AddCol(f.v1, f.v3);

                col.AddCol(f.v2, f.v1);
                col.AddCol(f.v2, f.v3);


                col.AddCol(f.v3, f.v1);
                col.AddCol(f.v3, f.v2);
            }
            
            /*
            for (int vindex = 0; vindex < this.Vertexs.Count;vindex++ )
            {
                int count = 0;
                for (int faceinx = 0; faceinx < this.Faces.Count; faceinx++)
                {
                    Face f = this.Faces[faceinx];
                    if (f.v1 == vindex)
                    {
                        col.Neigbours.Add(f.v2);
                        //col.AddCol(vindex, f.v1);
                        count++;
                    }
                    else if (f.v2 == vindex)
                    {
                        col.Neigbours.Add(f.v3);
                        //col.AddCol(vindex, f.v2);
                        count++;
                    }
                    else if (f.v3 == vindex)
                    {
                        col.Neigbours.Add(f.v1);
                        //col.AddCol(vindex, f.v3);
                        count++;
                    }
                }
                col.NeigboursCounts.Add(count);
            }

            /**/
            return col;
        }

        public string GetGeneralInfo()
        {
            string str = string.Empty;

            str += string.Format("[MeshName] {0}\r\n Vertices: {1}\r\n Faces: {2}\r\n Uvs: {3}\r\n", this.Name, this.Vertexs != null ? this.Vertexs.Count : 0, this.Faces != null ? this.Faces.Count : 0, this.UVs != null ? this.UVs.Count : 0);
            str += string.Format("[Shadow mesh] {0}\r\n", this.Shadow != null ? this.Shadow.Name : "");
            str += "[Materials]\r\n";
            foreach (Material mat in Materials)
            {
                str +=string.Format(" {0}\r\n",mat.Name);
            }
            str += "[FaceGroups]\r\n";
            int count = 0;
            foreach (Group group in Groups)
            {
                string material = string.Empty;
                if (group.Id < this.Materials.Count && group.Id >= 0)
                {
                    material = this.Materials[group.Id].Name;
                }
                else
                {
                    int d = 1;
                }

                str += string.Format("{5} [Material] {0} VertStart: {1} VertexCount {2} FaceStart: {3} FaceCount: {4}\r\n", material, group.VertexStart, group.VertexCount, group.FaceStart, group.FaceCount, group.Id);
            }

            if (this.OverrideLocalMatrix != null)
            {
                str += "[Local Matrix]\r\n";
                str += this.OverrideLocalMatrix.ToString();
            }

            return str;
        }

        public void FindType()
        {
            if (this.Name.StartsWith("L") == true && this.Name.EndsWith(" SH") == false)
            {
                this.Type = eNodeObjectType.e3dMeshLod;
                string[] lods = this.Name.Split(' ');
                if (lods.Length >= 2)
                {
                    int.TryParse(lods[1], out this.LodNumber);
                }
            }
            else
            {
                if (this.Name.StartsWith("x") == true)
                {
                    this.Type = eNodeObjectType.e3dMeshCollision;
                }
                else if (this.Name.EndsWith(" HK") == true)
                {
                    try
                    {
                        if (MeshObject.Hook3d != null)
                        {
                            this.Vertexs = MeshObject.Hook3d.Vertexs;
                            this.Faces = MeshObject.Hook3d.Faces;
                            this.Materials = MeshObject.Hook3d.Materials;
                            this.UVs = MeshObject.Hook3d.UVs;
                            this.ObjectMatrix = MeshObject.Hook3d.OverrideLocalMatrix;
                        }
                        else
                        {
                            Vertex v = new Vertex();
                            v.pos = new JVector(0);
                            this.Vertexs.Add(v);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMessages.Global.AddErrorMessage("Failed to load hook.obj from .\\resources\r\n", 1);
                    }
                    this.Type = eNodeObjectType.eMshHook;
                }
                else if (this.Name.EndsWith(" SH") == true)
                {
                    this.Type = eNodeObjectType.e3dMshShadow;
                }
                else if (this.Vertexs.Count == 1 || this.Name.StartsWith("node")==true || this.Name.StartsWith("[")==true)
                {
                    this.Type = eNodeObjectType.eNode;
                }
            }
        }

        public Hook GetHookFromName()
        {
            Hook h = new Hook();

            string[] strings = this.Name.Split(' ');

            if (strings.Length >= 2)
            {

                h.Name = strings[0];
                h.Value = strings[1];
                h.Position = this.OverrideLocalMatrix;
                return h;
            }
            return null;
        }

        public void CalculateNormals(bool smothe)
        {
            if (this.Faces.Count == 0 || this.Vertexs.Count==0)
            {
                return;
            }

            ushort[] sharedFaces = new ushort[this.Vertexs.Count];
            JVector[] summedNormals = new JVector[this.Vertexs.Count];
            JVector v1, v2, normal;
            try
            {
                if (Faces.Count == 0) return;

                SortedDictionary<int, Group> sortDict = new SortedDictionary<int, Group>();
                foreach (Group g in this.Groups)
                {
                    if (sortDict.ContainsKey(g.FaceStart) == false)
                        sortDict.Add(g.FaceStart, g);
                }

                int add = 0;
                for (int i = 0; i < this.Faces.Count; i++)
                {
                    Face f = Faces[i];

                    if (sortDict.ContainsKey(i) == true)
                        add = sortDict[i].VertexStart;
                    JVector a = null;
                    JVector b = null;
                    JVector c = null;

                    try
                    {
                        //a = Vertexs[add + f.v1].pos;
                        //b = Vertexs[add + f.v2].pos;
                        //c = Vertexs[add + f.v3].pos;
                        a = Vertexs[f.v1].pos;
                        b = Vertexs[f.v2].pos;
                        c = Vertexs[f.v3].pos;

                        v1 = b - c;
                        v2 = a - b;

                        // calculate the normal, sum it in the normal
                        // array and 
                        normal = JVector.CrossProduct(v1, v2).Mul(-1);
                        normal.Normalize();
                    }
                    catch (Exception e)
                    {
                        ErrorMessages.Global.AddErrorMessage(string.Format("Calculating normals {0} failed!\r\n", this.Name), 0);
                        return;
                    }

                    for (int k = 0; k < 3; k++)
                    {
                        try
                        {
                            //Vertexs[add + f.v(k)].n = normal;
                            Vertexs[f.v(k)].n = normal;
                            //int inx = f.v(k) + add;
                            int inx = f.v(k);
                            if (summedNormals[inx] == null)
                            {
                                summedNormals[inx] = new JVector();
                            }
                            summedNormals[inx] += normal;
                            sharedFaces[inx]++;
                        }
                        catch (Exception e)
                        {
                            ErrorMessages.Global.AddErrorMessage(string.Format("Calculating normals {0} failed!\r\n", this.Name), 0);
                            return;
                        }
                    }
                }

                if (true)
                {
                    // apply the smoothing groups
                    for (int iv = 0; iv < Vertexs.Count; iv++)
                    {
                        //if (sharedFaces[iv] == 0) continue;
                        // get the weighted normal
                        if (summedNormals[iv] == null)
                        {
                            summedNormals[iv] = new JVector();
                        }
                        JVector n = summedNormals[iv];
                        n = n / sharedFaces[iv];

                        // renorm
                        n.Normalize();

                        Vertexs[iv].n = n;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorMessages.Global.AddErrorMessage(string.Format("Calculating normals {0} failed!\r\n", this.Name),0);
            }
        }

        public void AddGroup(Group group)
        {
            bool add = true;
            foreach (Group g in Groups)
            {
                if (g.VertexStart == group.VertexStart)
                {
                    add = false;
                    break;
                }
            }
            if (add == true) Groups.Add(group);
        }

        public void AddGroup(Group group,int vertex_c,int face_c)
        {
            //group.Id = Groups.Count;
            if (Groups.Count > 0)
            {
                int vertex_start = 0;
                int face_start = 0;
                foreach (Group g in Groups)
                {
                    vertex_start += g.VertexCount;
                    face_start += g.FaceCount ;
                }
                group.VertexStart = vertex_start;
                group.VertexCount = vertex_c;
                group.FaceStart = face_start;
                group.FaceCount = face_c;
            }
            else
            {
                //if (vertex_c == 0) vertex_c = this.Vertexs.Count;
                group.VertexCount = vertex_c;
                group.FaceCount = face_c;
                
            }


            Groups.Add(group);
            
        }

        public int AddMaterial(Material mat)
        {
            
            int count = 0;
            foreach (Material m in Materials)
            {
                if (m.Name == mat.Name)
                {
                    return count;
                }
                count++;
            }
            int inx = Materials.Count;
            Materials.Add(mat);
            return inx;
        }

        public void Normalize2()
        {
            //return;

            if (UVs.Count > 0)
            {
                if (Vertexs.Count == UVs.Count)
                {
                    Normalize();
                    return;        
                }
            }
            
            this.Groups.Clear();

            Vertex [] sourceVert = new Vertex[Vertexs.Count];
            Vertexs.CopyTo(sourceVert);
            Vertexs.Clear();

            UV[] sourceUV = null;

            if (UVs.Count > 0)
            {
                sourceUV = new UV[UVs.Count];
                UVs.CopyTo(sourceUV);
                UVs.Clear();
            }

            int group_id = 0;
            Dictionary<int, Group> newGroups = new Dictionary<int, Group>();
            int fc = 0;
            foreach (Face face in Faces)
            {
                Group g = null;
                if (newGroups.ContainsKey(face.group_id) == false)
                {
                    g = new Group();
                    g.Id = face.group_id;
                    g.Faces = new List<Face>();
                    newGroups.Add(g.Id, g);
                    g.FaceStart = fc;
                    this.Groups.Add(g);
                }
                else
                {
                    g = newGroups[face.group_id];
                }
                g.Faces.Add(face);
                g.FaceCount++;
                fc++;
            }

            Group currentGroup = null;
            int lastgroup = -1;
            for (int ff = 0; ff < Faces.Count; ff++)
            {
                Face face = Faces[ff];
                currentGroup = newGroups[face.group_id];
                if (currentGroup.Id != lastgroup)
                {
                    currentGroup.VertexStart = Vertexs.Count;
                }
                for (int jl = 0; jl < 3; jl++)
                {
                    Vertex v = new Vertex();

                    currentGroup.VertexCount++;

                    v.pos = sourceVert[face.v(jl)].pos;
                    v.n = sourceVert[face.v(jl)].n;
                    if (sourceUV != null && sourceUV.Length > 0)
                    {
                        //int indext = face.t(jl);
                        int indext = face.v(jl);
                        JVector uv = new JVector(sourceUV[indext].u, sourceUV[indext].v, 0);
                        v.uv = uv;
                        UV Uv = new UV();
                        Uv.u = uv.x;
                        Uv.v = uv.y;
                        UVs.Add(Uv);
                    }
                    face.v(jl, Vertexs.Count - currentGroup.VertexStart);


                    Vertexs.Add(v);
                }
                lastgroup = currentGroup.Id;
            }
            
        }

        public void MshNormalizeGroups()
        {
            
        }


        public void Normalize()
        {
            //return;
            if (this.Groups.Count == 0)
            {
                Group g = new Group();
                g.Id = 0;
                g.FaceCount = this.Faces.Count;
                g.FaceStart = 0;
                g.VertexCount = this.Vertexs.Count;
                g.VertexStart = 0;
                this.Groups.Add(g);
            }

            
            if (this.Groups.Count > 0)
            {

                SortedDictionary<int, Group> sortDict = new SortedDictionary<int, Group>();
                foreach (Group g in this.Groups)
                {
                    sortDict.Add(g.FaceStart, g);
                }

                int max = -999999999;
                int min = 999999999;

                int maxN = -999999999;
                int minN = 999999999;

                int maxT = -999999999;
                int minT = 999999999;
                foreach (Face f in this.Faces)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (f.v(i) > max) max = f.v(i);
                        if (f.v(i) < min) min = f.v(i);

                        if (f.n(i) > maxN) maxN = f.n(i);
                        if (f.n(i) < minN) minN = f.n(i);

                        if (f.t(i) > maxT) maxT = f.t(i);
                        if (f.t(i) < minT) minT = f.t(i);
                    }
                }


               
                int c = 0;
                int add = 0;
                foreach (Face f in this.Faces)
                {
                    if (sortDict.ContainsKey(c) == true)
                        add = sortDict[c].VertexStart;
                    /**
                    f.v1 -= min + add;
                    f.v2 -= min + add;
                    f.v3 -= min + add;
                    /*
                    f.n1 -= minN + add;
                    f.n2 -= minN + add;
                    f.n3 -= minN + add;

                    f.t1 -= minT + add;
                    f.t2 -= minT + add;
                    f.t3 -= minT + add;
                    /**/
                    c++;
                }
            }
            /*
            if (this.Vertexs.Count == 0 && this.Positions.Count > 0)
            {
                foreach (Face f in Faces)
                {

                    for (int vp = 0; vp < 3; vp++)
                    {
                        Vertex v = new Vertex();
                        v.pos = Positions[f.v(vp)];
                        if (Normals.Count > 0)
                        {
                            v.n = Normals[f.n(vp)];
                        }
                        if (UVs.Count > 0)
                        {
                            UV uv = UVs[f.t(vp)];
                            v.uv = new JVector(uv.u, uv.v, 0);
                        }
                        Vertexs.Add(v);
                    }
                }
            }
            /**/
        }

    }

    public partial class MeshObject : NodeObject
    {
        public JMatrix LocalPosition = new JMatrix();
        public JMatrix GlobalPosition = new JMatrix();

        public static MeshData Hook3d = new MeshData();
        public static float Uoffset = 0;
        public static float Voffset = 0;
        public static float Uscale = 1;
        public static float Vscale = 1;

        public MeshData Meshes = new MeshData();

        public MshCommonData Common = new MshCommonData();
        
        /// <summary>
        /// Hier data
        /// </summary>
        public string               ParentName = string.Empty;
        public int                  ParentIndex = -1;
        /// <summary>
        /// Msh data
        /// </summary>
        public List<Hook>           Hooks = new List<Hook>();

        public List<MeshData> Lods = new List<MeshData>();
        public List<int>            LodsDist = new List<int>();
        public List<MeshCollisionBlock> Collisions = new List<MeshCollisionBlock>();

        public List<MeshObject>           Children = new List<MeshObject>();

        /// <summary>
        /// Private data
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// 
        int AllFileSize = 0;
        int CountX = 0;
        int FirstOffset = 0;
        int SecondOffset = 0;
        int ThirdOffset = 0;
        int OffsetToData = 0;
        int AllZeroCount = 0;
        int RealZeroCount = 0;
        int CountBytesX = 0;
        int FirstZeroCount = 0;
        byte[] SecondData = null;
        byte[] FileSignature = null;
        FileStream file = null;
        BinaryReader br = null;

        public int CountOfStrings = 0;
        public List<string> Strings = new List<string>();
        public List<string> StringFirstValues = new List<string>();
        public List<MshItem> ItemsList = new List<MshItem>();
        public List<string> XCount4 = new List<string>();
        public List<byte> XView = new List<byte>();
        public List<byte> XViewSort = new List<byte>();
        public List<int> ItemMapping = new List<int>();
        public int XViewAllCount = 0;
        public int AllDataBytes = 0;
        public List<XValue> XHeaderData = new List<XValue>();

        
        public bool LoadMsh(string fileName)
        {
            AllFileSize = 0;
            CountX = 0;
            FirstOffset = 0;
            SecondOffset = 0;
            ThirdOffset = 0;
            OffsetToData = 0;
            AllZeroCount = 0;
            RealZeroCount = 0;
            CountBytesX = 0;
            FirstZeroCount = 0;
            SecondData = null;
            Name = Path.GetFileName(fileName).Replace(".msh",string.Empty);
            file = File.OpenRead(fileName);
            br = new BinaryReader(file);
            ///Read header
            FileSignature = br.ReadBytes(4);

            if (FileSignature[0] != 1 && FileSignature[1] != 66 && FileSignature[2] != 83 && FileSignature[3] != 0)
            {
                file.Close();
                br.Close();
                /// Try Loading ascii version
                return LoadText(fileName);
            }

            AllFileSize = br.ReadInt32();
            CountOfStrings = br.ReadInt32();
            CountX = br.ReadInt32();
            FirstOffset = br.ReadInt32();
            SecondOffset = br.ReadInt32();
            ThirdOffset = br.ReadInt32();

            ///Read strings
            string str = string.Empty;
            bool readsize = true;
            while (br.BaseStream.Position < FirstOffset)
            {
                byte b = br.ReadByte();

                if (readsize == true)
                {
                    if (str != string.Empty) Strings.Add(str);
                    str = string.Empty;
                    readsize = false;
                }
                else if (b == 0)
                {
                    readsize = true;
                }
                else
                {
                    str += Convert.ToChar(b);
                }
            }
            if (str != string.Empty) Strings.Add(str);

            //Read data offset
            long oldPos = br.BaseStream.Position;
            br.BaseStream.Seek(this.ThirdOffset, SeekOrigin.Begin);
            OffsetToData = br.ReadInt32();
            br.BaseStream.Seek(oldPos, SeekOrigin.Begin);

            //Read first data
            while (br.BaseStream.Position < this.SecondOffset)
            {
                int num1 = (int)br.ReadInt32();
                int num2 = (int)br.ReadInt32();
                short num3 = br.ReadInt16();
                byte num4 = br.ReadByte();
                byte num5 = br.ReadByte();

                MshItem item = new MshItem();
                item.Number1 = num1;
                item.Number2 = num2;
                item.Number3 = num3;
                item.NumberOfBytes = num4;
                item.Signed = num5;

                ItemsList.Add(item);

                StringFirstValues.Add(String.Format("{0} {1} {2} {3} {4}", num1, num2, num3, num4,num5));
            }

            //Read second data
            long old = br.BaseStream.Position;
            br.BaseStream.Seek(this.SecondOffset, SeekOrigin.Begin);
            this.SecondData = br.ReadBytes(this.ThirdOffset - this.SecondOffset);
            br.BaseStream.Seek(old, SeekOrigin.Begin);
            CountBytesX = this.SecondData.Length - this.CountOfStrings;

            //Read x count header
            int count = 0;
            old = br.BaseStream.Position;
            br.BaseStream.Seek(this.ThirdOffset + 4, SeekOrigin.Begin);
            while (br.BaseStream.Position < this.OffsetToData)
            {
                int num1 = (int)br.ReadInt32();

                ItemMapping.Add(num1);

                AllDataBytes += num1;
                XCount4.Add(String.Format("{0}", num1));
                count++;

            }
            //SampleData();
            InternalData();
            return true;
        }

 
        protected void InternalData()
        {
            int count = 0;
            long old = br.BaseStream.Position;
            br.BaseStream.Seek(this.OffsetToData, SeekOrigin.Begin);

            int allreaddata = 0;

            MeshData mesh = new MeshData();
            mesh.ParentMeshObject = this;
            mesh.HierarchyLevel = -1;
            mesh.Name = this.Name;

            int current_lod = 0;

            MeshCollisionBlock currentBlock = null;
            MeshCollisionFrame currentFrame = null;
            MeshCollisionPart currentPart = null;
           
            foreach (MshItem item in ItemsList)
            {
                string def = (string)Strings[count];

                int startItem = item.Number2;
                int countItem = item.Number3;

                if (def.Contains("[LOD") == true && def.Contains("[LOD]")==false)
                {
                    int end = def.IndexOf("_")+1;
                    string number = def.Substring(4, end - 5);
                    int newLod = Convert.ToInt32(number);
                    if (current_lod != newLod)
                    {
                        if (current_lod > 0)
                        {
                            this.Lods.Add(mesh);
                        }
                        else
                        {
                            this.Meshes = mesh;
                        }
                        current_lod = newLod;
                        
                        mesh = new MeshData();
                        mesh.ParentMeshObject = this;
                        mesh.Name = string.Format("L{0}", newLod);
                        mesh.HierarchyLevel = 0;
                        mesh.Type = eNodeObjectType.e3dMeshLod;
                        mesh.LodNumber = current_lod;
                        if (current_lod - 1 < this.LodsDist.Count)
                            mesh.LodDistance = this.LodsDist[current_lod - 1];
                    }
                }


                if (def == "[Common]")
                {
                    int c = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);

                        startPos = lastPos;
                        if (Name == "NumBones")
                        {
                            int numBones = GetFirstInt(startPos + 1, num, ref lastPos);
                            Common.numberOfBones = numBones;
                        }
                        else if (Name == "FramesType")
                        {
                            string frameType = GetFirstString(startPos, num, ref lastPos);
                            Common.FrameType = frameType;
                        }
                        else if (Name == "NumFrames")
                        {
                            int numFrames = GetFirstInt(startPos + 1, num, ref lastPos);
                            Common.numberOfFrames = numFrames;
                        }
                        else
                        {

                            string str = string.Empty;
                            for (int j = lastPos; j < num.Length; j++)
                            {
                                str += Convert.ToInt32(num[j]).ToString() + " ";
                            }
                        }
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def == "[Bones]")
                {
                    int c = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;

                        Bone b = new Bone();
                        b.Id = mesh.Bones.Count;
                        b.Name = str;
                        mesh.Bones.Add(b);

                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def == "[LOD]")
                {
                    int c = 0;
                    // LOD count
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        
                        int lastPos = 0;
                        int startPos = 0;

                        int Lod = GetFirstInt(startPos, num, ref lastPos);
                        allreaddata += num.Length;

                        LodsDist.Add(Lod);

                        c += num.Length;
                    }
                }
                else if (def.Contains("Materials") == true)
                {
                    int c = 0;
                    // Materials count
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        /**/
                        string material = ReadString(false);
                        if (material.Length > size)
                        {
                            throw new Exception("Error");
                        }
                        allreaddata += size;
                        c += size;

                        Material m = new Material();
                        m.Name = material;
                        mesh.Materials.Add(m);

                    }
                }
                else if (def.Contains("FaceGroups]"))
                {
                    int c = 0;
                    //Face groups count
                    int lastNum = 0;
                    Group g = null;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        int read = 0;
                        int readValue = 0;
                        /**/
                        
                        if (i > 0)
                        {
                            if (g != null)
                            {
                                mesh.Groups.Add(g);
                            }
                            g = new Group();
                        }
                        int step = 0;

                        while (size - read > 0)
                        {
                            if (item.NumberOfBytes == 1)
                            {
                                if (i == 0)
                                    readValue = Convert.ToInt32(br.ReadByte());
                                else
                                    readValue = Convert.ToInt32(br.ReadSByte());
                                read += 1;
                                if(item.Signed>0) readValue = (readValue + lastNum) & 255;
                            }
                            if (item.NumberOfBytes == 2)
                            {
                                readValue = (int)br.ReadInt16();
                                read += 2;
                                if (item.Signed > 0) readValue = readValue + lastNum;
                            }
                            if (item.NumberOfBytes == 3)
                            {
                                byte[] num = br.ReadBytes(3);
                                readValue = (int)MeshObject.GetFloatFrom3Byte(num, 0);
                                read += 3;

                                if (item.Signed > 0) readValue = readValue + lastNum;
                            }
                            if (item.NumberOfBytes == 4)
                            {
                                readValue = br.ReadInt32();
                                read += 4;

                                if (item.Signed > 0) readValue = readValue + lastNum;
                            }
                            lastNum = readValue;

                            if (i > 0)
                            {
                                if (step == 0) g.Id = readValue;
                                if (step == 1) g.VertexStart = readValue;
                                if (step == 2) g.VertexCount= readValue;
                                if (step == 3) g.FaceStart = readValue;
                                if (step == 4) g.FaceCount = readValue;
                            }
                            step++;
                        }
                        allreaddata += size;
                        c += size;
                    }

                    if (g != null)
                    {
                        mesh.Groups.Add(g);
                    }
                }
                else if (def.Contains("ShVertices_Frame0]"))
                {
                    int c = 0;
                    //Vertices Frame0 count
                    float[] nums = new float[3] { 0, 0, 0};

                    bool failed = false;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        
                        if (item.NumberOfBytes == 0)
                        {
                            int read = 0;

                            int countx = 0;
                            while (size - read > 0)
                            {
                                byte first = br.ReadByte();
                                read += 1;
                                if (first == 0)
                                {
                                    int res1 = br.ReadInt16();
                                    nums[countx] = res1;
                                    read += 2;
                                }
                                else if (first == 1)
                                {
                                    float res2 = br.ReadSingle();
                                    nums[countx] = res2;
                                    read += 4;
                                }
                                countx++;
                            }
                               
                        }
                        else if (item.NumberOfBytes == 4)
                        {

                            for (int j = 0; j < size / 4; j++)
                            {
                                float num1 = br.ReadSingle();
                                nums[j] = num1;
                            }

                            allreaddata += size;
                            c += size;
                        }
                        else if (item.NumberOfBytes == 3)
                        {
                            for (int j = 0; j < size / 3; j++)
                            {
                                byte[] numb2 = br.ReadBytes(3);
                                nums[j] = MeshObject.GetFloatFrom3Byte(numb2, 0);
                            }

                            allreaddata += size;
                            c += size;
                        }
                        else
                        {
                            failed = true;
                            byte[] num = br.ReadBytes(size);
                            allreaddata += num.Length;
                            c += num.Length;
                        }
                        if (failed == false)
                        {
                            Vertex vertex = new Vertex();
                            vertex.pos.x = nums[0];
                            vertex.pos.y = nums[1];
                            vertex.pos.z = nums[2];

                            if (mesh.Shadow == null)
                            {
                                mesh.Shadow = new MeshData();
                                mesh.Shadow.Name = this.Name + " SH";
                                if (mesh.Type == eNodeObjectType.e3dMeshLod)
                                {
                                    mesh.Shadow.Name = "L" + current_lod.ToString()+ " "+ this.Name + " SH";
                                }
                            }

                            mesh.Shadow.Vertexs.Add(vertex);
                        }
                    }

                }
                else if (def.Contains("Vertices_Frame0]") && def.Contains("ShVertices_Frame0") == false)
                {
                    int c = 0;
                    //Vertices Frame0 count
                    float[] nums = new float[6] { 0, 0, 0, 0, 0, 0 };

                    bool failed = false;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        /**/

                        if (item.NumberOfBytes == 0)
                        {
                            int read = 0;
                            int countx = 0;
                            while (size - read > 0)
                            {
                                byte first = br.ReadByte();
                                read += 1;
                                if (first == 0)
                                {
                                    int res1 = br.ReadInt16();
                                    nums[countx] = res1;
                                    read += 2;
                                }
                                else if (first == 1)
                                {
                                    float res2 = br.ReadSingle();
                                    nums[countx] = res2;
                                    read += 4;
                                }
                                countx++;
                            }
                            allreaddata += size;

                        }
                        else if (item.NumberOfBytes == 4)
                        {

                            for (int j = 0; j < size / 4; j++)
                            {
                                float num1 = br.ReadSingle();
                                nums[j] = num1;
                            }

                            allreaddata += size;
                            c += size;
                        }
                        else if (item.NumberOfBytes == 3)
                        {
                            for (int j = 0; j < size / 3; j++)
                            {
                                byte[] numb2 = br.ReadBytes(3);
                                nums[j] = MeshObject.GetFloatFrom3Byte(numb2, 0);
                            }

                            allreaddata += size;
                            c += size;
                        }
                        else
                        {
                            failed = true;
                            byte[] num = br.ReadBytes(size);
                            allreaddata += num.Length;
                            c += num.Length;
                        }
                        if (failed == false)
                        {
                            Vertex vertex = new Vertex();
                            vertex.pos.x = nums[0];
                            vertex.pos.y = nums[1];
                            vertex.pos.z = nums[2];

                            vertex.n.x = nums[3];
                            vertex.n.y = nums[4];
                            vertex.n.z = nums[5];

                            mesh.Vertexs.Add(vertex);
                        }
                    }
                }
                else if (def.Contains("[CoVer"))
                {
                    int c = 0;
                    //Vertices Frame0 count
                    float[] nums = new float[3];
                    bool failed = false;
                    /*
                    if (currentFrame != null)
                    {
                        currentPart.CollisionFrames.Add(currentFrame);
                    }
                    /**/
                    currentFrame = new MeshCollisionFrame();

                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        /**/
                        if (item.NumberOfBytes == 4)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                float num1 = br.ReadSingle();
                                nums[j] = num1;
                            }
                            allreaddata += size;
                            c += size;
                        }
                        else if (item.NumberOfBytes == 3)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                byte[] numb2 = br.ReadBytes(3);
                                float res = MeshObject.GetFloatFrom3Byte(numb2, 0);
                            }
                            allreaddata += size;
                            c += size;
                        }
                        else
                        {
                            failed = true;
                            /**/
                            byte[] num = br.ReadBytes(size);
                            string str = string.Empty;
                            allreaddata += num.Length;
                            c += num.Length;
                            /**/
                        }

                        if (failed == false)
                        {
                            Vertex vertex = new Vertex();
                            vertex.pos.x = nums[0];
                            vertex.pos.y = nums[1];
                            vertex.pos.z = nums[2];

                            if (currentFrame != null)
                            {
                                currentFrame.Vertexs.Add(vertex);
                            }

                        }
                    }
                }
                else if (def.Contains("Space_Frame0]") || def.Contains("HookLoc"))
                {
                    float lastv = 0;
                    int c = 0;
                    //Space Frame0 count
                    int lastNum = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];

                        byte[] num = null;
                        sbyte[] snum = new sbyte[size];

                        int readValue = 0;

                        int read = 0;
                        string str = string.Empty;
                        
                        List<float> floatArray = new List<float>();

                        while (size - read > 0)
                        {
                            if (item.NumberOfBytes == 0)
                            {
                                byte first = br.ReadByte();
                                read += 1;
                                if (first == 0)
                                {
                                    int res1 = br.ReadInt16();
                                    floatArray.Add((float)res1);
                                    read += 2;
                                }
                                else if (first == 1)
                                {
                                    float res2 = br.ReadSingle();
                                    floatArray.Add(res2);
                                    read += 4;
                                }

                            }
                            else if (item.NumberOfBytes == 1)
                            {
                                num = br.ReadBytes(size);
                                read += size;
                                System.Buffer.BlockCopy(num, 0, snum, 0, size);
                                float val1 = 0;
                                for (int j = 0; j < size; j++)
                                {
                                    if (item.Signed > 0)
                                        val1 = (float)(byte)((byte)lastv + snum[j]);
                                    else
                                        val1 = (float)(byte)snum[j];
                                    lastv = val1;
                                    floatArray.Add(val1);
                                }
                            }
                            else if (item.NumberOfBytes == 2)
                            {
                                num = br.ReadBytes(2);
#warning TODO!!!
                                float res = (float)BitConverter.ToInt16(num, 0);
                                floatArray.Add(res);
                                read += 2;
                            }
                            else if (item.NumberOfBytes == 3)
                            {
                                num = br.ReadBytes(3);
                                float res = MeshObject.GetFloatFrom3Byte(num, 0);
                                floatArray.Add(res);
                                read += 3;
                            }
                            else if (item.NumberOfBytes == 4)
                            {
                                float res = br.ReadSingle();
                                floatArray.Add(res);
                                read += 4;
                            }
                        }

                        if (def.Contains("HookLoc") && floatArray.Count == 12)
                        {
                            Hook hook = this.Hooks[i];
                            hook.Position = new JMatrix();
                            hook.Position.RowX = new JVector(floatArray[0], floatArray[1], floatArray[2]);
                            hook.Position.RowY = new JVector(floatArray[3], floatArray[4], floatArray[5]);
                            hook.Position.RowZ = new JVector(floatArray[6], floatArray[7], floatArray[8]);
                            hook.Position.Pos = new JVector(floatArray[9], floatArray[10], floatArray[11]);
                            this.Hooks[i] = hook;
                        }


                    }
                }
                else if (def.Contains("MaterialMapping]"))
                {
                    int c = 0;
                    //MaterialMapping count
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);


                        float u = 0;
                        float v = 0;

                        if (item.NumberOfBytes == 1)
                        {
                            u = num[0];
                            v = num[1];
                        }
                        else if (item.NumberOfBytes == 4)
                        {
                            u = BitConverter.ToSingle(num, 0);
                            v = BitConverter.ToSingle(num, 4);
                        }
                        else if (item.NumberOfBytes == 3)
                        {
                            u = MeshObject.GetFloatFrom3Byte(num, 0);
                            v = MeshObject.GetFloatFrom3Byte(num, 3);
                        }

                        UV uv = new UV();
                        uv.u = u;
                        uv.v = v;
                        mesh.UVs.Add(uv);

                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def.Contains("Faces]"))
                {
                    int lastv = 0;
                    int[] v = new int[3];
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        int startPos = 0;
                        int lastPos = 0;
                        int countf = size / 3;
                        sbyte[] snum = new sbyte[size];
                        System.Buffer.BlockCopy(num, 0, snum, 0, size);

                        if (item.NumberOfBytes == 1)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (item.Signed > 0) 
                                    v[j] = (byte)((byte)lastv + snum[j]);
                                else
                                    v[j]=(byte)snum[j];
                                lastv = v[j];
                            }
                        }
                        else if (item.NumberOfBytes == 2)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                //v[j] = (ushort)((ushort)lastv + GetFirstIntCount(startPos, item.NumberOfBytes, num, ref lastPos));
                                if (item.Signed > 0) 
                                    v[j] = lastv + GetFirstIntCount(startPos, item.NumberOfBytes, num, ref lastPos);
                                else
                                    v[j] = (byte)snum[j];

                                startPos = lastPos;
                                lastv = v[j];
                            }
                        }

                        Face f = new Face();
                        f.v1 = v[0];
                        f.v2 = v[1];
                        f.v3 = v[2];
                        if (def.EndsWith("ShFaces]") == true)
                        {
                            if (mesh.Shadow == null)
                            {
                                mesh.Shadow = new MeshData();
                                mesh.Shadow.Name = this.Name + " SH";
                                if (mesh.Type == eNodeObjectType.e3dMeshLod)
                                {
                                    mesh.Shadow.Name = "L" + current_lod.ToString() + " " + this.Name + " SH";
                                }
                            }
                            mesh.Shadow.Faces.Add(f);
                        }
                        else
                            mesh.Faces.Add(f);
                    }
                }
                else if (def.Contains("CoFac_"))
                {
                    int lastv = 0;
                    int[] v = new int[3];
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        int startPos = 0;
                        int lastPos = 0;
                        int countf = size / 3;
                        
                        sbyte[] snum = new sbyte[size];
                        System.Buffer.BlockCopy(num, 0, snum, 0, size);

                        if (item.NumberOfBytes == 1)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (item.Signed > 0)
                                    v[j] = (byte)((byte)lastv + snum[j]);
                                else
                                    v[j] = (byte)snum[j];
                                lastv = v[j];
                            }
                        }
                        else if (item.NumberOfBytes == 2)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (item.Signed > 0) 
                                    v[j] = lastv + GetFirstIntCount(startPos, item.NumberOfBytes, num, ref lastPos);
                                else
                                    v[j] = (byte)snum[j];

                                startPos = lastPos;
                                lastv = v[j];
                            }
                        }

                        if (currentFrame != null)
                        {
                            Face f = new Face();
                            f.v1 = v[0] ;
                            f.v2 = v[1];
                            f.v3 = v[2];
                            currentFrame.Faces.Add(f);
                        }

                    }
                }
                else if (def.Contains("CoNei_"))
                {
                    int lastv = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        int value = 0;
                        if (item.NumberOfBytes == 1)
                        {
                            value = num[0];

                        }
                        else if (item.NumberOfBytes == 2)
                        {
                            value = (int)BitConverter.ToInt16(num, 0);
                        }
                        else if (item.NumberOfBytes == 3)
                        {
                            value = (int)GetFloatFrom3Byte(num, 0);
                        }
                        else if (item.NumberOfBytes == 3)
                        {
                            value = (int)BitConverter.ToInt32(num, 0);
                        }

                        if (currentFrame != null)
                        {
                            currentFrame.Neigbours.Add(value);
                        }
                    }
                }
                else if (def.Contains("CoNeiCnt_"))
                {
                    int lastv = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);

                        int value = 0;
                        if (item.NumberOfBytes == 1)
                        {
                            value = num[0];

                        }
                        else if (item.NumberOfBytes == 2)
                        {
                            value = (int)BitConverter.ToInt16(num, 0);
                        }
                        else if (item.NumberOfBytes == 3)
                        {
                            value = (int)GetFloatFrom3Byte(num, 0);
                        }
                        else if (item.NumberOfBytes == 3)
                        {
                            value = (int)BitConverter.ToInt32(num, 0);
                        }

                        if (currentFrame != null)
                        {
                            currentFrame.NeigboursCounts.Add(value);
                        }
                    }
                }

                else if (def == "[Hooks]")
                {
                    int c = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;

                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);
                        string Val = GetFirstString(lastPos, num, ref lastPos);

                        Hook hook = new Hook();
                        hook.Name = Name;
                        hook.Value = Val;
                        this.Hooks.Add(hook);

                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def == "[FrameNames]")
                {
                    int c = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;

                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                else if (def.Contains("CoCommon"))
                {
                    int c = 0;


                    string cleandef = def.Replace("[", string.Empty);
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
                            if (currentFrame != null)
                            {
                                currentPart.CollisionFrames.Add(currentFrame);
                            }

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

                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;


                        int lastPos = 0;
                        int startPos = 0;
                        string Name = GetFirstString(startPos, num, ref lastPos);
                        startPos = lastPos;
                        if (Name == "NBlocks")
                        {
                            int val = GetFirstInt(startPos + 1, num, ref lastPos);
                        }
                        else if (Name == "NParts")
                        {
                            int val = GetFirstInt(startPos + 1, num, ref lastPos);
                            if (currentBlock != null)
                                currentBlock.NParts = val;
                        }
                        else if (Name == "ParentBone")
                        {
                            int val = GetFirstInt(startPos + 1, num, ref lastPos);
                            if (currentBlock != null)
                                currentBlock.ParentBone = val;
                        }
                        else if (Name == "Type")
                        {
                            string val = GetFirstString(startPos, num, ref lastPos);
                            if (currentPart != null)
                                currentPart.Type = val;
                        }
                        else if (Name == "NFrames")
                        {
                            int val = GetFirstInt(startPos + 1, num, ref lastPos);
                            if (currentPart != null)
                                currentPart.NFrames = val;
                        }
                        else if (Name == "Name")
                        {
                            string val = GetFirstString(startPos, num, ref lastPos);
                            if (currentPart != null)
                                currentPart.Name = val;
                        }
                        else
                        {
                            str = string.Empty;
                            for (int j = lastPos; j < num.Length; j++)
                            {
                                str += Convert.ToInt32(num[j]).ToString() + " ";
                            }
                        }

                        c += num.Length;
                        allreaddata += num.Length;
                    }
                }
                else
                {
                    int c = 0;
                    for (int i = 0; i < countItem; i++)
                    {
                        int size = (int)ItemMapping[startItem + i];
                        byte[] num = br.ReadBytes(size);
                        string str = string.Empty;
                        /**/
                        for (int j = 0; j < num.Length; j++)
                        {
                            str += Convert.ToInt32(num[j]).ToString() + " ";
                        }
                        //**/
                        allreaddata += num.Length;
                        c += num.Length;
                    }
                }
                count++;
            }

            if (currentBlock != null)
            {
                this.Collisions.Add(currentBlock);
            }
            if (currentPart != null)
            {
                currentBlock.Parts.Add(currentPart);
            }
            /**/
            if (currentFrame != null)
            {
                if (currentPart.NFrames > currentPart.CollisionFrames.Count)
                    currentPart.CollisionFrames.Add(currentFrame);
                else
                {
                    currentPart = new MeshCollisionPart();
                    currentPart.ParentMeshObject = this;
                    currentPart.AddFrame(currentFrame);
                    currentBlock.AddPart(currentPart);
                }
            }
            /**/

            br.BaseStream.Seek(old, SeekOrigin.Begin);

            if (current_lod > 0)
            {
                this.Lods.Add(mesh);
            }
            else if (current_lod == 0)
            {
                this.Meshes = mesh;
            }
        }


        protected string ReadString(bool nullt)
        {
            string str = string.Empty;
            byte sizeb = br.ReadByte();
            int offset = 0;
            if (nullt == false) offset = 1;
            for (int k = 0; k < sizeb-offset; k++)
            {
                byte b = br.ReadByte();
                if (b != 0)
                    str += Convert.ToChar(b);
            }
            return str;
        }

        
        protected string GetFirstString(int offset,byte[] array,ref int lastpos)
        {
            string res = string.Empty;
            char[] p = { '\0' };
            int len = 0;
            int lenc = 0;
            bool startstr = true;
            string str = string.Empty;
            int i = offset;
            for (i = offset; i < array.Length; i++)
            {
                if (startstr == true)
                {
                    len = array[i];
                    lenc = 0;
                    if (str != string.Empty)
                    {
                        res = res.TrimEnd(p);
                    }
                    startstr = false;
                    res = string.Empty;
                }
                else
                {
                    lenc++;
                    if (lenc >= len)
                    {
                        break;
                    }
                    else
                    {
                        res += Convert.ToChar(array[i]);
                    }
                }

            }
            if (res != string.Empty)
            {
                res.TrimEnd(p);
            }
            lastpos = i;

            return res;
        }

        protected int GetFirstInt(int offset,byte[] array, ref int lasPos)
        {
            int res = 0;

            int count = array.Length - offset;
            if (count > 4)
            {
                throw new Exception("Error");
            }

            if (count == 1)
            {
                res = (int)array[offset];
            }
            else if (count == 2)
            {
                short r = BitConverter.ToInt16(array, offset);
                res = (int)r;
            }
            else if (count == 3)
            {
                byte[] a = { array[offset], array[offset + 1], array[offset+2],0};
                res = BitConverter.ToInt32(a, 0);
            }
            else if (count == 4)
            {
                res = BitConverter.ToInt32(array, offset);
            }
            lasPos += count;

            return res;
        }

        protected int GetFirstIntCount(int offset,int count, byte[] array, ref int lasPos)
        {
            int res = 0;

            if (count > 4)
            {
                throw new Exception("Error");
            }

            if (count == 1)
            {
                byte s = (byte)array[offset];
                res = (int)s;
            }
            else if (count == 2)
            {
                short r = BitConverter.ToInt16(array, offset);
                res = (int)r;
            }
            else if (count == 3)
            {
                byte[] a = { array[offset], array[offset + 1], array[offset + 2], 0 };
                res = BitConverter.ToInt32(a, 0);
            }
            else if (count == 4)
            {
                res = BitConverter.ToInt32(array, offset);
            }
            lasPos += count;

            return res;
        }

        protected float GetFirstFloat(int offset,int count, byte[] array, ref int lasPos)
        {
            float res = 0;

            //int count = array.Length - offset;
            if (count > 4)
            {
                throw new Exception("Error");
            }

            if (count == 1)
            {
                sbyte s = (sbyte)array[offset];
                res = (float)s;
            }
            else if (count == 2)
            {
                byte[] a = { array[offset], array[offset + 1],0, 0 };
                res = BitConverter.ToSingle(a, 0);
            }
            else if (count == 3)
            {
                byte[] a = { 0,array[offset], array[offset + 1], array[offset+2]};
                res = BitConverter.ToSingle(a, 0);
            }
            else if (count == 4)
            {
                res = BitConverter.ToSingle(array, offset);
            }
            lasPos += count;
            return res;
        }

        public static float GetFloatFrom3Byte(byte[] numb2, int offset)
        {
            byte[] numb = { 0, 0, 0, 0 };
            numb[0] = numb2[offset + 0];
            numb[1] = numb2[offset + 1];
            numb[2] = numb2[offset + 2];
            int num2 = 0;
            /**/
            if ((numb[2] & 128) > 0)
            {
                byte maskLow = 15;
                byte maskUp = 240;
                byte maskExtra = 14;

                byte val1 = (byte)((numb[1] & maskUp )>>4);
                byte val2 = (byte)(numb[1] & maskLow);
                byte val3 = (byte)((numb[0] & maskUp)>>4);
                byte val4 = (byte)(numb[0] & maskExtra);

                double valdec = 1 - val1 * System.Math.Pow(16, -1) - val2 * System.Math.Pow(16, -2) - val3 * System.Math.Pow(16, -3) - val4 * System.Math.Pow(16, -4);

                byte v1 = (byte)(numb[2] & 127);
                byte inv = (byte)(~v1);
                byte val5 = (byte)(inv & 127);

                return (float)(-(val5 + valdec));

            }
            else
            /**/
            {
                num2 = BitConverter.ToInt32(numb, 0);
            }
            float num1 = (float)num2 / (float)65536.0;
            return num1;

        }

        public void SampleData()
        {
            int count = 0;
            long old = br.BaseStream.Position;
            br.BaseStream.Seek(this.OffsetToData, SeekOrigin.Begin);
            /**/
            for (int j = 0; j < ItemsList.Count; j++)
            {
                MshItem item = (MshItem)ItemsList[j];
                item.StartOffset = br.BaseStream.Position;
                int startItem = item.Number2;
                int countItem = item.Number3;
                for (int i = 0; i < countItem; i++)
                {
                    int size = (int)ItemMapping[startItem + i];
                    byte[] num = br.ReadBytes(size);
                }

                item.EndOffset = br.BaseStream.Position;
                item.Lenght = item.EndOffset - item.StartOffset;
            }
            /**/
            br.BaseStream.Seek(old, SeekOrigin.Begin);
        }


        public static byte[] BitArrayToByteArray(BitArray bits)
        {
            // Who knows, might change
            const int BITSPERBYTE = 8;

            // Get the size of bytes needed to store all bytes
            int bytesize = bits.Length / BITSPERBYTE;
            // Any bit left over another byte is necessary
            if (bits.Length % BITSPERBYTE > 0)
            {
                bytesize++;
            }

            // For the result
            byte[] bytes = new byte[bytesize];

            // Must init to good value, all zero bit byte has value zero
            // Lowest significant bit has a place value of 1, each position to
            // to the left doubles the value
            byte value = 0;
            byte significance = 1;

            // Remember where in the input/output arrays
            int bytepos = 0;
            int bitpos = 0;

            while (bitpos < bits.Length)
            {
                // If the bit is set add its value to the byte
                if (true == bits[bitpos])
                {
                    value += significance;
                }
                bitpos++;
                if (0 == bitpos % BITSPERBYTE)
                {
                    // A full byte has been processed, store it
                    // increase output buffer index and reset work values
                    bytes[bytepos] = value;
                    bytepos++;
                    value = 0;
                    significance = 1;
                }
                else
                {
                    // Another bit processed, next has doubled value
                    significance *= 2;
                }
            }
            return bytes;
        }

        const int BITSPERBYTE = 8;
        // This gives the same result as BitArray[bitpos] on an 
        // array that was created with new BitArray(bytes)
        public static bool IsBitSet(byte[] bytes, int bitpos)
        {
            // Get position in byte array
            int bytepos = bitpos / BITSPERBYTE;

            // Create bitmask, the byte array is per byte and the
            // bitmask take this into consideration. Bit 10 in the
            // bit array is bit 2 on the second byte.
            byte mask = 1;
            byte shift = (byte)(bitpos % BITSPERBYTE);
            mask = (byte)(mask << shift);

            // "Bitwise and" with the mask will return the mask
            // if the bit is set or zero if it is not.
            return ((bytes[bytepos] & mask) != 0);
        }

        // Sets the bit if state is true, otherwise clears it
        public static void SetBit(byte[] bytes, int bitpos, bool state)
        {
            // Check if there actually is something to do
            if (MeshObject.IsBitSet(bytes, bitpos) == state)
            {
                return;
            }

            // get position in bytearray and bitmask
            int bytepos = bitpos / BITSPERBYTE;
            byte mask = 1;
            byte shift = (byte)(bitpos % BITSPERBYTE);
            mask = (byte)(mask << shift);

            if (true == state)
            {
                // bitwise or with mask
                bytes[bytepos] |= mask;
            }
            else
            {
                // bitwise and with inverted mask
                bytes[bytepos] &= (byte)~mask;
            }
        }

        public void AddToNode(Node parent)
        {
            if (this.Meshes != null)
            {
                this.Meshes.Type = eNodeObjectType.e3dMesh;
                parent.AddObject(this.Meshes);

                if (this.Meshes.Shadow != null)
                {
                    this.Meshes.Shadow.Type = eNodeObjectType.e3dMshShadow;
                    this.Meshes.Shadow.ParentMeshObject = this.Meshes.ParentMeshObject;
                    this.Meshes.Shadow.OverrideLocalMatrix = this.Meshes.OverrideLocalMatrix;
                    parent.AddObject(this.Meshes.Shadow);
                    this.Meshes.Shadow = null;
                }
            }
            
            foreach (MeshData lod in this.Lods)
            {
                lod.Type = eNodeObjectType.e3dMeshLod;
                lod.Name = lod.Name + " " + this.Name;
                parent.AddObject(lod);

                if (lod.Shadow != null)
                {
                    lod.Shadow.Type = eNodeObjectType.e3dMshShadow;
                    lod.Shadow.ParentMeshObject = lod.ParentMeshObject;
                    lod.Shadow.OverrideLocalMatrix = lod.OverrideLocalMatrix;
                    parent.AddObject(lod.Shadow);

                    lod.Shadow = null;
                }
            }

            int blockCOunt = 0;
            foreach (MeshCollisionBlock b in this.Collisions)
            {
                int count_p = 0;
                foreach (MeshCollisionPart p in b.Parts)
                {
                    int frame_count = 0;
                    foreach (MeshCollisionFrame f in p.CollisionFrames)
                    {
                        MeshData m = new MeshData();
                        m.ParentMeshObject = this;
                        //m.Name = p.Name + " [" + count_p.ToString() + "] " + this.Name;
                        m.Name = p.Name + string.Format(" b{0} p{1} {2} ", blockCOunt, count_p, frame_count) + this.Name;
                        m.Faces = f.Faces;
                        m.Vertexs = f.Vertexs;

                        m.Type = eNodeObjectType.e3dMeshCollision;
                        parent.AddObject(m);
                        frame_count++;
                        
                    }
                    count_p++;
                }
                blockCOunt++;
            }
            foreach (Hook h in this.Hooks)
            {
                MeshData mh = new MeshData();
               
                try
                {
                    if (Hook3d != null)
                    {
                        mh.Vertexs = Hook3d.Vertexs;
                        mh.Faces = Hook3d.Faces;
                        mh.Materials = Hook3d.Materials;
                        mh.UVs = Hook3d.UVs;
                    }
                    else
                    {
                        Vertex v = new Vertex();
                        v.pos = new JVector(0);
                        mh.Vertexs.Add(v);
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessages.Global.AddErrorMessage("Failed to load hook.obj from .\\resources\r\n", 1);
                }

                mh.ParentMeshObject = this;
                mh.Name = h.Name + " " + h.Value + " " +this.Name + " HK";
                //mh.Name = this.Name + " " + h.Value + " " + h.Name + " HK";
                mh.OverrideLocalMatrix = h.Position;
                mh.Type = eNodeObjectType.eMshHook;
                parent.AddObject(mh);
            }

        }

        

    }

    public class MeshHierObject : NodeObject
    {
        public MeshObject Root = new MeshObject();
        public List<MeshObject> Meshes = new List<MeshObject>();


        public void LoadHier(string fileName)
        {

        }
               
    }
}
