using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UCRSuite;

namespace UCRSuiteExample
{
    class Program
    {
        static void Main(string[] args)
        {
            DTW ucrDtw;
            Query query;
            DTWResult result;

            int dimensions         = Int16.Parse(args[2]);
            int queryStretchLength = Int16.Parse(args[3]);
            float warpingWindow    = (float)Double.Parse(args[4]);

            ucrDtw = new DTW(dimensions, warpingWindow);
            query  = ucrDtw.Query();
          
            using (TextReader reader = new StreamReader(args[0]))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] strs = line.Split(' ');

                    double[] a = new double[strs.Length];

                    for (int itt = 0; itt < strs.Length; itt++)
                    {
                        if (String.IsNullOrEmpty(strs[itt]))
                            continue;

                        a[itt] = double.Parse(strs[itt]);
                    }

                    ucrDtw.addDataItem(dimensions, a);
                }
            }

            List<double[]> source = new List<double[]>();
            List<double[]> stretched;

            using (TextReader reader = new StreamReader(args[1]))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;

                    string[] strs = line.Split(' ');

                    double[] a = new double[dimensions];

                    for (int itt = 0; itt < strs.Length; itt++)
                    {
                        if (String.IsNullOrEmpty(strs[itt]))
                            continue;

                        a[itt] = double.Parse(strs[itt]);
                    }

                    source.Add(a);
                }
            }

            stretched = Utilities.stretchData(source, dimensions, queryStretchLength);

            //writeCsvFile("source.csv", source, dimensions);
            //writeCsvFile("stretched.csv", stretched, dimensions);

            foreach (double[] row in stretched)
            {
                query.addQueryItem(row);
            }

            result = ucrDtw.warp(query);

            Console.WriteLine("Distance: " + result.Distance);
            Console.WriteLine("Location: " + result.Location);
        }


        static private void writeCsvFile(string fileName, List<double[]> data, int dimensions)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, false))
            {
                foreach (double[] row in data)
                {
                    for (int i = 0; i < dimensions; i++)
                    {
                        file.Write(row[i] + ", ");
                    }

                    file.WriteLine();
                }
            }
        }
    }
}

