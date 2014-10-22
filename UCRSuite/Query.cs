using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UCRSuite
{
    struct OrderedVector
    {
        public MDVector Vector;
        public int Index;
    }

    internal sealed class QueryComparer : IComparer<OrderedVector>
    {
        int IComparer<OrderedVector>.Compare(OrderedVector x, OrderedVector y)
        {
            int difference = 0;

            for (int i = 0; i < x.Vector.Dimensions; i++)
            {
                difference += Convert.ToInt16(Math.Abs(y.Vector[i]) - Math.Abs(x.Vector[i]));
            }

            return difference;
        }
    }

    public class Query
    {
        private readonly List<MDVector> _values;

        private OrderedVector[] _sortingBuffer;

        public MDVector[] BaseValues;
        public MDVector[] OrderedValues;

        public int[] Ordered;

        public MDVector[] OrderedUpperEnvelope;
        public MDVector[] OrderedLowerEnvelope;

        private readonly float _warpingWindow;
        private readonly int _dimensions;

        private readonly MDVector _mean;
        private readonly MDVector _stdDeviation;

        private LemireEnvelope _envelope;

        private bool _processed;


        public Query(float warpingWindow, int dimensions = 1)
        {
            if (dimensions < 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            _dimensions    = dimensions;
            _warpingWindow = warpingWindow;

            _mean         = new MDVector(dimensions);
            _stdDeviation = new MDVector(dimensions);

            _values = new List<MDVector>();

            _processed = false;

            this.Length = 0;
            this.WarpingWindow = 0;
        }

        public int Length { get; private set; }

        public int WarpingWindow { get; private set; }

        public void addQueryItem(params double[] list)
        {
            addQueryItem(new MDVector(list));
        }

        public void addQueryItem(int size, params double[] list)
        {
            addQueryItem(new MDVector(size, list));
        }

        public void addQueryItem(MDVector vector)
        {
            if (vector.Dimensions != _dimensions)
            {
                throw new Exception("Dimension of query item added [" + vector.Dimensions + "] does not match expected [" + _dimensions + "].");
            }

            _values.Add(new MDVector(vector));
        }

        public void process()
        {
            if (_processed)
                return;

            setup();
            calculateMean();
            calculateStdDeviation();
            normalize();
            sort();
            buildEnvelopes();

            _values.Clear(); // purge unneeded memory
            _processed = true;
        }

        private void setup()
        {
            this.Length = _values.Count;
            this.WarpingWindow = calculateWarpingWindow(this.Length, _warpingWindow);

            _sortingBuffer = new OrderedVector[this.Length];
            BaseValues = new MDVector[this.Length];
            OrderedValues = new MDVector[this.Length];

            Ordered = new int[this.Length];

            _envelope = new LemireEnvelope(this.Length, this.WarpingWindow, _dimensions);
            OrderedUpperEnvelope = new MDVector[this.Length];
            OrderedLowerEnvelope = new MDVector[this.Length];

            for (int i = 0; i < this.Length; i++)
            {
                this.BaseValues[i] = _values[i];
                _sortingBuffer[i] = new OrderedVector() { Vector = _values[i], Index = i };
                
                OrderedValues[i] = new MDVector(_dimensions);

                OrderedUpperEnvelope[i] = new MDVector(_dimensions);
                OrderedLowerEnvelope[i] = new MDVector(_dimensions);
            }
        }

        private void calculateMean()
        {
            foreach (MDVector vector in BaseValues)
            {
                for (int i = 0; i < vector.Dimensions; i++)
                {
                    _mean[i] = _mean[i] + vector[i];
                }
            }

            for (int i = 0; i < _mean.Dimensions; i++)
            {
                _mean[i] = _mean[i] / this.BaseValues.Length;
            }
        }

        private void calculateStdDeviation()
        {
            double difference;

            foreach (MDVector vector in this.BaseValues)
            {
                for (int i = 0; i < vector.Dimensions; i++)
                {
                    difference = (vector[i] - _mean[i]);

                    _stdDeviation[i] = _stdDeviation[i] + (difference * difference);
                }
            }

            for (int i = 0; i < _stdDeviation.Dimensions; i++)
            {
                _stdDeviation[i] = Math.Sqrt(_stdDeviation[i] / this.BaseValues.Length);
            }
        }

        private void normalize()
        {
            foreach (MDVector vector in this.BaseValues)
            {
                for (int i = 0; i < vector.Dimensions; i++)
                {
                    vector[i] = (vector[i] - _mean[i]) / _stdDeviation[i];
                }
            }
        }

        private void sort()
        {
            Array.Sort(_sortingBuffer, new QueryComparer());
        }

        private void buildEnvelopes()
        {
            _envelope.process(this.BaseValues, this.Length);

            // also create another arrays for keeping sorted envelope
            for (int i = 0; i < this.Length; i++)
            {
                int o = _sortingBuffer[i].Index;
                
                this.Ordered[i] = o;
                this.OrderedValues[i].set( this.BaseValues[o]);
                this.OrderedUpperEnvelope[i].set( _envelope.Upper[o] );
                this.OrderedLowerEnvelope[i].set( _envelope.Lower[o] );
            }
        }

        private int calculateWarpingWindow(int queryLength, double warpingWindow)
        {
            if (warpingWindow <= 0.0 || warpingWindow > 1.0)
            {
                throw new ArgumentException("Warping window size must be greater than 0.0 and no more than 1.0");
            }

            return (int)Math.Floor(warpingWindow * queryLength);
        }
    }
}
