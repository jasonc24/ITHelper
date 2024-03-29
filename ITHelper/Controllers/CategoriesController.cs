﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITHelper.Data;
using ITHelper.Models;
using Microsoft.AspNetCore.Authorization;

namespace ITHelper.Controllers
{
    [Authorize(Roles = "Domain Admins")]
    public class CategoriesController : MyController
    {
        public CategoriesController(ITHelperContext context) : base(context)
        { }

        // GET: Categories
        [AllowAnonymous]
        [Route("~/Categories/Index/{showDeleted?}/{pageNo:int?}")]
        public async Task<IActionResult> Index(bool showDeleted = false, int pageNo = 0)
        {
            var categories = await _context.Categories
                .Where(x => !x.Deleted)
                .Include(a => a.ParentCategory)
                .OrderBy(x => x.ParentCategory.Name)
                .ThenBy(x => x.Name)
                .ToListAsync();

            if(showDeleted)
            {
                var deletedCategories = await _context.Categories
                .Where(x => x.Deleted)
                .Include(a => a.ParentCategory)
                .OrderBy(x => x.ParentCategory.Name)
                .ThenBy(x => x.Name)
                .ToListAsync();

                categories = categories.Union(deletedCategories).ToList();
            }
            var itemsPerPage = GetItemsPerPage();
            var totalItems = categories.Count();
            pageNo = SetPageInformation(pageNo, totalItems, itemsPerPage);
            ViewBag.baseURL = $"/ITHelper/Categories/Index/{showDeleted}";

            return View(categories.Skip(itemsPerPage * pageNo).Take(itemsPerPage));
        }

        // GET: Categories/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            { return NotFound(); }

            var category = await _context.Categories
                .Where(x => x.Id.Equals(id))
                .Include(a => a.ParentCategory)
                .FirstOrDefaultAsync();
            if (category == null)
            { return NotFound(); }

            category.ParentCategories = await GetParentCategoriesAsync(category.ParentCategory?.Id, category.Id);

            return View(category);
        }

        // GET: Categories/Create
        public async Task<IActionResult> Create()
        {
            var category = new Category();
            category.ParentCategories = await GetParentCategoriesAsync(null, null);
            return View(category);
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                category.Id = Guid.NewGuid();
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            { return NotFound(); }

            var category = await _context.Categories
                .Where(x => x.Id.Equals(id))
                .Include(a => a.ParentCategory)
                .FirstOrDefaultAsync();
            if (category == null)
            { return NotFound(); }

            category.ParentCategories = await GetParentCategoriesAsync(category.ParentCategory?.Id, category.Id);

            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Category category)
        {
            if (id != category.Id)
            { return NotFound(); }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    { return NotFound(); }
                    else
                    { throw; }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            { return NotFound(); }

            var category = await _context.Categories
                .Where(x => x.Id.Equals(id))
                .Include(a => a.ParentCategory)
                .FirstOrDefaultAsync();
            if (category == null)
            { return NotFound(); }

            category.ParentCategories = await GetParentCategoriesAsync(category.ParentCategory?.Id, category.Id);

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            category.Deleted = true;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(Guid id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        /// <summary>
        /// Returns a list of categories for the application to use
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <param name="excludedItem"></param>
        /// <returns></returns>
        protected new async Task<List<SelectListItem>> GetParentCategoriesAsync(Guid? selectedItem, Guid? excludedItem)
        {
            var categories = new List<SelectListItem>();
            categories.Add(new SelectListItem() { Text = "Please Select...", Value = "", Selected = selectedItem == null });

            categories.AddRange(await _context.Categories
                .Where(w => !w.Deleted && (w.ParentCategory == null))
                .OrderBy(x => x.Name)
                .Select(y => new SelectListItem()
                {
                    Text = y.Name,
                    Value = y.Id.ToString(),
                    Selected = y.Id.Equals(selectedItem),
                    Disabled = y.Id.Equals(excludedItem)
                })
                .ToListAsync());

            return categories;
        }
    }
}
