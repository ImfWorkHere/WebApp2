using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain;

[Table("GeoNames", Schema = "dbo")]
public class GeoName
{
    [Required] [Column(TypeName = "int")] 
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "varchar(200)")]
    public string Name { get; set; }

    [Column(TypeName = "varchar(200)")] public string AsciiName { get; set; }

    [Column(TypeName = "varchar(5000)")] public string AlternativeNames { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 7)")]
    public double Latitude { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 7)")]
    public double Longitude { get; set; }

    [Column(TypeName = "char(1)")] public char FeatureClass { get; set; }

    [Column(TypeName = "varchar(10)")] public string FeatureCode { get; set; }

    [Column(TypeName = "char(2)")] public string CountryCode { get; set; }

    [Column(TypeName = "char(60)")] public string CountryCode2 { get; set; }

    [Column(TypeName = "varchar(20)")] public string AdminCode1 { get; set; }

    [Column(TypeName = "varchar(80)")] public string AdminCode2 { get; set; }

    [Column(TypeName = "varchar(20)")] public string AdminCode3 { get; set; }

    [Column(TypeName = "varchar(20)")] public string AdminCode4 { get; set; }

    [Column(TypeName = "bigint")] public long Population { get; set; }

    [Column(TypeName = "int")] public int? Elevation { get; set; }

    [Column(TypeName = "int")] public int Dem { get; set; }

    [Column(TypeName = "varchar(40)")] public string TimeZone { get; set; }

    [Column(TypeName = "date")] public DateTime ModificationDate { get; set; }
}