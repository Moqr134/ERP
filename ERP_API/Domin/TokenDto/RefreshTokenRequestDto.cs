namespace Domin.TokenDto;

public class RefreshTokenRequestDto
{
    public int UserId { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; } 
}
