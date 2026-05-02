namespace AuthService.Exceptions;

public class CreateProfileException : Exception
{
    public CreateProfileException(string message) : base(message) { }
}