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
using System.Runtime.CompilerServices;
using System.Text;
using Smx.SharpIO.Extensions;
using Smx.SharpIO.Memory.Buffers;

namespace Smx.SharpIO.Memory
{
	public record struct TypedPointer<T>(nint Address) where T : unmanaged
	{
		public ref T Value {
			get {
				unsafe {
					return ref Unsafe.AsRef<T>(Address.ToPointer());
				}
			}
		}

		public Span64<T> AsSpan(long numElements){
			return Address.AsSpan<T>(numElements);
		}

		public static TypedPointer<T> operator +(TypedPointer<T> ptr, long n) {
			return new TypedPointer<T>(ptr.Address + (Unsafe.SizeOf<T>() * new nint(n)));
		}

		public static TypedPointer<T> operator -(TypedPointer<T> ptr, long n) {
			return new TypedPointer<T>(ptr.Address - (Unsafe.SizeOf<T>() * new nint(n)));
		}

		public static TypedPointer<T> operator ++(TypedPointer<T> ptr) {
			return new TypedPointer<T>(ptr.Address + Unsafe.SizeOf<T>());
		}

		public static TypedPointer<T> operator --(TypedPointer<T> ptr) {
			return new TypedPointer<T>(ptr.Address - Unsafe.SizeOf<T>());
		}

		public unsafe T* ToPointer() {
			return (T*)Address.ToPointer();
		}
	}
}
