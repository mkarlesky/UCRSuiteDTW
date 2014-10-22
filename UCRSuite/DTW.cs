using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UCRSuite
{
    public struct DTWResult
    {
        public int Location;
        public double Distance;
    }

    public class DTW
    {
        // For every EPOCH points, all cummulative values, such as ex (sum), ex2 (sum square), will be restarted for reducing the doubleing point error.
        static int EPOCH = 100000;

        private readonly List<MDVector> _data;

        private readonly KimLowerBound _kimLb;
        private readonly KeoghLowerBound _keoghLb;

        private readonly int _dimensions;
        private readonly float _warpingWindow;

        private MDVector result;
        private MDVector ex, ex2, mean, mean2, std;
        private MDVector lb_kim, lb_k, lb_k2;
        private MDVector[] buffer;

        // Constructor
        public DTW (int dimensions = 1, float warpingWindow = 0.05f)
        {
            _data = new List<MDVector>();

            _dimensions    = dimensions;
            _warpingWindow = warpingWindow;

            _kimLb         = new KimLowerBound(dimensions);
            _keoghLb       = new KeoghLowerBound(dimensions);

            ex    = new MDVector(dimensions);
            ex2   = new MDVector(dimensions);
            mean  = new MDVector(dimensions);
            mean2 = new MDVector(dimensions);
            std   = new MDVector(dimensions);

            result = new MDVector(dimensions);

            lb_kim = new MDVector(dimensions);
            lb_k   = new MDVector(dimensions);
            lb_k2  = new MDVector(dimensions);

            buffer = new MDVector[EPOCH];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new MDVector(dimensions);
            }
        }

        public DTW(List<MDVector> data, int dimensions = 1, float warpWindow = 0.05f) :
            this(dimensions, warpWindow)
        {
            _data = new List<MDVector>(data);
        }

        public Query Query()
        {
            return new Query(_warpingWindow, _dimensions);
        }

        public void addDataItem(params double[] list)
        {
            addDataItem(new MDVector(list));
        }

        public void addDataItem(int size, params double[] list)
        {
            addDataItem(new MDVector(size, list));
        }

        public void addDataItem(MDVector vector)
        {
            if (vector.Dimensions != _dimensions)
            {
                throw new Exception("Dimension of data item added [" + vector.Dimensions + "] does not match expected [" + _dimensions + "].");
            }

            _data.Add( new MDVector(vector) );
        }

        public DTWResult warp(Query query)
        {
            double bsf; // best-so-far
            MDVector[] t; // data array

            MDVector[] tz, cb, cb1, cb2;
            
            MDVector d;
            int i, j;
            int dataIndex = 0;
            int matchIndex = 0;
            int kim = 0, keogh = 0, keogh2 = 0;
            double distance = 0;

            // prepare query object
            query.process();

            cb = new MDVector[query.Length];
            cb1 = new MDVector[query.Length];
            cb2 = new MDVector[query.Length];

            t = new MDVector[query.Length * 2];
            for (i = 0; i < t.Length; i++)
            {
                t[i] = new MDVector(_dimensions);
            }

            tz = new MDVector[query.Length];
            for (i = 0; i < tz.Length; i++)
            {
                tz[i] = new MDVector(_dimensions);
            }

            // Initialize the cummulative lower bound
            for (i = 0; i < query.Length; i++)
            {
                cb[i] = new MDVector(_dimensions);
                cb[i].set(0);

                cb1[i] = new MDVector(_dimensions);
                cb1[i].set(0);

                cb2[i] = new MDVector(_dimensions);
                cb2[i].set(0);
            }

            // Initialize
            bsf = double.PositiveInfinity;
            i = 0; /// current index of the data in current chunk of size EPOCH
            j = 0; /// the starting index of the data in the circular array, t
            ex.set(0);
            ex2.set(0);

            bool done = false;
            int it = 0, ep = 0, k = 0;
            int I; /// the starting index of the data in current chunk of size EPOCH

            LemireEnvelope lemireEnvelope = new LemireEnvelope(EPOCH, query.WarpingWindow, _dimensions);

            DTWCalculator dtwCalculator = new DTWCalculator(query.Length, query.WarpingWindow, _dimensions);

            while (!done)
            {
                // Read first query.Length-1 points
                ep = 0;
                if (it == 0)
                {
                    for (k = 0; k < query.Length - 1; k++)
                    {
                        if(dataIndex < _data.Count)
                        {
                            buffer[k].set( _data[dataIndex++] );
                        }
                    }
                }
                else
                {
                    for (k = 0; k < query.Length - 1; k++)
                    {
                        buffer[k].set( buffer[EPOCH - query.Length + 1 + k] );
                    }
                }

                // Read buffer of size EPOCH or when all data has been read.
                ep = query.Length - 1;
                while (ep < EPOCH)
                {
                    if (dataIndex >= _data.Count)
                        break;
                    buffer[ep].set(_data[dataIndex++]);
                    ep++;
                }

                // Data are read in chunk of size EPOCH.
                // When there is nothing to read, the loop is end.
                if (ep <= query.Length - 1)
                {
                    done = true;
                }
                else
                {
                    lemireEnvelope.process(buffer, ep);

                    /// Do main task here..
                    ex.set(0);
                    ex2.set(0);
                    for (i = 0; i < ep; i++)
                    {
                        // A bunch of data has been read and pick one of them at a time to use
                        d = buffer[i];

                        // Calcualte sum and sum square
                        ex = MDVector.add( ex, d, ex );
                        result = MDVector.multiply( d, d, result );
                        ex2 = MDVector.add(ex2, result, ex2);

                        // t is a circular array for keeping current data
                        t[i % query.Length].set(d);

                        // double the size for avoiding using modulo "%" operator
                        t[(i % query.Length) + query.Length].set(d);

                        // Start the task when there are more than query.Length-1 points in the current chunk
                        if (i >= query.Length - 1)
                        {
                            mean  = MDVector.divide( ex, query.Length, mean );
                            std   = MDVector.divide( ex2, query.Length, std );
                            mean2 = MDVector.multiply( mean, mean, mean2 );
                            std   = MDVector.subtract( std, mean2, std );
                            std.sqrt();

                            // compute the start location of the data in the current circular array, t
                            j = (i + 1)%query.Length;
                            // the start location of the data in the current chunk
                            I = i - (query.Length - 1);

                            // Use a constant lower bound to prune the obvious subsequence
                            lb_kim = _kimLb.hierarchy(t, query.BaseValues, j, query.Length, mean, std, bsf);

                            if (lb_kim < bsf)
                            {
                                // Use a linear time lower bound to prune; z_normalization of t will be computed on the fly.
                                // uo, lo are envelop of the query.
                                lb_k = _keoghLb.cumulative(query.Ordered, t, query.OrderedUpperEnvelope, query.OrderedLowerEnvelope, cb1, j, query.Length, mean, std, bsf);

                                if (lb_k < bsf)
                                {
                                    // Take another linear time to compute z_normalization of t.
                                    // Note that for better optimization, this can merge to the previous function.
                                    for (k = 0; k < query.Length; k++)
                                    {
                                        tz[k] = Utilities.normalize( t[(k + j)], mean, std, tz[k] );
                                    }

                                    // Use another lb_keogh to prune
                                    // qo is the sorted query. tz is unsorted z_normalized data.
                                    // l_buff, u_buff are big envelop for all data in this chunk

                                    lb_k2 = _keoghLb.dataCumulative(query.Ordered, query.OrderedValues, cb2, lemireEnvelope.Lower, lemireEnvelope.Upper, I, query.Length, mean, std, bsf);
                                     
                                    if (lb_k2 < bsf)
                                    {
                                        // Choose better lower bound between lb_keogh and lb_keogh2 to be used in early abandoning DTW
                                        // Note that cb and cb2 will be cumulative summed here.
                                        if (lb_k > lb_k2)
                                        {
                                            cb[query.Length - 1].set(cb1[query.Length - 1]);
                                            for (k = query.Length - 2; k >= 0; k--)
                                                cb[k] = MDVector.add( cb[k + 1], cb1[k], cb[k] );
                                        }
                                        else
                                        {
                                            cb[query.Length - 1].set(cb2[query.Length - 1]);
                                            for (k = query.Length - 2; k >= 0; k--)
                                                cb[k] = MDVector.add(cb[k + 1], cb2[k], cb[k]);
                                        }

                                        // Compute DTW and early abandoning if possible 
                                        distance = dtwCalculator.distance(tz, query.BaseValues, cb, bsf);

                                        if (distance < bsf)
                                        {
                                            // Update bsf
                                            // loc is the real starting location of the nearest neighbor in the file
                                            bsf = distance;
                                            matchIndex = (it)*(EPOCH - query.Length + 1) + i - query.Length + 1;
                                        }
                                    }
                                    else
                                        keogh2++;
                                }
                                else
                                    keogh++;
                            }
                            else
                                kim++;

                            // Reduce absolute points from sum and sum square
                            ex     = MDVector.subtract(ex, t[j], ex);
                            result = MDVector.multiply( t[j], t[j], result );
                            ex2    = MDVector.subtract( ex2, result, ex2 );
                        }
                    }

                    // If the size of last chunk is less than EPOCH, then no more data and terminate.
                    if (ep < EPOCH)
                        done = true;
                    else
                        it++;
                }
            }

            return new DTWResult() { Location = matchIndex, Distance = Math.Sqrt(bsf) };
        }

    }
   
}