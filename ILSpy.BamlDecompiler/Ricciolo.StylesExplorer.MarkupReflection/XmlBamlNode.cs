// Copyright (c) Cristian Civera (cristian@aspitalia.com)
// This code is distributed under the MS-PL (for details please see \doc\MS-PL.txt)

using System.Collections.Generic;
using System.Xml;

namespace Ricciolo.StylesExplorer.MarkupReflection
{
	internal class XmlBamlNode
	{
		public virtual XmlNodeType NodeType => XmlNodeType.None;
	}

	internal class XmlBamlNodeCollection : List<XmlBamlNode>
	{}
}
