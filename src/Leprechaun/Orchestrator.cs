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
		private readonly IItemMetadataGenerator _metadataGenerator;
		private readonly IArchitectureValidator _architectureValidator;

		public Orchestrator(IItemMetadataGenerator metadataGenerator, IArchitectureValidator architectureValidator)
		{
			_metadataGenerator = metadataGenerator;
			_architectureValidator = architectureValidator;
		}

		public virtual IReadOnlyList<ConfigurationCodeGenerationMetadata> GenerateMetadata(params IContainer[] configurations)
		{
			if (_metadataGenerator is IItemMetadataGenerator<TemplateInfo> metadataGenerator)
			{
				var templates = GetAllItems<TemplateInfo>(configurations);

				FilterIgnoredFields(templates);

				var metadata = metadataGenerator.Generate(templates);

				var allTemplatesMetadata = metadata.SelectMany(config => config.Metadata).ToArray();

				_architectureValidator.Validate(allTemplatesMetadata);

				return metadata;
			}

			return null;
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
