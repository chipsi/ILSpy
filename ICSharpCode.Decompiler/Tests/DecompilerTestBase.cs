﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp;
using Mono.Cecil;
using NUnit.Framework;

namespace ICSharpCode.Decompiler.Tests
{
	public abstract class DecompilerTestBase
	{
		protected static void ValidateFileRoundtrip(string samplesFileName)
		{
			var fullPath = Path.Combine(@"..\..\Tests", samplesFileName);
			AssertRoundtripCode(fullPath, useRoslyn: false);
			AssertRoundtripCode(fullPath, useRoslyn: true);
		}

		static string RemoveIgnorableLines(IEnumerable<string> lines)
		{
			return CodeSampleFileParser.ConcatLines(lines.Where(l => !CodeSampleFileParser.IsCommentOrBlank(l)));
		}

		protected static void AssertRoundtripCode(string fileName, bool useRoslyn = false, bool optimize = false, bool useDebug = false)
		{
			var code = RemoveIgnorableLines(File.ReadLines(fileName));
			AssemblyDefinition assembly = useRoslyn ? CompileRoslyn(code, optimize, useDebug) : CompileLegacy(code, optimize, useDebug);

			AstBuilder decompiler = new AstBuilder(new DecompilerContext(assembly.MainModule));
			decompiler.AddAssembly(assembly);
			new Helpers.RemoveCompilerAttribute().Run(decompiler.SyntaxTree);

			StringWriter output = new StringWriter();
			decompiler.GenerateCode(new PlainTextOutput(output));
			CodeAssert.AreEqual(code, output.ToString(),
				"Round-trip failed (" 
			  + (useRoslyn ? "Roslyn" : "Legacy") + ", " 
			  + (optimize ? "Optimized, " : "") 
			  + (useDebug ? "with PDB" : "no PDB")
			  + ")");
		}

		protected static AssemblyDefinition CompileLegacy(string code, bool optimize, bool useDebug)
		{
			CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });
			CompilerParameters options = new CompilerParameters();
			options.CompilerOptions = "/unsafe /o" + (optimize ? "+" : "-") + (useDebug ? " /debug" : "");
			options.ReferencedAssemblies.Add("System.Core.dll");
			CompilerResults results = provider.CompileAssemblyFromSource(options, code);
			try
			{
				if (results.Errors.Count > 0)
				{
					StringBuilder b = new StringBuilder("Compiler error:");
					foreach (var error in results.Errors)
					{
						b.AppendLine(error.ToString());
					}
					throw new Exception(b.ToString());
				}
				return AssemblyDefinition.ReadAssembly(results.PathToAssembly);
			}
			finally
			{
				File.Delete(results.PathToAssembly);
				results.TempFiles.Delete();
			}
		}
		protected static AssemblyDefinition CompileRoslyn(string code, bool optimize, bool useDebug)
		{
			var compilation = CSharpCompilation.Create(
				"RoslynAssembly-" + Guid.NewGuid(),
				new[] { CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(LanguageVersion.CSharp5)) },
				new[] { typeof(object).Assembly, typeof(Enumerable).Assembly }
					.Select(a => new MetadataFileReference(a.Location, MetadataReferenceProperties.Assembly)),
				new CSharpCompilationOptions(
					OutputKind.DynamicallyLinkedLibrary,
					optimizationLevel: optimize ? OptimizationLevel.Release : OptimizationLevel.Debug,
					allowUnsafe: true
				)
			);

			var peStream = new MemoryStream();
			var pdbStream = useDebug ? new MemoryStream() : null;

			var emitResult = compilation.Emit(peStream, pdbStream: pdbStream);
			if (!emitResult.Success)
				throw new ApplicationException(string.Join(Environment.NewLine, emitResult.Diagnostics));
			peStream.Position = 0;
			if (useDebug)
				pdbStream.Position = 0;
			return AssemblyDefinition.ReadAssembly(
				peStream,
				new ReaderParameters { ReadSymbols = useDebug, SymbolStream = pdbStream }
			);
		}
	}
}
