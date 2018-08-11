using System.Collections.Generic;
using System.Linq;
using Configy.Containers;
using Leprechaun.Filters;
using Leprechaun.MetadataGeneration;
using Leprechaun.Model;
using Leprechaun.TemplateReaders;
using Leprechaun.Validation;
using Sitecore.Diagnostics;

namespace Leprechaun
{
	public class Orchestrator
	{
		private readonly IArchitectureValidator _architectureValidator;

		public Orchestrator(IArchitectureValidator architectureValidator)
		{
			_architectureValidator = architectureValidator;
		}

		public virtual IReadOnlyList<ConfigurationCodeGenerationMetadata> GenerateMetadata(params IContainer[] configurations)
		{
			var metadata = new List<ConfigurationCodeGenerationMetadata>();

			foreach (var configuration in configurations)
			{
				var generator = configuration.Resolve<IItemMetadataGenerator>();

				if (generator is IItemMetadataGenerator<TemplateInfo> templateGenerator)
				{
					var templates = GetAllItems<TemplateInfo>(new [] { configuration });

					FilterIgnoredFields(templates);

					metadata.AddRange(templateGenerator.Generate(templates));
				}
				else if (generator is IItemMetadataGenerator<ItemInfo> itemGenerator)
				{
					var items = GetAllItems<ItemInfo>(new[] { configuration });

					FilterIgnoredFields(items);

					metadata.AddRange(itemGenerator.Generate(items));
				}
			}

			var allTemplatesMetadata = metadata.Where(config => config.Metadata != null).SelectMany(config => config.Metadata).ToArray();
			_architectureValidator.Validate(allTemplatesMetadata);

			var allItemsMetadata = metadata.Where(config => config.ItemMetadata != null).SelectMany(config => config.ItemMetadata).ToArray();
			//_architectureValidator.Validate(allItemsMetadata);

			return metadata;
		}

		protected virtual ItemConfiguration<T>[] GetAllItems<T>(IEnumerable<IContainer> configurations)
			where T :  ItemInfoBase
		{
			var results = new List<ItemConfiguration<T>>();

			foreach (var config in configurations)
			{
				var processingConfig = new ItemConfiguration<T>(config);
				processingConfig.Items = GetItems<T>(config);
				results.Add(processingConfig);
			}

			return results.ToArray();
		}

		protected virtual IEnumerable<T> GetItems<T>(IContainer configuration)
			where T : ItemInfoBase
		{
			var itemReader = configuration.Resolve<IItemReader<T>>();
			var filterPredicate = configuration.Resolve<IFilterPredicate<T>>();

			Assert.IsNotNull(itemReader, "itemReader != null");
			Assert.IsNotNull(filterPredicate, "filterPredicate != null");

			var roots = filterPredicate.GetRootPaths();
			return itemReader.GetItems(roots);
		}

		protected virtual void FilterIgnoredFields<T>(IEnumerable<ItemConfiguration<T>> configurations)
			where T : ItemInfoBase
		{
			foreach (var configuration in configurations)
			{
				var filter = configuration.Configuration.Resolve<IFieldFilter>();

				Assert.IsNotNull(filter, "filter != null");

				foreach (var template in configuration.Items.OfType<TemplateInfo>())
				{
					template.OwnFields = template.OwnFields.Where(field => filter.Includes(field)).ToArray();
				}
			}
		}
	}
}
