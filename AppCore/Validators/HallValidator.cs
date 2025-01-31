using AppCore.DTOs;
using FluentValidation;

namespace AppCore.Validators
{
    public class HallValidator : AbstractValidator<HallDTO>
    {
        public HallValidator()
        {
            RuleFor(hall => hall.Name)
                 .NotEmpty().WithMessage("Name is required.")
                 .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(hall => hall.Capacity)
                 .NotEmpty().WithMessage("Capacity is required.")
                .GreaterThan(0).WithMessage("Capacity must be greater than 0.");

            RuleFor(hall => hall.RowsCount)
                .NotEmpty().WithMessage("Rows count is required.")
                .GreaterThan(0).WithMessage("Rows count must be greater than 0.");

            RuleFor(hall => hall.SeatsPerRow)
               .NotEmpty().WithMessage("Seats per row is required.")
               .GreaterThan(0).WithMessage("Seats per row must be greater than 0.");

        }
    }
}