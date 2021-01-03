using System;
using System.DirectoryServices.AccountManagement;

namespace ITHelper.Models
{
    public class UserAccount
    {
        public Guid Id { get; set; }
        //
        // Summary:
        //     Gets a user principal object that represents the current user under which the
        //     thread is running.
        //
        // Returns:
        //     A System.DirectoryServices.AccountManagement.UserPrincipal representing the current
        //     user.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        //
        //   T:System.DirectoryServices.AccountManagement.NoMatchingPrincipalException:
        //     The user principal object for the current user could not be found. The principal
        //     object may contain an access control list to prevent access by unauthorized users.
        //
        //   T:System.DirectoryServices.AccountManagement.MultipleMatchesException:
        //     Multiple user principal objects matching the current user were found.
        public static UserPrincipal Current { get; }
        //
        // Summary:
        //     Gets or sets the middle name for the user principal.
        //
        // Returns:
        //     The middle name of the user principal.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string MiddleName { get; set; }
        //
        // Summary:
        //     Gets or sets the given name for the user principal.
        //
        // Returns:
        //     The given name of the user principal.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string GivenName { get; set; }
        //
        // Summary:
        //     Gets or sets the employee ID for this user principal.
        //
        // Returns:
        //     The employee ID of the user principal.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string EmployeeId { get; set; }
        //
        // Summary:
        //     Gets or sets the email address for this account.
        //
        // Returns:
        //     The email address of the user principal.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string EmailAddress { get; set; }
        //
        // Summary:
        //     Returns an System.DirectoryServices.AccountManagement.AdvancedFilters object
        //     to set read-only properties before passing the object to the System.DirectoryServices.AccountManagement.PrincipalSearcher.
        //
        // Returns:
        //     An System.DirectoryServices.AccountManagement.AdvancedFilters object.
        public string VoiceTelephoneNumber { get; set; }
        //
        // Summary:
        //     Gets or sets the surname for the user principal.
        //
        // Returns:
        //     The surname of the user principal.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The underlying store does not support this property.
        public string Surname { get; set; }
    }
}
