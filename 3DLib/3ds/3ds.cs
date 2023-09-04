using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

using Library3d.Mesh;
using Library3d.Math;
using Library3d.Nodes;

namespace Library3d.Format3ds
{
    public class ChunkData
    {
        
        public ushort       Identifier = 0;
        public uint         Lenght = 0;
        public int          Offset =0;

        public byte[] CustomData = null;

        public int Id = 0;
        public ChunkData Parent = null;
        public List<ChunkData> Children = new List<ChunkData>();
        public ChunkData()
        {

        }

        public ChunkData(ushort i)
        {
            Identifier = i;
        }
    }


    public partial class Object3ds
    {
        //
        // Constants
        //
        public const uint kTextureFilenameSize = 256;
        public const uint kObjectNameSize = 200;
        public const uint kHandlerHashSize = 131;
        public const uint kFileContextMaxDepth = 16;
        public const uint kMaxStringLength = 255;

        //
        // Chunk definitions
        //
        private const ushort kChunkEmpty = 0x0000;

        private const ushort kMagic3ds = 0x4d4d;
        private const ushort kMagicS = 0x2d2d;
        private const ushort kMagicL = 0x2d3d;
        // mli file
        private const ushort kMagicLib = 0x3daa;
        private const ushort kMagicMat = 0x3dff;
        // prj file
        private const ushort kMagicC = 0xc23d;
        // start of actual objs
        private const ushort kChunkObjects = 0x3d3d;

        private const ushort kVersionMax = 0x0002;
        private const ushort kVersionKF = 0x0005;
        private const ushort kVersionMesh = 0x3d3e;

        private const ushort kColor3f = 0x0010;
        private const ushort kColor24 = 0x0011;
        private const ushort kColorLin24 = 0x0012;
        private const ushort kColorLin3f = 0x0013;
        private const ushort kIntPercent = 0x0030;
        private const ushort kFloatPercent = 0x0031;
        private const ushort kMasterScale = 0x0100;
        private const ushort kImageFile = 0x1100;
        private const ushort kAmbLight = 0x2100;

        // object chunks
        private const ushort kNamedObject = 0x4000;
        private const ushort kObjMesh = 0x4100;
        private const ushort kObjLight = 0x4600;
        private const ushort kObjCamera = 0x4700;

        private const ushort kMeshVerts = 0x4110;
        private const ushort kVertexFlags = 0x4111;
        private const ushort kMeshFaces = 0x4120;
        private const ushort kMeshMaterial = 0x4130;
        private const ushort kMeshTexVert = 0x4140;
        private const ushort kMeshSmoothGroup = 0x4150;
        private const ushort kMeshXFMatrix = 0x4160;
        private const ushort kMeshColorInd = 0x4165;
        private const ushort kMeshTexInfo = 0x4170;
        private const ushort kHeirarchy = 0x4F00;

        private const ushort kLightSpot = 0x4620;


        private const ushort kViewportLayout = 0x7001;
        private const ushort kViewportData = 0x7011;
        private const ushort kViewportData3 = 0x7012;
        private const ushort kViewportSize = 0x7020;


        // material chunks
        private const ushort kMat = 0xAFFF;
        private const ushort kMatName = 0xA000;
        private const ushort kMatAmb = 0xA010;
        private const ushort kMatDiff = 0xA020;
        private const ushort kMatSpec = 0xA030;
        private const ushort kMatShin = 0xA040;
        private const ushort kMatShinPow = 0xA041;
        private const ushort kMatTransparency = 0xA050;
        private const ushort kMatTransFalloff = 0xA052;
        private const ushort kMatRefBlur = 0xA053;
        private const ushort kMatEmis = 0xA080;
        private const ushort kMatTwoSided = 0xA081;
        private const ushort kMatTransAdd = 0xA083;
        private const ushort kMatSelfIlum = 0xA084;
        private const ushort kMatWireOn = 0xA085;
        private const ushort kMatWireThickness = 0xA087;
        private const ushort kMatFaceMap = 0xA088;
        private const ushort kMatTransFalloffIN = 0xA08A;
        private const ushort kMatSoften = 0xA08C;
        private const ushort kMatShader = 0xA100;
        private const ushort kMatTexMap = 0xA200;
        private const ushort kMatTexFLNM = 0xA300;
        private const ushort kMatTexTile = 0xA351;
        private const ushort kMatTexBlur = 0xA353;
        private const ushort kMatTexUscale = 0xA354;
        private const ushort kMatTexVscale = 0xA356;
        private const ushort kMatTexUoffset = 0xA358;
        private const ushort kMatTexVoffset = 0xA35A;
        private const ushort kMatTexAngle = 0xA35C;
        private const ushort kMatTexCol1 = 0xA360;
        private const ushort kMatTexCol2 = 0xA362;
        private const ushort kMatTexColR = 0xA364;
        private const ushort kMatTexColG = 0xA366;
        private const ushort kMatTexColB = 0xA368;

        // keyframe chunks

        // start of keyframe info
        private const ushort kChunkKeyFrame = 0xB000;
        private const ushort kAmbientNodeTag = 0xB001;
        private const ushort kTrackInfoTag = 0xB002;
        private const ushort kCameraNodeTag = 0xB003;
        private const ushort kTargetNodeTag = 0xB004;
        private const ushort kLightNodeTag = 0xB005;
        private const ushort kLTargetNodeTag = 0xB006;
        private const ushort kSpotlightNodeTag = 0xB007;

