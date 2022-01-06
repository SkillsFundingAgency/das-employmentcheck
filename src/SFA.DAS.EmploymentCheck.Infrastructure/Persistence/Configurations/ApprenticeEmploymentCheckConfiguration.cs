using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmploymentCheck.Domain.Entities;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Persistence.Configurations
{
    public class ApprenticeEmploymentCheckConfiguration : IEntityTypeConfiguration<ApprenticeEmploymentCheck>
    {
        public void Configure(EntityTypeBuilder<ApprenticeEmploymentCheck> builder)
        {
            builder.Ignore(e => e.DomainEvents);

            builder.Property(t => t.CheckType)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}