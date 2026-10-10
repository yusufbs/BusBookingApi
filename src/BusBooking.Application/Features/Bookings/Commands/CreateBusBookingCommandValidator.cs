using FluentValidation;

namespace BusBooking.Application.Features.Bookings.Commands;

public sealed class CreateBusBookingCommandValidator : AbstractValidator<CreateBusBookingCommand>
{
    public CreateBusBookingCommandValidator()
    {
        RuleFor(x => x.CustId)
            .GreaterThan(0)
            .WithMessage("A valid Customer ID is required.");

        RuleFor(x => x.ScheduleId)
            .GreaterThan(0)
            .WithMessage("A valid Schedule ID is required.");

        RuleFor(x => x.Passengers)
            .NotEmpty()
            .WithMessage("At least one passenger must be provided for booking.");

        RuleForEach(x => x.Passengers).ChildRules(passenger =>
        {
            passenger.RuleFor(p => p.PassengerName)
                .NotEmpty()
                .MaximumLength(150)
                .WithMessage("Passenger name is required.");

            passenger.RuleFor(p => p.Age)
                .InclusiveBetween(1, 120)
                .WithMessage("Passenger age must be between 1 and 120.");

            passenger.RuleFor(p => p.Gender)
                .NotEmpty()
                .MaximumLength(10)
                .WithMessage("Gender is required.");

            passenger.RuleFor(p => p.SeatNo)
                .GreaterThan(0)
                .WithMessage("Seat number must be greater than 0.");
        });

        RuleFor(x => x.Passengers)
            .Must(passengers =>
            {
                var seatNumbers = passengers.Select(p => p.SeatNo).ToList();
                return seatNumbers.Distinct().Count() == seatNumbers.Count;
            })
            .WithMessage("Duplicate seat numbers cannot be selected within the same booking.");
    }
}