        private const ushort kKeyFrameSegment = 0xB008;
        private const ushort kKeyFrameCurtime = 0xB009;
        private const ushort kKeyFrameHdr = 0xB00A;
        private const ushort kKeyFrameObjectName = 0xB010;
        private const ushort kKeyFrameDummyName = 0xB011; // when tag is $$$DUMMY
        private const ushort kKeyFramePrescale = 0xB012;
        private const ushort kKeyFramePivot = 0xB013;
        private const ushort kBoundingBox = 0xB014;
        private const ushort kMorphSmooth = 0xB015;
        private const ushort kKeyFramePos = 0xB020;
        private const ushort kKeyFrameRot = 0xB021;
        private const ushort kKeyFrameScale = 0xB022;
        private const ushort kKeyFrameFov = 0xB023;
        private const ushort kKeyFrameRoll = 0xB024;
        private const ushort kKeyFrameCol = 0xB025;
        private const ushort kKeyFrameMorph = 0xB026;
        private const ushort kKeyFrameHot = 0xB027;
        private const ushort kKeyFrameFall = 0xB028;
        private const ushort kKeyFrameHide = 0xB029;
        private const ushort kKeyFrameNodeID = 0xB030;

        private const ushort kChunkEnd = 0xffff;

        /// <summary>
        /// My Data
        /// </summary>
        public ChunkData RootChunk = null;
        public List<ChunkData> Chunks = new List<ChunkData>();


