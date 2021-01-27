using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Wrapper.Models
{
	[BsonIgnoreExtraElements]
	internal sealed class KeyValueEntity
	{
		public string Key { get; set; }

		public string Value { get; set; }
	}
}
