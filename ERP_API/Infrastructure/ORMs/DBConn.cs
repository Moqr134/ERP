namespace Infrastructure.ORM;

public enum ConnectionType
{
    Local,
}

public static class DBConn
{
    public static readonly ConnectionType ConnectionType = ConnectionType.Local;

    public static string ConnectionString => ConnectionType switch
    {
        ConnectionType.Local => "Server=localhost\\SQLEXPRESS; Database=ERPProject; " +
            "Trusted_Connection=True; MultipleActiveResultSets=true; TrustServerCertificate=true",

        _ => "",
    };
    public readonly static string SecretKey = "cdAsuIt+MtE6pyD78t8W3tvBgtef+4l/2sFpWG4hloj" + "7Tre4iTOKtVWJY8xbbwvblGlkgJzQZdcR3Lqm/bgtSu==";
}