        public void SaveMesh(string fileName, List<MeshData> meshes,bool hierarchy)
        {
            FileStream file = File.Create(fileName);
            BinaryWriter wr = new BinaryWriter(file);

            //StreamWriter tr = File.CreateText("Debug3ds.txt");

            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            ChunkData   _main = new ChunkData(kMagic3ds);
            ChunkData _version = new ChunkData(kVersionMax);
            _version.CustomData =
            _version.CustomData = BitConverter.GetBytes((uint)3);

            ChunkData   _3deditor = new ChunkData(kChunkObjects);

            ChunkData _3dMeshVersion = new ChunkData(kVersionMesh);
            _3dMeshVersion.CustomData = BitConverter.GetBytes((uint)3);
            _3deditor.Children.Add(_3dMeshVersion);

            ChunkData _OneMasterUnit = new ChunkData(kMasterScale);
            _OneMasterUnit.CustomData = BitConverter.GetBytes((float)1.0f);
            _3deditor.Children.Add(_OneMasterUnit);


            Dictionary<string, Material> usedMaterials = new Dictionary<string, Material>();

            byte cr, cg, cb;
            cr = 255; cg = 255; cb = 255;
            int mcount = 0;
            foreach (MeshData meshExport in meshes)
            {
                ///Materials
                
               
                foreach (Material m in meshExport.Materials)
                {
                    
                    if (usedMaterials.ContainsKey(m.Name) == false)
                    {

                        ChunkData _material1 = new ChunkData(kMat);
                        ChunkData _materialName = new ChunkData(kMatName);

                        _materialName.CustomData = encoding.GetBytes(m.Name + "\0");
                        _material1.Children.Add(_materialName);

                        _3deditor.Children.Add(_material1);

                        usedMaterials.Add(m.Name, m);

                        ChunkData _matAmb = new ChunkData(kMatAmb);
                        

                        ChunkData _matColor1 = new ChunkData(kColor24);
                        _matColor1.CustomData = new byte[3];

                        if (mcount % 1 == 0)
                            cr -= 10;
                        if (mcount % 2 == 0)
                            cg -= 10;
                        if (mcount % 3 == 0)
                             cb-= 10;

                        _matColor1.CustomData[0] = cr;
                        _matColor1.CustomData[1] = cg;
                        _matColor1.CustomData[2] = cb;

                        _matAmb.Children.Add(_matColor1);
                        ChunkData _matDiff = new ChunkData(kMatDiff);
                        _matDiff.Children.Add(_matColor1);
                        
                        _material1.Children.Add(_matAmb);
                        _material1.Children.Add(_matDiff);
                        mcount++;
                    }
                }
            }

            
            foreach (MeshData meshExport in meshes)
            {

                SortedDictionary<int, Group> sortDict = new SortedDictionary<int, Group>();
                foreach (Group g in meshExport.Groups)
                {
                    sortDict.Add(g.FaceStart, g);
                }

                ///Materials
                /*
                foreach (Material m in meshExport.Materials)
                {

                    if (usedMaterials.ContainsKey(m.Name) == false)
                    {

                        ChunkData _material1 = new ChunkData(kMat);
                        ChunkData _materialName = new ChunkData(kMatName);

                        _materialName.CustomData = encoding.GetBytes(m.Name + "\0");
                        _material1.Children.Add(_materialName);

                        _3deditor.Children.Add(_material1);

                        usedMaterials.Add(m.Name, m);
                    }
                }
                /**/
                ChunkData _objectBlock = new ChunkData(kNamedObject);
                ChunkData _objectMesh = new ChunkData(kObjMesh);

                _objectBlock.CustomData = encoding.GetBytes(meshExport.Name + "\0");

                //root.CustomData = new byte[reserved.Length + sizeof(uint)];
                //Array.Copy(reserved,root.CustomData,10);
                //Array.Copy(BitConverter.GetBytes(version),0,root.CustomData,10,4);
                ushort count = 0;
                int offset = sizeof(ushort);
                int c = 0;
                //Write vertices //////////////////////////////
                ChunkData _vertices = null;

                if (meshExport.Vertexs.Count > 0)
                {
                    _vertices = new ChunkData(kMeshVerts);

                    count = (ushort)meshExport.Vertexs.Count;
                    _vertices.CustomData = new byte[sizeof(ushort) + meshExport.Vertexs.Count * 3 * 4];
                    Array.Copy(BitConverter.GetBytes(count), _vertices.CustomData, sizeof(ushort));

                    foreach (Vertex v in meshExport.Vertexs)
                    {
                        JVector rpos = v.pos;
                        /***/
                        if (meshExport.ParentNode != null && hierarchy == false)
                        {
                            rpos = rpos * meshExport.ParentNode.Global;
                        }
                        else if (meshExport.ParentMeshObject != null && hierarchy == false)
                        {
                            rpos = rpos * meshExport.ParentMeshObject.GlobalPosition;
                        }

                        if(meshExport.ObjectMatrix!=null)
                            rpos = rpos * meshExport.ObjectMatrix;
                        /**/

                        Array.Copy(BitConverter.GetBytes(rpos.x), 0, _vertices.CustomData, offset, 4);
                        offset += 4;
                        Array.Copy(BitConverter.GetBytes(rpos.y), 0, _vertices.CustomData, offset, 4);
                        offset += 4;
                        Array.Copy(BitConverter.GetBytes(rpos.z), 0, _vertices.CustomData, offset, 4);
                        offset += 4;

                        c++;
                    }
                }
                //// Write material mappings
                ChunkData _uvs = null;

                if (meshExport.UVs.Count > 0)
                {
                    _uvs = new ChunkData(kMeshTexVert);
                    _uvs.CustomData = new byte[sizeof(ushort) + meshExport.UVs.Count * 2 * 4];
                    count = (ushort)meshExport.UVs.Count;
                    Array.Copy(BitConverter.GetBytes(count), _uvs.CustomData, sizeof(ushort));
                    offset = sizeof(ushort);

                    foreach (UV uv in meshExport.UVs)
                    {
                        Array.Copy(BitConverter.GetBytes((uv.u+ MeshObject.Uoffset) * MeshObject.Uscale), 0, _uvs.CustomData, offset, 4);
                        offset += 4;
                        //Array.Copy(BitConverter.GetBytes(uv.v * -1), 0, _uvs.CustomData, offset, 4);
                        Array.Copy(BitConverter.GetBytes((uv.v + MeshObject.Voffset) * MeshObject.Vscale), 0, _uvs.CustomData, offset, 4);
                        offset += 4;
                    }
                }


                //// Write faces////////////////////////////////
                ChunkData _faces = null;
                if (meshExport.Faces.Count > 0)
                {
                    _faces = new ChunkData(kMeshFaces);
                    count = (ushort)meshExport.Faces.Count;
                    _faces.CustomData = new byte[sizeof(ushort) + meshExport.Faces.Count * 4 * 2];
                    Array.Copy(BitConverter.GetBytes(count), _faces.CustomData, sizeof(ushort));
                    offset = sizeof(ushort);
                    c = 0;
                    int add = 0;
                    foreach (Face f in meshExport.Faces)
                    {
                        if (sortDict.ContainsKey(c) == true)
                            add = sortDict[c].VertexStart;

                        for (int o = 0; o < 3; o++)
                        {
                            ushort val = (ushort)(add + f.v(o));
                            if (val >= meshExport.Vertexs.Count)
                            {
                                int d = 1;
                            }
                            Array.Copy(BitConverter.GetBytes(val), 0, _faces.CustomData, offset, 2);
                            offset += 2;
                        }
                        Array.Copy(BitConverter.GetBytes((ushort)3), 0, _faces.CustomData, offset, 2);
                        offset += 2;
                        c++;
                    }

                    ///Groupings
                    c = 0;
                    foreach (Group g in meshExport.Groups)
                    {
                        ChunkData _meshMaterial = new ChunkData(kMeshMaterial);

                        int mat = g.Id;
                        if (g.Id >= meshExport.Materials.Count)
                        {
                            mat = 0;
                        }
                        byte[] name = null;


                        /**/
                        if (mat < meshExport.Materials.Count) name = encoding.GetBytes(meshExport.Materials[mat].Name + "\0");
                        else
                            continue;
                        /**/

                        ushort numberOfEntries = (ushort)g.FaceCount;
                        ushort[] entries = new ushort[g.FaceCount];
                        int entry = 0;
                        for (int ff = g.FaceStart; ff < g.FaceStart + g.FaceCount; ff++)
                        {
                            /**/
                            if (ff >= meshExport.Faces.Count)
                            {
                                int d = 1;
                            }
                            /**/
                            entries[entry++] = (ushort)ff;
                            //entries[ff] = (ushort)(ff);
                            //entries[ff] = 0;
                        }
                        _meshMaterial.CustomData = new byte[name.Length + 2 + entries.Length * 2];

                        byte[] centries = new byte[entries.Length * 2];

                        offset = 0;
                        Array.Copy(name, 0, _meshMaterial.CustomData, offset, name.Length);
                        offset += name.Length;
                        Array.Copy(BitConverter.GetBytes(numberOfEntries), 0, _meshMaterial.CustomData, offset, 2);
                        offset += 2;

                        int o = 0;
                        foreach (ushort v in entries)
                        {
                            Array.Copy(BitConverter.GetBytes(v), 0, centries, o, 2);
                            o += 2;
                        }
                        Array.Copy(centries, 0, _meshMaterial.CustomData, offset, centries.Length);


                        _faces.Children.Add(_meshMaterial);
                        c++;
                    }
                }

                /// Local Matrix
                /// 
                
                ChunkData _localMatrix = new ChunkData(kMeshXFMatrix);
                _localMatrix.CustomData = new byte[48];

                JMatrix local = new JMatrix();
                /**/
                if (meshExport.Type == eNodeObjectType.eMshHook)
                {
                    int dddd = 1;
                }

                if (meshExport.ParentMeshObject != null && meshExport.OverrideLocalMatrix == null)
                {
                    //local = meshExport.ParentMeshObject.GlobalPosition;
                    local = meshExport.ParentMeshObject.LocalPosition;
                }
                else if (meshExport.OverrideLocalMatrix != null)
                {
                    local = meshExport.OverrideLocalMatrix;
                }
                /**/
                JVector axisx = local.RowX;
                JVector axisy = local.RowY;
                JVector axisz = local.RowZ;
                JVector pos = local.Pos;
                Array.Copy(axisx.GetBytes3x(), 0, _localMatrix.CustomData, 0, 12);
                Array.Copy(axisy.GetBytes3x(), 0, _localMatrix.CustomData, 12, 12);
                Array.Copy(axisz.GetBytes3x(), 0, _localMatrix.CustomData, 24, 12);
                Array.Copy(pos.GetBytes3x(), 0, _localMatrix.CustomData, 36, 12);

                _objectMesh.Children.Add(_vertices);
                if(_uvs!=null)
                    _objectMesh.Children.Add(_uvs);
                if(_faces!=null)
                    _objectMesh.Children.Add(_faces);
                _objectMesh.Children.Add(_localMatrix);

                _objectBlock.Children.Add(_objectMesh);
                _3deditor.Children.Add(_objectBlock);
            }

 
            ChunkData _keyFrameChunk = new ChunkData(kChunkKeyFrame);
            ///Object hierarchy

            if (hierarchy == true)
            {
                foreach (MeshData meshExport in meshes)
                {
                    int offset = 0;
                    /// Frame segment
                    ChunkData _keyFrameSegment = new ChunkData(kTrackInfoTag);
                    
                    /**/
                    ChunkData _keyObjectNode = new ChunkData(kKeyFrameObjectName);

                    byte[] name = encoding.GetBytes(meshExport.Name + "\0");

                    _keyObjectNode.CustomData = new byte[name.Length + 3 * 2];
                    offset = 0;
                    Array.Copy(name, 0, _keyObjectNode.CustomData, offset, name.Length);
                    offset += name.Length;
                    Array.Copy(BitConverter.GetBytes((short)0x4000), 0, _keyObjectNode.CustomData, offset, sizeof(short));
                    offset += 2;
                    Array.Copy(BitConverter.GetBytes((short)0), 0, _keyObjectNode.CustomData, offset, sizeof(short));
                    offset += 2;
                    Array.Copy(BitConverter.GetBytes((short)meshExport.HierarchyLevel), 0, _keyObjectNode.CustomData, offset, sizeof(short));
                    offset += 2;
                    _keyFrameSegment.Children.Add(_keyObjectNode);

                    if (meshExport.Name == "nose_d0_00")
                    {
                        int ddd = 1;
                    }

                    /** OBJECT NUMBER **/
                    ChunkData _keyObjectNodeId = new ChunkData(kKeyFrameNodeID);
                    _keyObjectNodeId.CustomData = BitConverter.GetBytes((short)meshExport.ModelNumber);
                    _keyFrameSegment.Children.Add(_keyObjectNodeId);
                    /**/
                    JVector vc = new JVector(0, 0, 0);
                    ///PIVOT POSITION///
                    if (meshExport.Pivot != null)
                    {
                        vc = meshExport.Pivot;
                        ChunkData _keyObjectPivot = new ChunkData(kKeyFramePivot);
                        _keyObjectPivot.CustomData = new byte[3 * sizeof(Single)];
                        Array.Copy(vc.GetBytes3x(), 0, _keyObjectPivot.CustomData, 0, 3 * sizeof(Single));
                        _keyFrameSegment.Children.Add(_keyObjectPivot);
                    }
                    //// KEY POSITION //
                    
                    if (meshExport.KeyPosition != null)
                    {
                        vc = meshExport.KeyPosition;
                        ChunkData _keyObjectPos = new ChunkData(kKeyFramePos);
                        _keyObjectPos.CustomData = WriteKey(vc);
                        _keyFrameSegment.Children.Add(_keyObjectPos);
                    }
                    ///// KEY ROTATION ////
                    if (meshExport.KeyRotation != null)
                    {
                        vc = meshExport.KeyRotation;
                        ChunkData _keyObjectRot = new ChunkData(kKeyFrameRot);
                        _keyObjectRot.CustomData = WriteKey(vc, meshExport.KeyAngle);
                        _keyFrameSegment.Children.Add(_keyObjectRot);
                    }
                    //// KEY SCALE ////
                    if (meshExport.KeyScale != null)
                    {
                        vc = meshExport.KeyScale;
                        ChunkData _keyObjectScale = new ChunkData(kKeyFrameScale);
                        _keyObjectScale.CustomData = WriteKey(vc);
                        _keyFrameSegment.Children.Add(_keyObjectScale);
                    }
                    /**/
                    

                    
                    _keyFrameChunk.Children.Add(_keyFrameSegment);

                }
            }


            
            //_3deditor.Children.Add(_keyFrameChunk);
            
            _main.Children.Add(_version);
            _main.Children.Add(_3deditor);
            _main.Children.Add(_keyFrameChunk);
            uint size = WriteChunk(ref _main, wr);


            //TraverseChunks(_main, 0, tr);

            wr.Close();
            file.Close();

            //tr.Close();
        }

