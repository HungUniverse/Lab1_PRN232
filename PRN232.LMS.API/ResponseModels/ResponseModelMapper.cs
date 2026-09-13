using PRN232.Lab1.Service.Business;

namespace PRN232.Lab1.API.ResponseModels;

public static class ResponseModelMapper
{
    public static StudentResponse ToResponse(this StudentModel model, string? fields, string? expand)
    {
        var selected = Fields(fields, "studentId", "fullName", "email", "dateOfBirth");
        return new StudentResponse
        {
            StudentId = selected.Contains("studentId") ? model.StudentId : null,
            FullName = selected.Contains("fullName") ? model.FullName : null,
            Email = selected.Contains("email") ? model.Email : null,
            DateOfBirth = selected.Contains("dateOfBirth") ? model.DateOfBirth : null,
            Enrollments = Expanded(expand, "enrollments") ? Map(model.Enrollments) : null
        };
    }

    public static StudentDetailResponse ToDetailResponse(this StudentModel model) => new()
    {
        StudentId = model.StudentId,
        FullName = model.FullName,
        Email = model.Email,
        DateOfBirth = model.DateOfBirth,
        Enrollments = Map(model.Enrollments)
    };

    public static SemesterResponse ToResponse(this SemesterModel model, string? fields, string? expand)
    {
        var selected = Fields(fields, "semesterId", "semesterName", "startDate", "endDate");
        return new SemesterResponse
        {
            SemesterId = selected.Contains("semesterId") ? model.SemesterId : null,
            SemesterName = selected.Contains("semesterName") ? model.SemesterName : null,
            StartDate = selected.Contains("startDate") ? model.StartDate : null,
            EndDate = selected.Contains("endDate") ? model.EndDate : null,
            Courses = Expanded(expand, "courses") ? Map(model.Courses) : null
        };
    }

    public static SemesterDetailResponse ToDetailResponse(this SemesterModel model) => new()
    {
        SemesterId = model.SemesterId,
        SemesterName = model.SemesterName,
        StartDate = model.StartDate,
        EndDate = model.EndDate,
        Courses = Map(model.Courses)
    };

    public static SubjectResponse ToResponse(this SubjectModel model, string? fields, string? expand)
    {
        var selected = Fields(fields, "subjectId", "subjectCode", "subjectName", "credit");
        return new SubjectResponse
        {
            SubjectId = selected.Contains("subjectId") ? model.SubjectId : null,
            SubjectCode = selected.Contains("subjectCode") ? model.SubjectCode : null,
            SubjectName = selected.Contains("subjectName") ? model.SubjectName : null,
            Credit = selected.Contains("credit") ? model.Credit : null,
            Courses = Expanded(expand, "courses") ? Map(model.Courses) : null
        };
    }

    public static SubjectDetailResponse ToDetailResponse(this SubjectModel model) => new()
    {
        SubjectId = model.SubjectId,
        SubjectCode = model.SubjectCode,
        SubjectName = model.SubjectName,
        Credit = model.Credit,
        Courses = Map(model.Courses)
    };

    public static CourseResponse ToResponse(this CourseModel model, string? fields, string? expand)
    {
        var selected = Fields(fields, "courseId", "courseName", "semesterId", "subjectId");
        return new CourseResponse
        {
            CourseId = selected.Contains("courseId") ? model.CourseId : null,
            CourseName = selected.Contains("courseName") ? model.CourseName : null,
            SemesterId = selected.Contains("semesterId") ? model.SemesterId : null,
            SubjectId = selected.Contains("subjectId") ? model.SubjectId : null,
            Semester = Expanded(expand, "semester") ? MapSemester(model) : null,
            Subject = Expanded(expand, "subject") ? MapSubject(model) : null,
            Enrollments = Expanded(expand, "enrollments") ? Map(model.Enrollments) : null
        };
    }

