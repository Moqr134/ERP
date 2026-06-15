using Domin.TokenDto;
using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.UsersEntity;
using Infrastructure.Logger;
using Infrastructure.ORM;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Infrastructure.JWT;

public class Jwt
{
    private DBContext dbContext;
    private readonly byte[] symmetricKey = Convert.FromBase64String(DBConn.SecretKey);
    public Jwt(DBContext context)
    {
        dbContext = context;
    }
    private string GenerateToken(Users user,List<Permation> permations)
    {
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(symmetricKey);
            string algorithms = SecurityAlgorithms.HmacSha256Signature;

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "ERP",
                Audience = "Users",
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(1),
                Subject = new ClaimsIdentity(new[] {
                new Claim("ID", user.Id.ToString()),
                new Claim("USERNAME", user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("Permations", string.Join(",", permations.Select(p => p.Name)))
            }),
                SigningCredentials = new SigningCredentials(securityKey, algorithms)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken stoken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(stoken);
    }
    public int ValidateToken(string jwtToken)
    {
        TokenValidationParameters validationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidAudience = "Users",
            ValidIssuer = "ERP",
            IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
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
    public async Task<TokenResponseDto?> RefreshTokensAsync(Users request, List<Permation> permations)
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
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await dbContext.SaveChangesAsync();
        return refreshToken;
    }
    public async Task<TokenResponseDto?> CreateTokenResponse(Users? user, List<Permation> permations)
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
}
