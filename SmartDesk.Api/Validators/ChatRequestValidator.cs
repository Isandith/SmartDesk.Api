using FluentValidation;
using SmartDesk.Api.Models.Chat;

namespace SmartDesk.Api.Validators;

/// <summary>
/// Validates incoming chat request payloads.
/// </summary>
public class ChatRequestValidator : AbstractValidator<ChatRequest>
{
    public ChatRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MinimumLength(2).WithMessage("Message must be at least 2 characters.")
            .MaximumLength(1000).WithMessage("Message cannot exceed 1000 characters.");

        RuleFor(x => x.SessionId)
            .MaximumLength(100).WithMessage("Session ID cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SessionId));
    }
}