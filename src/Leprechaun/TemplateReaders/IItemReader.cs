using Leprechaun.Model;
using Rainbow.Storage;

namespace Leprechaun.TemplateReaders
{
	public interface IItemReader<T> where T : ItemInfoBase
	{
		T[] GetItems(params TreeRoot[] rootPaths);
	}
}