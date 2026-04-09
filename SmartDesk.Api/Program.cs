using FluentValidation;
using SmartDesk.Api.Services;
using SmartDesk.Api.Adapters;
using SmartDesk.Api.Strategies;
using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
DotNetEnv.Env.Load(".env");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:4200";

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSingleton<IKnowledgeBaseService, KnowledgeBaseService>();
builder.Services.AddSingleton<ISessionService, InMemorySessionService>();
builder.Services.AddSingleton<ISentimentService, RuleBasedSentimentService>();
builder.Services.AddSingleton<IAiServiceAdapter, GeminiServiceAdapter>();
builder.Services.AddSingleton<AiAnswerStrategy>();
builder.Services.AddSingleton<KeywordFallbackStrategy>();
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        context.Response.ContentType = "application/json";

        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var errors = validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Validation failed.",
                errors
            });

            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            message = "An unexpected error occurred."
        });
    });
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("FrontendPolicy");

app.MapControllers();

app.Run();