
using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;

namespace Helpdesk.UnitTests.Domain;

public class TicketTests
{
    [Fact]
    public void Create_WithValidDetails_CreatesOpenTicket()
    {
        var ticket = Ticket.Create(
            " Cannot log in ",
            " User cannot access their account. ",
            TicketPriority.High);

        Assert.NotEqual(Guid.Empty, ticket.Id);
        Assert.Equal("Cannot log in", ticket.Title);
        Assert.Equal(
            "User cannot access their account.",
            ticket.Description);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Equal(TicketPriority.High, ticket.Priority);
        Assert.Equal(ticket.CreatedAtUtc, ticket.UpdatedAtUtc);
    }

    [Fact]
    public void Create_WithDefaultPriority_UsesNormalPriority()
    {
        var ticket = Ticket.Create(
            "Cannot log in",
            "User cannot access their account.");

        Assert.Equal(TicketPriority.Normal, ticket.Priority);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ThrowsArgumentException(
        string? title)
    {
        Assert.Throws<ArgumentException>(() =>
            Ticket.Create(title!, "A valid description"));
    }

    [Fact]
    public void Create_WithTitleExceedingMaximumLength_Throws()
    {
        var title = new string('a', Ticket.MaxTitleLength + 1);

        Assert.Throws<ArgumentException>(() =>
            Ticket.Create(title, "A valid description"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidDescription_ThrowsArgumentException(
        string? description)
    {
        Assert.Throws<ArgumentException>(() =>
            Ticket.Create("A valid title", description!));
    }

    [Fact]
    public void Create_WithDescriptionExceedingMaximumLength_Throws()
    {
        var description =
            new string('a', Ticket.MaxDescriptionLength + 1);

        Assert.Throws<ArgumentException>(() =>
            Ticket.Create("A valid title", description));
    }

    [Fact]
    public void ChangeStatus_FromOpenToInProgress_UpdatesStatus()
    {
        var ticket = CreateTicket();

        ticket.ChangeStatus(TicketStatus.InProgress);

        Assert.Equal(TicketStatus.InProgress, ticket.Status);
    }

    [Fact]
    public void ChangeStatus_FromResolvedToClosed_IsAllowed()
    {
        var ticket = CreateTicket();
        ticket.ChangeStatus(TicketStatus.Resolved);

        ticket.ChangeStatus(TicketStatus.Closed);

        Assert.Equal(TicketStatus.Closed, ticket.Status);
    }

    [Fact]
    public void ChangeStatus_FromClosedToOpen_ThrowsAndPreservesStatus()
    {
        var ticket = CreateTicket();
        ticket.ChangeStatus(TicketStatus.Resolved);
        ticket.ChangeStatus(TicketStatus.Closed);

        Assert.Throws<InvalidOperationException>(() =>
            ticket.ChangeStatus(TicketStatus.Open));

        Assert.Equal(TicketStatus.Closed, ticket.Status);
    }

    [Fact]
    public void ChangeStatus_ToSameStatus_DoesNotThrow()
    {
        var ticket = CreateTicket();

        ticket.ChangeStatus(TicketStatus.Open);

        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public void ChangePriority_WithValidPriority_UpdatesPriority()
    {
        var ticket = CreateTicket();

        ticket.ChangePriority(TicketPriority.Urgent);

        Assert.Equal(TicketPriority.Urgent, ticket.Priority);
    }

    [Fact]
    public void UpdateDetails_WithValidDetails_UpdatesBothProperties()
    {
        var ticket = CreateTicket();

        ticket.UpdateDetails(
            "Updated title",
            "Updated description");

        Assert.Equal("Updated title", ticket.Title);
        Assert.Equal("Updated description", ticket.Description);
    }

    [Fact]
    public void UpdateDetails_WithInvalidDescription_PreservesExistingDetails()
    {
        var ticket = CreateTicket();
        var originalTitle = ticket.Title;
        var originalDescription = ticket.Description;

        Assert.Throws<ArgumentException>(() =>
            ticket.UpdateDetails("New title", "   "));

        Assert.Equal(originalTitle, ticket.Title);
        Assert.Equal(originalDescription, ticket.Description);
    }

    private static Ticket CreateTicket() =>
        Ticket.Create("Test ticket", "Test description");
}
