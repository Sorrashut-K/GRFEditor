﻿using System.Collections.Generic;
using System.Text;
using GRF.FileFormats.LubFormat.Types;
using GRF.IO;

namespace GRF.FileFormats.LubFormat {
	public class Lub {
		public static string String_LoopPcBounds = "-- loop_pc_bounds: ";

		public int TotalFunctionCount = 0;
		private List<LubFunction> _functions = new List<LubFunction>();
		private LubDictionary _globalVariables = new LubDictionary(0, 0);

		public Lub(MultiType data) {
			IBinaryReader reader = data.GetBinaryReader();

			Header = new LubHeader(reader);

			_functions.Add(new LubFunction(0, reader, this));

			while (reader.CanRead) {
				_functions.Add(new LubFunction(1, reader, this));
			}
		}

		public string SourceName { get; set; }

		public LubHeader Header { get; set; }

		public List<LubFunction> Functions {
			get { return _functions; }
			set { _functions = value; }
		}

		public LubDictionary GlobalVariables {
			get { return _globalVariables; }
			set { _globalVariables = value; }
		}

		public int FunctionDecompiledCount { get; set; }

		public string Decompile() {
			FunctionDecompiledCount = -1;

			StringBuilder builder = new StringBuilder();
			_functions[0].Print(builder, 0);
			return builder.ToString();
		}

		public static string Escape(string str) {
			StringBuilder builder = new StringBuilder(str.Length);
			char c;

			for (int i = 0; i < str.Length; i++) {
				c = str[i];

				if (c <= 124) {
					switch(c) {
						case '\t':
							builder.Append(@"\t");
							break;
						case '\n':
							builder.Append(@"\n");
							break;
						case '\f':
							builder.Append(@"\f");
							break;
						case '\r':
							builder.Append(@"\r");
							break;
						case '\\':
							builder.Append(@"\\");
							break;
						case '\"':
							builder.Append("\\\"");
							break;
						default:
							builder.Append(c);
							break;
					}
				}
				else {
					builder.Append(c);
				}
			}
			return builder.ToString();
		}

		public static string EscapeIgnoreLineFeed(string str) {
			StringBuilder builder = new StringBuilder();
			char c;

			for (int i = 0; i < str.Length; i++) {
				c = str[i];

				if (c <= 124) {
					switch(c) {
						case '\t':
							builder.Append(@"\t");
							break;
						case '\f':
							builder.Append(@"\f");
							break;
						case '\\':
							builder.Append(@"\\");
							break;
						case '\"':
							builder.Append("\\\"");
							break;
						default:
							builder.Append(c);
							break;
					}
				}
				else {
					builder.Append(c);
				}
			}
			return builder.ToString();
		}

		public static bool ContainsEscapeChar(string str) {
			char c;

			for (int i = 0; i < str.Length; i++) {
				c = str[i];

				if (c <= 124) {
					switch(c) {
						case '\t':
						case '\f':
						case '\\':
						case '\"':
							return true;
					}
				}
			}
			return false;
		}
	}
}