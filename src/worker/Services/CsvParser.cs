using System;
using System.Collections.Generic;

namespace Covid19id.Services {
	public static class CsvParser {
		private enum ParserState {
			InStartingWhiteSpace,
			InUnquotedValue,
			InQuotedValue,
			InEscapeSequence,
			InTrailingWhiteSpace
		}

		public static List<string> Split(string csv) {
			ReadOnlySpan<char> span = csv.AsSpan();
			List<string> values = new List<string>();
			int startOfLiteral = 0;
			int endOfLiteral = 0;
			ParserState state = ParserState.InStartingWhiteSpace;
			for (int i = 0, length = csv.Length; i <= length; i++) {
				if (i == length) {
					switch (state) {
						case ParserState.InStartingWhiteSpace:
						case ParserState.InUnquotedValue:
						case ParserState.InEscapeSequence:
							values.Add(span.Slice(startOfLiteral, i - startOfLiteral).ToString());
							return values;
						case ParserState.InQuotedValue:
							throw new FormatException("End of file in quoted literal.");
						case ParserState.InTrailingWhiteSpace:
							values.Add(span.Slice(startOfLiteral, endOfLiteral - startOfLiteral + 1).ToString());
							return values;
					}
				} else {
					switch (span[i]) {
						case '"':
							switch (state) {
								case ParserState.InStartingWhiteSpace:
									startOfLiteral = i;
									state = ParserState.InQuotedValue;
									break;
								case ParserState.InUnquotedValue:
									int endOfLine = span.IndexOf('\n');
									string line = endOfLine == -1 ? csv : span.Slice(0, endOfLine).ToString();
									throw new FormatException($"Invalid character at position {i}: \"");
								case ParserState.InQuotedValue:
									state = ParserState.InEscapeSequence;
									break;
								case ParserState.InEscapeSequence:
									state = ParserState.InQuotedValue;
									break;
								case ParserState.InTrailingWhiteSpace:
									endOfLine = span.IndexOf('\n');
									line = endOfLine == -1 ? csv : span.Slice(0, endOfLine).ToString();
									throw new FormatException($"Invalid character at position {i}: \"");
							}
							break;
						case char c when c == ',':
							switch (state) {
								case ParserState.InStartingWhiteSpace:
								case ParserState.InUnquotedValue:
								case ParserState.InEscapeSequence:
									values.Add(span.Slice(startOfLiteral, i - startOfLiteral).ToString());
									startOfLiteral = i + 1;
									state = ParserState.InStartingWhiteSpace;
									break;
								case ParserState.InTrailingWhiteSpace:
									values.Add(span.Slice(startOfLiteral, endOfLiteral - startOfLiteral + 1).ToString());
									startOfLiteral = i + 1;
									state = ParserState.InStartingWhiteSpace;
									break;
							}
							break;
						case char c:
							switch (state) {
								case ParserState.InStartingWhiteSpace:
									state = ParserState.InUnquotedValue;
									break;
								case ParserState.InEscapeSequence:
									endOfLiteral = i - 1;
									state = ParserState.InTrailingWhiteSpace;
									break;
								case ParserState.InTrailingWhiteSpace:
									if (!char.IsWhiteSpace(c)) {
										int endOfLine = span.IndexOf('\n');
										string line = endOfLine == -1 ? csv : span.Slice(0, endOfLine).ToString();
										throw new FormatException($"Invalid character at position {i}: {c}");
									}
									break;
							}
							break;
					}
				}
			}
			throw new InvalidOperationException("Parser internal error.");
		}
	}
}
