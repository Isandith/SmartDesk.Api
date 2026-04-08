using System.Text.Json.Serialization;

namespace SmartDesk.Api.Models.KnowledgeBase;

/// <summary>
/// Represents the root knowledge base document used for FAQ answering.
/// </summary>
public class KnowledgeBaseDocument
{
    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [JsonPropertyName("headquarters")]
    public string Headquarters { get; set; } = string.Empty;

    [JsonPropertyName("contact")]
    public ContactInfo Contact { get; set; } = new();

    [JsonPropertyName("support_policy")]
    public SupportPolicy SupportPolicy { get; set; } = new();

    [JsonPropertyName("faqs")]
    public List<FaqItem> Faqs { get; set; } = new();
}

/// <summary>
/// Represents contact information for support channels.
/// </summary>
public class ContactInfo
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("response_time")]
    public string ResponseTime { get; set; } = string.Empty;
}

/// <summary>
/// Represents support policy settings and escalation details.
/// </summary>
public class SupportPolicy
{
    [JsonPropertyName("compliance")]
    public string Compliance { get; set; } = string.Empty;

    [JsonPropertyName("standard_support")]
    public string StandardSupport { get; set; } = string.Empty;

    [JsonPropertyName("escalation_threshold")]
    public string EscalationThreshold { get; set; } = string.Empty;
}

/// <summary>
/// Represents an FAQ entry in the knowledge base.
/// </summary>
public class FaqItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("answer")]
    public string Answer { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public FaqMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Represents metadata attached to an FAQ entry.
/// </summary>
public class FaqMetadata
{
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("tier_options")]
    public List<string> TierOptions { get; set; } = new();

    [JsonPropertyName("is_critical")]
    public bool IsCritical { get; set; }
}