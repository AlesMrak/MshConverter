using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Library3d.Math;
using Library3d.Nodes;
using Library3d.Error;

namespace Library3d.Mesh
{
    public partial class HierLoader
    {

        public void Save(string filename, Node root)
        {
            FileName = filename;

            StreamWriter rw = File.CreateText(filename);
            if (rw == null) return;

            foreach (Node child in root.Children)
            {
                HierNode n = new HierNode();
                CreateHierNodes(child, n);
                Root = n;
                break;
                
            }
            
            WriteHierNode(Root, rw);


            rw.Close();

            ErrorMessages.Global.AddErrorMessage(string.Format("Saved {0} to {1}!\r\n", root.Name, filename), 2);
        }

        public void CreateHierNodes(Node root, HierNode hroot)
        {
            hroot.Name = root.Name;
            hroot.Local = root.Local;
            hroot.Global = root.Global;

            hroot.Name = hroot.Name.Replace("[", string.Empty);
            hroot.Name = hroot.Name.Replace("]", string.Empty);
            

            
            if (root.Objects.Count > 0)
            {
                MeshData msh = null;
                SortedDictionary<int, MeshData> lods = new SortedDictionary<int, MeshData>();
                SortedDictionary<string, MeshData> hooks = new SortedDictionary<string, MeshData>();
                List<MeshData> collisionObj = new List<MeshData>();

                hroot.MeshLoaded = new MeshObject();

                foreach (NodeObject no in root.Objects)
                {
                    if (no.Type == eNodeObjectType.e3dMesh && msh == null)
                        msh = (MeshData)no;
                    else if (no.Type == eNodeObjectType.e3dMeshLod)
                    {
                        MeshData ld = (MeshData)no;
                        if (lods.ContainsKey(ld.LodNumber) == false)
                            lods.Add(ld.LodNumber, ld);
                        else
                        {
                            lods.Add(lods.Values.Count, ld);
                        }
                    }
                    else if (no.Type == eNodeObjectType.eMshHook)
                    {
                        MeshData ld = (MeshData)no;
                        hooks.Add(no.Name, ld);
                    }
                    else if (no.Type == eNodeObjectType.e3dMeshCollision)
                    {
                        MeshData ld = (MeshData)no;
                        collisionObj.Add(ld);
                    }

                }
                if (msh != null)
                {
                    hroot.Mesh = msh.Name;
                    hroot.MeshLoaded.Meshes = msh;
                    hroot.MeshLoaded.Name = msh.Name;
                    foreach (KeyValuePair<int, MeshData> slod in lods)
                    {
                        hroot.MeshLoaded.LodsDist.Add(slod.Value.LodDistance);
                        hroot.MeshLoaded.Lods.Add(slod.Value);
                    }

                    foreach (KeyValuePair<string, MeshData> hook in hooks)
                    {
                        Hook h = new Hook();
                        h.Position = new JMatrix(hook.Value.OverrideLocalMatrix);
                        
                        string [] str = hook.Value.Name.Split(' ');
                        if (str.Length > 1)
                        {
                            h.Name = str[0];
                            h.Value = str[1];
                            hroot.MeshLoaded.Hooks.Add(h);
                        }
                    }

                    if (collisionObj.Count > 0)
                    {
                        SortedDictionary<int, MeshCollisionBlock> blocks = new SortedDictionary<int, MeshCollisionBlock>();
                        SortedDictionary<int, MeshCollisionPart> parts = new SortedDictionary<int, MeshCollisionPart>();

                        foreach (MeshData data in collisionObj)
                        {
                            char[] split = { ' ', '\t' };
                            string[] names = data.Name.Split(split);

                            string fname = string.Empty;
                            string blockn = string.Empty;
                            string partnn = string.Empty;
                            string framenum = string.Empty;
                            string modelName = string.Empty;

                            if (names.Length > 0)
                                fname = names[0];
                            if (names.Length > 1)
                                blockn = names[1];
                            if (names.Length > 2)
                                partnn = names[2];
                            if (names.Length > 3)
                                framenum = names[3];
                            if (names.Length > 4)
                                modelName = names[4];

                            MeshCollisionBlock currBlock = null;
                            MeshCollisionPart currPart = null;

                            if (blockn != string.Empty)
                            {
                                string[] numspl = blockn.Split('b');
                                int numb = 0;
                                if (numspl.Length > 1)
                                    int.TryParse(numspl[1], out numb);
                                if (blocks.ContainsKey(numb) == true)
                                {
                                    currBlock = blocks[numb];
                                }
                                else
                                {
                                    currBlock = new MeshCollisionBlock();
                                    currBlock.Id = numb;
                                    blocks.Add(numb, currBlock);

                                    hroot.MeshLoaded.Collisions.Add(currBlock);
                                }
                            }

                            if (partnn != string.Empty)
                            {
                                string[] numspl = partnn.Split('p');
                                int numb = 0;
                                if (numspl.Length > 1)
                                    int.TryParse(numspl[1], out numb);
                                if (parts.ContainsKey(numb) == true)
                                {
                                    currPart = parts[numb];
                                }
                                else
                                {

                                    currPart = currBlock.GetPart(numb);
                                    if (currPart == null)
                                    {
                                        currPart = new MeshCollisionPart();
                                        currPart.Id = numb;
                                        currPart.Name = fname;
                                        parts.Add(numb, currPart);
                                        currBlock.AddPart(currPart);
                                    }
                                }
                            }

                            int fnumb = 0;

                            if (framenum != string.Empty)
                            {
                                int.TryParse(framenum, out fnumb);
                            }

                            MeshCollisionFrame from = data.GetCollision();
                            from.Id = fnumb;
                            from.FName = fname;

                            currPart.AddFrame(from);

                        }
                    }
                }
                else
                {
                    hroot.MeshLoaded = null;
                }
            }


            foreach (Node child in root.Children)
            {
                HierNode n = new HierNode();
                n.ParentName = hroot.Name;
                CreateHierNodes(child, n);
                hroot.Nodes.Add(n);
            }
        }

