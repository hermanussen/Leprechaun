using System.Collections.Generic;
using Configy.Containers;

namespace Leprechaun.Model
{
	public class ItemConfiguration<T> where T : ItemInfoBase
	{
		public ItemConfiguration(IContainer configuration)
		{
			Configuration = configuration;
		}

		public IContainer Configuration { get; }

		public IEnumerable<T> Items { get; set; }
	}
}
