using System;
using System.Collections.Generic;

namespace assignmentAPI.Models;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string UserRole { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
