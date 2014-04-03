using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UCRSuite
{
    internal sealed class CircularBuffer<T>
    {
        private T[] _dq;
        private int _size, _capacity;
        private int _f, _r;

        /// Initial the queue at the begining step of envelop calculation
        public CircularBuffer(int capacity)
        {
            _capacity = capacity;

            _dq = new T[capacity];

            reset();
        }

        public bool Empty
        {
            get { return _size == 0; }
        }

        public void reset()
        {
            _size = 0;

            _f = 0;
            _r = _capacity - 1;
        }

        /// Insert to the queue at the back
        public void pushBack(T v)
        {
            _dq[_r] = v;
            _r--;
            if (_r < 0)
            {
                _r = _capacity - 1;
            }
            _size++;
        }

        /// Delete the current (front) element from queue
        public void popFront()
        {
            _f--;
            if (_f < 0)
                _f = _capacity - 1;
            _size--;
        }

        /// Delete the last element from queue
        public void popBack()
        {
            _r = (_r + 1) % _capacity;
            _size--;
        }

        /// Get the value at the current position of the circular queue
        public T front()
        {
            int aux = _f - 1;

            if (aux < 0)
                aux = _capacity - 1;
            return _dq[aux];
        }

        /// Get the value at the last position of the circular queueint back(struct deque *d)
        public T back()
        {
            int aux = (_r + 1) % _capacity;
            return _dq[aux];
        }
    }
}
