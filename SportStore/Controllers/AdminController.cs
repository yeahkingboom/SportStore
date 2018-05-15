using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportStore;
using SportStore.Models;

namespace SportStore.Controllers
{
    [Authorize(Roles ="Administrator")]
    public class AdminController : Controller
    {
        private readonly SportStoreDbContext _context;

        public AdminController(SportStoreDbContext context)
        {
            _context = context;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            //var s = _context.Shops.Include(x => x.Merchant).GroupBy(x => x.Check);
            ViewBag.Shop = _context.Shops.Include(x=>x.Merchant).ToList();
            return View(_context.Applies.Include(x=>x.Account).Where(x=>x.Status==ApplyStatus.Uncheck).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Check(long id,bool check)
        {

            Result result = new Result { Success = false };
            try
            {
                
                var userid = _context.Accounts.Where(x => x.Apply.ID == id).Select(x=>x.ID).FirstOrDefault();
                var apply = _context.Applies.SingleOrDefault(s => s.ID == id);
                if (apply == null) throw new Exception("信息异常！");
                apply.Status = check ? ApplyStatus.True : ApplyStatus.False;
                if (check)
                {
                    _context.Database.ExecuteSqlCommand($"EXECUTE [dbo].[MerchantRegister] @ID={userid},@Role='Merchant'");
                    dynamic account = _context.Accounts.SingleOrDefault(x => x.ID == userid);
                    Shop shop = new Shop
                    {
                        CreateTime = DateTime.Now,
                        Name = apply.DocumentName,
                        Merchant = account,
                        ShopDiscount = 10,
                        Check = true
                    };
                    account.Shop = shop;
                    _context.Shops.Add(shop);
                }
                _context.SaveChanges();
                result.Success = true;
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            ViewBag.Result = result;
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> MallInfo()
        {
            return View(_context.MallInfo.FirstOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> MallInfo(MallInfo mallInfo)
        {
            _context.MallInfo.Update(mallInfo);
            _context.SaveChanges();
            return View(mallInfo);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUser(string keyword)
        {
            if (!string.IsNullOrEmpty(keyword))
                return View(_context.Accounts.Where(x => x.Name.Contains(keyword) &&x.Name!="NULL").ToList());
            else
                return View(_context.Accounts.Where(x=>x.Name!="NULL").ToList());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(long id,string newpassword)
        {
            Result result = new Result { Success = false };
            try
            {
                var account = _context.Accounts.SingleOrDefault(x => x.ID == id&&x.Name!="NULL");
                if (account == null) throw new Exception("账号信息异常！");
                account.Password = newpassword.Trim();
                _context.SaveChanges();
                result.Success = true;
            }
            catch(Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return RedirectToAction("ManageUser","Admin");
        }


        public async Task<IActionResult> DeleteUser(long id)
        {
            Result result = new Result { Success = false };
            try
            {
                var account = _context.Accounts.SingleOrDefault(x => x.ID == id && x.Name != "NULL");
                if (account == null) throw new Exception("账号信息异常！");
                account.Name = "NULL";
                _context.SaveChanges();
                result.Success = true;
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return RedirectToAction("ManageUser", "Admin");
        }

        [HttpPost]
        public async Task<JsonResult> AddCategory(string name)
        {
            Result result = new Result { Success = false };
            try {
                var c = _context.Categories.SingleOrDefault(x => x.Name == name.Trim());
                if (c != null) throw new Exception("该类别已经存在！");
                _context.Categories.Add(new Models.Category { Name = name.Trim() });
                _context.SaveChanges();
                result.Success = true;
            }
            catch(Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return Json(result);
        }

        public async Task<IActionResult> Category()
        {
            return View(await _context.Categories.ToListAsync());
        }


        public async Task<IActionResult> Freeze(long id)
        {

                var shop = _context.Shops.SingleOrDefault(x=>x.ID==id);
                if (shop == null)
                {
                return Content("商店信息异常");
                }
            shop.Status = -1;
            _context.SaveChanges();
            return RedirectToAction("Index", "Admin");
            
        }

        public async Task<IActionResult> UnFreeze(long id)
        {
            var shop = _context.Shops.SingleOrDefault(x => x.ID == id);
            if (shop == null)
            {
                return Content("商店信息异常");
            }
            shop.Status = 0;
            _context.SaveChanges();
            return RedirectToAction("Index", "Admin");
        }
        // GET: Admin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .SingleOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.SingleOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name")] Category category)
        {
            if (id != category.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.ID))
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
            return View(category);
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .SingleOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.SingleOrDefaultAsync(m => m.ID == id);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.ID == id);
        }
    }
}
