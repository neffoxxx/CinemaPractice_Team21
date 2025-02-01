using AppCore.DTOs;
using FluentValidation;

namespace AppCore.Validators
{
    public class TicketValidator : AbstractValidator<TicketDTO>
    {
        public TicketValidator()
        {
            RuleFor(ticket => ticket.RowNumber)
                .NotEmpty().WithMessage("Row number is required.")
                 .GreaterThan(0).WithMessage("Row number must be greater than 0.");

            RuleFor(ticket => ticket.SeatNumber)
               .NotEmpty().WithMessage("Seat number is required.")
               .Matches("^[0-9]+$").WithMessage("Seat number must be numeric.");

        }
    }
}