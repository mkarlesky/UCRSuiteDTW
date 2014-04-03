using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UCRSuite
{
    // Multidimensional Euclidean Vector
    public class MDVector
    {
        private double[] _values;

        public MDVector(int dimensions = 1)
        {
            this.Dimensions = dimensions;
            _values = new double[dimensions];
        }

        public MDVector(params double[] list) :
            this(list.Length)
        {
            for (int i = 0; i < this.Dimensions; i++)
            {
                _values[i] = list[i];
            }
        }

        public MDVector(int size, params double[] list) :
            this(size)
        {
            for (int i = 0; i < this.Dimensions; i++)
            {
                _values[i] = list[i];
            }
        }

        public MDVector(MDVector vector) :
            this(vector.Dimensions)
        {
            for (int i = 0; i < this.Dimensions; i++)
            {
                this._values[i] = vector._values[i];
            }
        }

        public int Dimensions { get; private set; }

        // Reference assignment
        public double this[int i]
        {
            get { return _values[i]; }
            set { _values[i] = value; }
        }

        // Set by copy
        public void set(MDVector v)
        {
            for (int i = 0; i < this.Dimensions; i++)
            {
                this._values[i] = v[i];
            }
        }

        // Set by assignment
        public void set(double d)
        {
            for (int i = 0; i < this.Dimensions; i++)
            {
                this._values[i] = d;
            }
        }

        public static bool operator >(MDVector v1, MDVector v2)
        {
            return (v1.magnitude() > v2.magnitude());
        }

        public static bool operator >(MDVector v1, double d)
        {
            bool x = true;

            for (int i = 0; i < v1.Dimensions; i++)
            {
                if (v1[i] <= d)
                {
                    x = false;
                    break;
                }
            }

            return x;
        }
        
        public static bool operator >=(MDVector v1, MDVector v2)
        {
            return (v1.magnitude() >= v2.magnitude());
        }

        public static bool operator >=(MDVector v1, double d)
        {
            bool x = true;

            for (int i = 0; i < v1.Dimensions; i++)
            {
                if (v1[i] < d)
                {
                    x = false;
                    break;
                }
            }

            return x;
        }

        public static bool operator <(MDVector v1, MDVector v2)
        {
            return (v1.magnitude() < v2.magnitude());
        }

        public static bool operator <(MDVector v1, double d)
        {
            bool x = true;

            for (int i = 0; i < v1.Dimensions; i++)
            {
                if (v1[i] >= d)
                {
                    x = false;
                    break;
                }
            }

            return x;
        }

        public static bool operator <=(MDVector v1, MDVector v2)
        {
            return (v1.magnitude() <= v2.magnitude());
        }

        public static bool operator <=(MDVector v1, double d)
        {
            bool x = true;

            for (int i = 0; i < v1.Dimensions; i++)
            {
                if (v1[i] > d)
                {
                    x = false;
                    break;
                }
            }

            return x;
        }

        public static MDVector operator -(MDVector v1, MDVector v2)
        {
            return subtract(v1, v2, new MDVector(v1.Dimensions));
        }

        public static MDVector operator +(MDVector v1, MDVector v2)
        {
            return add(v1, v2, new MDVector(v1.Dimensions));
        }

        public static MDVector operator *(MDVector v1, MDVector v2)
        {
            return multiply(v1, v2, new MDVector(v1.Dimensions));
        }

        public static MDVector operator *(MDVector v1, double d)
        {
            MDVector v = new MDVector(v1.Dimensions);

            for (int i = 0; i < v1.Dimensions; i++)
            {
                v[i] = (v1[i] * d);
            }

            return v;
        }

        public static MDVector operator /(MDVector v1, MDVector v2)
        {
            return divide(v1, v2, new MDVector(v1.Dimensions));
        }

        public static MDVector operator /(MDVector v1, double d)
        {
            return divide(v1, d, new MDVector(v1.Dimensions));
        }

        public static MDVector multiply(MDVector v1, MDVector v2, MDVector result)
        {
            for (int i = 0; i < v1.Dimensions; i++)
            {
                result[i] = (v1[i] * v2[i]);
            }

            return result;
        }

        public static MDVector divide(MDVector v1, MDVector v2, MDVector result)
        {
            for (int i = 0; i < v1.Dimensions; i++)
            {
                result[i] = (v1[i] / v2[i]);
            }

            return result;
        }

        public static MDVector divide(MDVector v1, double d, MDVector result)
        {
            for (int i = 0; i < v1.Dimensions; i++)
            {
                result[i] = (v1[i] / d);
            }

            return result;
        }

        public static MDVector add(MDVector v1, MDVector v2, MDVector result)
        {
            for (int i = 0; i < v1.Dimensions; i++)
            {
                result[i] = (v1[i] + v2[i]);
            }

            return result;
        }

        public static MDVector subtract(MDVector v1, MDVector v2, MDVector result)
        {
            for (int i = 0; i < v1.Dimensions; i++)
            {
                result[i] = (v1[i] - v2[i]);
            }

            return result;
        }

        public double magnitude()
        {
            double sum = 0.0;

            if (this.Dimensions == 1)
            {
                return _values[0];
            }

            foreach (double d in _values)
            {
                sum += (d * d);
            }

            return Math.Sqrt(sum);
        }

        public MDVector sqrt()
        {
            for (int i = 0; i < this.Dimensions; i++)
            {
                this._values[i] = Math.Sqrt(this._values[i]);
            }

            return this;
        }

        public double absSum()
        {
            double sum = 0;

            for (int i = 0; i < this.Dimensions; i++)
            {
                sum += Math.Abs(this._values[i]);
            }

            return sum;
        }
    }
}
