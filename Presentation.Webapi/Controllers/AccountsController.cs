using Business.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Presentation.Webapi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountsController(IAccountApiService accountApiService) : ControllerBase
{

    private readonly IAccountApiService _accountApiService = accountApiService;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOneAccount(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Account ID cannot be null or empty.");

            var result = await _accountApiService.GetAccountByIdAsync(id);

            return result.StatusCode switch
            {
                200 => Ok(result.Result),
                400 => BadRequest(result.Error),
                404 => NotFound(result.Error),
                _ => Problem(result.Error)
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return Problem(ex.Message, statusCode: 500);
        }
    }
}
