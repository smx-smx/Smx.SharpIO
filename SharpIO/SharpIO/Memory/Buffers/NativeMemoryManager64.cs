#region License
/*
 * Copyright (C) 2026 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Smx.SharpIO.Memory.Buffers;

public sealed class NativeMemoryManager64<T> : MemoryManager64<T>
{
    private unsafe void* _pointer;
    private long _length;
    private bool _disposed;

    private readonly bool _ownsMemory;
    private readonly Action? _onDispose;

    /// <summary>
    /// Allocates new native memory.
    /// </summary>
    public unsafe NativeMemoryManager64(long length, bool zeroMemory = true) {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

        nuint byteCount = (nuint)length * (nuint)Unsafe.SizeOf<T>();
        _pointer = NativeMemory.Alloc(byteCount);
        _length = length;
        _ownsMemory = true;
        _onDispose = null;

        if (zeroMemory) {
            NativeMemory.Clear(_pointer, byteCount);
        }
    }

    /// <summary>
    /// Wraps an existing pointer (e.g. from MemoryMappedFile).
    /// </summary>
    public unsafe NativeMemoryManager64(void* pointer, long length, Action? onDispose = null) {
        if (pointer == null) throw new ArgumentNullException(nameof(pointer));
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

        _pointer = pointer;
        _length = length;
        _ownsMemory = false;
        _onDispose = onDispose;
    }

	public override unsafe Span64<T> GetSpan() {
        if (_disposed) throw new ObjectDisposedException(nameof(NativeMemoryManager64<T>));
        return new Span64<T>(_pointer, _length);
    }

    public override unsafe MemoryHandle Pin(long elementIndex = 0) {
        if (elementIndex < 0 || (ulong)elementIndex > (ulong)_length)
            throw new ArgumentOutOfRangeException(nameof(elementIndex));

        if (_disposed) throw new ObjectDisposedException(nameof(NativeMemoryManager64<T>));

        void* offsetPtr = (byte*)_pointer + ((nint)elementIndex * Unsafe.SizeOf<T>());

        // Pass 'this' as IPinnable so MemoryHandle.Dispose() calls our Unpin()
        return new MemoryHandle(offsetPtr, default, this);
    }

    public override void Unpin() {
        // No-op for raw native memory
    }

    protected override unsafe void Dispose(bool disposing) {
        if (!_disposed) {
            _disposed = true;

            if (_ownsMemory && _pointer != null) {
                NativeMemory.Free(_pointer);
            }

            _onDispose?.Invoke();

            _pointer = null;
            _length = 0;
        }
    }
}
