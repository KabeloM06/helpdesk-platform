
using Helpdesk.Domain.Enums;

namespace Helpdesk.Application.Features.Tickets.Commands.CreateTicket;

public sealed record CreateTicketCommand(
    string Title,
    string Description,
    TicketPriority Priority = TicketPriority.Normal);
