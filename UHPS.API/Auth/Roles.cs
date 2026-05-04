namespace UHPS.API.Auth;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Supervisor = "Supervisor";
    public const string Employee = "Employee";
    public const string Customer = "Customer";

    public const string AdminOnly = Admin;
    public const string AdminOrSupervisor = $"{Admin},{Supervisor}";
    public const string Staff = $"{Admin},{Supervisor},{Employee}";
}
