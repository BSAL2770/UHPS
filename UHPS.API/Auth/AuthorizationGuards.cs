namespace UHPS.API.Auth;

public static class AuthorizationGuards
{
    /// Throws ForbiddenAccessException unless the current user is in one of the privileged roles
    /// or owns the resource (current user's UserId matches resourceOwnerUserId).
    public static void EnsureOwnerOrInRole(
        ICurrentUser currentUser,
        int? resourceOwnerUserId,
        params string[] privilegedRoles)
    {
        if (privilegedRoles.Length > 0 && currentUser.IsInRole(privilegedRoles))
            return;

        if (resourceOwnerUserId.HasValue
            && currentUser.UserId.HasValue
            && currentUser.UserId.Value == resourceOwnerUserId.Value)
            return;

        throw new ForbiddenAccessException();
    }
}
