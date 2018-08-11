using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Leprechaun.Model;
using Rainbow.Storage;
using Sitecore.Diagnostics;

namespace Leprechaun.Filters
{
	/// <summary>
	/// Eliminates the need for setting up which roots are needed in a configuration.
	/// Aggressively searches the filesystem and determines the roots for you (this may slightly impact performance).
	/// </summary>
	public class MatchAllItemPredicate : IFilterPredicate<ItemInfo>, ITreeRootFactory
	{
		private readonly string _rootNamespace;
		private readonly IList<ItemTreeRoot> _includeEntries;

		public MatchAllItemPredicate(IDataStore dataStore, string rootNamespace)
		{
			if (dataStore is SerializationFileSystemDataStore fileSystemDataStore)
			{
				_rootNamespace = rootNamespace;
				Assert.ArgumentNotNull(rootNamespace, nameof(rootNamespace));

				_includeEntries = FindTreeRoots(fileSystemDataStore.GetConfigurationDetails()[1].Value);
			}
			else
			{
				throw new InvalidOperationException($"The match all item predicate can only function if the data store that is used is {nameof(SerializationFileSystemDataStore)}");
			}
		}

		private IList<ItemTreeRoot> FindTreeRoots(string basePath)
		{
			List<ItemTreeRoot> result = new List<ItemTreeRoot>();

			ResolveTreeRoots(result, basePath);

			return result;
		}

		/// <summary>
		/// This method aggressively searches the path for the first .yml files it can find.
		/// It will then read the Sitecore path from that file and determines what the configuration should look like.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="path"></param>
		private void ResolveTreeRoots(List<ItemTreeRoot> result, string path)
		{
			var ymlFiles = Directory.GetFiles(path, "*.yml", SearchOption.TopDirectoryOnly);
			if (ymlFiles.Any())
			{
				var allLines = File.ReadAllLines(ymlFiles.First());
				string sitecorePath = allLines.FirstOrDefault(l => l.StartsWith("Path: "))?.Substring(6);
				string db = allLines.FirstOrDefault(l => l.StartsWith("DB: "))?.Substring(4);

				if (!string.IsNullOrWhiteSpace(sitecorePath) && !string.IsNullOrWhiteSpace(db))
				{
					result.Add(new ItemTreeRoot(new DirectoryInfo(path).Name, sitecorePath, db));
				}
			}
			else
			{
				foreach (string directory in Directory.GetDirectories(path))
				{
					ResolveTreeRoots(result, directory);
				}
			}
		}

		public bool Includes(ItemInfo item)
		{
			return true;
		}

		public TreeRoot[] GetRootPaths()
		{
			return _includeEntries.ToArray<TreeRoot>();
		}

		public string GetRootNamespace(ItemInfo item)
		{
			return _rootNamespace;
		}

		public IEnumerable<TreeRoot> CreateTreeRoots()
		{
			return _includeEntries;
		}
	}
}
