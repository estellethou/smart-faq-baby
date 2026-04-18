namespace SmartFaq.Api.Domain.Models;

public class FaqSection
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public int MinMonths { get; set; }
    public int MaxMonths { get; set; }
    // 🔥 embedding vector
    public float[] Embedding { get; set; } = Array.Empty<float>();
}