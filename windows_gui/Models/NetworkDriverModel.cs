namespace DNSChanger.Models
{
    public class NetworkDriverModel
    {
        public string InterfaceId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LinkSpeedDisplay { get; set; } = string.Empty;
        public bool IsUp { get; set; }
    }
}