        public void TraverseChunks(ChunkData parent, int level, StreamWriter tr)
        {
            for (int i = 0; i < level; i++)  tr.Write("\t");
            string line = string.Empty;

            line = string.Format("Identifier:{0:x}\r\n", parent.Identifier);
            tr.Write(line);
            line = string.Format("Lenght:{0}\r\n", parent.Lenght);
            for (int i = 0; i < level; i++) tr.Write("\t");
            tr.Write(line);

            for (int j = 0; j < parent.Children.Count; j++)
            {
                ChunkData child = parent.Children[j];
                TraverseChunks(child, level + 1, tr);
            }
        }

        public uint WriteChunk(ref ChunkData parent, BinaryWriter wr)
        {
            if (parent == null) return 0;

            wr.Write(parent.Identifier);
            uint len = 0;
            uint lenOffset = (uint)wr.BaseStream.Position;
            wr.Write(len);
            uint custom_lenght = 6;
            if (parent.CustomData != null)
            {
                wr.Write(parent.CustomData);
                custom_lenght += (uint)parent.CustomData.Length;
            }

            uint alllenght = custom_lenght;
            for(int j=0;j<parent.Children.Count;j++)
            {
                ChunkData child = parent.Children[j];
                uint lenght = WriteChunk(ref child, wr);
                alllenght += lenght;
            }
            long curOffset = wr.BaseStream.Position;
            wr.BaseStream.Seek(lenOffset, SeekOrigin.Begin);
            wr.Write(alllenght);
            wr.BaseStream.Seek(curOffset, SeekOrigin.Begin);
            parent.Lenght = (uint)alllenght;

            return alllenght;
        }


