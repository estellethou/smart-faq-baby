using SmartFaq.Api.Domain.Models;
using SmartFaq.Api.Infrastructure;

namespace SmartFaq.Api.Services;

public class QuestionService : IQuestionService
{
    private readonly List<FaqSection> _sections;

    public QuestionService(DatasetLoader loader)
    {
        _sections = loader.Load("Data/faq.txt");
    }
    
    public string GetContext(string question)
    {
        var lowerQuestion = question.ToLower();
    
        var age = ExtractAge(lowerQuestion);

        var filteredSections = _sections
            .Where(s => age >= s.MinMonths && age <= s.MaxMonths)
            .ToList();

        var words = lowerQuestion
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !_stopWords.Contains(w))
                .ToList();

        var match = filteredSections
            .Select(s => new
            {
                Section = s,
                Score = words.Sum(word =>
                    GetWordWeight(word) * 
                    s.Content.ToLower().Split(' ').Count(w => w.Contains(word)))
            })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();
        
        return match != null && match.Score > 0
            ? match.Section.Content
            : "Aucune information trouvée";
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

    private int GetWordWeight(string word)
    {
        if (word.Length <= 2) return 0;
        if (word == "biberon" || word == "lait") return 3;
        if (word == "eau") return 2;

        return 1;
    }
}