using System;
using System.Collections.Generic;

namespace VKRvs.Models;

public partial class Reservation
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? ReservationDate { get; set; }

    public int? NumberOfPeople { get; set; }

    public virtual Customer? Customer { get; set; }
}
