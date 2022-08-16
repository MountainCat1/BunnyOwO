namespace BunnyOwO.Exceptions;

public class EventValidationException : Exception
{
    public EventValidationException()
    {
    }

    public EventValidationException(string? message) : base(message)
    {
    }

    public EventValidationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}