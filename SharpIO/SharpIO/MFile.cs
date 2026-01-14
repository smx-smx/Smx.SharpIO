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
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smx.SharpIO
{
	public class MFile : IDisposable
	{
		private readonly FileStream fs;

		private MemoryMappedFile? mmf;
		public MemoryMappedSpan<byte>? Span;

		private void RemapFile() {
			MemoryMappedFileAccess mmapFlags = MemoryMappedFileAccess.Read;
			if (fs.CanWrite) {
				mmapFlags = MemoryMappedFileAccess.ReadWrite;
			}

			// memory mapped files cannot be backed by an empty file
			if(fs.Length == 0) {
				fs.SetLength(1);
			}

			this.mmf = MemoryMappedFile.CreateFromFile(
				fs, null, 0,
				mmapFlags, HandleInheritability.Inheritable, true
			);
			this.Span = new MemoryMappedSpan<byte>(mmf, (int)fs.Length, mmapFlags);
		}

		public MFile(FileStream fs) {
			this.fs = fs;
			this.RemapFile();
		}

		public void SetLength(long newSize) {
			DisposeMFile();
			this.fs.SetLength(newSize);
			RemapFile();
		}

		public static MFile Open(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare) {
			FileStream fs = new FileStream(filePath, fileMode, fileAccess, fileShare);
			return new MFile(fs);
		}

		private void DisposeMFile() {
			Span?.Dispose();
			Span = null;

			mmf?.Dispose();
			mmf = null;
		}

		public void Dispose() {
			DisposeMFile();
			fs.Dispose();
		}
	}
}
