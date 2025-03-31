using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;
using AspNetExample.Domain.Dtos;
using AspNetExample.Domain.Entities;
using AspNetExample.Exceptions.Api;
using AspNetExample.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AspNetExample.Controllers.Api;

[ApiController]
[Route("api/account")]
[Produces(MediaTypeNames.Application.Json)]
public class AccountApiController : ControllerBase
{
    private readonly UserManager<User> _userManager;

    public AccountApiController(
        UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("login")]
    public async Task<TokenDto> Login(
        [FromBody] LoginDto login)
    {
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        var user = await _userManager.FindByNameAsync(login.Login);
        if (user == null || !await _userManager.CheckPasswordAsync(user, login.Password))
            throw new UnauthorizedException("Некорректные логин и(или) пароль");

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name)
            }
            .Concat(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var symmetricSecurityKey = Constants.GetJwtSymmetricSecurityKey();
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
            Constants.JwtIssuer,
            Constants.JwtAudience,
            claims,
            expires: DateTime.UtcNow.Add(Constants.JwtLifetime),
            signingCredentials: signingCredentials);

        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return new TokenDto { Token = token };
    }

    [HttpPost("register")]
    public async Task Register(
        [FromBody] RegisterDto register)
    {
        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        if (register.Password.Length < 6)
            ModelState.AddModelError(nameof(register.Password), "Минимальная длина пароля 6 символов");

        var conflictedUser = await _userManager.FindByNameAsync(register.Login);
        if (conflictedUser != null)
            ModelState.AddModelError(nameof(register.Login), "Логин уже зарегистрирован");

        if (!ModelState.IsValid)
            throw new BadRequestException(ModelState.JoinErrors());

        var user = new User { Id = Guid.NewGuid(), Name = register.Login };
        var result = await _userManager.CreateAsync(user, register.Password);
        if (!result.Succeeded)
            throw new InternalServerErrorException(result.Errors.JoinErrors());
    }
}