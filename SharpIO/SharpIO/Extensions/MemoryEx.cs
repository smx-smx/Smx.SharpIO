#region License
/*
 * Copyright (C) 2024 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using Smx.SharpIO.Memory;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Smx.SharpIO.Extensions
{
	public static class MemoryEx
	{
		public static Memory<TTo> Cast<TFrom, TTo>(this Memory<TFrom> from)
			where TFrom : unmanaged
			where TTo : unmanaged
		{
			// avoid the extra allocation/indirection, at the cost of a gen-0 box
			if (typeof(TFrom) == typeof(TTo)) return (Memory<TTo>)(object)from;

			return new CastMemoryManager<TFrom, TTo>(from).Memory;
		}
	}
}
