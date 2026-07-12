using UBP.Booking.Application.Dtos;
using UBP.Booking.Application.Errors;
using UBP.Booking.Domain.Entities;
using UBP.Booking.Domain.Enums;
using UBP.Booking.Persistence.Interfaces;
using UBP.Core.Persistence.Interfaces;
using UBP.CQRS;
using UBP.Results;

namespace UBP.Booking.Application.Commands;

public record CreateBookingCommand(
    Guid SlotId,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string? CustomerNotes,
    bool AutoConfirm = false) : IRequest<Result<BookingDto>>;

internal sealed class CreateBookingCommandHandler(IUnitOfWork unitOfWork, IAvailabilitySlotRepository slotRepository)
    : IRequestHandler<CreateBookingCommand, Result<BookingDto>>
{
    private const int MaxReferenceAttempts = 5;

    public async Task<Result<BookingDto>> HandleAsync(CreateBookingCommand request, CancellationToken cancellationToken = default)
    {
        var slot = await slotRepository.GetSingleOrDefaultAsync(s => s.Id == request.SlotId, cancellationToken);
        if (slot is null)
            return Result.Failure<BookingDto>(SlotErrors.NotFound);

        var reserved = await slotRepository.TryReserveAsync(request.SlotId, cancellationToken);
        if (!reserved)
            return Result.Failure<BookingDto>(BookingErrors.SlotUnavailable);

        var bookingRepository = unitOfWork.Repository<BookingEntity>();

        var externalReference = await GenerateExternalReferenceAsync(bookingRepository, cancellationToken);
        if (externalReference is null)
        {
            await slotRepository.ReleaseAsync(request.SlotId, cancellationToken);
            return Result.Failure<BookingDto>(BookingErrors.ReferenceGenerationFailed);
        }

        var status = request.AutoConfirm ? BookingStatus.Confirmed : BookingStatus.Pending;
        var now = DateTimeOffset.UtcNow;

        var booking = new BookingEntity
        {
            Id = Guid.CreateVersion7(),
            SlotId = slot.Id,
            TenantId = slot.TenantId,
            ResourceId = slot.ResourceId,
            SlotStartAt = slot.StartAt,
            SlotEndAt = slot.EndAt,
            Status = status,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            CustomerNotes = request.CustomerNotes,
            ExternalReference = externalReference,
            ConfirmedAt = status == BookingStatus.Confirmed ? now.UtcDateTime : null
        };

        bookingRepository.Add(booking);

        unitOfWork.Repository<BookingStatusHistoryEntity>().Add(new BookingStatusHistoryEntity
        {
            Id = Guid.CreateVersion7(),
            BookingId = booking.Id,
            FromStatus = null,
            ToStatus = status,
            ChangedByType = ChangedByType.Customer,
            ChangedAt = now
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(BookingDto.FromEntity(booking));
    }

    private static async Task<string?> GenerateExternalReferenceAsync(IRepository<BookingEntity> bookingRepository, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxReferenceAttempts; attempt++)
        {
            var candidate = $"BK-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100_000):D5}";
            var existing = await bookingRepository.GetSingleOrDefaultAsync(b => b.ExternalReference == candidate, cancellationToken);
            if (existing is null)
                return candidate;
        }

        return null;
    }
}
