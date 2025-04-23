using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarberRating.Data;
using BarberRating.Models;

[Authorize]
public class ReviewController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Review/Create/5
    public async Task<IActionResult> Create(int id)
    {
        var barber = await _context.Barbers.FindAsync(id);
        if (barber == null) return NotFound();

        ViewBag.BarberId = barber.Id;
        ViewBag.BarberName = barber.Name;
        return View();
    }

    // POST: Review/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int id, [Bind("Content,Rating")] Review review)
    {
        var barber = await _context.Barbers.FindAsync(id);
        if (barber == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);

        review.UserId = user.Id;
        review.BarberId = barber.Id;
        review.CreatedAt = DateTime.UtcNow;

        if (ModelState.IsValid)
        {
            _context.Add(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "BarberPublic", new { id = barber.Id });
        }

        ViewBag.BarberId = barber.Id;
        ViewBag.BarberName = barber.Name;
        return View(review);
    }


        public async Task<IActionResult> MyReviews()
        {
            var user = await _userManager.GetUserAsync(User);
            var myReviews = await _context.Reviews
                .Include(r => r.Barber)
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(myReviews);
        }

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var review = await _context.Reviews
            .Include(r => r.Barber)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

        if (review == null)
            return NotFound();

        return View(review);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Review updatedReview)
    {
        var user = await _userManager.GetUserAsync(User);
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

        if (review == null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(updatedReview);

        review.Content = updatedReview.Content;
        review.Rating = updatedReview.Rating;
        //review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(MyReviews));
    }

    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var review = await _context.Reviews
            .Include(r => r.Barber)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

        if (review == null)
            return NotFound();

        return View(review);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

        if (review == null)
            return NotFound();

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(MyReviews));
    }


}
