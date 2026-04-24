using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Auth.Commands.Logout;

/// <summary>
/// Validates <see cref="LogoutCommand"/>: ensures the user identifier is valid.
/// </summary>
public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.UserId).ValidId();
    }
}
