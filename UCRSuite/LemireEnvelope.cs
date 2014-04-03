using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UCRSuite
{
    internal sealed class LemireEnvelope
    {
        private readonly int _size;
        private readonly int _window;

        private CircularBuffer<int> _du;
        private CircularBuffer<int> _dl;

        public LemireEnvelope(int size, int window, int dimensions = 1)
        {
            _size = size;
            _window = window;

            this.Upper = new MDVector[size];
            this.Lower = new MDVector[size];

            for (int i = 0; i < size; i++)
            {
                this.Upper[i] = new MDVector(dimensions);
                this.Lower[i] = new MDVector(dimensions);
            }

            _du = new CircularBuffer<int>(2 * window + 2);
            _dl = new CircularBuffer<int>(2 * window + 2);
        }

        public MDVector[] Upper { get; private set; }
        public MDVector[] Lower { get; private set; }

        /// Finding the envelop of min and max value for LB_Keogh
        /// Implementation idea is intoruduced by Danial Lemire in his paper
        /// "Faster Retrieval with a Two-Pass Dynamic-Time-Warping Lower Bound", Pattern Recognition 42(9), 2009.
        public void process(MDVector[] t, int length)
        {
            _du.reset();
            _du.pushBack(0);

            _dl.reset();
            _dl.pushBack(0);

            for (int i = 1; i < length; i++)
            {
                if (i > _window)
                {
                    this.Upper[i - _window - 1].set(t[_du.front()]);
                    this.Lower[i - _window - 1].set(t[_dl.front()]);
                }

                if (t[i] > t[i - 1])
                {
                    _du.popBack();
                    while (!_du.Empty && t[i] > t[_du.back()])
                        _du.popBack();
                }
                else
                {
                    _dl.popBack();
                    while (!_dl.Empty && t[i] < t[_dl.back()])
                        _dl.popBack();
                }

                _du.pushBack(i);
                _dl.pushBack(i);

                if (i == 2 * _window + 1 + _du.front())
                    _du.popFront();
                else if (i == 2 * _window + 1 + _dl.front())
                    _dl.popFront();
            }

            for (int i = length; i < length + _window + 1; i++)
            {
                this.Upper[i - _window - 1].set( t[_du.front()] );
                this.Lower[i - _window - 1].set( t[_dl.front()] );

                if (i - _du.front() >= 2 * _window + 1)
                    _du.popFront();
                if (i - _dl.front() >= 2 * _window + 1)
                    _dl.popFront();
            }
        }


    }
}
