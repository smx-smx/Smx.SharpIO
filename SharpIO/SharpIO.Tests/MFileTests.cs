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
using System.Text;

namespace SharpIO.Tests
{
	public class Tests
	{
		[SetUp]
		public void Setup() {
		}

		private string GetFilePath(string fileName) {
			return Path.Combine(Path.GetTempPath(), fileName);
		}

		private MFile OpenMFile(string fileName, bool writable = false) {
			return MFile.Open(
				GetFilePath(fileName), 
				FileMode.OpenOrCreate, 
				(writable) ? FileAccess.ReadWrite : FileAccess.Read,
				FileShare.Read);
		}

		[Test]
		public void TestExtend() {
			var fileName = "mfile.bin";

			using (var mf = OpenMFile(fileName, writable: true)) {
				mf.SetLength(10);
			}

			using (var fs = new FileStream(
				GetFilePath(fileName),
				FileMode.Open, FileAccess.Read, FileShare.Read)
			) {
				Assert.AreEqual(10, fs.Length);
			}
		}

		[Test]
		public void TestEndianness() {
			byte[] buf = new byte[48];
			var sst = new SpanStream(buf);
			sst.Endianness = Endianness.BigEndian;
			{
				sst.WriteUInt32(0xDEADBEEF);
			}
			sst.Seek(0, SeekOrigin.Begin);
			sst.Endianness = Endianness.LittleEndian;
			{
				Assert.AreEqual(0xEFBEADDE, sst.ReadUInt32());
			}
		}

		[Test]
		public void TestSpanStream() {
			var filePath = Path.Combine(Path.GetTempPath(), "mfile.bin");
			using (var mf = OpenMFile(filePath, writable: true)) {
				mf.SetLength(64);
				var sst = new SpanStream(mf);
				sst.PerformAt(60, () => {
					sst.WriteString("FOOT");
				});
				sst.Seek(4, SeekOrigin.Current);
				sst.WriteUInt32(0xDEADBEEF);
				sst.WriteUInt16(0xC0FF);
				sst.WriteByte(0xF0);
				sst.WriteCString("test");

				for(byte i=0; i<10; i++) {
					sst.WriteByte(i);
				}

				sst.Write(new byte[] { 0xfa, 0xfe }, 0, 2);

				sst.PerformAt(0, () => {
					sst.WriteString("HEAD");
				});

				sst.Seek(0, SeekOrigin.Begin);
				Assert.AreEqual("HEAD", sst.ReadString(4));
				Assert.AreEqual(0xDEADBEEF, sst.ReadUInt32());
				Assert.AreEqual(0xC0FF, sst.ReadUInt16());
				Assert.AreEqual(0xF0, sst.ReadByte());
				Assert.AreEqual("test", sst.ReadCString());
				for (byte i = 0; i < 10; i++) {
					Assert.AreEqual(i, sst.ReadByte());
				}

				var bufRead = new byte[2];
				sst.Read(bufRead, 0, 2);
				Assert.AreEqual(0xfa, bufRead[0]);
				Assert.AreEqual(0xfe, bufRead[1]);

				Assert.AreEqual("FOOT", sst.PerformAt(60, () => {
					return sst.ReadString(4);
				}));
			}
		}
	}
}
