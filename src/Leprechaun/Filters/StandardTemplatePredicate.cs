using System;
using System.Xml;
using Configy.Containers;
using Leprechaun.Model;

namespace Leprechaun.Filters
{
	public class StandardTemplatePredicate : StandardPredicate<TemplateInfo>
	{
		public StandardTemplatePredicate(XmlNode configNode, IContainer configuration, string rootNamespace) : base(configNode, configuration, rootNamespace)
		{
		}
	}
}
