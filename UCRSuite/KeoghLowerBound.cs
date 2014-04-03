using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UCRSuite
{
    internal sealed class KeoghLowerBound
    {
        private MDVector _lb;
        private MDVector _d;

        private MDVector _z;

        private MDVector _uu;
        private MDVector _ll;

        private MDVector _result; // temp

        public KeoghLowerBound(int dimensions = 1)
        {
            _lb = new MDVector(dimensions);
            _d  = new MDVector(dimensions);
            _z  = new MDVector(dimensions);
            _uu = new MDVector(dimensions);
            _ll = new MDVector(dimensions);

            _result = new MDVector(dimensions);
        }


        /// LB_Keogh 1: Create Envelop for the query
        /// Note that because the query is known, envelop can be created once at the begenining.
        ///
        /// Variable Explanation,
        /// order : sorted indices for the query.
        /// uo, lo: upper and lower envelops for the query, which already sorted.
        /// t     : a circular array keeping the current data.
        /// j     : index of the starting location in t
        /// cb    : (output) current bound at each position. It will be used later for early abandoning in DTW.
        public MDVector cumulative(
            int[] order,
            MDVector[] t,
            MDVector[] uo,
            MDVector[] lo,
            MDVector[] cb,
            long j,
            int len,
            MDVector mean,
            MDVector std,
            double bsf = double.PositiveInfinity) // best so far
        {
            MDVector a;

            _lb.set(0);
            _d.set(0);

            for (int i = 0; i < len && _lb < bsf; i++)
            {
                _z = Utilities.normalize( t[(order[i] + j)], mean, std, _z );

                a = _d;

                if (_z > uo[i])
                    a = Utilities.distanceSquared(_z, uo[i], _result);
                else if (_z < lo[i])
                    a = Utilities.distanceSquared(_z, lo[i], _result);

                _lb += a;

                cb[order[i]].set(a);
            }

            return _lb;
        }

        /// LB_Keogh 2: Create Envelop for the data
        /// Note that the envelops have been created (in main function) when each data point has been read.
        ///
        /// Variable Explanation,
        /// qo: sorted query
        /// cb: (output) current bound at each position. Used later for early abandoning in DTW.
        /// l,u: lower and upper envelop of the current data
        /// I: array pointer
        public MDVector dataCumulative(
            int[] order,
            MDVector[] qo,
            MDVector[] cb,
            MDVector[] l,
            MDVector[] u,
            int I,
            int len,
            MDVector mean,
            MDVector std,
            double bsf = double.PositiveInfinity) // best so far
        {
            MDVector a;

            _lb.set(0);
            _d.set(0);

            for (int i = 0; i < len && _lb < bsf; i++)
            {
                _uu = Utilities.normalize( u[order[i] + I], mean, std, _uu );
                _ll = Utilities.normalize( l[order[i] + I], mean, std, _ll );

                a = _d;

                if (qo[i] > _uu)
                {
                    a = Utilities.distanceSquared( qo[i], _uu, _result );
                }
                else
                {
                    if (qo[i] < _ll)
                    {
                        a = Utilities.distanceSquared(qo[i], _ll, _result);
                    }
                }

                _lb = MDVector.add( _lb, a, _lb );
                cb[order[i]].set( a );
            }

            return _lb;
        }
    }
}
