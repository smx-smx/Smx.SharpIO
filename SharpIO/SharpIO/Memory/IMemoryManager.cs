#region License
/*
 * Copyright (C) 2024 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
namespace Smx.SharpIO.Memory
{
	public interface IMemoryManager {
		nint Alloc(nuint size);
		nint Realloc(nint ptr, nuint size);
		void Free(nint ptr);
	}
}
