using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using System.Linq;
using System.Threading.Tasks;

namespace HomeOwners.Controllers
{
    public class PollController : Controller
    {
        private readonly HomeDbContext _context;

        public PollController(HomeDbContext context)
        {
            _context = context;
        }

        // Show poll and options
        public async Task<IActionResult> Index()
        {
            var poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync();

            return View(poll);
        }

        // Handle vote submission
        [HttpPost]
        public async Task<IActionResult> Vote(int optionId)
        {
            var option = await _context.PollOptions.FindAsync(optionId);
            if (option != null)
            {
                option.Votes++;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Results");
        }

        // Show poll results
        public async Task<IActionResult> Results()
        {
            var poll = await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync();

            return View(poll);
        }
    }
}
