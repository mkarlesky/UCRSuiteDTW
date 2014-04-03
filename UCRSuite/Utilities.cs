using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UCRSuite
{
    /*internal*/ public sealed class Utilities
    {
        // Z-score normalization: zero mean & unit variance
        public static MDVector normalize(MDVector value, MDVector mean, MDVector std, MDVector result)
        {
            MDVector.subtract(value, mean, result);
            MDVector.divide(result, std, result);

            return result;
        }

        /*
         * L2-norm without square root operation
         * (omitting square root operation saves computation in comparison concerned only with unit-less magnitudes)
         */
        static public MDVector distanceSquared(MDVector x, MDVector y, MDVector result)
        {
            MDVector.subtract(x, y, result);

            return MDVector.multiply(result, result, result);
        }
        
        static public MDVector min(MDVector x, MDVector y)
        {
            return x < y ? x : y;
        }

        static public int min(int x, int y)
        {
            return x < y ? x : y;
        }

        static public MDVector max(MDVector x, MDVector y)
        {
            return x > y ? x : y;
        }

        static public int max(int x, int y)
        {
            return x > y ? x : y;
        }

        static public List<double[]> stretchData(List<double[]> source, int dimensions, int newLength)
        {
            if (source.Count == newLength)
                return source;

            int sourceLength = source.Count;

            double[][] sourceArrays = new double[dimensions][];
            double[][] stretchedArrays = new double[dimensions][];
            
            for (int i = 0; i < dimensions; i++)
            {
                sourceArrays[i] = new double[sourceLength];
            }

            for (int i = 0; i < dimensions; i++)
            {
                for (int j = 0; j < sourceLength; j++)
                {
                    sourceArrays[i][j] = source[j][i];
                }
            }

            for (int i = 0; i < dimensions; i++)
            {
                stretchedArrays[i] = stretchData(sourceArrays[i], dimensions, newLength);
            }

            List<double[]> stretchedData = new List<double[]>();

            for (int j = 0; j < newLength; j++)
            {
                double[] data = new double[dimensions];

                for (int i = 0; i < dimensions; i++)
                {
                    data[i] = stretchedArrays[i][j];
                }

                stretchedData.Add(data);
            }

            return stretchedData;
        }

        static public double[] stretchData(double[] source, int dimensions, int newLength)
        {
            float ratio = (float)(source.Length-1) / (float)(newLength-1);

            double[] destination = new double[newLength];
  
            float cumulativeIndex = ratio;

            // fill first and last array elements of new array
            destination[0]             = source[0];
            destination[newLength - 1] = source[source.Length - 1];
  
            // fill in middle of new array
            for (int i = 1; i < newLength-1; i++)
            {
                int index = (int)cumulativeIndex;             // integer floor of cumulative index
                float alpha = cumulativeIndex - (float)index; // fraction between source's neighboring indices

                destination[i] = (alpha * (source[index+1] - source[index])) + source[index];

                cumulativeIndex += ratio;
            }
    
            return destination;
        }
    }
}
