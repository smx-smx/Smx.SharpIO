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

namespace Smx.SharpIO.Memory.Buffers;

public abstract class MemoryManager64<T> : IMemoryOwner64<T>, IPinnable64, IPinnable
{
    // -------------------------------------------------------------
    // Abstract Methods (Must be implemented by concrete classes)
    // -------------------------------------------------------------

    /// <summary>
    /// Returns a Span64 wrapping the underlying memory.
    /// </summary>
    public abstract Span64<T> GetSpan();

    /// <summary>
    /// Pins the memory at a 64-bit offset.
    /// </summary>
    public abstract MemoryHandle Pin(long elementIndex = 0);

    /// <summary>
    /// Unpins the memory. Called automatically by MemoryHandle.Dispose().
    /// </summary>
    public abstract void Unpin();

    /// <summary>
    /// Disposes the unmanaged resources.
    /// </summary>
    protected abstract void Dispose(bool disposing);

    // -------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------

    public virtual Memory64<T> Memory => new Memory64<T>(this, 0, GetSpan().Length);

    public virtual void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // -------------------------------------------------------------
    // IPinnable (Standard 32-bit) Implementation
    // -------------------------------------------------------------

    // Explicit implementation handles the compatibility with standard MemoryHandle
    MemoryHandle IPinnable.Pin(int elementIndex) {
        // Forward to the 64-bit implementation
        return Pin((long)elementIndex);
    }

    void IPinnable.Unpin() {
        Unpin();
    }
}