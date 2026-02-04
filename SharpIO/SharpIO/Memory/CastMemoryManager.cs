#region License
/*
 * Copyright (C) 2024 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Smx.SharpIO.Memory.Buffers;

namespace Smx.SharpIO.Memory
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>https://stackoverflow.com/a/54512940/11782802</remarks>
	public sealed class CastMemoryManager<TFrom, TTo> : MemoryManager<TTo>
		where TFrom : unmanaged
		where TTo : unmanaged
	{
		private readonly Memory<TFrom> _from;

		public CastMemoryManager(Memory<TFrom> from) => _from = from;

		public override Span<TTo> GetSpan()
			=> MemoryMarshal.Cast<TFrom, TTo>(_from.Span);

		protected override void Dispose(bool disposing) { }
		public override MemoryHandle Pin(int elementIndex = 0)
			=> throw new NotImplementedException();
		public override void Unpin()
			=> throw new NotImplementedException();
	}

	public sealed class CastMemoryManager64<TFrom, TTo> : MemoryManager64<TTo>
		where TFrom : unmanaged
		where TTo : unmanaged
	{
		private readonly Memory64<TFrom> _from;

		public CastMemoryManager64(Memory64<TFrom> from) => _from = from;

		public override Span64<TTo> GetSpan()
			=> MemoryMarshal64.Cast<TFrom, TTo>(_from.Span);

		protected override void Dispose(bool disposing) { }
		public override MemoryHandle Pin(long elementIndex = 0)
			=> throw new NotImplementedException();
		public override void Unpin()
			=> throw new NotImplementedException();
	}
}
