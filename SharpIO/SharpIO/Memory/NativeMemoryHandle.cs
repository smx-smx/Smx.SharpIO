#region License
/*
 * Copyright (C) 2024 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Smx.SharpIO.Extensions;
using Smx.SharpIO.Memory.Buffers;
using static Smx.SharpIO.Memory.MemoryAllocator;

namespace Smx.SharpIO.Memory
{
	public enum MemoryHandleType
	{
		HGlobal = 0,
		Native = 1,
		Custom = 2
	}

	public class NativeMemoryHandle : MemoryManager64<byte>, IDisposable
	{
		private nint handle;
		private nuint size;
		private readonly bool owned;
		private bool disposed = false;
		private readonly IMemoryManager alloctator;

		public NativeMemoryHandle(nint handle, nuint size, IMemoryManager allocator, bool owned = true) {
			this.handle = handle;
			this.size = size;
			this.owned = owned;
			this.alloctator = allocator;
		}

		public Span64<T> AsSpan<T>(int byteOffset = 0) where T : unmanaged {
			return Span.Slice(byteOffset)
				.Cast<T>();
		}

		public Span64<T> AsSpan<T>(int byteOffset, int count) where T : unmanaged {
			return Span.Slice(byteOffset, Unsafe.SizeOf<T>() * count)
				.Cast<T>();
		}

		public Span64<byte> Span => GetSpan();

		public nint Address => handle;
		public nuint Size => size;

		public nint Realloc(nuint size) {
			var newHandle = this.alloctator.Realloc(handle, size);
			if(newHandle == 0) {
				return newHandle;
			}
			this.handle = newHandle;
			this.size = size;
			return handle;			
		}

		protected override void Dispose(bool disposing) {
			if (disposed) return;
			
			if (!owned) return;

			if (disposing) {
				// no-op
			}

			var handle = Interlocked.Exchange(ref this.handle, IntPtr.Zero);
			if (handle != IntPtr.Zero) {
				this.alloctator.Free(handle);
			}
			disposed = true;
		}

		public override Span64<byte> GetSpan() {
			unsafe {
				return new Span64<byte>(handle.ToPointer(), (int)Size);
			}
		}

		public override MemoryHandle Pin(long elementIndex = 0) {
			if (elementIndex < 0 || elementIndex >= (long)Size) {
				throw new ArgumentOutOfRangeException(nameof(elementIndex));
			}
			unsafe {
				byte* dptr = (byte *)handle.ToPointer();
				return new MemoryHandle(dptr + elementIndex);
			}
		}

		public override void Unpin() { }
	}

}
