using SmartFaq.Api.Infrastructure;
using SmartFaq.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DatasetLoader>();
builder.Services.AddSingleton<IQuestionService, QuestionService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Produce error msg "Failed to determine the https port for redirect." because .NET tries to redirect to HTTPS and in config it's HTTP
//app.UseHttpsRedirection();

app.MapPost("/ask", (AskRequest request, IQuestionService questionService) =>
{
    var context = questionService.GetContext(request.Question);
    return new { context };
});

app.Run();



public record AskRequest(string Question);