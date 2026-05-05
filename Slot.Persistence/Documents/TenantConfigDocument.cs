using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Slot.Persistence.Documents;

internal sealed class TenantConfigDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = string.Empty;

    [BsonElement("terminology")]
    public TerminologyDocument Terminology { get; init; } = new();

    [BsonElement("booking")]
    public BookingDocument Booking { get; init; } = new();

    [BsonElement("creditPack")]
    public CreditPackDocument CreditPack { get; init; } = new();

    [BsonElement("slot")]
    public SlotDocument Slot { get; init; } = new();
}

internal sealed class TerminologyDocument
{
    [BsonElement("customer")]
    public string Customer { get; init; } = "Customer";

    [BsonElement("resource")]
    public string Resource { get; init; } = "Resource";

    [BsonElement("serviceType")]
    public string ServiceType { get; init; } = "Service";

    [BsonElement("slot")]
    public string Slot { get; init; } = "Slot";

    [BsonElement("creditPack")]
    public string CreditPack { get; init; } = "Credit Pack";
}

internal sealed class BookingDocument
{
    [BsonElement("cancellationDeadlineHours")]
    public int CancellationDeadlineHours { get; init; } = 2;

    [BsonElement("allowCustomerCancellation")]
    public bool AllowCustomerCancellation { get; init; } = true;

    [BsonElement("creditReturnOnLateCancellation")]
    public bool CreditReturnOnLateCancellation { get; init; } = false;
}

internal sealed class CreditPackDocument
{
    [BsonElement("enabled")]
    public bool Enabled { get; init; } = true;

    [BsonElement("freezingEnabled")]
    public bool FreezingEnabled { get; init; } = false;

    [BsonElement("expiryExtendedOnFreeze")]
    public bool ExpiryExtendedOnFreeze { get; init; } = false;
}

internal sealed class SlotDocument
{
    [BsonElement("requiresResource")]
    public bool RequiresResource { get; init; } = true;

    [BsonElement("allowGroupBookings")]
    public bool AllowGroupBookings { get; init; } = false;
}
