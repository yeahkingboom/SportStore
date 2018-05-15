using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportStore;
using SportStore.Models;

namespace SportStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly SportStoreDbContext _context;

        public ProductController(SportStoreDbContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index(long? id)
        {
            if (id.HasValue)
            {
                var Product = _context.Products.Include(x=>x.Category).Include(x=>x.Status).Include(x => x.Shop).ThenInclude(x=>x.Merchant).Include(x => x.Images).SingleOrDefault(x => x.ID == id);
                if (Product.Shop.Status == -1) return Content("该商店已被冻结！");
                Product.Comments = _context.Comments.Include(x => x.Account).Include(x => x.Replies).ThenInclude(r => r.Account).Where(x => x.Product == Product).OrderBy(x => x.CreateTime).ToList();
                if (Product == null)
                    return NotFound();
                return View(Product);
            }
            return NotFound();
        }

       
        [HttpGet]
        public async Task<IActionResult> Comment(long? id)
        {
            if (id.HasValue)
            {
                var Product = _context.Products.Include(x => x.Category).Include(x => x.Shop).Include(x => x.Images).SingleOrDefault(x => x.ID == id);
                if (Product == null)
                    return NotFound();
                return View(Product);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Comment(long? id,string content)
        {
            if (id.HasValue)
            {
                var product = _context.Products.SingleOrDefault(x => x.ID == id);
                if (product == null)
                    return NotFound();
                Comment comment = new Comment
                {
                    CreateTime = DateTime.Now,
                    Account = _context.Accounts.SingleOrDefault(x => x.Name == User.Identity.Name),
                    Content = content.Trim(),
                    Product = product
                };
                _context.Comments.Add(comment);
                _context.SaveChanges();
                return RedirectToAction("Index", "Order");
            }
            return NotFound();
        }

    }
}
