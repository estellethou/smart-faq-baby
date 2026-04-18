using SmartFaq.Api.Infrastructure;
using SmartFaq.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DatasetLoader>();
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddSingleton<IQuestionService, QuestionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/ask", async (AskRequest request, IQuestionService questionService) =>
{
    var context = await questionService.GetContext(request.Question);
    return new { context };
});

app.Run();

public record AskRequest(string Question);