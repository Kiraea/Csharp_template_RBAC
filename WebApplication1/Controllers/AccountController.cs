using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dtos.Account;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Route("/api/[controller]")]
public class AccountController : ControllerBase
{

    private readonly ITokenService _tokenService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    
    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _signInManager = signInManager;
    }
    
    [HttpPost("/register")]
    public async Task<ActionResult<string>> Register([FromBody] RegisterUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = string.Join(", ", (ModelState.Values.SelectMany((e) => e.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(modelErrors);
        }

        var appUser = new AppUser()
        {
            Email = request.Email,
            UserName = request.Email,
            DisplayName = request.DisplayName,
        };

        var createdUser = await _userManager.CreateAsync(appUser, request.Password);

        if (!createdUser.Succeeded)
        {
            return BadRequest(string.Join(": ", createdUser.Errors.Select(e => e.Description)));
        }

        var rolesResult = await _userManager.AddToRoleAsync(appUser, "Member");
        if (!rolesResult.Succeeded)
        {
            return BadRequest(string.Join(": ", rolesResult.Errors.Select(e => e.Description)));
        }

        var userRoles = await _userManager.GetRolesAsync(appUser);

        var token = _tokenService.GenerateToken(appUser, userRoles); 
        
         
        
        return Ok(new RegisterUserResponse()
        {
            DisplayName = request.DisplayName,
            Email = request.Email,
            Token = token
        });
    }

    [HttpPost("/login")]
    public async Task<ActionResult<dynamic>> Login([FromBody] LoginUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(" ", ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage));
            return BadRequest(errors);
        }

        var findUserResult = await _userManager.FindByEmailAsync(request.Email);

        if (findUserResult == null)
        {
            return Unauthorized("Invalid Username/Password");
        }

        var checkPassResult = await _userManager.CheckPasswordAsync(findUserResult, request.Password);

        if (checkPassResult == false)
        {
            
            return Unauthorized("Invalid Username/Password");
        }

        var roles = await _userManager.GetRolesAsync(findUserResult);
        var token = _tokenService.GenerateToken(findUserResult, roles);
        return Ok(new LoginUserResponse
        {
            Email = request.Email,
            token = token
        });
    }
}