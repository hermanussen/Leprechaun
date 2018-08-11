using System;
using System.Diagnostics;

namespace Leprechaun.Model
{
	[DebuggerDisplay("{FullTypeName} ({Id})")]
	public class ItemCodeGenerationMetadata : CodeGenerationMetadataBase
	{
		public virtual ItemInfo ItemInfo { get; }

		public override string Path => ItemInfo.Path;

		public override Guid Id => ItemInfo.Id;

		public override string Name => ItemInfo.Name;

		public ItemCodeGenerationMetadata(ItemInfo itemInfo, string fullTypeName, string rootNamespace)
		{
			ItemInfo = itemInfo;
			RootNamespace = rootNamespace;
			_fullTypeName = fullTypeName;
		}
	}
}
