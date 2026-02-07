#region License
/*
 * Copyright (C) 2026 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Smx.SharpIO.Memory.Buffers;

public readonly ref struct ReadOnlySpan64<T>
{
    internal readonly ref T _reference;
    private readonly long _length;

    public long Length => _length;
    public bool IsEmpty => _length == 0;

	public static implicit operator ReadOnlySpan64<T>(ReadOnlySpan<T> span) {
		ref var dref = ref MemoryMarshal.GetReference(span);
		return MemoryMarshal64.CreateReadOnlySpan(ref dref, span.Length);
	}

    public static explicit operator ReadOnlySpan<T>(ReadOnlySpan64<T> span64) {
        if (span64._length > int.MaxValue)
        {
            throw new OverflowException("Span64 length exceeds the 32-bit limit of Span<T>.");
        }
        return MemoryMarshal.CreateReadOnlySpan(ref span64._reference, (int)span64._length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan64<T>(T[]? array) {
        return new ReadOnlySpan64<T>(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan64<T>(ArraySegment<T> segment) {
        return new ReadOnlySpan64<T>(segment.Array).Slice(segment.Offset, segment.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ReadOnlySpan64(void* pointer, long length) {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        _reference = ref Unsafe.AsRef<T>(pointer);
        _length = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan64(ref T reference, long length) {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        _reference = ref reference;
        _length = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan64(T[]? array) {
        if (array == null) {
            this = default;
            return;
        }
        _reference = ref MemoryMarshal.GetArrayDataReference(array);
        _length = array.LongLength;
    }

    public ref readonly T this[long index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if ((ulong)index >= (ulong)_length) ThrowHelper.ThrowIndexOutOfRangeException();
            return ref Unsafe.Add(ref _reference, (nint)index);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan64<T> Slice(long start) {
        if ((ulong)start > (ulong)_length) ThrowHelper.ThrowArgumentOutOfRangeException();
        return new ReadOnlySpan64<T>(ref Unsafe.Add(ref _reference, (nint)start), _length - start);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan64<T> Slice(long start, long length) {
        // Overflow-safe check
        if ((ulong)start > (ulong)_length || (ulong)length > (ulong)(_length - start)) {
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }
        return new ReadOnlySpan64<T>(ref Unsafe.Add(ref _reference, (nint)start), length);
    }

    public void CopyTo(Span64<T> destination) {
        if ((ulong)_length > (ulong)destination.Length) ThrowHelper.ThrowArgumentException_DestTooShort();
        new Span64<T>(ref _reference, _length).CopyTo(destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly T GetPinnableReference() => ref _reference;

    public T[] ToArray() {
        if (_length > int.MaxValue)
            throw new OverflowException("Length exceeds int.MaxValue.");

        if (_length == 0)
            return Array.Empty<T>();

        var destination = new T[(int)_length];

        // Reuse CopyTo inside ReadOnlySpan64
        this.CopyTo(new Span64<T>(destination));

        return destination;
    }

    /// <summary>
    /// Tries to copy the current span into the destination span.
    /// Returns false if the destination is too short.
    /// </summary>
    public bool TryCopyTo(Span64<T> destination) {
        if ((ulong)_length > (ulong)destination.Length) {
            return false;
        }
        CopyTo(destination);
        return true;
    }

    public static bool operator ==(ReadOnlySpan64<T> left, ReadOnlySpan64<T> right) {
        return left._length == right._length &&
               Unsafe.AreSame(ref left._reference, ref right._reference);
    }

    public static bool operator !=(ReadOnlySpan64<T> left, ReadOnlySpan64<T> right) {
        return !(left == right);
    }

    public override string ToString() {
        return $"Smx.SharpIO.Memory.Buffers.ReadOnlySpan64<{typeof(T).Name}>[{_length}]";
    }

    public override bool Equals(object? obj) => throw new NotSupportedException("ReadOnlySpan64<T> cannot be boxed");
    public override int GetHashCode() => throw new NotSupportedException("ReadOnlySpan64<T> cannot be boxed");

    public Enumerator GetEnumerator() => new Enumerator(this);

    public ref struct Enumerator {
        private readonly ReadOnlySpan64<T> _span;
        private long _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ReadOnlySpan64<T> span) {
            _span = span;
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() {
            long index = _index + 1;
            if (index < _span.Length) {
                _index = index;
                return true;
            }
            return false;
        }

        public ref readonly T Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _span[_index];
        }
    }

	public ChunkEnumerator GetChunks() => new ChunkEnumerator(this);


	// Must be a ref struct because it holds a Span<T>
	public ref struct ChunkEnumerator
	{
		private readonly ReadOnlySpan64<T> _source;
		private long _position;
		private ReadOnlySpan<T> _current;

		internal ChunkEnumerator(ReadOnlySpan64<T> source) {
			_source = source;
			_position = 0;
			_current = default;
		}

		public readonly ReadOnlySpan<T> Current => _current;

		public bool MoveNext() {
			if (_position >= _source.Length) {
				return false;
			}

			long remaining = _source.Length - _position;

			// Cap the size at int.MaxValue (standard Span limit)
			int currentChunkSize = (remaining > int.MaxValue)
				? int.MaxValue
				: (int)remaining;

			var slice64 = _source.Slice(_position, currentChunkSize);
			_current = (ReadOnlySpan<T>)slice64;

			_position += currentChunkSize;
			return true;
		}

		public readonly ChunkEnumerator GetEnumerator() => this;
	}
}
