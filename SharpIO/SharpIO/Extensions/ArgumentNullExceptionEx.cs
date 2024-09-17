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
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Smx.SharpIO.Extensions
{
	public static class ArgumentNullExceptionEx
	{
		/// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
		/// <param name="argument">The reference type argument to validate as non-null.</param>
		/// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
		public static void ThrowIfNull([NotNull] object? argument, string? paramName = null) {
			if (argument is null) {
				throw new ArgumentNullException(paramName);
			}
		}
	}
}
