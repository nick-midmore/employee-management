﻿using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IUserAccount accountInterface) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(Register user)
    {
        if (user == null) return BadRequest("Model is empty");
        var result = await accountInterface.RegisterAsync(user);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> SignInAsync(Login user)
    {
        if (user == null) return BadRequest("Model is empty");
        var result = await accountInterface.SignInAsync(user);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshTokenAsync(RefreshToken token)
    {
        if (token == null) return BadRequest("Model is empty");
        var result = await accountInterface.RefreshTokenAsync(token);
        return Ok(result);
    }
}
