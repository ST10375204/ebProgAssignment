using System;
using System.Collections.Generic;

namespace assignmentAPI.Models;

public partial class Item
{
    public int ItemId { get; set; }

    public string ItemName { get; set; } = null!;

    public string ItemCategory { get; set; } = null!;

    public string? ItemDesc { get; set; }

    public double? ItemPrice { get; set; }

    public string? ProductionDate { get; set; }

    public string? ImgUrl { get; set; }

    public string UserId { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
