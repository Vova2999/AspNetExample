using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AspNetExample;

public static class Constants
{
    public const string DescSuffix = "Desc";

    public const int FirstPage = 1;
    public const int PageSize = 20;

    public const string MultiAuthScheme = "MultiAuthScheme";

    public const string JwtIssuer = "AspNetExample";
    public const string JwtAudience = "AspNetExampleClient";
    public static readonly TimeSpan JwtLifetime = TimeSpan.FromMinutes(600);
    private const string JwtKey = "AnySecretKeyForAspNetExample!123456";

    public static SymmetricSecurityKey GetJwtSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
    }
}