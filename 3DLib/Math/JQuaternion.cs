using System;
using System.Collections.Generic;
using System.Text;

namespace Library3d.Math
{
    public class JQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public void Ax(float a,float b,float c,float d)
        {
            x=a;
            y=b;
            z=c;
            w=d;
        }

        public void Ax(JVector v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            w = v.w;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", x, y, z);
        }

        public JVector ax
        {
            get
            {
                return new JVector(x, y, z, w);
            }
            set
            {
                x = value.x;
                y = value.y;
                z = value.z;
                w = value.w;
            }
        }

        public void Set(int index, float v)
        {
            if (index == 0) x = v;
            if (index == 1) y = v;
            if (index == 2) z = v;
            if (index == 3) w = v;
        }



        public JQuaternion()  {}
        public JQuaternion( float f ){
            Ax(0,0,0,f);
        }

        public JQuaternion( JQuaternion q )
        {
            Ax(q.ax);
        }
        public JQuaternion( JVector v, float f )
        {
            Ax(v.x,v.y,v.z,f);
        }
        public JQuaternion( JVector v){
            Ax(v.x,v.y,v.z,1);
        }

        public JQuaternion( float a , float x, float y , float z ){
            Ax(x,y,z,a);
        }

        public static JQuaternion                         operator + ( JQuaternion q,JQuaternion q1)  
        {
            return new JQuaternion(q.ax + q1.ax, q.ax.w + q1.ax.w); 
        }
        public static JQuaternion                         operator - ( JQuaternion q,JQuaternion q1)  
        {
            return new JQuaternion(q.ax - q1.ax, q.ax.w - q1.ax.w); 
        }
        public static JQuaternion                         operator * ( JQuaternion q1,JQuaternion q) 
        {
            return new JQuaternion(q.ax.w * q1.ax.w - q.ax.x * q1.ax.x - q.ax.y * q1.ax.y - q.ax.z * q1.ax.z,
                q.ax.w * q1.ax.x + q.ax.x * q1.ax.w + q.ax.y * q1.ax.z - q.ax.z * q1.ax.y,
                q.ax.w * q1.ax.y + q.ax.y * q1.ax.w + q.ax.z * q1.ax.x - q.ax.x * q1.ax.z,
                q.ax.w * q1.ax.z + q.ax.z * q1.ax.w + q.ax.x * q1.ax.y - q.ax.y * q1.ax.x);

        }

       public static JQuaternion                         operator * ( JQuaternion q,float d )     
       { 
          return new JQuaternion( q.ax.Mul( d), q.ax.w * d ); 
       }
        public void Identity()
        {
            x = y = z = 0; w = 1;
        }
        public void                                FromAngleAxis( float angle, JVector axis )
        {
            // The fquaternion representing the rotation is
	        //   q = cos(A/2)+sin(A/2)*(x*i+y*j+z*k)
            ax.w = (float)System.Math.Cos(0.5f*angle);
	        float		sn = (float)System.Math.Sin(0.5f*angle);
            ax.x = sn*axis.x;
	        ax.y = sn*axis.y;
            ax.z = sn*axis.z;
         }

         public JMatrix ToMatrix()
        {
            JMatrix r =new JMatrix();
            ToMatrix(ref r);
            return r;
        }

         public void                                ToMatrix(ref JMatrix m)
        {
            float a = ax.w, b = ax.x, c = ax.y, d = ax.z;

            m.m(0,0, a * a + b * b - c * c - d * d);
            m.m(1,0, 2 * (b * c - a * d));
            m.m(2,0, 2 * (a * c + b * d));
            m.m(0,1, 2 * (b * c + a * d));
            m.m(1,1, a * a - b * b + c * c - d * d);
            m.m(2,1, 2 * (-a * b + c * d));
            m.m(0,2, 2 * (-a * c + b * d));
            m.m(1,2, 2 * (a * b + c * d));
            m.m(2, 2, a * a - b * b - c * c + d * d);
            

        }

