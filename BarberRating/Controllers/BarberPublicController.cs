using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarberRating.Data;

public class BarberPublicController : Controller
{
    private readonly ApplicationDbContext _context;

    public BarberPublicController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Details(int id)
    {
        var barber = await _context.Barbers
            .Include(b => b.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (barber == null)
            return NotFound();

        return View(barber);
    }
}
