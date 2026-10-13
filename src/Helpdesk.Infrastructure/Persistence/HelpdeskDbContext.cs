
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Persistence;

public sealed class HelpdeskDbContext(
    DbContextOptions<HelpdeskDbContext> options)
    : DbContext(options)
{
}
