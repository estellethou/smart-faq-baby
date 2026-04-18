namespace SmartFaq.Api.Services;

public class EmbeddingService
{
    // 🔥 MOCK deterministic embeddings
    public Task<float[]> GetEmbedding(string text)
    {
        var vector = new float[16];

        for (int i = 0; i < text.Length; i++)
        {
            vector[i % 16] += (text[i] % 10) / 10f;
        }

        return Task.FromResult(vector);
    }
}