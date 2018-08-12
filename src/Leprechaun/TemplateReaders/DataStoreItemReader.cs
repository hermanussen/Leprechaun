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

			return result;
		}
	}
}
