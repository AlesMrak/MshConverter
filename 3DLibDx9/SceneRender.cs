using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Library3d.Math;
using Library3d.Mesh;
using Library3d.Nodes;
using Library3d.Md5;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Library3dDx9
{
    public class SceneRender
    {
        public Node Root;
        Device Device = null;

        public void Init(object data)
        {
            Device = (Device)data;
            if (Root != null)
                _Init(Root);
        }
        protected void _Init(Node root)
        {
            foreach (NodeObject o in root.Objects)
            {
                if (o.GetType() == typeof(Library3d.Mesh.MeshData))
                {
                    //if (o.Type == eNodeObjectType.e3dMesh)
                    {
                        Library3d.Mesh.MeshData m = (Library3d.Mesh.MeshData)o;
                        if (o.RenderObject == null)
                        {
                            o.RenderObject = new ModelRenderer();
                            o.RenderObject.DataObject = o;
                            o.RenderObject.Init(Device);
                            

                        }
                    }
                }
            }
            foreach (Node c in root.Children)
            {
                _Init(c);
            }
        }

        public void Render(Node root,object data,int width,int height)
        {
            Device = (Device)data;
            Root = root;


            Microsoft.DirectX.Direct3D.Viewport leftViewPort = new Viewport();
            leftViewPort.X = 0;
            leftViewPort.Y = 0;
            leftViewPort.Width = width;
            leftViewPort.Height = height;
            leftViewPort.MinZ = 0.0f;
            leftViewPort.MaxZ = 1.0f;


            Device.Viewport = leftViewPort;

            Device.RenderState.CullMode = Cull.None;
                        
            //Device.Transform.View = Matrix.Identity;

            // Now we can clear just view-port's portion of the buffer to red...
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer,
                             Color.FromArgb(255, 0, 0, 255), 1.0f, 0);

            Device.BeginScene();


            Microsoft.DirectX.Direct3D.Material teapotMtrl;
            teapotMtrl = new Microsoft.DirectX.Direct3D.Material();
            teapotMtrl.Diffuse = Color.White;

            Device.Material = teapotMtrl;
            
            _Render(Root, Device);

            Device.EndScene();

            Device.Present();
        }

        protected void _Render(Node root, object data)
        {
            foreach (NodeObject o in root.Objects)
            {
                if (o.RenderObject != null)
                {
                    if (o.Type == eNodeObjectType.eMshHook)
                    {
                        int d = 1;
                    }
                     o.RenderObject.Render(data);
                }
            }
            foreach (Node c in root.Children)
            {
                _Render(c, data);
            }
        }

        public void Dispose()
        {
            if (Root != null)
                _Dispose(Root);
            
        }

        public void _Dispose(Node root)
        {
            foreach (NodeObject o in root.Objects)
            {
                if (o.RenderObject != null)
                {
                    o.RenderObject.Dispose();
                }
            }
            foreach (Node c in root.Children)
            {
                _Dispose(c);
            }
        }
    }
}
