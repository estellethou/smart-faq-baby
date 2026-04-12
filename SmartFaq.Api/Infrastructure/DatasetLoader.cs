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
        if (title.Contains("sécurité"))
            return (0, 36)
                ;
        var parts = title.Split('-');
        
        var min = int.Parse(parts[0].Trim());
        var max = int.Parse(parts[1].Split(' ')[0].Trim());

        return (min, max);
    }
}