using System;
using System.Collections.Generic;

namespace Leprechaun.Model
{
	public class ItemInfo : ItemInfoBase
	{
		public Guid ParentId { get; set; }

		public Guid TemplateId { get; set; }
	}
}
