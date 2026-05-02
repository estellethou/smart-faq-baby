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

    public async Task<IEnumerable<string>> GetContext(string question)
    {
        var lowerQuestion = question.ToLower();
        var age = ExtractAge(lowerQuestion);

        // 1. Filter by age + evergreen sections
        var filteredSections = _sections
            .Where(s =>
                (age >= s.MinMonths && age <= s.MaxMonths)
                || s.Title.Contains("sécurité")
                || s.Title.Contains("allergies")
                || s.Title.Contains("hydratation"))
            .ToList();

        // 2. Keywords
        var words = lowerQuestion
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !_stopWords.Contains(w))
            .ToList();

        // 3. Question embedding
        var questionEmbedding = await _embeddingService.GetEmbedding(lowerQuestion);

        // 4. Rank sections
        var topMatches = filteredSections
            .Select(s =>
            {
                var content = s.Content.ToLower();

                var keywordScore = words.Sum(word =>
                    content.Split(' ').Count(w => w.Contains(word)));

                var embeddingScore = CosineSimilarity(questionEmbedding, s.Embedding);

                var finalScore = (keywordScore * 2) + embeddingScore;

                return new
                {
                    Section = s,
                    Score = finalScore
                };
            })
            .Where(x => x.Score > 1)
            .OrderByDescending(x => x.Score)
            .Take(3)
            .ToList();

        if (!topMatches.Any())
            return ["Aucune information trouvée"];

        // 5. Merge contexts
        return topMatches.Select(x => x.Section.Content);
    }

    private int ExtractAge(string question)
    {
        var match = System.Text.RegularExpressions.Regex.Match(question, @"\d+");

        if (match.Success)
            return int.Parse(match.Value);

        return 0;
    }
    
    private readonly HashSet<string> _stopWords = new()
    {
        "le", "la", "les", "de", "des", "du", "un", "une",
        "à", "au", "aux", "en", "et", "est", "pour",
        "que", "qui", "dans", "sur", "avec", "par",
        "ce", "cet", "cette"
    };

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