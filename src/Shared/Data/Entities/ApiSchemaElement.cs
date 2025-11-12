namespace Apivia.Shared.Data.Entities;

/// <summary>
/// Link between API Schema and Data Dictionary
/// </summary>
public class ApiSchemaElement : BaseEntity
{
    public Guid ApiSpecId { get; set; }
    public string SchemaPath { get; set; } = string.Empty; // JSONPath like #/components/schemas/Customer/properties/id
    public Guid? DataEntityId { get; set; }
    public Guid? DataAttributeId { get; set; }
    public DateTime LinkedAt { get; set; }
    public Guid LinkedBy { get; set; }
    public bool AutoSynced { get; set; }

    // Navigation properties
    public ApiSpec ApiSpec { get; set; } = null!;
    public DataEntity? DataEntity { get; set; }
    public DataAttribute? DataAttribute { get; set; }
}