    public static CourseDetailResponse ToDetailResponse(this CourseModel model) => new()
    {
        CourseId = model.CourseId,
        CourseName = model.CourseName,
        SemesterId = model.SemesterId,
        SubjectId = model.SubjectId,
        Semester = MapSemester(model),
        Subject = MapSubject(model),
        Enrollments = Map(model.Enrollments)
    };

    public static EnrollmentResponse ToResponse(this EnrollmentModel model, string? fields, string? expand)
    {
        var selected = Fields(fields, "enrollmentId", "studentId", "courseId", "enrollDate", "status");
        return new EnrollmentResponse
        {
            EnrollmentId = selected.Contains("enrollmentId") ? model.EnrollmentId : null,
            StudentId = selected.Contains("studentId") ? model.StudentId : null,
            CourseId = selected.Contains("courseId") ? model.CourseId : null,
            EnrollDate = selected.Contains("enrollDate") ? model.EnrollDate : null,
            Status = selected.Contains("status") ? model.Status : null,
            Student = Expanded(expand, "student") ? Map(model.Student!) : null,
            Course = Expanded(expand, "course") ? Map(model.Course!) : null
        };
    }

    public static EnrollmentDetailResponse ToDetailResponse(this EnrollmentModel model) => new()
    {
        EnrollmentId = model.EnrollmentId,
        StudentId = model.StudentId,
        CourseId = model.CourseId,
        EnrollDate = model.EnrollDate,
        Status = model.Status,
        Student = Map(model.Student!),
        Course = Map(model.Course!)
    };

    private static HashSet<string> Fields(string? fields, params string[] defaults)
    {
        return string.IsNullOrWhiteSpace(fields)
            ? defaults.ToHashSet(StringComparer.OrdinalIgnoreCase)
            : fields.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool Expanded(string? expand, string value)
    {
        return !string.IsNullOrWhiteSpace(expand) && expand
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Contains(value, StringComparer.OrdinalIgnoreCase);
    }

    private static StudentEnrollmentResponse[] Map(IEnumerable<StudentEnrollmentModel> models) => models
        .Select(model => new StudentEnrollmentResponse
        {
            EnrollmentId = model.EnrollmentId,
            CourseId = model.CourseId,
            CourseName = model.CourseName,
            EnrollDate = model.EnrollDate,
            Status = model.Status
        }).ToArray();

    private static CourseSummaryResponse[] Map(IEnumerable<CourseSummaryModel> models) => models
        .Select(model => new CourseSummaryResponse
        {
            CourseId = model.CourseId,
            CourseName = model.CourseName,
            SemesterId = model.SemesterId,
            SubjectId = model.SubjectId
        }).ToArray();

    private static CourseEnrollmentResponse[] Map(IEnumerable<CourseEnrollmentModel> models) => models
        .Select(model => new CourseEnrollmentResponse
        {
            EnrollmentId = model.EnrollmentId,
            StudentId = model.StudentId,
            StudentName = model.StudentName,
            EnrollDate = model.EnrollDate,
            Status = model.Status
        }).ToArray();

    private static SemesterSummaryResponse MapSemester(CourseModel model) => new()
    {
        SemesterId = model.SemesterId,
        SemesterName = model.SemesterName ?? string.Empty
    };

    private static SubjectSummaryResponse MapSubject(CourseModel model) => new()
    {
        SubjectId = model.SubjectId,
        SubjectCode = model.SubjectCode ?? string.Empty,
        SubjectName = model.SubjectName ?? string.Empty
    };

    private static StudentSummaryResponse Map(StudentSummaryModel model) => new()
    {
        StudentId = model.StudentId,
        FullName = model.FullName,
        Email = model.Email
    };

    private static EnrollmentCourseResponse Map(EnrollmentCourseModel model) => new()
    {
        CourseId = model.CourseId,
        CourseName = model.CourseName,
        SemesterId = model.SemesterId,
        SemesterName = model.SemesterName,
        SubjectId = model.SubjectId,
        SubjectCode = model.SubjectCode,
        SubjectName = model.SubjectName
    };
}
