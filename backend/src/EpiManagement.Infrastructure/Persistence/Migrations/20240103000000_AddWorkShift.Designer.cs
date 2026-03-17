using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using EpiManagement.Infrastructure.Persistence;

#nullable disable

namespace EpiManagement.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240103000000_AddWorkShift")]
    partial class AddWorkShift
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder) { }
    }
}
