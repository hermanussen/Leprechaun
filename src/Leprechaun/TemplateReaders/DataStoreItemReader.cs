using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leprechaun.Model;
using Rainbow.Model;
using Rainbow.Storage;

namespace Leprechaun.TemplateReaders
{
	public class DataStoreItemReader : IItemReader<ItemInfo>
	{
		private readonly IDataStore _dataStore;

		public DataStoreItemReader(IDataStore dataStore)
		{
			_dataStore = dataStore;
		}

		public ItemInfo[] GetItems(params TreeRoot[] rootPaths)
		{
			return rootPaths
				.AsParallel()
				.SelectMany(root =>
				{
					var rootItem = _dataStore.GetByPath(root.Path, root.DatabaseName);

					if (rootItem == null) return Enumerable.Empty<ItemInfo>();

					// because a path could match more than one item we have to SelectMany again
					return rootItem.SelectMany(ParseItems);
				})
				.ToArray();
		}

		protected virtual IEnumerable<ItemInfo> ParseItems(IItemData root)
		{
			var processQueue = new Queue<IItemData>();

			processQueue.Enqueue(root);

			while (processQueue.Count > 0)
			{
				var currentItem = processQueue.Dequeue();
				
				yield return ParseItem(currentItem);

				var children = currentItem.GetChildren();
				foreach (var child in children)
				{
					processQueue.Enqueue(child);
				}
			}
		}

		protected virtual ItemInfo ParseItem(IItemData item)
		{
			if (item == null) throw new ArgumentException("Item passed to parse was null", nameof(item));

			var result = new ItemInfo
			{
				Id = item.Id,
				Name = item.Name,
				Path = item.Path,
				ParentId = item.ParentId,
				TemplateId = item.TemplateId
			};

			result.FieldValues.AddRange(
				item.SharedFields.Select(f => new FieldValue()
				{
					FieldId = f.FieldId,
					FieldName = f.NameHint,
					FieldType = f.FieldType,
					RawValue = f.Value
				}));

			result.FieldValues.AddRange(
				item.UnversionedFields.SelectMany(l => l.Fields.Select(f => new FieldValue()
				{
					Language = l.Language,
					FieldId = f.FieldId,
					FieldName = f.NameHint,
					FieldType = f.FieldType,
					RawValue = f.Value
				})));

			result.FieldValues.AddRange(
				item.Versions.SelectMany(v => v.Fields.Select(f => new FieldValue()
				{
					Version = v.VersionNumber,
					Language = v.Language,
					FieldId = f.FieldId,
					FieldName = f.NameHint,
					FieldType = f.FieldType,
					RawValue = f.Value
				})));

			var maxVersions = result.FieldValues
				.Where(f => f.Language != null && f.Version.HasValue)
				.GroupBy(f => f.Language)
				.Select(f => new { Language = f.Key, MaxVersion = f.Max(x => x.Version) });
			foreach (var fieldValue in result.FieldValues.Where(f => maxVersions.Select(m => m.Language).Contains(f.Language) && f.Version.HasValue && f.Version == maxVersions.FirstOrDefault(m => m.Language == f.Language)?.MaxVersion))
			{
				fieldValue.IsLatestVersion = true;
			}

			return result;
		}
	}
}
