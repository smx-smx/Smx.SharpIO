#region License
/*
 * Copyright (C) 2021 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using NUnit.Framework;
using Smx.SharpIO;
using System;
using System.IO;

namespace SharpIO.Tests
{
	public class Tests
	{
		[SetUp]
		public void Setup() {
		}

		[Test]
		public void TestExtend() {
			var filePath = Path.Combine(Path.GetTempPath(), "mfile.bin");
			using (var mf = MFile.Open(filePath,
				FileMode.OpenOrCreate,
				FileAccess.ReadWrite,
				FileShare.ReadWrite))
			{
				mf.SetLength(10);
			}

			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				Assert.AreEqual(10, fs.Length);
			}
		}
	}
}
