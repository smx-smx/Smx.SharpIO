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
using System.Runtime.InteropServices;
using System.Text;

namespace Smx.SharpIO.Memory
{
	public class MemoryManagerHGlobal : IMemoryManager
	{
		public nint Alloc(nuint size) {
			return Marshal.AllocHGlobal((int)size);
		}

		public void Free(nint ptr) {
			Marshal.FreeHGlobal(ptr);
		}

		public nint Realloc(nint ptr, nuint size) {
			return Marshal.ReAllocHGlobal(ptr, (nint)size);
		}
	}
}
