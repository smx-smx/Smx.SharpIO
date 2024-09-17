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
	public static class MemoryHGlobal
	{
		public static readonly MemoryAllocator Allocator = new MemoryAllocator(new MemoryManagerHGlobal());

		public static TypedMemoryHandle<T> Alloc<T>(nuint? size = null) where T : unmanaged {
			return Allocator.Alloc<T>(size);
		}

		public static NativeMemoryHandle Alloc(nuint size, bool owned = true) {
			return Allocator.Alloc(size);
		}

		public static NativeMemoryHandle Alloc(nint size, bool owned = true) {
			return Alloc((nuint)size, owned);
		}
	}
}
