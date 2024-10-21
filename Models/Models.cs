using System.ComponentModel.DataAnnotations;

namespace Poliklinika.Models;
public class EditorModel
{
    public required Patient Patient { get; set; }
    public Dictionary<uint, Diagnose> Diagnoses { get; set; } = new();
    //public bool Test { get; set; } = false;
}
public record Patient
{
    [Required]
    public uint PatientId { get; set; }
    [Required]
    public string? FirstName { get; set; }
    [Required]
    public string? LastName { get; set; }
    [Required]
    public DateOnly BirthDate { get; set; }
}
public record Diagnose
{
    public string? DiagnoseName { get; set; }
    [Required]
    public bool Checked { get; set; } = false;
}