        public Node Load3ds(string fileName,out List<MeshData>  mshList)
        {
            Node root = null;
            Chunks.Clear();
            FileStream file = File.OpenRead(fileName);

            mshList =null;
            List<MeshData> meshObjects = new List<MeshData>();

            if (file == null) return root;
            BinaryReader br = new BinaryReader(file);

            MeshData mesh = null;
            MeshData lastmesh = null;
            Material material = null;
            Group group = null;
            int oldFaceStart =0;
            int oldVertexStart = 0;

            root = new Node();
            root.Name = Path.GetFileNameWithoutExtension(fileName);

            SortedDictionary<string, MeshData> dictMeshes = new SortedDictionary<string, MeshData>();
            SortedDictionary<string, Material> dictMaterials = new SortedDictionary<string, Material>();
            SortedDictionary<int, MeshData> dictMeshNumber = new SortedDictionary<int, MeshData>();
            List<Material> Materials = new List<Material>();

            int modelNumber = 0;

            Mesh3dsParent parent = null;
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                ChunkData c = new ChunkData();
                c.Id = Chunks.Count;
                c.Parent = null;
                c.Identifier = br.ReadUInt16();
                c.Lenght = br.ReadUInt32();

                Chunks.Add(c);

                int count = 0;
                int i = 0;

                switch (c.Identifier)
                {
                    //////////// Material ////////
                    case 0xAFFF://kMat
                        
                    break;

                    case 0xA000: //kMatName
                        Material mat = ReadMaterial(ref br);
                        if (material != null)
                        {
                            if (dictMaterials.ContainsKey(material.Name) != true)
                            {
                                dictMaterials.Add(material.Name, material);
                                Materials.Add(material);
                            }
                            
                        }
                        material = mat;
                        
                    break;

                    case 0xA200: //kMatTexMap

                    break;

                    case 0xA300:  //Mapping filename
                        string texture = ReadString(ref br);
                        if (material != null)
                        {
                            MaterialLayer lay = new MaterialLayer();
                            lay.Name = texture;
                            material.Layers.Add(lay);
                        }

                    break;

                    ///// KeyFrames
                    case 0xB000: //kChunkKeyFrame
                        Close(ref mesh, ref group, ref dictMeshes, ref dictMeshNumber, root, ref meshObjects);
                        mesh = null;
                        group = null;
                    break;

                    case 0xB002://kObjectNodeTag

                    break;

                    case 0xB010://kKeyFrameNodeHdr
                    {
                        parent = ReadMeshParent(ref br);
                        if (parent != null)
                        {
                            MeshData meshChild = null;
                            MeshData meshParent = null;
                            if (dictMeshes.ContainsKey(parent.Name) == true)
                            {
                                meshChild = dictMeshes[parent.Name];

                                if (parent.Parent >= 0)
                                {
                                    if (dictMeshNumber.ContainsKey(parent.Parent) == true)
                                    {
                                        meshParent = dictMeshNumber[parent.Parent];
                                    }
                                }
                            }
                            if (meshChild!=null & meshParent!=null)
                            {
                                if (meshParent.Node != null && meshChild.Node!=null)
                                {
                                    meshParent.Node.AddNode(meshChild.Node);
                                }
                                else if (meshChild.Node == null)
                                {
                                    if(meshParent.Node!=null)
                                        meshParent.Node.AddObject(meshChild);
                                }
                            }
                            else if (meshChild != null)
                            {
                                if (meshChild.Node != null)
                                {
                                    root.AddNode(meshChild.Node);
                                }
                                else
                                {
                                    root.AddObject(meshChild);
                                }
                            }

                        }
                    }

                    break;

                    case 0xB030://kKeyFrameNodeID
                    {
                        Mesh3dsId id = ReadMeshId(ref br);
                    }

                    break;

                    case kKeyFramePos:
                    {
                        short n, nf = 0;
                        float [] pos = new float[3];
                        short unkown;
                        short flags;
                        int ii;

                        for (ii = 0; ii < 5; ii++)
                        {
                            unkown = br.ReadInt16();
                        }
                        n = br.ReadInt16();
                        unkown = br.ReadInt16();

                        while (n-- > 0)
                        {
                            nf=br.ReadInt16();
                            unkown = br.ReadInt16();
                            flags = br.ReadInt16();
                            pos[0] = br.ReadSingle();
                            pos[1] = br.ReadSingle();
                            pos[2] = br.ReadSingle();
                        }

                        JVector posv = new JVector(pos[0], pos[1], pos[2]);
                        if (parent != null)
                        {
                            MeshData msh = null;
                            if (dictMeshes.ContainsKey(parent.Name) == true)
                            {
                                msh = dictMeshes[parent.Name];

                                if (msh!=null)
                                {
                                    if (msh.KeyPosition == null)
                                        msh.KeyPosition = new JVector();
                                    msh.KeyPosition = posv;
                                }

                                //msh.OverrideLocalMatrix.Pos = msh.OverrideLocalMatrix.Pos + msh.KeyPosition;
                            }
                        }
                    }
                    break;
                    case kKeyFrameRot:
                    {
                        short n, nf = 0;
                        float[] pos = new float[4];
                        short unkown;
                        short flags;
                        int ii;

                        for (ii = 0; ii < 5; ii++)
                        {
                            unkown = br.ReadInt16();
                        }
                        n = br.ReadInt16();
                        unkown = br.ReadInt16();

                        while (n-- > 0)
                        {
                            nf = br.ReadInt16();
                            unkown = br.ReadInt16();
                            flags = br.ReadInt16();

                            pos[0] = br.ReadSingle();
                            pos[1] = br.ReadSingle();
                            pos[2] = br.ReadSingle();
                            pos[3] = br.ReadSingle();
                        }

                        JVector posv = new JVector(pos[1], pos[2], pos[3]);
                        if (parent != null)
                        {
                            MeshData msh = null;
                            if (dictMeshes.ContainsKey(parent.Name) == true)
                            {
                                if (msh!=null)
                                {
                                    if (msh.KeyRotation == null)
                                        msh.KeyRotation = new JVector();

                                    msh = dictMeshes[parent.Name];
                                    msh.KeyAngle = pos[0];
                                    msh.KeyRotation = posv;
                                }
                                
                            }
                        }
                    }
                    break;

                    case kKeyFrameScale:
                    {
                        short n, nf = 0;
                        float[] pos = new float[3];
                        short unkown;
                        short flags;
                        int ii;

                        for (ii = 0; ii < 5; ii++)
                        {
                            unkown = br.ReadInt16();
                        }
                        n = br.ReadInt16();
                        unkown = br.ReadInt16();

                        while (n-- > 0)
                        {
                            nf = br.ReadInt16();
                            unkown = br.ReadInt16();
                            flags = br.ReadInt16();
                            pos[0] = br.ReadSingle();
                            pos[1] = br.ReadSingle();
                            pos[2] = br.ReadSingle();
                        }

                        JVector posv = new JVector(pos[0], pos[1], pos[2]);
                        if (parent != null)
                        {
                            MeshData msh = null;
                            if (dictMeshes.ContainsKey(parent.Name) == true)
                            {
                                msh = dictMeshes[parent.Name];

                                if (msh != null)
                                {
                                    if (msh.KeyScale == null)
                                        msh.KeyScale = new JVector();
                                    msh.KeyScale = posv;

                                    if (msh.Node != null)
                                    {
                                        //msh.Node.Local.Scale(msh.KeyScale);
                                    }
                                }
                            }
                        }
                    }
                    break;

                    case   0x4d4d:   //kMagic3ds
                    break;


                    case 0x3d3d: //kChunkObjects
                     break;

                    case 0x4000: //kNamedObject
                    {
                        string object_name = ReadString(ref br);
                        if (mesh != null)
                        {
                            if (dictMeshes.ContainsKey(mesh.Name) != true)
                            {
                                /**/
                                if (group != null)
                                {
                                    mesh.AddGroup(group,group.VertexCount,group.FaceCount);
                                    group = null;
                                }
                                /**/
                                mesh.ModelNumber = modelNumber;
                                dictMeshes.Add(mesh.Name, mesh);
                                dictMeshNumber.Add(mesh.ModelNumber, mesh);
                                mesh.FindType();

                                //mesh.CalculateNormals(true);
                                if (mesh.Type == eNodeObjectType.eNode)
                                {
                                    Node node = new Node();
                                    node.Name = mesh.Name;
                                    node.Local = mesh.OverrideLocalMatrix;

                                    mesh.Node = node;

                                    root.AddNode(node);
                                }
                                else
                                {
                                    root.AddObject(mesh);
                                }
                                mesh.Normalize2();
                                mesh.CalculateNormals(true);
                                meshObjects.Add(mesh);
                                modelNumber++;


                            }
                        }
                        oldFaceStart = 0;
                        oldVertexStart = 0;


                                                                        
                        mesh = new MeshData();
                        mesh.Name = object_name;
                        mesh.ModelNumber = modelNumber;
                        mesh.Type = Library3d.Nodes.eNodeObjectType.e3dMesh;

                        mesh.FindType();

                        /// Test
                        /*
                        Node n = new Node(object_name);
                        n.AddObject(mesh);
                        mesh.Node = n;
                        /**/

                    }
                    break;


                    case 0x4100: //kObjMesh
                    {

                    }
                    break;

                    case 0x4110://kMeshVerts
                    {
                        List<Vertex> list = ReadVertex(ref br);
                        if (mesh != null)
                        {
                            mesh.Vertexs = list;

                            
                        }
                    }
                    break;

                    case kKeyFramePivot:
                    {
                        float [] position =new float[3];
                        for (int pi = 0; pi < 3; pi++)
                        {
                            position[pi] = br.ReadSingle();
                        }

                        MeshData msh = null;
                        if (dictMeshes.ContainsKey(parent.Name) == true)
                        {
                            msh = dictMeshes[parent.Name];

                            if (msh!=null)
                            {
                                if (msh.Pivot == null)
                                    msh.Pivot = new JVector();
                                msh.Pivot.x = position[0];
                                msh.Pivot.y = position[1];
                                msh.Pivot.z = position[2];
                            }
                            
                        }


                    }
                    break;

                    case 0x4120://kMeshFaces
                    {
                        List<Face> list = ReadFaces(ref br);
                        if (mesh != null)
                        {
                            mesh.Faces = list;
                        }
                    }
                    break;

                    case 0x4130://kMeshMaterial
                    {
                        Group3ds gr = ReadGroup(ref br,mesh);
                        if (gr != null && gr.Faces.Count>0)
                        {
                            
                            if (mesh != null)
                            {
                                if (mesh.Name == "Blister1_D")
                                {
                                    int d = 1;
                                }
                                if (group != null)
                                {
                                    mesh.AddGroup(group,group.VertexCount,group.FaceCount);
                                }
                                                                
                                group = new Group();
                                group.FaceCount = gr.Faces.Count;
                                group.VertexCount = gr.VertexCount;
                                group.VertexStart = gr.VertexStart;
                                group.FaceStart = gr.FaceStart;


                                //Add new material
                                Material matm = new Material();
                                matm.Name = gr.MaterialName;
                                group.Id = mesh.AddMaterial(matm);


                                if (mesh.Faces != null)
                                {
                                    for (int f = group.FaceStart; f < group.FaceStart + group.FaceCount; f++)
                                    //foreach (int f in gr.Faces)
                                    {
                                        if (f < mesh.Faces.Count)
                                        {
                                            Face face = mesh.Faces[f];
                                            face.group_id = group.Id;
                                        }
                                    }
                                }
                                /*
                                int min = 99999999;
                                int max = -99999999;
                                if (mesh.Faces != null)
                                {
                                    //for (int f = group.FaceStart; f < group.FaceStart + group.FaceCount; f++)
                                    foreach (int f in gr.Faces)
                                    {
                                        if (f < mesh.Faces.Count)
                                        {
                                            Face face = mesh.Faces[f];
                                            face.group_id = group.Id;

                                            for (int ff = 0; ff < 3; ff++)
                                            {
                                                if (face.v(ff) < min) min = face.v(ff);
                                                if (face.v(ff) > max) max = face.v(ff);

                                            }
                                        }
                                    }

                                    group.VertexCount = max - min+1;
                                    group.VertexStart = min;


                                    for (int f = group.FaceStart; f < group.FaceStart + group.FaceCount; f++)
                                    {
                                        if (f >= mesh.Faces.Count) continue;
                                        Face face = mesh.Faces[f];

                                        face.v1 -= group.VertexStart;
                                        face.v2 -= group.VertexStart;
                                        face.v3 -= group.VertexStart;
                                    }

                                }

                                int id = 0;

                                /*
                                int countx = 0;
                                foreach (Material m in Materials)
                                {
                                    if (m.Name == gr.Name)
                                    {
                                        id = countx;
                                        break;
                                    }
                                    countx++;
                                }
                                group.Id = id;
                                /**/
                                oldFaceStart += group.FaceCount;
                                oldVertexStart += group.VertexCount;
                            }
                        }
                    }

                    break;

                    case 0x4140://kMeshTexVert
                    {
                        List<UV> list = ReadUVs(ref br);
                        if (mesh != null)
                        {
                            mesh.UVs = list;
                        }
                    }
                    break;

                    case 0x4150://kMeshSmoothGroup
                    {
                        if (mesh != null)
                        {
                            int faces = mesh.Faces.Count;
                            List<ushort> list = ReadSmoothing(ref br, faces);

                            int ddd = 1;
                            ddd++;
                        }
                    }
                    break;

                    case 0x4160: //kMeshXFMatrix
                    {
                        JMatrix localMat = ReadMatrix(ref br);
                        if (mesh != null)
                        {
                            mesh.OverrideLocalMatrix = localMat;

                            if (mesh.Node != null)
                                mesh.Node.Local = localMat;

                        }

                    }
                    break;

               
                    default:
                        br.BaseStream.Seek(c.Lenght - 6, SeekOrigin.Current);
                    break;
                }
            }
            br.Close();
            file.Close();

