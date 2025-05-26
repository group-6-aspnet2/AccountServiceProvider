using Data;
using Domain.Models;
using Domain.Responses;
using System.Diagnostics;

namespace Business.Services;
public interface IAccountApiService
{
    Task<AccountResult<Account>> GetAccountByIdAsync(string id);
    Task<AccountResult<IEnumerable<Account>>> GetAllAccountsAsync();
}

public class AccountApiService(IAccountRepository accountRepository) : IAccountApiService
{
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async Task<AccountResult<Account>> GetAccountByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return new AccountResult<Account>
                {
                    Succeeded = false,
                    StatusCode = 400,
                    Error = "Account ID cannot be null or empty."
                };

            var result = await _accountRepository.GetAccountById(id);

            if (!result.Succeeded)
                return new AccountResult<Account>
                {
                    Succeeded = false,
                    StatusCode = result.StatusCode,
                    Error = result.Error
                };

            return new AccountResult<Account>
            {
                StatusCode = result.StatusCode,
                Succeeded = true,
                Result = result.Result
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new AccountResult<Account>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = ex.Message
            };
        }
    }

    public async Task<AccountResult<IEnumerable<Account>>> GetAllAccountsAsync()
    {
        try
        {
            var result = await _accountRepository.GetAllAcounts(); 
            if (!result.Succeeded)
                return new AccountResult<IEnumerable<Account>>
                {
                    Succeeded = false,
                    StatusCode = result.StatusCode,
                    Error = result.Error
                };

            return new AccountResult<IEnumerable<Account>>
            {
                StatusCode = result.StatusCode,
                Succeeded = true,
                Result = result.Result
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new AccountResult<IEnumerable<Account>>
            {
                Succeeded = false,
                StatusCode = 500,
                Error = ex.Message
            };
        }
    }
}
