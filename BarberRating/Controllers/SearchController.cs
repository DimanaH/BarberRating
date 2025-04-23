using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarberRating.Data;
using BarberRating.Models;

public class SearchController : Controller
{
    private readonly ApplicationDbContext _context;

    public SearchController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Търсене по име
    public async Task<IActionResult> Index(string query)
    {
        var barbers = string.IsNullOrEmpty(query)
            ? new List<Barber>()
            : await _context.Barbers
                .Where(b => b.Name.Contains(query))
                .ToListAsync();

        ViewBag.Query = query;
        return View(barbers);
    }
}
