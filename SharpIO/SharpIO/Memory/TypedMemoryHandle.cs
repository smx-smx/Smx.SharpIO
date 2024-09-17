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
using System.Runtime.InteropServices;
using System.Text;

namespace Smx.SharpIO.Memory
{
	public class TypedMemoryHandle<T> : IDisposable where T : unmanaged
	{
		public NativeMemoryHandle Memory { get; private set; }
		public TypedPointer<T> Pointer;

		private T tmpValue = default;

		public ref T Value {
			get {
				if (_isPacked) {
					tmpValue = Marshal.PtrToStructure<T>(Pointer.Address);
					return ref tmpValue;
				} else {
					return ref Pointer.Value;
				}
			}
		}

		public Span<T> Span => Memory.AsSpan<T>(0);

		public nint Address => Pointer.Address;

		private readonly bool _isPacked;

		public void Write() {
			if (!_isPacked) return;
			Marshal.StructureToPtr<T>(Value, Memory.Address, false);
		}

		public TypedMemoryHandle(NativeMemoryHandle memory) {
			var structLayout = typeof(T).StructLayoutAttribute;
			_isPacked = structLayout != null && structLayout.Pack != 0 && structLayout.Pack != NintEx.Size;

			Memory = memory;
			Pointer = new TypedPointer<T>(Memory.Address);
		}

		public void Dispose() {
			Memory.Dispose();
		}
	}
}
