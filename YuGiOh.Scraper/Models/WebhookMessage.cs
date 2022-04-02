using System;
using System.Text.Json.Serialization;

namespace YuGiOh.Scraper.Models;

public class WebhookMessage
{
    
    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("embeds")]
    public WebhookMessageEmbed[] Embeds { get; set; }

}

public class WebhookMessageEmbed
{
    
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp => DateTimeOffset.UtcNow;

}