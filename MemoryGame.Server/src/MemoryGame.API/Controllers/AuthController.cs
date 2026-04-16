using System.Security.Claims;
using MediatR;
using MemoryGame.Application.Auth.Commands.FinalizeRegistration;
using MemoryGame.Application.Auth.Commands.Register;
using MemoryGame.Application.Auth.Commands.ResendVerification;
using MemoryGame.Application.Auth.Commands.VerifyRegistration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemoryGame.API.Controllers;

/// <summary>
/// Handles authentication, registration, session management.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Initializes the controller with its MediatR sender dependency.
    /// </summary>
    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Starts the registration process by sending a verification PIN to the email.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var pin = await _mediator.Send(command);
        return Ok(new { Message = pin });
    }

    /// <summary>
    /// Validates the registration PIN without creating the user account.
    /// </summary>
    [HttpPost("verify-registration")]
    public async Task<IActionResult> VerifyRegistration([FromBody] VerifyRegistrationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Valid = result });
    }

    /// <summary>
    /// Completes the registration by validating the PIN and creating the user account.
    /// </summary>
    [HttpPost("finalize-registration")]
    public async Task<IActionResult> FinalizeRegistration([FromBody] FinalizeRegistrationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Resends the verification PIN to the given email address.
    /// </summary>
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationCommand command)
    {
        var pin = await _mediator.Send(command);
        return Ok(new { Message = pin });
    }
}
