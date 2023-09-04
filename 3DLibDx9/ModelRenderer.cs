using System;
using System.Collections.Generic;
using System.Text;

using Library3d.Interface;
using Library3d.Math;
using Library3d.Mesh;
using Library3d.Nodes;
using Library3d.Error;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Library3dDx9
{
    public class ModelRenderer : IRenderObject
    {
        public Library3d.Mesh.MeshData meshData = null;

        protected Mesh DxMesh = null;

        public NodeObject DataObject
        {
            get { return meshData; }
            set { if (value.GetType() == typeof(Library3d.Mesh.MeshData)) meshData = (Library3d.Mesh.MeshData)value; }
        }

        public bool Visible
        {
            get { if (DataObject != null)
                return DataObject.Visible; 
                    else return false; }
            set { if (DataObject != null) DataObject.Enabled = value; }
        }

        public bool CanRender
        {
            get { return Visible;}
        }

        public void Dispose()
        {
            if (DxMesh != null)
                DxMesh.Dispose();
            DxMesh = null;
            if (DataObject != null)
                DataObject.RenderObject = null;
        }

        public void Render(object data)
        {
            if (Visible == true)
            {
                if (DxMesh == null) return;
                Device device = (Device)data;

                
                // ... and use the world matrix to spin and translate the teapot  
                // out where we can see it...
                JMatrix global = new JMatrix();


                
                if (this.DataObject != null && DataObject.ParentNode != null)
                    if(this.DataObject.ObjectMatrix!=null)
                        global = DataObject.ParentNode.Global * this.DataObject.ObjectMatrix ;
                    else
                        global = DataObject.ParentNode.Global;

                Matrix world = Utility3d.ToMatrix(global);
                device.Transform.World = world;


                int i = 0;
                do
                {
                    DxMesh.DrawSubset(i);
                    i++;
                }
                while (i < DxMesh.NumberAttributes);
            }
        }
        public void Update(object data)
        {

        }
        public void Init(object data)
        {
            if (DxMesh != null)
                DxMesh.Dispose();
            if (meshData == null) return;

            Device device = (Device)data;

            DxMesh = Utility3d.ConvertMeshData2Data(meshData, device);
        }

        public void Save(string fileName)
        {
            if (DxMesh == null) return;

            ExtendedMaterial[] mat = new ExtendedMaterial[1];

            try
            {
                int[] adj = new int[meshData.Vertexs.Count];
                DxMesh.GenerateAdjacency(0.1f, adj);
                DxMesh.Save(fileName, adj, mat, XFileFormat.Binary);
            }
            catch (Exception e)
            {
                ErrorMessages.Global.AddErrorMessage(string.Format("Failed to save {0}!\r\n", fileName), 0);
            }
        }
    }
}
