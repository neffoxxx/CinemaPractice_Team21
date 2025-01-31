using AppCore.DTOs;
using FluentValidation;

namespace AppCore.Validators
{
    public class MovieValidator : AbstractValidator<MovieDTO>
    {
        public MovieValidator()
        {
            RuleFor(movie => movie.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(movie => movie.Description)
                .NotEmpty().WithMessage("Description is required.");

            RuleFor(movie => movie.DurationMinutes)
                .GreaterThan(0).WithMessage("Duration must be greater than 0.");
        }
    }
}