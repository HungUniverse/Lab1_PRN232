namespace PRN232.Lab1.Repository.Entity;

public class Course
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;

    public int SemesterId { get; set; }
    public Semester Semester { get; set; } = null!;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
