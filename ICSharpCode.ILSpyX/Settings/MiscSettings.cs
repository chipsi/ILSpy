﻿// Copyright (c) 2022 AlphaSierraPapa for the SharpDevelop Team
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
using System.Xml.Linq;

namespace ICSharpCode.ILSpyX.Settings
{
	public class MiscSettings : IMiscSettings, ISettingsSection<MiscSettings>
	{
		private MiscSettings()
		{
		}

		public bool AllowMultipleInstances { get; set; }
		public bool LoadPreviousAssemblies { get; set; }

		public bool EnableExplainThisCode { get; set; }
		public string OpenAIApiKey { get; set; } = "";

		public static MiscSettings Load(ISettingsProvider settingsProvider)
		{
			XElement e = settingsProvider["MiscSettings"];
			var s = new MiscSettings();
			s.AllowMultipleInstances = (bool?)e.Attribute(nameof(s.AllowMultipleInstances)) ?? false;
			s.LoadPreviousAssemblies = (bool?)e.Attribute(nameof(s.LoadPreviousAssemblies)) ?? true;
			s.EnableExplainThisCode = (bool?)e.Attribute(nameof(s.EnableExplainThisCode)) ?? false;
			s.OpenAIApiKey = (string?)e.Attribute(nameof(s.OpenAIApiKey)) ?? "";

			return s;
		}
	}
}
