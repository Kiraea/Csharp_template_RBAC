using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
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

        var token = _tokenService.GenerateAccessToken(appUser, userRoles); 
        
         
        
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
        var accessToken = _tokenService.GenerateAccessToken(findUserResult, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        
        // store refreshtoken to db
        findUserResult.RefreshToken = refreshToken;
        findUserResult.RefreshTokenExpiry = DateTime.UtcNow.AddHours(5);
        await _userManager.UpdateAsync(findUserResult);
        
        // make refreshtokens and access tokens put them in cookie and set to browser
        _tokenService.PutTokensInsideCookie(accessToken, refreshToken, HttpContext);
        // dont need to return but for testing 
        return Ok(new LoginUserResponse
        {
            Email = request.Email,
            RefreshToken = refreshToken,
            AccessToken = accessToken 
        });
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<dynamic>> RefreshToken(RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany((e) => e.Errors).Select(e => e.ErrorMessage);
            return BadRequest(errors);
        }

        ClaimsPrincipal principal;
        try
        {
            principal = _tokenService.GetPrincipalFromExpiredAccessToken(request.AccessToken);

        }
        catch (SecurityTokenException)
        {
            return Unauthorized("Invalid AccessToken");
        }
        catch (Exception)
        {
            return Unauthorized("Processing Error");
        }

        //                                                          ? to returnnull if ever .Value is because claim to retrieve value is .Value 
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        
        // if u use userId.IsNullOrEmpty() this function is not directly to .NetRuntime so better to use
        // string.IsNuLLeMPTY(USERID) so that it can recognize null statistics
        
        if (string.IsNullOrEmpty(userId)) {
            return BadRequest("Invalid Tokens");
        }
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow) {
            return BadRequest("Invalid Tokens");
        }

        var roles = await _userManager.GetRolesAsync(user);
        
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        
        // store refreshtoken to db
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(5);
        await _userManager.UpdateAsync(user);
        
        // make refreshtokens and access tokens put them in cookie and set to browser
        _tokenService.PutTokensInsideCookie(accessToken, refreshToken, HttpContext);

        return null;
    }

    [HttpPost("/forgot-password")]
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
        var callback = QueryHelpers.AddQueryString("http://localhost:5246", queryParams!);
        await _emailSender.SendEmailAsync(request.Email, "Reset Password", $"Please Reset YourPassWord: {queryParams}");
        
        return Ok(new { Token = resetToken, Email = request.Email});

    }

    [HttpPost("/forgot-password-callback")]
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