using System;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