            if (material != null)
            {
                if (dictMaterials.ContainsKey(material.Name) != true)
                {
                    dictMaterials.Add(material.Name, material);
                    if (mesh != null)
                    {
                        mesh.Normalize2();
                        mesh.CalculateNormals(true);
                        meshObjects.Add(mesh);
                    }
                    Materials.Add(material);
                }
            }

            Close(ref mesh, ref group, ref dictMeshes, ref dictMeshNumber, root, ref meshObjects);

            mshList = meshObjects;

            return root;
        }

        public void Close(ref MeshData mesh, ref Group group, ref SortedDictionary<string, MeshData> dictMeshes, ref SortedDictionary<int, MeshData> dictMeshNumber, Node root, ref List<MeshData> meshObjects)
        {
            if (mesh != null)
            {
                if (dictMeshes.ContainsKey(mesh.Name) != true)
                {
                    if (group != null)
                    {
                       mesh.AddGroup(group,group.VertexCount,group.FaceCount);
                       group = null;
                    }
                    
                    mesh.FindType();
                    dictMeshes.Add(mesh.Name, mesh);
                    dictMeshNumber.Add(mesh.ModelNumber, mesh);
                    meshObjects.Add(mesh);

                    if (mesh.Type == eNodeObjectType.eNode)
                    {
                        Node node = new Node();
                        node.Name = mesh.Name;
                        node.Local = mesh.OverrideLocalMatrix;

                        mesh.Node = node;

                        root.AddNode(node);
                    }
                    else
                    {
                        root.AddObject(mesh);
                    }

                    //root.AddObject(mesh);
                    //meshObjects.Add(mesh);
                }
            }
        }

