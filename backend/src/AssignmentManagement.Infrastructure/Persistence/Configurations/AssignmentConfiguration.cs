using AssignmentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentManagement.Infrastructure.Persistence.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("assignments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(5000);
        builder.Property(a => a.MaxMarks).IsRequired();
        builder.Property(a => a.Status).HasConversion<int>().IsRequired();
        builder.Ignore(a => a.IsPublished);

        builder.HasOne(a => a.ClassCourse)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.ClassCourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Subject)
            .WithMany(s => s.Assignments)
            .HasForeignKey(a => a.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Teacher)
            .WithMany(u => u.CreatedAssignments)
            .HasForeignKey(a => a.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.ClassCourseId);
        builder.HasIndex(a => a.SubjectId);
        builder.HasIndex(a => a.TeacherId);
    }
}
