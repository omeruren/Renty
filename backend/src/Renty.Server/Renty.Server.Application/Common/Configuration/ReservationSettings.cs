namespace Renty.Server.Application.Common.Configuration;

public sealed class ReservationSettings
{
    public const string SectionName = "ReservationSettings";

    public int MinDurationDays { get; init; } = 1;

    public int MaxDurationDays { get; init; } = 30;
}
