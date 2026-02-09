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

namespace Smx.SharpIO.Memory.Buffers;

internal sealed unsafe class UnmanagedMemoryManager<T> : MemoryManager64<T>
{
	private readonly nint _pointer;
    private readonly long _length;
	private readonly MemoryHandle? _handle;


    public UnmanagedMemoryManager(nint pointer, long length, MemoryHandle? handle = null)
    {
        _pointer = pointer;
        _length = length;
		_handle = handle;
    }

    public override Span64<T> GetSpan() => new Span64<T>(_pointer.ToPointer(), _length);

    public override MemoryHandle Pin(long elementIndex = 0) {
		return new MemoryHandle((void *)(_pointer + (Unsafe.SizeOf<T>() * elementIndex)));
	} 

    public override void Unpin() { /* No-op for unmanaged memory */ }

    protected override void Dispose(bool disposing) {
		_handle?.Dispose();
	}
}
