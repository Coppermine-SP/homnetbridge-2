using System.ComponentModel.DataAnnotations;

namespace CloudInteractive.HomNetBridge.Models;

public class Car
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(20)]
    public string? LicensePlate { get; set; }
    
    [Required]
    [StringLength(20)]
    public string? HaEntityName { get; set; }
    
    public bool EntryStatus { get; set; }
}