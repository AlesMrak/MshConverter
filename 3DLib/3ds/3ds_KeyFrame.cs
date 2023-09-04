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
        protected Mesh3dsParent ReadMeshParent(ref BinaryReader br)
        {
            Mesh3dsParent mp = new Mesh3dsParent();

            mp.Name = ReadString(ref br);
            mp.Flags1 = br.ReadUInt16();
            mp.Flags2 = br.ReadUInt16();
            mp.Parent = br.ReadUInt16();

            return mp;
        }

        protected Mesh3dsId ReadMeshId(ref BinaryReader br)
        {
            Mesh3dsId mp = new Mesh3dsId();
            mp.Id = br.ReadUInt16();
            return mp;
        }

        protected byte[] WriteKey(JVector p)
        {
            byte[] val = new byte[10 * 2 + 3 * 4];

            byte[] head = new byte[20];

            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 0, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 2, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 4, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 6, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 8, 2);
            Array.Copy(BitConverter.GetBytes((short)1), 0, head, 10, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 12, 2);

            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 14, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 16, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 18, 2);
            

            Array.Copy(head, 0, val, 0, head.Length);
            Array.Copy(p.GetBytes3x(), 0, val, head.Length, 3 * 4);


            return val;
        }


        protected byte[] WriteKey(JVector p,float angle)
        {
            byte[] val = new byte[10 * 2 + 4 * 4];

            byte[] head = new byte[20];

            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 0, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 2, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 4, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 6, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 8, 2);
            Array.Copy(BitConverter.GetBytes((short)1), 0, head, 10, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 12, 2);

            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 14, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 16, 2);
            Array.Copy(BitConverter.GetBytes((short)0), 0, head, 18, 2);


            Array.Copy(head, 0, val, 0, head.Length);
            Array.Copy(BitConverter.GetBytes(angle), 0, val, head.Length, 4);
            Array.Copy(p.GetBytes3x(), 0, val, head.Length+4, 3 * 4);


            return val;
        }

        
    }
}
