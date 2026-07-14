using Domin.TokenDto;
using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.UsersEntity;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Infrastructure.JWT;

public class Jwt : IScopped
{
    private readonly DBContext dbContext;
    private readonly byte[] symmetricKey;
    private readonly JwtSettings settings;

    public Jwt(DBContext context, IOptions<JwtSettings> jwtOptions)
    {
        dbContext = context;
        settings = jwtOptions.Value;
        symmetricKey = Convert.FromBase64String(settings.SecretKey);
    }

    private string GenerateToken(Users user, List<Permission> permations)
    {
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(symmetricKey);
        string algorithms = SecurityAlgorithms.HmacSha256Signature;
        var distinctPermissions = permations
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("USERNAME", user.Username),
            new Claim("ID", user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        ];
        for (int i = 0; i < distinctPermissions.Count; i++)
        {
            claims.Add(new Claim(ClaimTypes.Role, distinctPermissions[i].Name));
        }
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(settings.AccessTokenMinutes),
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(securityKey, algorithms)
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken stoken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(stoken);
    }

    public int ValidateToken(string? jwtToken)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
            throw new UnauthorizedAccessException("رمز المصادقة غير موجود");

        TokenValidationParameters validationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidAudience = settings.Audience,
            ValidIssuer = settings.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(symmetricKey),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
        ClaimsPrincipal principal = new JwtSecurityTokenHandler()
            .ValidateToken(jwtToken, validationParameters, out SecurityToken validatedToken);
        return Convert.ToInt32(principal?.FindFirst("ID")?.Value);
    }

    private Users? ValidateRefreshToken(Users? user)
    {
        if (user is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return null;
        }

        return user;
    }

    public async Task<TokenResponseDto?> RefreshTokensAsync(Users request, List<Permission> permations)
    {
        var user = ValidateRefreshToken(request);
        if (user is null)
            return null;

        return await CreateTokenResponse(user, permations);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(Users user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = TokenHasher.Hash(refreshToken);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(settings.RefreshTokenDays);
        await dbContext.SaveChangesAsync();
        return refreshToken;
    }

    public async Task<TokenResponseDto?> CreateTokenResponse(Users? user, List<Permission> permations)
    {
        if (user is null)
        {
            return null;
        }
        return new TokenResponseDto
        {
            AccessToken = GenerateToken(user, permations),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
        };
    }

    public int AccessTokenMinutes => settings.AccessTokenMinutes;
    public int RefreshTokenDays => settings.RefreshTokenDays;
}
