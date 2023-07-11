// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Delimits a section of a one-dimensional array.
    /// </summary>
    // Note: users should make sure they copy the fields out of an ArraySegment onto their stack
    // then validate that the fields describe valid bounds within the array.  This must be done
    // because assignments to value types are not atomic, and also because one thread reading
    // three fields from an ArraySegment may not see the same ArraySegment from one call to another
    // (ie, users could assign a new value to the old location).
    [Serializable]
    [TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
#pragma warning disable CA1066 // adding IEquatable<T> implementation could change semantics of code like that in xunit that queries for IEquatable vs enumerating contents
    public readonly struct ArraySegment<T> : IList<T>, IReadOnlyList<T>
#pragma warning restore CA1066
    {
        // ArraySegment<T> doesn't implement IEquatable<T>, even though it provides a strongly-typed
        // Equals(T), as that results in different comparison semantics than comparing item-by-item
        // the elements returned from its IEnumerable<T> implementation.  This then is a breaking change
        // for usage like that in xunit's Assert.Equal, which will prioritize using an instance's IEquatable<T>
        // over its IEnumerable<T>.

        // Do not replace the array allocation with Array.Empty. We don't want to have the overhead of
        // instantiating another generic type in addition to ArraySegment<T> for new type parameters.
#pragma warning disable CA1825
        public static ArraySegment<T> Empty { get; } = new ArraySegment<T>(new T[0]);
#pragma warning restore CA1825

        private readonly T[]? _array; // Do not rename (binary serialization)
        private readonly int _offset; // Do not rename (binary serialization)
        private readonly int _count; // Do not rename (binary serialization)

        public ArraySegment(T[] array)
        {
            if (array == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);

            _array = array;
            _offset = 0;
            _count = array.Length;
        }

        public ArraySegment(T[] array, int offset, int count)
        {
            // Validate arguments, check is minimal instructions with reduced branching for inlinable fast-path
            // Negative values discovered though conversion to high values when converted to unsigned
            // Failure should be rare and location determination and message is delegated to failure functions
            if (array == null || (uint)offset > (uint)array.Length || (uint)count > (uint)(array.Length - offset))
                ThrowHelper.ThrowArraySegmentCtorValidationFailedExceptions(array, offset, count);

            _array = array;
            _offset = offset;
            _count = count;
        }

        public T[]? Array => _array;

        public int Offset => _offset;

        public int Count => _count;

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_count)
                {
                    ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
                }

                return _array![_offset + index];
            }
            set
            {
                if ((uint)index >= (uint)_count)
                {
                    ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
                }

                _array![_offset + index] = value;
            }
        }

        public Enumerator GetEnumerator()
        {
            ThrowInvalidOperationIfDefault();
            return new Enumerator(this);
        }

        public override int GetHashCode() =>
            _array is null ? 0 : HashCode.Combine(_offset, _count, _array.GetHashCode());

        public void CopyTo(T[] destination) => CopyTo(destination, 0);

        public void CopyTo(T[] destination, int destinationIndex)
        {
            ThrowInvalidOperationIfDefault();
            System.Array.Copy(_array!, _offset, destination, destinationIndex, _count);
        }

        public void CopyTo(ArraySegment<T> destination)
        {
            ThrowInvalidOperationIfDefault();
            destination.ThrowInvalidOperationIfDefault();

            if (_count > destination._count)
            {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }

            System.Array.Copy(_array!, _offset, destination._array!, destination._offset, _count);
        }

        public override bool Equals([NotNullWhen(true)] object? obj) =>
            obj is ArraySegment<T> other && Equals(other);

        public bool Equals(ArraySegment<T> obj) =>
            obj._array == _array && obj._offset == _offset && obj._count == _count;

        public ArraySegment<T> Slice(int index)
        {
            ThrowInvalidOperationIfDefault();

            if ((uint)index > (uint)_count)
            {
                ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException();
            }

            return new ArraySegment<T>(_array!, _offset + index, _count - index);
        }

        public ArraySegment<T> Slice(int index, int count)
        {
            ThrowInvalidOperationIfDefault();

            if ((uint)index > (uint)_count || (uint)count > (uint)(_count - index))
            {
                ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException();
            }

            return new ArraySegment<T>(_array!, _offset + index, count);
        }

        public T[] ToArray()
        {
            ThrowInvalidOperationIfDefault();

            if (_count == 0)
            {
                return Empty._array!;
            }

            var array = new T[_count];
            System.Array.Copy(_array!, _offset, array, 0, _count);
            return array;
        }

        public static bool operator ==(ArraySegment<T> a, ArraySegment<T> b) => a.Equals(b);

        public static bool operator !=(ArraySegment<T> a, ArraySegment<T> b) => !(a == b);

        public static implicit operator ArraySegment<T>(T[] array) => array != null ? new ArraySegment<T>(array) : default;

        #region IList<T>
        T IList<T>.this[int index]
        {
            get
            {
                ThrowInvalidOperationIfDefault();
                if (index < 0 || index >= _count)
                    ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();

                return _array![_offset + index];
            }

            set
            {
                ThrowInvalidOperationIfDefault();
                if (index < 0 || index >= _count)
                    ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();

                _array![_offset + index] = value;
            }
        }

        int IList<T>.IndexOf(T item)
        {
            ThrowInvalidOperationIfDefault();

            int index = System.Array.IndexOf(_array!, item, _offset, _count);

            Debug.Assert(index < 0 ||
                            (index >= _offset && index < _offset + _count));

            return index >= 0 ? index - _offset : -1;
        }

        void IList<T>.Insert(int index, T item) => ThrowHelper.ThrowNotSupportedException();

        void IList<T>.RemoveAt(int index) => ThrowHelper.ThrowNotSupportedException();
        #endregion

        #region IReadOnlyList<T>
        T IReadOnlyList<T>.this[int index]
        {
            get
            {
                ThrowInvalidOperationIfDefault();
                if (index < 0 || index >= _count)
                    ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();

                return _array![_offset + index];
            }
        }
        #endregion IReadOnlyList<T>

        #region ICollection<T>
        bool ICollection<T>.IsReadOnly =>
            // the indexer setter does not throw an exception although IsReadOnly is true.
            // This is to match the behavior of arrays.
            true;

        void ICollection<T>.Add(T item) => ThrowHelper.ThrowNotSupportedException();

        void ICollection<T>.Clear() => ThrowHelper.ThrowNotSupportedException();

        bool ICollection<T>.Contains(T item)
        {
            ThrowInvalidOperationIfDefault();

            int index = System.Array.IndexOf(_array!, item, _offset, _count);

            Debug.Assert(index < 0 ||
                            (index >= _offset && index < _offset + _count));

            return index >= 0;
        }

        bool ICollection<T>.Remove(T item)
        {
            ThrowHelper.ThrowNotSupportedException();
            return default;
        }
        #endregion

        #region IEnumerable<T>

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowInvalidOperationIfDefault();
            return
                Count == 0 ? SZGenericArrayEnumerator<T>.Empty :
                new Enumerator(this);
        }
        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();
        #endregion

        private void ThrowInvalidOperationIfDefault()
        {
            if (_array == null)
            {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_NullArray);
            }
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[]? _array;
            private readonly int _start;
            private readonly int _end; // cache Offset + Count, since it's a little slow
            private int _current;

            internal Enumerator(ArraySegment<T> arraySegment)
            {
                Debug.Assert(arraySegment.Array != null);
                Debug.Assert(arraySegment.Offset >= 0);
                Debug.Assert(arraySegment.Count >= 0);
                Debug.Assert(arraySegment.Offset + arraySegment.Count <= arraySegment.Array.Length);

                _array = arraySegment.Array;
                _start = arraySegment.Offset;
                _end = arraySegment.Offset + arraySegment.Count;
                _current = arraySegment.Offset - 1;
            }

            public bool MoveNext()
            {
                if (_current < _end)
                {
                    _current++;
                    return _current < _end;
                }
                return false;
            }

            public T Current
            {
                get
                {
                    if (_current < _start)
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumNotStarted();
                    if (_current >= _end)
                        ThrowHelper.ThrowInvalidOperationException_InvalidOperation_EnumEnded();
                    return _array![_current];
                }
            }

            object? IEnumerator.Current => Current;

            void IEnumerator.Reset()
            {
                _current = _start - 1;
            }

            public void Dispose()
            {
            }
        }
    }
}
