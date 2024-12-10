using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eticaret.Core.Entities;
using Eticaret.Data;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList
using Eticaret.WebUI.Utils;
using Microsoft.AspNetCore.Authorization;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Policy = "AdminPolicy")]
    public class CategoriesController : Controller
    {
        private readonly DatabaseContext _context;

        public CategoriesController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            ViewBag.Kategoriler = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                category.Image = await FileHelper.FileLoaderAsync(Image, "/Img/Categories/");
                await _context.AddAsync(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Kategoriler = new SelectList(_context.Categories, "Id", "Name");
            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewBag.Kategoriler = new SelectList(_context.Categories, "Id", "Name");
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category, IFormFile? Image, bool cbResmiSil = false)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (cbResmiSil)
                        category.Image = string.Empty;
                    if (Image is not null)
                        category.Image = await FileHelper.FileLoaderAsync(Image, "/Img/Categories/");
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Kategoriler = new SelectList(_context.Categories, "Id", "Name");
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                if (!string.IsNullOrEmpty(category.Image))
                {
                    FileHelper.FileRemover(category.Image, "/Img/Categories/");
                }
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
