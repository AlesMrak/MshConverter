using System;
using System.Collections.Generic;
using System.Text;

namespace Library3d.Math
{

    public enum PointComponent
    {
        _X = 0,
        _Y = 1,
        _Z = 2,
        _W = 3,
        _FORCE_DWORD = 0x7fffffff
    };

    public class JVector
    {
        public static float CDEG=0.017453292519943295769236907684886f;
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float w = 0;

        public JVector()
        {
            SetZero();
        }

        public JVector(float a)
        {
            x = y = z = w = a;
        }

        public JVector(float xx,float yy,float zz,float ww)
        {
            x = xx; y = yy; z = zz; w = ww;
        }

        public JVector(float xx, float yy, float zz)
        {
            x = xx; y = yy; z = zz; w = 0;
        }

        public JVector(JVector a)
        {
            Set(a);
        }

        public void Set(JVector a)
        {
            x = a.x; y = a.y; z = a.z; w = a.w;
        }

        public static JVector operator ^(JVector p1, JVector p2)	
	    {
		    return new JVector(
            p1.y * p2.z - p1.z * p2.y,
            p1.z * p2.x - p1.x * p2.z,
            p1.x * p2.y - p1.y * p2.x);
	    }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", x, y, z);
        }


        public float Get(int index)
        {
            if (index == 0) return x;
            else if (index == 1) return y;
            else if (index == 2) return z;
            else if (index == 3) return w;
            else throw new Exception("Error");
        }

