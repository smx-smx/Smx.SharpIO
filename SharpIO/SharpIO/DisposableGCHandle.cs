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

namespace Smx.SharpIO
{
	public struct DisposableGCHandle : IDisposable
	{
		private readonly GCHandle gch;

		public DisposableGCHandle(object value) {
			gch = GCHandle.Alloc(value);
		}

		public DisposableGCHandle(object value, GCHandleType type) {
			gch = GCHandle.Alloc(value, type);
		}

		public static DisposableGCHandle Pin(object value) {
			return new DisposableGCHandle(value, GCHandleType.Pinned);
		}

		public object? Target {
			get => gch.Target;
		}

		public IntPtr AddrOfPinnedObject() => gch.AddrOfPinnedObject();

		public void Dispose() {
			gch.Free();
		}
	}
}
