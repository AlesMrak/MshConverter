using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

using Library3d.Mesh;
using Library3d.Math;

namespace Library3d.Format3ds
{
    public partial class Object3ds
    {
        protected Material ReadMaterial(ref BinaryReader br)
        {
            Material mat = new Material();

            mat.Name = ReadString(ref br);

            return mat;
        }
    }
}
