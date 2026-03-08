namespace AirportLounge.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static Result<T> Success(T data, string? message = null)
        => new() { IsSuccess = true, Data = data, Message = message };

    public static Result<T> Failure(string message, List<string>? errors = null)
        => new() { IsSuccess = false, Message = message, Errors = errors };
}
