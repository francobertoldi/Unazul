namespace Shared.Contract.Exceptions;

public abstract class DomainException(string code, string message, int httpStatusCode = 422) : Exception(message)
{
    public string Code { get; } = code;
    public int HttpStatusCode { get; } = httpStatusCode;
}

public class NotFoundException(string code, string message) : DomainException(code, message, 404);

public class ConflictException(string code, string message) : DomainException(code, message, 409);

public class ValidationException(string code, string message) : DomainException(code, message, 422);