        protected string ReadString(ref BinaryReader br)
        {
            byte l_char;
            string object_name = string.Empty;
            do
            {
                l_char = br.ReadByte();
                if(l_char!='\0') object_name += Convert.ToChar(l_char);
            } while (l_char != '\0');
            return object_name;
        }

        protected JMatrix ReadMatrix(ref BinaryReader br)
        {
            JMatrix mat = new JMatrix();
            mat.RowX = ReadVector(ref br);
            mat.RowY = ReadVector(ref br);
            mat.RowZ = ReadVector(ref br);
            mat.Pos = ReadVector(ref br);




            return mat;
        }

        protected JVector ReadVector(ref BinaryReader br)
        {
            JVector vec = new JVector();

            vec.x = br.ReadSingle();
            vec.y = br.ReadSingle();
            vec.z = br.ReadSingle();

            return vec;
        }

        public static void TraverseAndLink(Node root)
        {
            if (root == null) return;

            List<NodeObject> removed = new List<NodeObject>();
            for(int k=0;k<root.Objects.Count;k++)
            {
                NodeObject o = root.Objects[k];

                if (o.GetType() == typeof(MeshData))
                {
                    MeshData d = (MeshData)o;

                    if (d.Type == eNodeObjectType.e3dMeshCollision)
                    {

                        int dd= 1;
                    }
                    else
                    if (d.Type == eNodeObjectType.e3dMshShadow)
                    {
                        string name = string.Empty;
                        string[] split = d.Name.Split(' ');
                        StringBuilder strb = new StringBuilder();
                        for (int i = 0; i < split.Length - 1; i++)
                        {
                            strb.Append(' ');
                            strb.Append(split[i]);
                        }
                        name = strb.ToString();
                        NodeObject no = root.FindObject(name);
                        if (no != null && no.GetType() == typeof(MeshData))
                        {
                            MeshData pmsh = (MeshData)no;
                            pmsh.Shadow = d;
                            //root.RemoveObject(d);
                            //removed.Add(d);
                        }
                    }
                }
            }
            foreach (NodeObject o in removed)
            {
                root.RemoveObject(o);
            }

            for(int j=0;j<root.Children.Count;j++)
            {
                Node node = root.Children[j];
                TraverseAndLink(node);
            }
        }
        
       
    }
}
