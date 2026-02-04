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
using System.IO.MemoryMappedFiles;
using Smx.SharpIO.Memory.Buffers;

namespace Smx.SharpIO
{
	public unsafe class MemoryMappedSpan<T> : MemoryManager64<T>, IDisposable where T : unmanaged
	{
		public readonly int Length;

		private readonly MemoryMappedViewAccessor acc;
		private readonly byte* dptr = null;
		private bool disposed = false;

		public MemoryMappedSpan(MemoryMappedFile mf, int length, MemoryMappedFileAccess mmapFlags) {
			this.Length = length;
			this.acc = mf.CreateViewAccessor(0, length, mmapFlags);
			this.acc.SafeMemoryMappedViewHandle.AcquirePointer(ref dptr);
		}

		public override Span64<T> GetSpan() {
			return new Span64<T>((void*)dptr, Length);
		}

		public override MemoryHandle Pin(long elementIndex = 0) {
			if (elementIndex < 0 || elementIndex >= Length) {
				throw new ArgumentOutOfRangeException(nameof(elementIndex));
			}

			return new MemoryHandle(dptr + elementIndex);
		}

		public override void Unpin() { }

		public override void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
			base.Dispose();
		}

		protected override void Dispose(bool disposing) {
			if (disposed) {
				return;
			}
			if (disposing) {
				acc.SafeMemoryMappedViewHandle.ReleasePointer();
				acc.Dispose();
			}
			disposed = true;
		}
	}
}
