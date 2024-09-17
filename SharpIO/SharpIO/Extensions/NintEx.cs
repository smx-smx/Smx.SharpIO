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

namespace Smx.SharpIO.Extensions
{
	public static class NintEx
	{
		public static unsafe void* ToPointer(this nint @this) {
			return (void*)@this;
		}

		public static Span<T> AsSpan<T>(this nint ptr, int numElements) {
			Span<T> span;
			unsafe {
				span = new Span<T>(ptr.ToPointer(), numElements);
			}
			return span;
		}

		public static readonly unsafe int Size = sizeof(void *);
	}
}
