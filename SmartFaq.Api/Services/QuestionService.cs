using SmartFaq.Api.Domain.Models;
using SmartFaq.Api.Infrastructure;

namespace SmartFaq.Api.Services;

public class QuestionService : IQuestionService
{
    private readonly List<FaqSection> _sections;
    private readonly EmbeddingService _embeddingService;

    public QuestionService(DatasetLoader loader, EmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;

        _sections = loader.Load("Data/faq.txt");

        // Precompute embeddings (MOCK)
        foreach (var section in _sections)
        {
            section.Embedding = _embeddingService
                .GetEmbedding(section.Content)
                .Result;
        }
    }

    public async Task<string> GetContext(string question)
    {
        var age = ExtractAge(question);

        // 1. Filter by metadata (age)
        var filteredSections = _sections
            .Where(s => age >= s.MinMonths && age <= s.MaxMonths)
            .ToList();

        // 2. Question embedding
        var questionEmbedding = await _embeddingService.GetEmbedding(question);

        // 3. Vector similarity search
        var match = filteredSections
            .Select(s => new
            {
                Section = s,
                Score = CosineSimilarity(questionEmbedding, s.Embedding)
            })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        return match?.Section.Content ?? "Aucune information trouvée";
    }

    private int ExtractAge(string question)
    {
        var match = System.Text.RegularExpressions.Regex.Match(question, @"\d+");

        if (match.Success)
            return int.Parse(match.Value);

        return 0;
    }

    private double CosineSimilarity(float[] v1, float[] v2)
    {
        double dot = 0, norm1 = 0, norm2 = 0;

        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
            norm1 += v1[i] * v1[i];
            norm2 += v2[i] * v2[i];
        }

        return dot / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
    }
}