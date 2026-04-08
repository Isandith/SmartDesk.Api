using FluentValidation;
using SmartDesk.Api.Services;
using SmartDesk.Api.Adapters;
using SmartDesk.Api.Strategies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSingleton<IKnowledgeBaseService, KnowledgeBaseService>();
builder.Services.AddSingleton<ISessionService, InMemorySessionService>();
builder.Services.AddSingleton<ISentimentService, RuleBasedSentimentService>();
builder.Services.AddSingleton<IAiServiceAdapter, DisabledAiServiceAdapter>();
builder.Services.AddSingleton<AiAnswerStrategy>();
builder.Services.AddSingleton<KeywordFallbackStrategy>();
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();