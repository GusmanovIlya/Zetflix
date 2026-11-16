using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Zetflix.Models;

public class AuditEvent
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public DateTimeOffset Time { get; set; }
    public string User { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
