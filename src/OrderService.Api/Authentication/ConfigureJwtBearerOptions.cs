using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderService.Infrastructure.Auth;

namespace OrderService.Api.Authentication;

public sealed class ConfigureJwtBearerOptions(IOptions<JwtOptions> options) : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions jwt = options.Value;

    public void Configure(string? name, JwtBearerOptions options)
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            NameClaimType = JwtClaimNames.Subject,
            RoleClaimType = JwtClaimNames.Role
        };
    }

    public void Configure(JwtBearerOptions options) => Configure(Options.DefaultName, options);
}
