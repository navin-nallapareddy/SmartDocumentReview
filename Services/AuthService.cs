public class AuthService
{
    private string _currentUser;

    public Task LoginAsync(string username)
    {
        _currentUser = username;
        return Task.CompletedTask;
    }

    public string GetCurrentUser() => _currentUser ?? "anonymous";
    public bool IsLoggedIn() => !string.IsNullOrEmpty(_currentUser);
}