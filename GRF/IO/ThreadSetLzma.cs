﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GRF.Core;
using GRF.Threading;

namespace GRF.IO {
	/// <summary>
	/// This class extracts and copy the files from the GRF from a given range
	/// It's used to optimize the data transfer.
	/// </summary>
	public class ThreadSetCustomCompression : GrfWriterThread<FileEntry> {
		private const int _bufferSize = 8388608;
		private readonly StreamReadBlockInfo _srb = new StreamReadBlockInfo(_bufferSize);

		public override void Start() {
			new Thread(_start) {Name = "GRF - Set custom compression flag thread " + StartIndex}.Start();
		}

		private void _start() {
			try {
				long offset;

				using (var originalStream = _grfData.GetSourceStream()) {
					byte[] data;
					int toIndex = 0;
					int fromIndex;
					FileEntry entry;

					if (IsPaused)
						Pause();

					List<FileEntry> sortedEntries = _entries.Skip(StartIndex).Take(EndIndex - StartIndex).ToList();

					int indexMax = sortedEntries.Count;

					while (toIndex < indexMax) {
						fromIndex = toIndex;

						data = _srb.ReadMisaligned(sortedEntries, out toIndex, fromIndex, indexMax, originalStream.Value);

						for (int i = fromIndex; i < toIndex; i++) {
							if (_grfData.IsCancelling)
								return;

							if (IsPaused)
								Pause();

							entry = sortedEntries[i];

							offset = entry.TemporaryOffset + 1;

							if (entry.SizeCompressed >= 2) {
								if (data[offset - 1] == 0) {
									entry.Flags |= EntryType.CustomCompressed;
									entry.OnPropertyChanged("CustomCompressed");
								}
							}

							NumberOfFilesProcessed++;
						}
					}
				}
			}
			catch (Exception err) {
				Exception = err;
				Error = true;
			}
			finally {
				Terminated = true;
			}
		}
	}
}