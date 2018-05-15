using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportStore.Models;

namespace SportStore.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly SportStoreDbContext _context;

        public HomeController(SportStoreDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(string KeyWord)
        {
            ViewBag.MallInfo = _context.MallInfo.SingleOrDefault();
            IEnumerable<Category> categories;
            if (string.IsNullOrEmpty(KeyWord))
            {
                categories = _context.Categories;
                foreach(var item in categories)
                {
                    item.Products = _context.Products.Include(x => x.Images).Where(x => x.Status.ID == (int)ProductStatus.热销&&x.Category==item&&x.Shop.Status!=-1).Take(8).ToList();
                }
            }
            else
            {
                ViewBag.Products = _context.Products.Include(x => x.Images).Where(x => (x.Shop.Status != -1&&x.Name.Contains(KeyWord)&& x.Status.ID != (int)ProductStatus.下架) || (x.Shop.Status != -1&&x.Category.Name.Contains(KeyWord) && x.Status.ID != (int)ProductStatus.下架)).ToList();
                categories = new List<Category>();
            }
            return View(categories);
        }


    }
}
