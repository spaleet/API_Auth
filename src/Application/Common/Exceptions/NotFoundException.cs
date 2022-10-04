namespace Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public static void ThrowIfNull(object? data, string message)
    {
        if (data is null)
            throw new NotFoundException(message);
    }
}
