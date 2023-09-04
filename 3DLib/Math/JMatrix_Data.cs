

using System;
using System.Collections.Generic;
using System.Text;

namespace Library3d.Math
{
    public partial class JMatrix
    {
        public float[] f = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public float m11
        {
            get { return f[0]; }
            set { f[0] = value; }
        }
        public float m12
        {
            get { return f[1]; }
            set { f[1] = value; }
        }
        public float m13
        {
            get { return f[2]; }
            set { f[2] = value; }
        }
        public float m14
        {
            get { return f[3]; }
            set { f[3] = value; }
        }
        public float m21
        {
            get { return f[4]; }
            set { f[4] = value; }
        }
        public float m22
        {
            get { return f[5]; }
            set { f[5] = value; }
        }
        public float m23
        {
            get { return f[6]; }
            set { f[6] = value; }
        }
        public float m24
        {
            get { return f[7]; }
            set { f[7] = value; }
        }
        public float m31
        {
            get { return f[8]; }
            set { f[8] = value; }
        }
        public float m32
        {
            get { return f[9]; }
            set { f[9] = value; }
        }
        public float m33
        {
            get { return f[10]; }
            set { f[10] = value; }
        }
        public float m34
        {
            get { return f[11]; }
            set { f[11] = value; }
        }
        public float m41
        {
            get { return f[12]; }
            set { f[12] = value; }
        }
        public float m42
        {
            get { return f[13]; }
            set { f[13] = value; }
        }
        public float m43
        {
            get { return f[14]; }
            set { f[14] = value; }
        }
        public float m44
        {
            get { return f[15]; }
            set { f[15] = value; }
        }
        public float m(int a,int b)
        {
            return f[a * 4 + b];
        }

        public void m(int a, int b, float set)
        {
            f[a * 4 + b] = set;
        }
        public JVector v(int index)
        {
            if (index == 0) return RowX;
            else if (index == 1) return RowY;
            else if (index == 2) return RowZ;
            else if (index == 3) return Pos;
            else throw new Exception("Error");
        }

        public void v(int index,JVector v)
        {
            if (index == 0) RowX = v;
            else if (index == 1) RowY = v;
            else if (index == 2) RowZ = v;
            else if (index == 3) Pos = v;
            else throw new Exception("Error");
        }
        public JVector RowX
        {
            get
            {
                return new JVector(f[0], f[1], f[2], f[3]);
            }
            set
            {
                f[0] = value.x; f[1] = value.y; f[2] = value.z; f[3] = value.w;
            }
        }
        public JVector RowY
        {
            get
            {
                return new JVector(f[4], f[5], f[6], f[7]);
            }
            set
            {
                f[4] = value.x; f[5] = value.y; f[6] = value.z; f[7] = value.w;
            }
        }

        public JVector RowZ
        {
            get
            {
                return new JVector(f[8], f[9], f[10], f[11]);
            }
            set
            {
                f[8] = value.x; f[9] = value.y; f[10] = value.z; f[11] = value.w;
            }
        }

        public JVector Pos
        {
            get
            {
                return new JVector(f[12], f[13], f[14], f[15]);
            }
            set
            {
                f[12] = value.x; f[13] = value.y; f[14] = value.z; f[15] = value.w;
            }
        }

        
    }
}
