using AssignmentManagement.Application.DTOs.Classes;
using AssignmentManagement.Application.DTOs.Subjects;
using AssignmentManagement.Application.DTOs.TeacherAssignments;
using FluentValidation;

namespace AssignmentManagement.Application.Validators;

public class CreateClassCourseRequestValidator : AbstractValidator<CreateClassCourseRequest>
{
    public CreateClassCourseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateClassCourseRequestValidator : AbstractValidator<UpdateClassCourseRequest>
{
    public UpdateClassCourseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class CreateSubjectRequestValidator : AbstractValidator<CreateSubjectRequest>
{
    public CreateSubjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.ClassCourseId).NotEmpty();
    }
}

public class UpdateSubjectRequestValidator : AbstractValidator<UpdateSubjectRequest>
{
    public UpdateSubjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
    }
}

public class AssignTeacherRequestValidator : AbstractValidator<AssignTeacherRequest>
{
    public AssignTeacherRequestValidator()
    {
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.ClassCourseId).NotEmpty();
    }
}
