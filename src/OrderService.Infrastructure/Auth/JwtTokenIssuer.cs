using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OrderService.Application.Abstractions;
using OrderService.Application.Auth;
using OrderService.Application.Auth.Dtos;

namespace OrderService.Infrastructure.Auth;

public sealed class JwtTokenIssuer(IOptions<JwtOptions> options, TimeProvider clock) : ITokenIssuer
{
    private readonly JwtOptions jwt = options.Value;

    public TokenDto Issue(AuthenticatedUser user)
    {
        var now = clock.GetUtcNow();
        var expiresAt = now.AddMinutes(jwt.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtClaimNames.Subject, user.Username),
            new(JwtClaimNames.Role, user.Role)
        };

        if (user.CustomerId is not null)
        {
            claims.Add(new Claim(JwtClaimNames.CustomerId, user.CustomerId));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                SecurityAlgorithms.HmacSha256)
        };

        return new TokenDto(new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }
}
