using System;
using System.Xml;
using Configy.Containers;
using Leprechaun.Model;

namespace Leprechaun.Filters
{
	public class StandardItemPredicate : StandardPredicate<ItemInfo>
	{
		public StandardItemPredicate(XmlNode configNode, IContainer configuration, string rootNamespace) : base(configNode, configuration, rootNamespace)
		{
		}
	}
}
