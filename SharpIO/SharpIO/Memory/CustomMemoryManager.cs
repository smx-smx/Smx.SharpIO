#region License
/*
 * Copyright (C) 2024 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace Smx.SharpIO.Memory
{
	public class CustomMemoryManager : IMemoryManager
	{
		public delegate nint pfnAlloc(nuint size);
		public delegate void pfnFree(nint handle);
		public delegate nint pfnRealloc(nint ptr, nuint size);

		private readonly pfnAlloc fnAlloc;
		private readonly pfnFree fnFree;
		private readonly pfnRealloc? fnRealloc;

		public CustomMemoryManager(pfnAlloc alloc, pfnFree free, pfnRealloc? realloc = null) {
			this.fnAlloc = alloc;
			this.fnFree = free;
			this.fnRealloc = realloc;
		}

		public nint Alloc(nuint size) {
			return this.fnAlloc(size);
		}

		public void Free(nint ptr) {
			this.fnFree(ptr);
		}

		public nint Realloc(nint ptr, nuint size) {
			if(this.fnRealloc == null) {
				throw new InvalidOperationException(nameof(this.fnRealloc));
			}
			return this.fnRealloc(ptr, size);
		}
	}
}
