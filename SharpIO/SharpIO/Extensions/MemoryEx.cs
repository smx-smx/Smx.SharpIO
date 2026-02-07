#region License
/*
 * Copyright (C) 2024 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using Smx.SharpIO.Memory;
using Smx.SharpIO.Memory.Buffers;
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

		public static Memory64<TTo> Cast<TFrom, TTo>(this Memory64<TFrom> from) 
			where TFrom : unmanaged
			where TTo : unmanaged
		{
			// avoid the extra allocation/indirection, at the cost of a gen-0 box
			if (typeof(TFrom) == typeof(TTo)) return (Memory64<TTo>)(object)from;

			return new CastMemoryManager64<TFrom, TTo>(from).Memory;
		}

		/// <summary>
		/// Copies the contents of the array into the span. If the source
		/// and destinations overlap, this method behaves as if the original values in
		/// a temporary location before the destination is overwritten.
		/// </summary>
		///<param name="source">The array to copy items from.</param>
		/// <param name="destination">The span to copy items into.</param>
		/// <exception cref="ArgumentException">
		/// Thrown when the destination Span is shorter than the source array.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyTo<T>(this T[]? source, Span64<T> destination) {
			new ReadOnlySpan64<T>(source).CopyTo(destination);
		}

		/// <summary>
		/// Copies the contents of the array into the memory. If the source
		/// and destinations overlap, this method behaves as if the original values are in
		/// a temporary location before the destination is overwritten.
		/// </summary>
		///<param name="source">The array to copy items from.</param>
		/// <param name="destination">The memory to copy items into.</param>
		/// <exception cref="ArgumentException">
		/// Thrown when the destination is shorter than the source array.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CopyTo<T>(this T[]? source, Memory64<T> destination) {
			source.CopyTo(destination.Span);
		}
	}
}
