using AppCore.DTOs;
using FluentValidation;

namespace AppCore.Validators
{
    public class SessionValidator : AbstractValidator<SessionDTO>
    {
        public SessionValidator()
        {
            RuleFor(session => session.MovieId)
                .NotEmpty().WithMessage("Movie is required.");

            RuleFor(session => session.HallId)
                .NotEmpty().WithMessage("Hall is required.");

            RuleFor(session => session.StartTime)
                .NotEmpty().WithMessage("Start time is required.");

            RuleFor(session => session.Price)
                 .GreaterThan(0).WithMessage("Price must be greater than 0.");
        }
    }
}