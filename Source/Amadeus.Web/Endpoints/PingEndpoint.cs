namespace Mozart.Web;

public static class PingEndpoint
{
    public static IResult Get()
    {
        return Results.Ok(new { success = true, message = "pong" });
    }
}
