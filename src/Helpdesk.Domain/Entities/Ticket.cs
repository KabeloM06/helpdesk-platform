
using Helpdesk.Domain.Enums;

namespace Helpdesk.Domain.Entities;

public sealed class Ticket
{
    public const int MaxTitleLength = 150;
    public const int MaxDescriptionLength = 5_000;

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    // Required by Entity Framework Core.
    private Ticket()
    {
    }

    private Ticket(
        string title,
        string description,
        TicketPriority priority)
    {
        Id = Guid.NewGuid();
        Title = ValidateTitle(title);
        Description = ValidateDescription(description);

        if (!Enum.IsDefined(priority))
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority), "The ticket priority is invalid.");
        }

        Priority = priority;
        Status = TicketStatus.Open;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public static Ticket Create(
        string title,
        string description,
        TicketPriority priority = TicketPriority.Normal)
    {
        return new Ticket(title, description, priority);
    }

    public void UpdateDetails(string title, string description)
    {
        // Validate both values before changing either property.
        var validatedTitle = ValidateTitle(title);
        var validatedDescription = ValidateDescription(description);

        Title = validatedTitle;
        Description = validatedDescription;
        Touch();
    }

    public void ChangePriority(TicketPriority priority)
    {
        if (!Enum.IsDefined(priority))
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority), "The ticket priority is invalid.");
        }

        if (Priority == priority)
        {
            return;
        }

        Priority = priority;
        Touch();
    }

    public void ChangeStatus(TicketStatus newStatus)
    {
        if (!Enum.IsDefined(newStatus))
        {
            throw new ArgumentOutOfRangeException(
                nameof(newStatus), "The ticket status is invalid.");
        }

        if (Status == newStatus)
        {
            return;
        }

        var isAllowed = Status switch
        {
            TicketStatus.Open =>
                newStatus is TicketStatus.InProgress
                    or TicketStatus.Resolved,

            TicketStatus.InProgress =>
                newStatus is TicketStatus.Open
                    or TicketStatus.Resolved,

            TicketStatus.Resolved =>
                newStatus is TicketStatus.Closed
                    or TicketStatus.InProgress,

            TicketStatus.Closed => false,

            _ => false
        };

        if (!isAllowed)
        {
            throw new InvalidOperationException(
                $"Cannot change a ticket from {Status} to {newStatus}.");
        }

        Status = newStatus;
        Touch();
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "A ticket title is required.", nameof(title));
        }

        var normalized = title.Trim();

        if (normalized.Length > MaxTitleLength)
        {
            throw new ArgumentException(
                $"The title cannot exceed {MaxTitleLength} characters.",
                nameof(title));
        }

        return normalized;
    }

    private static string ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "A ticket description is required.",
                nameof(description));
        }

        var normalized = description.Trim();

        if (normalized.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"The description cannot exceed {MaxDescriptionLength} characters.",
                nameof(description));
        }

        return normalized;
    }
}
