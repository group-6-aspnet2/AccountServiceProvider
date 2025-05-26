namespace Domain.Responses;

public class AccountResult : ResponseResult
{
}

public class AccountResult<T> : AccountResult
{
    public T? Result { get; set; }
}
