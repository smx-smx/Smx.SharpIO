#region License
/*
 * Copyright (C) 2024 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using Smx.SharpIO.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Smx.SharpIO.Memory
{
	public class MemoryAllocator : IMemoryAllocator
	{
		private readonly IMemoryManager MemoryManager;

		public MemoryAllocator(IMemoryManager mman) {
			this.MemoryManager = mman;
		}

		public TypedMemoryHandle<T> StructureToPtr<T>(T obj, bool owned = true) where T : unmanaged {
			var mem = Alloc((nuint)Unsafe.SizeOf<T>(), owned);
			Marshal.StructureToPtr(obj, mem.Address, false);
			return new TypedMemoryHandle<T>(mem);
		}

		public TypedMemoryHandle<T> Alloc<T>(nuint? size = null) where T : unmanaged {
			var allocSize = size == null ? (nuint)Unsafe.SizeOf<T>() : size.Value;
			return new TypedMemoryHandle<T>(Alloc(allocSize));
		}

		public NativeMemoryHandle Alloc(nuint size, bool owned = true) {
			var mem = MemoryManager.Alloc(size);
			mem.AsSpan<byte>((int)size).Clear();
			return new NativeMemoryHandle(mem, size, MemoryManager, owned: owned);
		}

		public NativeMemoryHandle AllocString(string str, Encoding enc) {
			var bytes = enc.GetBytes(str + char.MinValue);
			var mem = Alloc((nuint)bytes.Length);
			Marshal.Copy(bytes, 0, mem.Address, bytes.Length);
			return mem;
		}
	}
}
