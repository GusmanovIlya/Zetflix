namespace Zetflix.Models;

public class MongoAuditSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "zetflix_logs";
    public string CollectionName { get; set; } = "audit_events";
    public bool Enabled { get; set; } = true;
}
