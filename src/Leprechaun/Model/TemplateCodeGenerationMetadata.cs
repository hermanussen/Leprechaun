using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Leprechaun.Model
{
	[DebuggerDisplay("{FullTypeName} ({Id})")]
	public class TemplateCodeGenerationMetadata : CodeGenerationMetadataBase
	{
		public virtual TemplateInfo TemplateInfo { get; }

		public override string Path => TemplateInfo.Path;

		public override Guid Id => TemplateInfo.Id;

		public override string Name => TemplateInfo.Name;

		public TemplateCodeGenerationMetadata(TemplateInfo templateInfo, string fullTypeName, string rootNamespace, IEnumerable<TemplateFieldCodeGenerationMetadata> ownFields)
		{
			TemplateInfo = templateInfo;
			RootNamespace = rootNamespace;
			_fullTypeName = fullTypeName;
			OwnFields = ownFields.ToArray();
		}

		public virtual string HelpText => string.IsNullOrWhiteSpace(TemplateInfo.HelpText) ? $"Represents the {TemplateInfo.Name} field ({Id})." : TemplateInfo.HelpText;

		/// <summary>
		/// The template's fields that should get passed to code generation
		/// </summary>
		public virtual TemplateFieldCodeGenerationMetadata[] OwnFields { get; }

		/// <summary>
		/// All known immediate templates implemented by this type (transitive inheritance is not included eg a -> b -> c, will have b but not c for a)
		/// </summary>
		public virtual IList<TemplateCodeGenerationMetadata> BaseTemplates { get; } = new List<TemplateCodeGenerationMetadata>();

		/// <summary>
		/// All fields that make up this template, including all base templates' fields
		/// </summary>
		public virtual IEnumerable<TemplateFieldCodeGenerationMetadata> AllFields
		{
			get
			{
				var templates = new Queue<TemplateCodeGenerationMetadata>();
				templates.Enqueue(this);

				var knownFields = new HashSet<Guid>();

				while (templates.Count > 0)
				{
					var current = templates.Dequeue();
					foreach (var field in current.OwnFields)
					{
						if(knownFields.Contains(field.Id)) continue;

						knownFields.Add(field.Id);

						yield return field;
					}

					foreach (var baseTemplate in current.BaseTemplates)
					{
						templates.Enqueue(baseTemplate);
					}
				}
			}
		}
	}
}
