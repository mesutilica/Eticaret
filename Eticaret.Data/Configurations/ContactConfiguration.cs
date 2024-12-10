using Eticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eticaret.Data.Configurations
{
    internal class ContactConfiguration : IEntityTypeConfiguration<Contact>
    {
        public void Configure(EntityTypeBuilder<Contact> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Surname).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Email).HasMaxLength(50);
            builder.Property(x => x.Phone).HasColumnType("varchar(20)").HasMaxLength(20);
            builder.Property(x => x.Message).IsRequired().HasMaxLength(500);
        }
    }
}
