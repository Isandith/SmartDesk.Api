using FluentValidation;
using SmartDesk.Api.Models.Chat;

namespace SmartDesk.Api.Validators;

public class ChatResponseValidator : AbstractValidator<ChatResponse>
{
    public ChatResponseValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");

        RuleFor(x => x.UserMessage)
            .NotEmpty().WithMessage("User message is required.");

        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Answer is required.");

        RuleFor(x => x.SentimentScore)
            .InclusiveBetween(-1.0, 1.0)
            .WithMessage("Sentiment score must be between -1.0 and 1.0.");

        RuleFor(x => x.ResponseSource)
            .NotEmpty().WithMessage("Response source is required.");

        RuleFor(x => x.Context)
            .NotNull().WithMessage("Context is required.");

        RuleFor(x => x.Context.Count)
            .LessThanOrEqualTo(3)
            .WithMessage("Context can contain at most 3 messages.");
    }
}