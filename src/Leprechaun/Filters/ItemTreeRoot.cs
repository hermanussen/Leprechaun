using System.Collections.Generic;
using Rainbow.Storage;

namespace Leprechaun.Filters
{
	public class ItemTreeRoot : TreeRoot
	{
		public ItemTreeRoot(string name, string path) : base(name, path, "master")
		{
			Exclusions = new List<IPresetTreeExclusion>();
		}

		public IList<IPresetTreeExclusion> Exclusions { get; set; }
	}
}
