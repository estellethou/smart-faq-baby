namespace SmartFaq.Api.Services;

public interface IQuestionService
{
    Task<string> GetContext(string question);
}