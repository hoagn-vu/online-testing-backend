using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Backend_online_testing.Dtos;
using Backend_online_testing.Models;

namespace Backend_online_testing.Services;

public class AuthService
{
    private readonly IMongoCollection<UsersModel> _usersCollection;
    private readonly IConfiguration _config;

    public AuthService(IMongoDatabase database, IConfiguration config)
    {
        _usersCollection = database.GetCollection<UsersModel>("users");
        _config = config;
    }
    
    public async Task<AuthResponseDto?> Authenticate(string username, string password)
    {
        var user = await _usersCollection.Find(u => u.UserName == username).FirstOrDefaultAsync();
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            return null;

        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        // Lưu refresh token vào DB
        var update = Builders<UsersModel>.Update
            .Set(u => u.RefreshToken, refreshToken)
            .Set(u => u.TokenExpiration, DateTime.UtcNow.AddDays(int.Parse(_config["JwtSettings:RefreshTokenExpirationDays"] ?? string.Empty)));

        await _usersCollection.UpdateOneAsync(u => u.Id == user.Id, update);

        return new AuthResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
    }
    
    public async Task<AuthResponseDto?> RefreshToken(string refreshToken)
    {
        var user = await _usersCollection.Find(u => u.RefreshToken == refreshToken).FirstOrDefaultAsync();
        if (user == null || user.TokenExpiration < DateTime.UtcNow)
            return null;

        var newAccessToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        var update = Builders<UsersModel>.Update
            .Set(u => u.RefreshToken, newRefreshToken)
            .Set(u => u.TokenExpiration, DateTime.UtcNow.AddDays(int.Parse(_config["JwtSettings:RefreshTokenExpirationDays"] ?? string.Empty)));

        await _usersCollection.UpdateOneAsync(u => u.Id == user.Id, update);

        return new AuthResponseDto { AccessToken = newAccessToken, RefreshToken = newRefreshToken };
    }

    public async Task<UserDto?> GetUserProfile(string userId)
    {
        var projection = Builders<UsersModel>.Projection
            .Expression(u => new UserDto
            {
                Id = u.Id.ToString(),
                UserName = u.UserName,
                FullName = u.FullName,
                Role = u.Role ?? string.Empty,
                UserCode = u.UserCode,
                Gender = u.Gender ?? string.Empty,
                DateOfBirth = u.DateOfBirth ?? string.Empty,
                GroupName = u.GroupName ?? new List<string>(),
                Authenticate = u.Authenticate ?? new List<string>(),
                AccountStatus = u.AccountStatus
            });

        var user = await _usersCollection
            .Find(u => u.Id == userId)
            .Project<UserDto>(projection)
            .FirstOrDefaultAsync();
        
        return user;
    }

    private string GenerateJwtToken(UsersModel user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? string.Empty);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["AccessTokenExpirationMinutes"])),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        return Convert.ToBase64String(randomNumber);
    }
}
