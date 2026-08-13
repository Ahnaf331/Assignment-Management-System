using AssignmentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentManagement.Infrastructure.Persistence.Configurations;

public class ClassCourseConfiguration : IEntityTypeConfiguration<ClassCourse>
{
    public void Configure(EntityTypeBuilder<ClassCourse> builder)
    {
        builder.ToTable("classes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Description).HasMaxLength(500);

        builder.HasIndex(c => c.Code).IsUnique();

        builder.HasMany(c => c.Subjects)
            .WithOne(s => s.ClassCourse)
            .HasForeignKey(s => s.ClassCourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
