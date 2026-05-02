using SmartFaq.Api.Domain.Models;

namespace SmartFaq.Api.Infrastructure;

public class DatasetLoader
{
    public List<FaqSection> Load(string path)
    {
        var lines = File.ReadAllLines(path);
        var sections = new List<FaqSection>();

        string currentTitle = "";
        int min = 0, max = 0;

        foreach (var line in lines)
        {
            if (line.StartsWith("##"))
            {
                currentTitle = line.Replace("##", "").Trim();
                (min, max) = ParseAge(currentTitle);
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                sections.Add(new FaqSection()
                {
                    Title = currentTitle,
                    Content = line.Trim(),
                    MinMonths = min,
                    MaxMonths = max
                });
            }
        }
        return sections;
    }

    private (int min, int max) ParseAge(string title)
    {
        // Example: "1-3 mois"
        var match = System.Text.RegularExpressions.Regex.Match(title, @"(\d+)\s*-\s*(\d+)");

        if (match.Success)
        {
            var min = int.Parse(match.Groups[1].Value);
            var max = int.Parse(match.Groups[2].Value);
            return (min, max);
        }

        // Section without age
        return (0, 99);
    }
}