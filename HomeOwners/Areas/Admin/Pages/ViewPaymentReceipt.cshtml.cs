// HomeOwners/Areas/Admin/Pages/ViewPaymentReceipt.cshtml.cs
using System;
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class ViewPaymentReceiptModel : PageModel
    {
        private readonly PaymentService _paymentService;

        public ViewPaymentReceiptModel(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public Payment Payment { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Payment = await _paymentService.GetPaymentByIdAsync(id);

            if (Payment == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}