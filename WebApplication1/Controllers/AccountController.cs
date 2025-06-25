using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
    private readonly IEmailSender _emailSender;    
    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IEmailSender emailSender)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
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

    [HttpPost("/forgotpassword")]
    public async Task<ActionResult<dynamic>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany((e) => e.Errors).Select(e => e.ErrorMessage);
        }

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Unauthorized("Invalid Request");
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var queryParams = new Dictionary<string, string>
        {
            { "Token", resetToken },
            { "Email", request.Email }
        };
        // cant create since no frontend
        //var callback = QueryString.Create()
        // this is just to test

        await _emailSender.SendEmailAsync(request.Email, "Reset Password", $"Please Reset YourPassWord: {queryParams}");
        
        return Ok(new { Token = resetToken, Email = request.Email});

    }

    [HttpPost("/forgotpasswordcallback")]
    public async Task<ActionResult<dynamic>> ForgotPasswordCallback([FromBody] ForgotPasswordCallbackRequest request)
    {
       if (!ModelState.IsValid)
       {
           var errors = ModelState.Values.SelectMany((e) => e.Errors).Select((e) => e.ErrorMessage);
           return BadRequest(errors);
       }

       var userResult = await _userManager.FindByEmailAsync(request.Email);

       if (userResult == null)
       {
           return BadRequest("Invalid Request, Try the process again");
       }

       var checkTokenResult = await _userManager.ResetPasswordAsync(userResult, request.Token, request.Password);

       if (checkTokenResult.Succeeded == false)
       { 
           return BadRequest("Invalid Request, Try the process again");
       }

       return Ok("Password succesfully changed");
        
    }
}