        public void WriteHierNode(HierNode node, StreamWriter rw)
        {
            rw.WriteLine(string.Format("[{0}]",node.Name));
            if (node.VisibilitySphere > 0)
            {
                rw.WriteLine(string.Format("VisibilitySphere {0}", node.VisibilitySphere));
            }
            if (node.Mesh != string.Empty)
            {
                rw.WriteLine("Mesh {0}", node.Mesh);
            }
            if (node.ParentName != string.Empty)
            {
                rw.WriteLine("Parent {0}", node.ParentName);
            }
            if (node.Local != null)
            {
                rw.WriteLine("Attaching {0} {1} {2} {3}", node.Local.RowX.ToString(), node.Local.RowY.ToString(), node.Local.RowZ.ToString(), node.Local.Pos.ToString());
            }
            if (node.Separable == true)
            {
                rw.WriteLine("Separable");
            }
            if (node.Visible == false)
            {
                rw.WriteLine("Hidden");
            }

            if (node.MeshLoaded != null)
            {
                MeshObject.SaveToMsh(node.Mesh + ".msh", node.MeshLoaded);

                foreach( MeshCollisionBlock b in node.MeshLoaded.Collisions)
                {
                    foreach( MeshCollisionPart p in b.Parts)
                    {
                        rw.WriteLine(string.Format("CollisionObject {0}", p.Name));
                    }
                }
            }

            foreach (HierCollisionObject obj in node.Collisions)
            {
                if (obj.Type !=string.Empty)
                {
                    rw.WriteLine(string.Format("CollisionObject sphere {0} {1} {2} {3}", obj.R,obj.Position.x,obj.Position.y,obj.Position.z));
                }
                else
                {
                    rw.WriteLine(string.Format("CollisionObject {0}", obj.Name));
                }
            }

            foreach (HierNode child in node.Nodes)
            {
                WriteHierNode(child, rw);
            }
        }

    }
}
