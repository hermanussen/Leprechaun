using System;
using System.Globalization;

namespace Leprechaun.Model
{
	public class FieldValue
	{
		public Guid FieldId { get; set; }

		public string FieldName { get; set; }

		public string FieldType { get; set; }

		public string RawValue { get; set; }

		public CultureInfo Language { get; set; }

		public int? Version { get; set; }

		public bool IsShared => IsUnversioned && Language == null;

		public bool IsUnversioned => !Version.HasValue;

		public bool IsLatestVersion { get; set; }
	}
}
