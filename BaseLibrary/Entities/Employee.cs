namespace BaseLibrary.Entities;
public class Employee : BaseEntity
{
    public string? GovId { get; set; }
    public string? FileNumber { get; set; }
    public string? FullName { get; set; }
    public string? JobTitle { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Photo { get; set; }
    public string? Other { get; set; }
}
