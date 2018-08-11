using System;

namespace Leprechaun.Model
{
	public abstract class CodeGenerationMetadataBase
	{
		protected string _fullTypeName;

		public virtual string RootNamespace { get; protected set; }

		public abstract string Path { get; }

		public abstract Guid Id { get; }

		public abstract string Name { get; }

		/// <summary>
		/// A unique name for this template, usable as a name for a C# class. e.g. for "Foo Bar" this would be "FooBar"
		/// </summary>
		public virtual string CodeName => _fullTypeName.Contains(".") ? _fullTypeName.Substring(_fullTypeName.LastIndexOf('.') + 1) : _fullTypeName;

		/// <summary>
		/// Gets a namespace-formatted relative path from the root template path to this template
		/// e.g. if root is /Foo, and this template's path is /Foo/Bar/Baz/Quux, this would be "Bar.Baz"
		/// </summary>
		public virtual string RelativeNamespace => _fullTypeName.Contains(".") ? _fullTypeName.Substring(0, _fullTypeName.LastIndexOf('.')) : string.Empty;

		/// <summary>
		/// Gets the full namespace for the template (e.g. RootNamespace.RelativeNamespace)
		/// </summary>
		public virtual string Namespace
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(RelativeNamespace))
				{
					return $"{RootNamespace}.{RelativeNamespace}";
				}
				return RootNamespace;
			}
		}

		/// <summary>
		/// Gets the full type name, qualified by namespace, of this template
		/// e.g. RootNamespace.RelativeNamespace.CodeName
		/// </summary>
		public virtual string FullTypeName => $"{RootNamespace}.{_fullTypeName}";
	}
}