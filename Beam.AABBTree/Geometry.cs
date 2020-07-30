using System;

namespace Beam
{
    public class Vector3
    {
        public double X;
        public double Y;
        public double Z;

        public double this[int index]
        {
            get
            {
                if (index == 0) return X;
                if (index == 1) return Y;
                if (index == 2) return Z;
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (index == 0) X = value;
                else if (index == 1) Y = value;
                else if (index == 2) Z = value;
                else throw new IndexOutOfRangeException();
            }
        }

        public Vector3()
        {
            X = Y = Z = 0;
        }
        public Vector3(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }

        public Vector3 normalize()
        {
            var lsq = (this[0] * this[0]) + (this[1] * this[1]) + (this[2] * this[2]);
            if (lsq > 0.0)
            {
                var lr = 1.0 / Math.Sqrt(lsq);
                this[0] = (this[0] * lr);
                this[1] = (this[1] * lr);
                this[2] = (this[2] * lr);
            }
            else
            {
                this[0] = 0;
                this[1] = 0;
                this[2] = 0;
            }
            return this;
        }

        public Vector3 negative()
        {
            this[0] = -this[0];
            this[1] = -this[1];
            this[2] = -this[2];

            return this;
        }

        public Vector3 clone()
        {
            return new Vector3(X, Y, Z);
        }
    }

    public class Point3
    {
        public double X;
        public double Y;
        public double Z;
        public double this[int index]
        {
            get
            {
                if (index == 0) return X;
                if (index == 1) return Y;
                if (index == 2) return Z;
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (index == 0) X = value;
                else if (index == 1) Y = value;
                else if (index == 2) Z = value;
                else throw new IndexOutOfRangeException();
            }
        }

        public Point3()
        {
            X = Y = Z = 0;
        }
        public Point3(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }
    }

    public class Plane3
    {
        public double A;
        public double B;
        public double C;
        public double D;

        public double this[int index]
        {
            get
            {
                if (index == 0) return A;
                if (index == 1) return B;
                if (index == 2) return C;
                if (index == 3) return D;
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (index == 0) A = value;
                else if (index == 1) B = value;
                else if (index == 2) C = value;
                else if (index == 3) D = value;
                else throw new IndexOutOfRangeException();
            }
        }

        public Plane3()
        {
            A = B = C = D = 0;
        }
        public Plane3(double a, double b, double c, double d)
        {
            A = a; B = b; C = c; D = d;
        }
    }
}
