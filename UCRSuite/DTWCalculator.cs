using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UCRSuite
{
    internal sealed class DTWCalculator
    {
        private readonly int _length;
        private readonly int _warpingWindow;
        private readonly int _dimensions;

        private MDVector[] _cost;
        private MDVector[] _costPrevious;
        private MDVector[] _costTemp;

        private MDVector _x;
        private MDVector _y;
        private MDVector _z;
        private MDVector _minCost;

        private MDVector _result; // temp


        public DTWCalculator (int length, int window, int dimensions = 1)
        {
            _length        = length;
            _warpingWindow = window;
            _dimensions    = dimensions;

            _x = new MDVector(dimensions);
            _y = new MDVector(dimensions);
            _z = new MDVector(dimensions);

            _minCost = new MDVector(dimensions);

            _result = new MDVector(dimensions);

            /// Instead of using matrix of size O(_length^2) or O(mr), we will reuse two array of size O(_window).
            _cost         = new MDVector[2 * _warpingWindow + 1];
            _costPrevious = new MDVector[2 * _warpingWindow + 1];

            for (int i = 0; i < 2 * _warpingWindow + 1; i++)
            {
                _cost[i]         = new MDVector(dimensions);
                _costPrevious[i] = new MDVector(dimensions);
            }
        }

        /// Calculate Dynamic Time Warping distance
        /// A,B: data and query, respectively
        /// cb : cummulative bound used for early abandoning
        /// _warpingWindow: size of Sakoe-Chiba warping band
        public double distance(MDVector[] A, MDVector[] B, MDVector[] cb, double bsf = double.PositiveInfinity)
        {
            int i, j, k;

            for (k = 0; k < 2 * _warpingWindow + 1; k++)
            {
                _costPrevious[k].set(float.PositiveInfinity);
                _cost[k].set(float.PositiveInfinity);
            }

            for (i = 0; i < _length; i++)
            {
                k = Utilities.max( 0, _warpingWindow - i );
                _minCost.set(float.PositiveInfinity);

                for (j = Utilities.max(0, i - _warpingWindow); j <= Utilities.min(_length - 1, i + _warpingWindow); j++, k++)
                {
                    // Initialize all row and column
                    if ((i == 0) && (j == 0))
                    {
                        _cost[k] = Utilities.distanceSquared( A[0], B[0], _cost[k] );
                        _minCost.set(_cost[k]);
                        continue;
                    }

                    if ((j - 1 < 0) || (k - 1 < 0))
                        _y.set(float.PositiveInfinity);
                    else
                        _y.set(_cost[k - 1]);

                    if ((i - 1 < 0) || (k + 1 > 2 * _warpingWindow))
                        _x.set(float.PositiveInfinity);
                    else
                        _x.set(_costPrevious[k + 1]);

                    if ((i - 1 < 0) || (j - 1 < 0))
                        _z.set(float.PositiveInfinity);
                    else
                        _z.set(_costPrevious[k]);

                    // Classic DTW calculation
                    _cost[k].set(
                        MDVector.add(
                            Utilities.min(Utilities.min(_x, _y), _z),
                            Utilities.distanceSquared(A[i], B[j], _result),
                            _result ) );

                    // Find minimum cost in row for early abandoning (possibly to use column instead of row).
                    if (_cost[k] < _minCost)
                    {
                        _minCost.set(_cost[k]);
                    }
                }

                // We can abandon early if the current cummulative distace with lower bound together are larger than bsf
                if ( ((i + _warpingWindow) < (_length - 1)) && (MDVector.add( _minCost, cb[i + _warpingWindow + 1], _result ) >= bsf) )
                {
                    return _result.absSum(); // _result was calculated in if() above
                }

                // Move current array to previous array.
                _costTemp     = _cost;
                _cost         = _costPrevious;
                _costPrevious = _costTemp;
            }

            k--;

            // the DTW distance is in the last cell in the matrix of size O(_length^2) or at the middle of our array.
            return _costPrevious[k].absSum();
        }

    }
}
