using System.Text.Json.Serialization;

namespace Todo.Api.Models
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum Priority
	{
		low,
		medium,
		high
	}
}