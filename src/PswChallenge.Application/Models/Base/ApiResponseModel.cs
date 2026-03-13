using System.Diagnostics.CodeAnalysis;

namespace PswChallenge.Application.Models.Base;

[ExcludeFromCodeCoverage]
public sealed class ApiResponseModel<T>(bool succeeded, T? data, IEnumerable<string> messages)
{
    public bool Succeeded { get; set; } = succeeded;
    public T? Data { get; set; } = data;
    public IEnumerable<string> Messages { get; set; } = messages;

    public static ApiResponseModel<T> Success(T data, IEnumerable<string>? messages = null)
    {
        return new ApiResponseModel<T>(true, data, messages ?? []);
    }

    public static ApiResponseModel<T> Failure(IEnumerable<string> messages)
    {
        return new ApiResponseModel<T>(false, default, messages);
    }
}