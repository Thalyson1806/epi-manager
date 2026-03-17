using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using EpiManagement.Infrastructure.Persistence;

#nullable disable

namespace EpiManagement.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240102000000_AddEarlyReplacement")]
    partial class AddEarlyReplacement
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
        }
    }
}
