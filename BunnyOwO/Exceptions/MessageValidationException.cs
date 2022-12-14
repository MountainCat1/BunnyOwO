namespace BunnyOwO.Exceptions;

public class MessageValidationException : Exception
{
    public MessageValidationException()
    {
    }

    public MessageValidationException(string? message) : base(message)
    {
    }

    public MessageValidationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}