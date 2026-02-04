#region License
/*
 * Copyright (C) 2026 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System.Buffers;

namespace Smx.SharpIO.Memory.Buffers;

public interface IPinnable64 : IPinnable
{
    /// <summary>
    /// Pins the memory at the specified 64-bit element index.
    /// </summary>
    MemoryHandle Pin(long elementIndex);
}