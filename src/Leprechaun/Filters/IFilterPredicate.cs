using Leprechaun.Model;
using Rainbow.Storage;

namespace Leprechaun.Filters
{
	public interface IFilterPredicate<T> where T : ItemInfoBase
	{
		bool Includes(T item);

		TreeRoot[] GetRootPaths();

		string GetRootNamespace(T item);
	}
}
