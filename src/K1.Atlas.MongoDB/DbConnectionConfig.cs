namespace K1.Atlas.MongoDB;

public class DbConnectionConfig
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string Database { get; set; } = null!;
}
