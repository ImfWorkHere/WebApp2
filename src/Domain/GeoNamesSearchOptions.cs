namespace Domain;

public class GeoNamesSearchOptions
{
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; } = 0;
    public double Longitude { get; set; } = 0;
    public double Radius { get; set; } = 20;
    public uint MinimumPopulation { get; set; } = 1500;
    public uint MaximumPopulation { get; set; } = uint.MaxValue;
    public int Count { get; set; } = 20;
}