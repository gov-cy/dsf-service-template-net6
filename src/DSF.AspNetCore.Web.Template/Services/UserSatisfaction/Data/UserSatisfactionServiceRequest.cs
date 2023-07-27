namespace DSF.AspNetCore.Web.Template.Services.UserSatisfaction.Data;

using Microsoft.Win32.SafeHandles;
using System.Text.Json.Serialization;

public class UserSatisfactionServiceRequest
{
    [JsonPropertyName("pageSource")]
    public string PageSource { get; set; } = string.Empty;

    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("rating")]
    public long Rating { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}
