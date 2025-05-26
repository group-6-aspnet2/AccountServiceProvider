using Data.Contexts;
using Domain.Models;
using Domain.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Data;
public interface IAccountRepository
{
	Task<RepositoryResult<Account>> GetAccountById(string id);
}

public class AccountRepository(DataContext context) : IAccountRepository
{
    private readonly DataContext _context = context;

    public async Task<RepositoryResult<Account>> GetAccountById(string id)
    {
		try
		{
			var identityUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
			if(identityUser == null)
				return new RepositoryResult<Account> { Succeeded = false, StatusCode = 404, Error="Identity User could not be found."};

			return new RepositoryResult<Account>
			{
				Result = new Account
				{
					Id = identityUser.Id,
					UserName = identityUser.UserName,
					Email = identityUser.Email,
					PhoneNumber = identityUser.PhoneNumber
				},
				StatusCode = 200,
				Succeeded = true
            };
        }
		catch (Exception ex)
		{
			Debug.WriteLine($"Error in GetAccountById: {ex.Message}");
			return null!;
		}
    }
}
