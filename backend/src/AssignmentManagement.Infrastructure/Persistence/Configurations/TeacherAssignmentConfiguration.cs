using AssignmentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentManagement.Infrastructure.Persistence.Configurations;

public class TeacherAssignmentConfiguration : IEntityTypeConfiguration<TeacherAssignment>
{
    public void Configure(EntityTypeBuilder<TeacherAssignment> builder)
    {
        builder.ToTable("teacher_assignments");

        builder.HasKey(t => t.Id);

        builder.HasOne(t => t.Teacher)
            .WithMany(u => u.TeacherAssignments)
            .HasForeignKey(t => t.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Subject)
            .WithMany(s => s.TeacherAssignments)
            .HasForeignKey(t => t.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.ClassCourse)
            .WithMany(c => c.TeacherAssignments)
            .HasForeignKey(t => t.ClassCourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // A teacher is assigned to a given subject/class only once.
        builder.HasIndex(t => new { t.TeacherId, t.SubjectId, t.ClassCourseId }).IsUnique();
    }
}
