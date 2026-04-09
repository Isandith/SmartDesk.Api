# SmartDesk AI FAQ System

`SmartDesk AI` is a simple FAQ assistant built with `ASP.NET Core (.NET 10)`.
It answers user questions from a knowledge base, tracks short session context, detects sentiment, and escalates frustrated users.

## Features

- FAQ answering from `Data/Knowledge-Base.json`
- AI-first response flow (Gemini)
- Automatic keyword fallback when AI is unavailable
- Sentiment scoring (`-1.0` to `1.0`)
- Priority escalation when sentiment `< -0.6`
- Session support with short conversation history
- Request/response validation with `FluentValidation`
- Clean layering with Strategy + Adapter patterns

## Tech Stack

- `ASP.NET Core Web API` (`net10.0`)
- `FluentValidation`
- `DotNetEnv`
- `Swagger / OpenAPI`
- `Google Gemini API` (via adapter)

## Project Structure

- `Controllers/` → API endpoints
- `Services/` → core business logic (chat, session, sentiment, knowledge base)
- `Strategies/` → answer strategies (`AiAnswerStrategy`, `KeywordFallbackStrategy`)
- `Adapters/` → AI integration abstraction (`IAiServiceAdapter`, `GeminiServiceAdapter`)
- `Models/` → request/response/domain DTOs
- `Validators/` → request/response validation
- `Data/Knowledge-Base.json` → FAQ source data

## Architecture Notes

- **Clean separation of concerns** across controller, service, strategies, adapters, and validators.
- **Strategy Pattern**:
  - `AiAnswerStrategy` for AI-generated answers
  - `KeywordFallbackStrategy` for resilient FAQ matching
- **Adapter Pattern**:
  - `IAiServiceAdapter` abstraction
  - `GeminiServiceAdapter` implementation
  - `DisabledAiServiceAdapter` for non-AI/manual mode behavior

## Prerequisites

- `.NET SDK 10`
- Gemini API key (or run in fallback mode)

## Environment Setup

Create/update `SmartDesk.Api/.env`:

```env
GEMINI_API_KEY=YOUR_GEMINI_API_KEY
LLM_MODEL=gemini-2.5-flash-lite
GEMINI_API_VERSION=v1
FRONTEND_URL=http://localhost:4200
```

If you see an API configuration error, check that the file is named `.env` exactly.
If the file is named `env` without the leading dot, the app will not load it.

If you want a safe template instead of a real key, copy `.env.example` to `.env` and fill in your own values.

> Important: Never commit real API keys to public repositories.

## Run the API

From repository root:

```powershell
dotnet run --project SmartDesk.Api/SmartDesk.Api.csproj
```

Swagger UI will be available after launch (default ASP.NET Core development URL).

## API Endpoints

### 1) Ask a question

`POST /api/chat/ask`

Request:

```json
{
  "session_id": "optional-session-id",
  "message": "This system is terrible and not working!"
}
```

Response (example):

```json
{
  "session_id": "abc123",
  "user_message": "This system is terrible and not working!",
  "answer": "⚠️ Priority Support: We're sorry you're facing issues. Our team will assist you immediately. ...",
  "sentiment_score": -0.8,
  "priority_escalation": true,
  "response_source": "ai",
  "manual_mode": false,
  "system_status_message": "",
  "context": []
}
```

### 2) Reset a session

`POST /api/chat/reset/{sessionId}`

Response:

```json
{
  "session_id": "abc123",
  "cleared": true
}
```

## Sentiment and Escalation Rules

- Sentiment is computed in `RuleBasedSentimentService`
- Score range is clamped to `[-1.0, 1.0]`
- If score `< -0.6`:
  - `priority_escalation = true`
  - answer is prefixed with a priority support message
- If AI falls back to keyword matching, `system_status_message` explains why the fallback happened

## Fallback Behavior

If AI fails (missing key, invalid key, quota, API failure, empty response, exception):

1. System switches to keyword fallback strategy
2. Best FAQ match is returned
3. If no FAQ matches, a generic support contact response is returned

### What the fallback messages mean

- API configuration error: the `.env` file is missing, misnamed, or does not contain a valid `GEMINI_API_KEY`
- Quota exceeded: the Gemini API has reached its usage limit for the current key or project
- To continue after a quota error, create a new API key in Google AI Studio, then paste the new key into `.env`

## Validation

- Request validation: `ChatRequestValidator`
- Response validation: `ChatResponseValidator`
- Invalid payloads throw validation errors

## Frontend Integration

Set `FRONTEND_URL` in `.env` to allow CORS from your UI app.

Example:

```env
FRONTEND_URL=http://localhost:4200
```

## Notes for Submission

- Keep `.env` local only
- Use `.env.example` as the shareable dummy template
- Commit source code and `README.md`
- Provide setup instructions and architecture explanation (this file)
