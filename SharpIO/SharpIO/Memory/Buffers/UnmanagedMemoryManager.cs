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

internal sealed unsafe class UnmanagedMemoryManager<T> : MemoryManager<T>
{
	private readonly nint _pointer;
    private readonly int _length;


    public UnmanagedMemoryManager(nint pointer, int length)
    {
        _pointer = pointer;
        _length = length;
    }

    public override Span<T> GetSpan() => new Span<T>(_pointer.ToPointer(), _length);

    public override MemoryHandle Pin(int elementIndex = 0) {
		return new MemoryHandle((void *)(_pointer + (Unsafe.SizeOf<T>() * elementIndex)));
	} 

    public override void Unpin() { /* No-op for unmanaged memory */ }

    protected override void Dispose(bool disposing) {}
}