         public JQuaternion	FromMatrix(JMatrix m)
        {
           int 	[] next = new int[3]{1, 2, 0};
           float	trace = m.m(0,0) + m.m(1,1) + m.m(2,2), s;

           if(trace > 0.0f)
           {
              s = (float)System.Math.Sqrt(trace + 1.0f);
              w  = (float)(s * 0.5f);
              s = 0.5f / s;
              x = (m.m(2,1) - m.m(1,2)) * (float)s;
              y = (m.m(0,2) - m.m(2,0)) * (float)s;
              z = (m.m(1,0) - m.m(0,1)) * (float)s;
           } 
           else
           {
              int		i = 0;
              if( m.m(1,1) > m.m(0,0) )		i = 1;
              if( m.m(2,2) > m.m(i,i) )		i = 2;
              int		j = next[i];  
              int		k = next[j];

              s = (float)System.Math.Sqrt( (m.m(i,i)- (m.m(j,j)+m.m(k,k))) + 1.0f );
              Set(i,(float)(s * 0.5f));
              s = 0.5f / s;

              w = (m.m(k,j) - m.m(j,k)) * (float)s;
              Set(j, (m.m(j,i) + m.m(i,j)) * (float)s);
              Set(k,(m.m(k,i) + m.m(i,k)) * (float)s);
           }

           w = -w;

           return this;
        }



        public JQuaternion                         Inverse()
        {
            float	norm = SqrLength();
            if( norm > 0.0 )
            {
		        norm = 1.0f / norm;
		        return new JQuaternion( ax.w*norm, -ax.x *norm, -ax.y *norm, -ax.z *norm );
            }
            else return new JQuaternion( 0,0,0,0 );
        }

        public JQuaternion                         Exp ()
        {
            float angle = (float) System.Math.Sqrt( ax.SqrLength());
            float sn = (float) System.Math.Sin(angle);
            float		nr = (float)System.Math.Cos(angle);

            if (System.Math.Abs(sn) >= 1E-5f) return new JQuaternion(ax.Mul((float)(sn / angle)), nr);
            return new JQuaternion( ax, nr );

         }
         public float                               SqrLength()  { return ax.SqrLength() + ax.w*ax.w; }

        public void SetZero() { x = 0.0f; y = 0.0f; z = 0.0f; w = 0.0f; }

         public void Init() { SetZero(); }

        public void Set(float xa, float ya, float za, float wa) { x = xa; y = ya; z = za; w = wa; }

        public void      FromAngle    (float xa,float ya,float za)
        {
           float		angle;
           float		sr, sp, sy, cr, cp, cy;
           
           // FIXME: rescale the inputs to 1/2 angle
           angle = za * 0.5f;
           sy = (float)System.Math.Sin(angle);
           cy = (float)System.Math.Cos(angle);
           angle = ya * 0.5f;
           sp = (float)System.Math.Sin(angle);
           cp = (float)System.Math.Cos(angle);
           angle = xa * 0.5f;
           sr = (float)System.Math.Sin(angle);
           cr = (float)System.Math.Cos(angle);
           
           x = sr*cp*cy-cr*sp*sy; // X
           y = cr*sp*cy+sr*cp*sy; // Y
           z = cr*cp*sy-sr*sp*cy; // Z
           w = cr*cp*cy+sr*sp*sy; // W

        }

        public JVector Rotate (JVector v) 
        {
           JQuaternion vec =new JQuaternion(v.x, v.y, v.z,0);
	        JQuaternion q = this;
	        JQuaternion qinv =new JQuaternion(-v.x,-v.y,-v.z,w);

	        JQuaternion vec2 = q*vec*qinv;

	        return new JVector(vec2.x, vec2.y, vec2.z);
	        //nVidia SDK
	        // trouve dans Ogre3d
	        /*Vector3 uv, uuv;
	        Vector3 qvec(x,y,z);
	        uv = qvec%v;
	        uuv = qvec%uv;
	        uv *= (2.0f*r);
	        uuv *= 2.0f;
	        return v + uv + uuv;*/
	        /*Quaternion qv(0, v.x, v.y, v.z);
	        Quaternion qm = (*this)*qv*(this->Inverse());
	        return Vector3 (qm.x, qm.y, qm.z);*/ 

        }
    }
}
