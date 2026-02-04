#region License
/*
 * Copyright (C) 2026 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;

namespace Smx.SharpIO.Memory.Buffers;

// Interface for ownership of 64-bit memory blocks
public interface IMemoryOwner64<T> : IDisposable
{
    Memory64<T> Memory { get; }
}