        public void Set(float _x,float _y,float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public void Set(int index,float val)
        {
            if (index == 0) x=val;
            else if (index == 1) y = val;
            else if (index == 2) z = val;
            else if (index == 3) w = val;
            else 
                throw new Exception("Error");
        }

        public int	 MinIdx()  { return( x<y ? (x<z ? 0:2) : (y<z ? 1:2) ); }
	    public int	 MaxIdx()  { return( x>y ? (x>z ? 0:2) : (y>z ? 1:2) ); }
        public JVector		Neg( JVector a )	{  return new JVector(-a.x,-a.y,-a.z); }
        public JVector		Max( JVector a,JVector b )
	    {
		    return new JVector( a.x > b.x ? a.x:b.x,
		    a.y > b.y ? a.y:b.y,
		    a.z > b.z ? a.z:b.z );
	    }

	    public JVector		Min( JVector a, JVector b )
	    {
		    return new JVector( a.x < b.x ? a.x:b.x,
		    a.y < b.y ? a.y:b.y,
		    a.z < b.z ? a.z:b.z );
	    }

        public JVector    MulPart(JVector a,JVector b) // partial multiplication into *this
        {
          return new JVector( a.x * b.x , a.y * b.y , a.z * b.z );
        }

        public JVector    Int(JVector a) // converts vector to int
        {
            return new JVector((float)System.Math.Floor(a.x), (float)System.Math.Floor(a.y), (float)System.Math.Floor(a.z));
        }

        public JVector           Int() // converts vector to int
        {
            x=(float)System.Math.Floor(x);
            y = (float)System.Math.Floor(y);
            z = (float)System.Math.Floor(z);
            //
            return this;
        }

        public float			Distance( JVector b)			
	    {
            return (float)System.Math.Sqrt((x - b.x) * (x - b.x) + (y - b.y) * (y - b.y) + (z - b.z) * (z - b.z));
	    }

		//! Computes square distance to another point
        public float			SquareDistance(JVector b)		
	    {
		    return ((x - b.x)*(x - b.x) + (y - b.y)*(y - b.y) + (z - b.z)*(z - b.z));
	    }
        public void SetZero() { x = 0.0f; y = 0.0f; z = 0.0f; w = 0.0f; }
         public  float InvSqrt (float fValue)
        {
            return (float)(1.0f / System.Math.Sqrt(fValue));
        }

         
        public void SetByCubicInterpolation(JVector A, JVector TA, JVector TB,  JVector B, float t)
   {
      float a,b,c,d;   

      a=2*t*t*t - 3*t*t + 1;
      b=-2*t*t*t + 3*t*t;
      c=t*t*t - 2*t*t + t;
      d=t*t*t - t*t;
      x=a*A.x + b*B.x + c*TA.x + d*TB.x;
      y=a*A.y + b*B.y + c*TA.y + d*TB.y;
      z=a*A.z + b*B.z + c*TA.z + d*TB.z;
      w=0;
   }

   public void SetAsNormal(JVector A, JVector B,  JVector C)
   {
      JVector P=C-B;
      JVector Q=A-B;
      JVector n=P%Q;
      x=n.x;
      y=n.y;
      z=n.z;
      Normalize();
   }

   public float Unitize ()
   {
       float fTolerance= 1e-06f;
       return Unitize(fTolerance);

   }
   public float Unitize (float fTolerance)
   {
       float fLength = Length();

       if ( fLength > fTolerance )
       {
           float fInvLength = 1.0f/fLength;
           x *= fInvLength;
           y *= fInvLength;
           z *= fInvLength;
       }
       else
       {
           fLength = 0.0f;
       }

       return fLength;
   }


        public float SqrLength() 
        {
           return x * x + y * y + z * z;
        }

        public float	Length() 
        {
            return (float)System.Math.Sqrt(x * x + y * y + z * z);
        }

        public JVector Normalize()
        {
            return Normalize(1);
        }
        public JVector Normalize(float f)
        {
            //
            float l = Length();
            if (System.Math.Abs(l) < 1E-5)
            {
                return this;
            }
            //
            JVector temp = new JVector(this);

            float asp = f / l;
            temp.x *= asp;
            temp.y *= asp;
            temp.z *= asp;
            return temp;
        }

        public JVector Neg()
        {
           x=-x;
           y=-y;
           z=-z;
           //
           return this;
       }

        public bool IsSameAs( float f,float error)
        {
            return (System.Math.Abs( x-f ) < error && System.Math.Abs( y-f ) < error && System.Math.Abs( z-f ) < error);
        }

        public bool IsSameAs(JVector v, float error)
        {
            return (System.Math.Abs( x-v.x ) < error && System.Math.Abs( y-v.y ) < error && System.Math.Abs( z-v.z ) < error);
        }


        public static JVector operator +(JVector v, JVector v1) 
        {
               return new JVector( v.x + v1.x , v.y + v1.y , v.z + v1.z );
        }

        public static JVector operator -(JVector v, JVector v1)
        {
            return new JVector(v.x - v1.x, v.y - v1.y, v.z - v1.z);
        }

        public static  JVector operator % ( JVector v,JVector v1) 
        {
           return new JVector(   v.y * v1.z - v.z * v1.y,
						           v.z * v1.x - v.x * v1.z,
						           v.x * v1.y - v.y * v1.x );
        }
        public static float operator *(JVector v, JVector v1) 
        {
           return v.x*v1.x + v.y*v1.y + v.z*v1.z;;
        }


        public static JVector operator /(JVector v, float d)
        {
           JVector vec = new JVector(v);
           vec.x/=d;
           vec.y/= d;
           vec.z/= d;
           return vec;
        }

         public static bool operator >(JVector v1,JVector v2)
        {
            if (((object)v1) == null || ((object)v2) == null) return false;
           return	v1.x > v2.x &&
			           v1.y > v2.y &&
			           v1.z > v2.z;
        }

         public static bool operator >=(JVector v1,JVector v2)
        {
            if (((object)v1) == null || ((object)v2) == null) return false;
           return	v1.x >= v2.x &&
			           v1.y >= v2.y &&
			           v1.z >= v2.z;
        }

         public static bool operator <(JVector v1,JVector v2)
        {
            if (((object)v1) == null || ((object)v2) == null) return false;
	        return	v1.x < v2.x &&
			           v1.y < v2.y &&
			           v1.z < v2.z;
        }

         public static bool operator <=(JVector v1,JVector v2)
        {
            if (((object)v1) == null || ((object)v2) == null) return false;
	        return	v1.x <= v2.x &&
			           v1.y <= v2.y &&
			           v1.z <= v2.z;
        }


        public byte[] GetBytes3x()
        {
            byte[] b = new byte[3 * 4];
            Array.Copy(BitConverter.GetBytes(x), 0, b, 0, 4);
            Array.Copy(BitConverter.GetBytes(y), 0, b, 4, 4);
            Array.Copy(BitConverter.GetBytes(z), 0, b, 8, 4);
            return b;
        }

        public byte[] GetBytes4x()
        {
            byte[] b = new byte[4 * 4];
            Array.Copy(BitConverter.GetBytes(x), 0, b, 0, 4);
            Array.Copy(BitConverter.GetBytes(y), 0, b, 4, 4);
            Array.Copy(BitConverter.GetBytes(z), 0, b, 8, 4);
            Array.Copy(BitConverter.GetBytes(x), 0, b, 12, 4);
            return b;
        }


        /*
        public static  implicit  operator JVector(float val )
        {
            JVector v = new JVector();
            v.x=v.y=v.z=val;
            return v ;
        }
        /**/
        public JVector Mul2(float val)
        {
            this.x *= val;
            this.y *= val;
            this.z *= val;
            return this;
        }

        public static   bool operator == ( JVector v,JVector v1 ) 
        {
            if (((object)v) == null && ((object)v1) == null) return true;
            if (((object)v) == null || ((object)v1) == null) return false;

           return v.x==v1.x && v.y==v1.y && v.z==v1.z;
        }

        public static   bool operator == (JVector v,float d ) 
        {
            if (v == null) return false;
           return v.x==d && v.y==d && v.z==d;
        }

        public static   bool operator != ( JVector v,JVector v1 ) 
        {
            if (((object)v) == null && ((object)v1) == null) return false;
            if (((object)v) == null || ((object)v1) == null) return true;

           return v.x!=v1.x || v.y!=v1.y || v.z!=v1.z;
        }

        public static bool operator !=(JVector v, float d) 
        {
           return v.x!=d || v.y!=d || v.z!=d;
        }


        public override int GetHashCode()
        {
            return string.Format("{0} {1} {2}", x, y, z).GetHashCode();
        }

        public JVector CreatePlane(JVector norm,JVector pos)
        {
           JVector v = new JVector(norm);
           JVector t = JVector.CrossProduct(pos, norm);
           v.w = -t.SqrLength();
           return v;
        }

        public JVector CreatePlane(JVector norm)
        {
           return CreatePlane( norm , this );
        }

        public float floatDistance2Plane(JVector v) 
        {
           return (this * v) + v.w;
        }

        public JVector  NormalizePlane(float f)
        {
           float oolen = 1.0f / Length();
           x*=oolen;
           y*=oolen;
           z*=oolen;
           w*=oolen;
           return this;
        }

        public JVector Mul(float val)
        {
            this.x *= val;
            this.y *= val;
            this.z *= val;
            return this;
        }

        public static  float DotProduct(JVector v1, JVector v2)
        {
	        return v1.x*v2.x + v1.y*v2.y + v1.z*v2.z;
        }

        public static JVector CrossProduct(JVector v1, JVector v2)
        {
            //A x B = <Ay*Bz - Az*By, Az*Bx - Ax*Bz, Ax*By - Ay*Bx>
            return new JVector(   v1.y * v2.z - v1.z * v2.y,
						   v1.z * v2.x - v1.x * v2.z,
						   v1.x * v2.y - v1.y * v2.x );

        }
        public static void VectorSubtract(JVector a, JVector b, ref JVector c)
        {
	        c.x=a.x-b.x;
	        c.y=a.y-b.y;
	        c.z=a.z-b.z;
        }
        public static  void VectorAdd(JVector a, JVector b, ref JVector c)
        {
	        c.x=a.x+b.x;
	        c.y=a.y+b.y;
	        c.z=a.z+b.z;
        }
        public static  void VectorCopy(JVector a, ref JVector b)
        {
	        b.x=a.x;
	        b.y=a.y;
	        b.z=a.z;
        }
        public static  void VectorClear(ref JVector a)
        {
	        a.x=a.y=a.z=0;
        }

        public static  float VectorMaximum(JVector v)
        {
            return System.Math.Max(v.x, System.Math.Max(v.y, v.z));
        }




   }
}
