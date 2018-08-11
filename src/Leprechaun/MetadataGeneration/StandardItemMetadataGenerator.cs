using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leprechaun.Filters;
using Leprechaun.Model;

namespace Leprechaun.MetadataGeneration
{
	public class StandardItemMetadataGenerator : IItemMetadataGenerator<ItemInfo>
	{
		public IReadOnlyList<ConfigurationCodeGenerationMetadata> Generate(params ItemConfiguration<ItemInfo>[] configurations)
		{
			var results = new List<ConfigurationCodeGenerationMetadata>(configurations.Length);

			foreach (var configuration in configurations)
			{
				var nameGenerator = configuration.Configuration.Resolve<ITypeNameGenerator>();
				var predicate = configuration.Configuration.Resolve<IFilterPredicate<ItemInfo>>();

				var items = configuration.Items
					.Where(item => predicate.Includes(item))
					.Select(item => CreateItem(nameGenerator, predicate, item))
					.OrderBy(item => item.Name, StringComparer.Ordinal)
					.ToArray();

				results.Add(new ConfigurationCodeGenerationMetadata(configuration.Configuration, null, items));
			}

			results.Sort((a, b) => string.Compare(a.Configuration.Name, b.Configuration.Name, StringComparison.Ordinal));

			return results;
		}
		
		protected virtual ItemCodeGenerationMetadata CreateItem(ITypeNameGenerator nameGenerator, IFilterPredicate<ItemInfo> predicate, ItemInfo item)
		{
			var fullName = nameGenerator.GetFullTypeName(item.Path);
			
			return new ItemCodeGenerationMetadata(item, fullName, predicate.GetRootNamespace(item));
		}
	}
}
