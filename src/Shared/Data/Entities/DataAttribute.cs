namespace Apivia.Shared.Data.Entities;

/// <summary>
/// Data Attribute - attribute of a data entity
/// </summary>
public class DataAttribute : BaseEntity
{
    public Guid DataEntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = "string";
    public bool IsRequired { get; set; }
    public string? Format { get; set; }
    public string? AllowedValues { get; set; } // JSON array
    public string? Metadata { get; set; } // JSON metadata
    public bool IsPii { get; set; } // Personal Identifiable Information
    public string? GdprCategory { get; set; }

    // Navigation properties
    public DataEntity DataEntity { get; set; } = null!;
    public ICollection<ApiSchemaElement> LinkedSchemas { get; set; } = new List<ApiSchemaElement>();
}
