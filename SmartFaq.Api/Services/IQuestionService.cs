namespace SmartFaq.Api.Services;

public interface IQuestionService
{
    Task<IEnumerable<string>> GetContext(string question);
}