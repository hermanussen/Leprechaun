﻿using System.IO;
using System.Linq;
using System.Xml;
using Configy;
using Configy.Containers;
using Configy.Parsing;
using Leprechaun.CodeGen;
using Leprechaun.Filters;
using Leprechaun.Logging;
using Leprechaun.MetadataGeneration;
using Leprechaun.Model;
using Leprechaun.TemplateReaders;
using Leprechaun.Validation;
using Rainbow.Storage;
using Sitecore.Diagnostics;

namespace Leprechaun
{
	public class LeprechaunConfigurationBuilder : XmlContainerBuilder
	{
		private readonly XmlElement _configsElement;
		private readonly XmlElement _baseConfigElement;
		private readonly XmlElement _sharedConfigElement;
		private readonly string _configFilePath;
		private readonly ConfigurationImportPathResolver _configImportResolver;

		private IContainer _sharedConfig;
		private IContainer[] _configurations;

		public LeprechaunConfigurationBuilder(IContainerDefinitionVariablesReplacer variablesReplacer, XmlElement configsElement, XmlElement baseConfigElement, XmlElement sharedConfigElement, string configFilePath, ConfigurationImportPathResolver configImportResolver) : base(variablesReplacer)
		{
			Assert.ArgumentNotNull(variablesReplacer, nameof(variablesReplacer));
			Assert.ArgumentNotNull(configsElement, nameof(configsElement));
			Assert.ArgumentNotNull(baseConfigElement, nameof(baseConfigElement));
			Assert.ArgumentNotNull(sharedConfigElement, nameof(sharedConfigElement));

			_configsElement = configsElement;
			_baseConfigElement = baseConfigElement;
			_sharedConfigElement = sharedConfigElement;
			_configFilePath = configFilePath;
			_configImportResolver = configImportResolver;

			ProcessImports();
		}

		public virtual IContainer Shared
		{
			get
			{
				if (_sharedConfig == null) LoadSharedConfiguration();
				return _sharedConfig;
			}
		}

		public virtual IContainer[] Configurations
		{
			get
			{
				if (_configurations == null) LoadConfigurations();
				return _configurations;
			}
		}

		public string[] ImportedConfigFilePaths { get; protected set; }

		protected virtual void LoadConfigurations()
		{
			var parser = new XmlContainerParser(_configsElement, _baseConfigElement, new XmlInheritanceEngine());

			var definitions = parser.GetContainers();

			var configurations = GetContainers(definitions).ToArray();

			foreach (var configuration in configurations)
			{
				// Assert that expected dependencies exist - and in the case of data stores are specifically singletons (WEIRD things happen otherwise)
				configuration.AssertSingleton(typeof(IDataStore));
				configuration.AssertSingleton(typeof(IFieldFilter));
				configuration.AssertSingleton(typeof(IFilterPredicate<TemplateInfo>));
				configuration.AssertSingleton(typeof(ITypeNameGenerator));
				configuration.AssertSingleton(typeof(ITemplateReader));
				configuration.Assert(typeof(ICodeGenerator));

				// register the container with itself. how meta!
				configuration.Register(typeof(IContainer), () => configuration, true);
			}

			_configurations = configurations.ToArray();
		}

		protected virtual void LoadSharedConfiguration()
		{
			var definition = new ContainerDefinition(_sharedConfigElement);

			var sharedConfiguration = GetContainer(definition);

			// Assert that expected dependencies exist - and in the case of data stores are specifically singletons (WEIRD things happen otherwise)
			sharedConfiguration.AssertSingleton(typeof(ITemplateMetadataGenerator));
			sharedConfiguration.AssertSingleton(typeof(IArchitectureValidator));
			sharedConfiguration.Assert(typeof(ILogger));

			_sharedConfig = sharedConfiguration;
		}

		protected virtual void ProcessImports()
		{
			var imports = _configsElement.Attributes["import"]?.InnerText;

			if (imports == null) return;

			var allImportsGlobs = imports.Split(';');

			var allImportsRepathedGlobs = allImportsGlobs.Select(glob =>
			{
				// fix issues if "; " is used as a separator
				glob = glob.Trim();

				// absolute path with drive letter, so use the path raw
				if (glob[0] == ':') return glob;

				// relative path (absolutize with root config file path as base)
				return Path.Combine(Path.GetDirectoryName(_configFilePath), glob);
			});

			var allImportsFiles = allImportsRepathedGlobs
				.SelectMany(glob => _configImportResolver.ResolveImportPaths(glob))
				.Concat(new [] { _configFilePath })
				.ToArray();
			
			foreach (var import in allImportsFiles)
			{
				var xml = new XmlDocument();
				xml.Load(import);

				var importedXml = _baseConfigElement.OwnerDocument.ImportNode(xml.DocumentElement, true);

				_configsElement.AppendChild(importedXml);
			}

			// we'll use this to watch imports for changes later
			ImportedConfigFilePaths = allImportsFiles;
		}
	}
}
