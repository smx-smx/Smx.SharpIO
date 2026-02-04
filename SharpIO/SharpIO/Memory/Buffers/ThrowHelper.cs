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

internal static class ThrowHelper
{
    public static void ThrowIndexOutOfRangeException() => throw new IndexOutOfRangeException();
    public static void ThrowArgumentOutOfRangeException() => throw new ArgumentOutOfRangeException();
    public static void ThrowArgumentException_DestTooShort() => throw new ArgumentException("Destination is too short.");
}
