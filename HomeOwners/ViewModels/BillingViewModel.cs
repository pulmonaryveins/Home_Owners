// HomeOwners/ViewModels/BillingViewModel.cs
using System.Collections.Generic;
using HomeOwners.Models;

namespace HomeOwners.ViewModels
{
    public class BillingViewModel
    {
        public List<Booking> UnpaidBookings { get; set; } = new List<Booking>();
        public List<Payment> Payments { get; set; } = new List<Payment>();
        public decimal TotalUnpaidAmount { get; set; }
    }
}