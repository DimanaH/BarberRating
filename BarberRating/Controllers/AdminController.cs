using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarberRating.Data;
using BarberRating.Models;
using Microsoft.AspNetCore.Identity;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var userCount = await _context.Users.CountAsync();
        var barberCount = await _context.Barbers.CountAsync();
        var reviewCount = await _context.Reviews.CountAsync();

        var model = new AdminDashboardViewModel
        {
            UserCount = userCount,
            BarberCount = barberCount,
            ReviewCount = reviewCount
        };

        return View(model);
    }

    public async Task<IActionResult> Users()
    {
        var users = await _context.Users.ToListAsync();
        var userRoles = await _context.UserRoles.ToListAsync();
        var roles = await _context.Roles.ToListAsync();

        var result = users.Select(user =>
        {
            var roleId = userRoles.FirstOrDefault(ur => ur.UserId == user.Id)?.RoleId;
            var roleName = roles.FirstOrDefault(r => r.Id == roleId)?.Name;

            return new UserViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roleName
            };
        });

        return View(result);
    }

    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var roleId = await _context.UserRoles
            .Where(r => r.UserId == id)
            .Select(r => r.RoleId)
            .FirstOrDefaultAsync();

        var role = await _context.Roles.FindAsync(roleId);
        if (role?.Name == "Admin") return Forbid(); // Не редактираме админи

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(string id, ApplicationUser updated)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var roleId = await _context.UserRoles
            .Where(r => r.UserId == id)
            .Select(r => r.RoleId)
            .FirstOrDefaultAsync();

        var role = await _context.Roles.FindAsync(roleId);
        if (role?.Name == "Admin") return Forbid();

        if (!ModelState.IsValid) return View(updated);

        user.FirstName = updated.FirstName;
        user.LastName = updated.LastName;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var roleId = await _context.UserRoles
            .Where(r => r.UserId == id)
            .Select(r => r.RoleId)
            .FirstOrDefaultAsync();

        var role = await _context.Roles.FindAsync(roleId);
        if (role?.Name == "Admin") return Forbid();

        return View(user);
    }

    [HttpPost, ActionName("DeleteUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var roleId = await _context.UserRoles
            .Where(r => r.UserId == id)
            .Select(r => r.RoleId)
            .FirstOrDefaultAsync();

        var role = await _context.Roles.FindAsync(roleId);
        if (role?.Name == "Admin") return Forbid();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Users));
    }
    public async Task<IActionResult> Reviews()
    {
        var reviews = await _context.Reviews
            .Include(r => r.Barber)
            .Include(r => r.User)
            //.OrderByDescending(r => r.CreatedOn)
            .Select(r => new ReviewAdminViewModel
            {
                Id = r.Id,
                BarberName = r.Barber.Name,
                Content = r.Content,
                UserName = r.User.UserName,
                //CreatedOn = r.CreatedOn
            }).ToListAsync();

        return View(reviews);
    }
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.Barber)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null) return NotFound();

        return View(review);
    }

    [HttpPost, ActionName("DeleteReview")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReviewConfirmed(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Reviews));
    }

    // Списък
    public async Task<IActionResult> Barbers()
    {
        var barbers = await _context.Barbers.ToListAsync();
        return View(barbers);
    }

    // Създаване
    public IActionResult AddBarber() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBarber(BarberFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var imagePath = await SaveImage(model.Image);

        var barber = new Barber
        {
            Name = model.Name,
            Description = model.Description,
            PhotoPath = imagePath
        };

        _context.Barbers.Add(barber);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Barbers));
    }
    // Редактиране
    public async Task<IActionResult> EditBarber(int id)
    {
        var barber = await _context.Barbers.FindAsync(id);
        if (barber == null) return NotFound();

        return View(new BarberFormViewModel
        {
            Id = barber.Id,
            Name = barber.Name,
            Description = barber.Description,
            ExistingImagePath = barber.PhotoPath
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBarber(BarberFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var barber = await _context.Barbers.FindAsync(model.Id);
        if (barber == null) return NotFound();

        if (model.Image != null)
        {
            var newImagePath = await SaveImage(model.Image);
            barber.PhotoPath = newImagePath;
        }

        barber.Name = model.Name;
        barber.Description = model.Description;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Barbers));
    }

    // Изтриване
    public async Task<IActionResult> DeleteBarber(int id)
    {
        var barber = await _context.Barbers.FindAsync(id);
        if (barber == null) return NotFound();

        return View(barber);
    }

    [HttpPost, ActionName("DeleteBarber")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBarberConfirmed(int id)
    {
        var barber = await _context.Barbers.FindAsync(id);
        if (barber == null) return NotFound();

        _context.Barbers.Remove(barber);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Barbers));
    }
    private async Task<string?> SaveImage(IFormFile? image)
    {
        if (image == null || image.Length == 0) return null;

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
        var path = Path.Combine("wwwroot/images", fileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return "/images/" + fileName;
    }



}
