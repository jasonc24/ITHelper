using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITHelper.Data;
using ITHelper.Models;
using Microsoft.AspNetCore.Authorization;

namespace ITHelper.Controllers
{
    public class SystemParametersController : MyController
    {

        public SystemParametersController(ITHelperContext context) : base(context) { }

        // GET: SysParams
        [AllowAnonymous]
        [Route("~/SystemParameters/Index/{pageNo:int?}")]
        public async Task<ActionResult> Index(int pageNo = 0)
        {
            var sysParams = await _context.SystemParameters.ToListAsync();
            ViewBag.BaseURL = string.Format("/ITHelper/SystemParameters/Index");
            var itemsPerPage = GetItemsPerPage();
            SetPageInformation(pageNo, sysParams.Count, itemsPerPage);
            var labelList = sysParams.Skip(itemsPerPage * pageNo).Take(itemsPerPage).ToList();
            return View(labelList);
        }

        // GET: SysParams/Details/5
        [AllowAnonymous]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View(NotFound());
            }
            var sysParam = await _context.SystemParameters.FindAsync(id);
            if (sysParam == null)
            {
                return View(NotFound());
            }
            return View(sysParam);
        }

        // GET: SysParams/Create
        [Authorize(Roles = "Domain Admins")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: SysParams/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Domain Admins")]
        public async Task<ActionResult> Create(SystemParameter sysParam)
        {
            if (ModelState.IsValid)
            {
                //sysParam.Id = await GetMaxId() + 1;
                sysParam.DateCreated = DateTimeOffset.Now;
                sysParam.LastUpdated = DateTimeOffset.Now;
                sysParam.UpdatedBy = User.Identity.Name;
                _context.SystemParameters.Add(sysParam);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(sysParam);
        }

        // GET: SysParams/Edit/5
        [Authorize(Roles = "Domain Admins")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View(NotFound());
            }
            var sysParam = await _context.SystemParameters.FindAsync(id);
            if (sysParam == null)
            {
                return View(NotFound());
            }
            return View(sysParam);
        }

        // POST: SysParams/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Domain Admins")]
        public async Task<ActionResult> Edit(SystemParameter sysParam)
        {
            if (ModelState.IsValid)
            {
                sysParam.LastUpdated = DateTimeOffset.Now;
                sysParam.UpdatedBy = User.Identity.Name;
                _context.Entry(sysParam).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(sysParam);
        }

        // GET: SysParams/Delete/5
        [Authorize(Roles = "Domain Admins")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return View(NotFound());
            }
            var tSysParam = await _context.SystemParameters.FindAsync(id);
            if (tSysParam == null)
            {
                return View(NotFound());
            }
            return View(tSysParam);
        }

        // POST: SysParams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Domain Admins")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var sysParam = await _context.SystemParameters.FindAsync(id);
            _context.SystemParameters.Remove(sysParam);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Retrieves the max Id in the SystemParameters relation
        /// </summary>
        /// <returns></returns>
        protected async Task<int> GetMaxId()
        {
            try
            {
                var maxId = await _context.SystemParameters
                    .MaxAsync(x => x.Id);
                return (maxId);
            }
            catch(InvalidOperationException ioe) { return 1; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SystemParameterExists(int id)
        {
            return _context.SystemParameters.Any(e => e.Id == id);
        }
    }
}
