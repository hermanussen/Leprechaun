using System.Collections.Generic;
using Rainbow.Storage;

namespace Leprechaun.Filters
{
	public class ItemTreeRoot : TreeRoot
	{
		public ItemTreeRoot(string name, string path, string database = "master") : base(name, path, database)
		{
			Exclusions = new List<IPresetTreeExclusion>();
		}

		public IList<IPresetTreeExclusion> Exclusions { get; set; }
	}
}
