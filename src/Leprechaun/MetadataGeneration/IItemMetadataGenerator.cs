using System.Collections.Generic;
using Leprechaun.Model;

namespace Leprechaun.MetadataGeneration
{
	public interface IItemMetadataGenerator
	{
	}

	public interface IItemMetadataGenerator<T> : IItemMetadataGenerator where T : ItemInfoBase
	{
		IReadOnlyList<ConfigurationCodeGenerationMetadata> Generate(params ItemConfiguration<T>[] configurations);
	}
}