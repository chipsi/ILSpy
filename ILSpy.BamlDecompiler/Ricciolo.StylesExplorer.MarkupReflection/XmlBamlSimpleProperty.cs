﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code is distributed under the MS-PL (for details please see \doc\MS-PL.txt)

using System;
using System.Xml;

namespace Ricciolo.StylesExplorer.MarkupReflection
{
	class XmlBamlSimpleProperty : XmlBamlNode
	{
		public string NamespaceName { get; private set; }
		public string LocalName { get; private set; }
		public string Value { get; private set; }
		
		public XmlBamlSimpleProperty(string namespaceName, string localName, string value)
		{
			if (string.IsNullOrWhiteSpace(namespaceName))
				throw new ArgumentException("namespaceName");
			if (string.IsNullOrWhiteSpace(localName))
				throw new ArgumentException("localName");
			this.NamespaceName = namespaceName;
			this.LocalName = localName;
			this.Value = value ?? throw new ArgumentNullException("value");
		}
		
		public override XmlNodeType NodeType => XmlNodeType.Attribute;

		public override string ToString()
		{
			return string.Format("{{{0}}}{1}=\"{2}\"", NamespaceName, LocalName, Value);
		}

	}
}
