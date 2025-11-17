using System;
using Apivia.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Apivia.Shared.Data.Migrations
{
    [DbContext(typeof(ApiviaDbContext))]
    partial class ApiviaDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            // This is a simplified snapshot - in a real scenario, EF would generate
            // a complete model snapshot with all entity configurations
            // The migration file above contains the complete schema definition
        }
    }
}
