namespace APIPMSoftware.Src.Application.DTO
{
    public class JwtSettings
    {
        public string key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
    }
}
