namespace Renty.Server.IntegrationTests.Common;

public static class HttpClientAuthExtensions
{
    public static void AuthenticateAs(this HttpClient client, Guid userId, params string[] roles)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);

        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());

        if (roles.Length > 0)
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, string.Join(',', roles));
    }
}
