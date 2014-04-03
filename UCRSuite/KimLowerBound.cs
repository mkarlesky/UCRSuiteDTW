using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UCRSuite
{
    internal sealed class KimLowerBound
    {
        private MDVector _x0, _x1, _x2;
        private MDVector _y0, _y1, _y2;

        private MDVector _lb;

        private MDVector _result1, _result2, _result3, _result4, _result5; // temps

        public KimLowerBound(int dimensions = 1)
        {
            _x0 = new MDVector(dimensions);
            _x1 = new MDVector(dimensions);
            _x2 = new MDVector(dimensions);

            _y0 = new MDVector(dimensions);
            _y1 = new MDVector(dimensions);
            _y2 = new MDVector(dimensions);

            _lb = new MDVector(dimensions);

            _result1 = new MDVector(dimensions);
            _result2 = new MDVector(dimensions);
            _result3 = new MDVector(dimensions);
            _result4 = new MDVector(dimensions);
            _result5 = new MDVector(dimensions);
        }

        /// Calculate quick lower bound
        /// Usually, LB_Kim take time O(m) for finding top,bottom,fist and last.
        /// However, because of z-normalization the top and bottom cannot give siginifant benefits.
        /// And using the first and last points can be computed in constant time.
        /// The prunning power of LB_Kim is non-trivial, especially when the query is not long, say in length 128.
        public MDVector hierarchy(MDVector[] t, MDVector[] q, long j, int len, MDVector mean, MDVector std, double bsf = double.PositiveInfinity)
        {
            MDVector d;

            /// 1 point at front and back
            _x0 = Utilities.normalize( t[j], mean, std, _x0 );
            _y0 = Utilities.normalize( t[(len - 1 + j)], mean, std, _y0 );

            _lb = MDVector.add(
                Utilities.distanceSquared( _x0, q[0], _result1 ),
                Utilities.distanceSquared( _y0, q[len - 1], _result2 ),
                _lb);
            if (_lb >= bsf) return _lb;

            /// 2 points at front
            _x1 = Utilities.normalize( t[(j + 1)], mean, std, _x1 );
            d = Utilities.min(
                    Utilities.distanceSquared(_x1, q[0], _result1),
                    Utilities.distanceSquared(_x0, q[1], _result2) );
            d = Utilities.min( d, Utilities.distanceSquared( _x1, q[1], _result3 ) );
            _lb = MDVector.add( _lb, d, _lb );
            if (_lb >= bsf) return _lb;

            /// 2 points at back
            _y1 = Utilities.normalize( t[(len - 2 + j)], mean, std, _y1 );
            d = Utilities.min(
                    Utilities.distanceSquared(_y1, q[len - 1], _result1),
                    Utilities.distanceSquared(_y0, q[len - 2], _result2) );
            d = Utilities.min( d, Utilities.distanceSquared( _y1, q[len - 2], _result3 ) );
            _lb = MDVector.add(_lb, d, _lb);
            if (_lb >= bsf) return _lb;

            /// 3 points at front
            _x2 = Utilities.normalize( t[(j + 2)], mean, std, _x2 );
            d = Utilities.min(
                    Utilities.distanceSquared( _x0, q[2], _result1 ),
                    Utilities.distanceSquared( _x1, q[2], _result2 ) );
            d = Utilities.min( d, Utilities.distanceSquared( _x2, q[2], _result3 ) );
            d = Utilities.min( d, Utilities.distanceSquared( _x2, q[1], _result4 ) );
            d = Utilities.min( d, Utilities.distanceSquared( _x2, q[0], _result5 ) );
            _lb = MDVector.add(_lb, d, _lb);
            if (_lb >= bsf) return _lb;

            /// 3 points at back
            _y2 = Utilities.normalize( t[(len - 3 + j)], mean, std, _y2 );
            d = Utilities.min(
                    Utilities.distanceSquared( _y0, q[len - 3], _result1 ),
                    Utilities.distanceSquared( _y1, q[len - 3], _result2 ) );
            d = Utilities.min( d, Utilities.distanceSquared( _y2, q[len - 3], _result3 ) );
            d = Utilities.min( d, Utilities.distanceSquared( _y2, q[len - 2], _result4 ) );
            d = Utilities.min( d, Utilities.distanceSquared( _y2, q[len - 1], _result5 ) );
            _lb = MDVector.add(_lb, d, _lb);

            return _lb;
        }
    }
}
