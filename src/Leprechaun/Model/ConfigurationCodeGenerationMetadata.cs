using System.Collections.Generic;
using System.Diagnostics;
using Configy.Containers;
using Leprechaun.Filters;

namespace Leprechaun.Model
{
	[DebuggerDisplay("{Configuration.Name}")]
	public class ConfigurationCodeGenerationMetadata
	{
		public ConfigurationCodeGenerationMetadata(IContainer configuration, IReadOnlyCollection<TemplateCodeGenerationMetadata> metadata, IReadOnlyCollection<ItemCodeGenerationMetadata> itemMetadata)
		{
			Configuration = configuration;
			Metadata = metadata;
			ItemMetadata = itemMetadata;
		}

		public IContainer Configuration { get; }

		public string RootNamespace => Configuration.Resolve<IFilterPredicate<TemplateInfo>>().GetRootNamespace(null);

		public IReadOnlyCollection<TemplateCodeGenerationMetadata> Metadata { get; }

		public IReadOnlyCollection<ItemCodeGenerationMetadata> ItemMetadata { get; }
	}
}
