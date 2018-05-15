using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportStore;
using SportStore.Models;

namespace SportStore.Controllers
{
    
    public class ShopController : Controller
    {
        private readonly SportStoreDbContext _context;

        public ShopController(SportStoreDbContext context)
        {
            _context = context;
        }

        private Shop _shop;

        public Shop CurrentShop
        {
            get
            {
                if(_shop==null)
                {
                    _shop = _context.Shops.SingleOrDefault(x => x.Merchant.Name == User.Identity.Name);
                    _shop.Orders = _context.Orders.Include(x=>x.Status).Include(x=>x.Receiver).Include(x=>x.OrderItems).ThenInclude(x=>x.Product).ThenInclude(x=>x.Images).Where(x => x.Shop == _shop).ToList();
                    return _shop;
                }
                else
                {
                    return _shop;
                }
            }
        }

        // GET: Shop
        [AllowAnonymous]
        public async Task<IActionResult> Index(long? shopId, long? productId,string KeyWord)
        {

            ViewBag.Category = _context.Categories.OrderBy(x => x.ID).ToList();
            try
            {
                if (!shopId.HasValue)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var shop = _context.Shops.Include(s => s.Merchant).SingleOrDefault(x => x.ID == shopId);
                    if (shop == null) throw new Exception("商店不存在!");
                    if (shop.Check == false) throw new Exception("商店未通过审核!");
                    if (shop.Status == -1) throw new Exception("商店已被冻结！");
                    if (productId.HasValue)
                    {
                        ViewBag.Products = _context.Products.Include(x => x.Category).Include(x => x.Images).Where(x => x.ID == productId && x.Status.ID != (int)ProductStatus.下架).ToList();
                    }
                    else if (!string.IsNullOrEmpty(KeyWord))
                    {
                        ViewBag.Products = _context.Products.Include(x => x.Images).Include(x => x.Category).Where(x => (x.Shop==shop&&x.Name.Contains(KeyWord) && x.Status.ID != (int)ProductStatus.下架) || (x.Shop == shop && x.Category.Name.Contains(KeyWord) && x.Status.ID != (int)ProductStatus.下架)).ToList();
                    }
                    else
                        ViewBag.Products = _context.Products.Include(x => x.Images).Include(x => x.Category).Where(x => x.Shop == shop && x.Status.ID != (int)ProductStatus.下架).ToList();
                    return View(shop);
                }
                
                
            }
            catch(Exception e)
            {
                return Content(e.Message);
            }
           
        }


        [Authorize(Roles ="Merchant")]
        [HttpGet]
        public async Task<IActionResult> UpdateShopInfo()
        {
            return View(await _context.Shops.SingleOrDefaultAsync(x => x.Merchant.Name == User.Identity.Name));
        }

        [Authorize(Roles = "Merchant")]
        [HttpPost]
        public async Task<IActionResult> UpdateShopInfo(Shop shop)
        {
            try
            {
                _context.Shops.Update(shop);
                _context.SaveChanges();
                return RedirectToAction("MyShop", "Shop");
            }
            catch(Exception e)
            {
                return Content(e.Message);
            }
            
        }
        public async Task<IActionResult> ManageOrder()
        {
            return View(CurrentShop.Orders.ToList());
        }

        [Authorize(Roles = "Merchant")]
        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            ViewBag.Category = _context.Categories.OrderBy(x => x.ID).ToList();
            return View();
        }

        [Authorize(Roles = "Merchant")]
        [HttpPost]
        public async Task<IActionResult> AddProduct(Products product, [FromServices]IHostingEnvironment environment)
        {
            Result result = new Result { Success = false };
            var imgs = Request.Form.Files.Where(c => c.Name == "image");

            string path = string.Empty;
            try
            {
                if (imgs == null || imgs.Count() <= 0) throw new Exception("请选择上传的文件。");
                List<Image> images = new List<Image>();
                if (imgs.All(x=>x.ContentType== "image/jpeg"||x.ContentType== "image/png"))
                {
                    if (imgs.Sum(x=>x.Length) <= 1024 * 1024 * 4)
                    {
                        var shop = _context.Shops.Include(x=>x.Products).SingleOrDefault(x => x.Merchant.Name == User.Identity.Name);
                        product.Category = _context.Categories.FirstOrDefault(x => x.ID == product.Category.ID);
                        product.Status = _context.Statuses.FirstOrDefault(x => x.ID == 2);
                        foreach (var img in imgs)
                        {
                            string strpath = Path.Combine("UpLoad", DateTime.Now.ToString("MMddHHmmss") + img.FileName);
                            path = Path.Combine(environment.WebRootPath, strpath);
                            images.Add(new Image { Url = "\\"+strpath });
                            using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                              await img.CopyToAsync(stream);
                            }
                        }
                        product.Images = images;
                        shop.Products.Add(product);
                        await _context.SaveChangesAsync();
                        result.Success = true;
                    }
                    else
                    {
                        throw new Exception("图片不能大于4MB");
                        
                    }
                }
                else

                {
                    throw new Exception("请上传jpg或png图片");
                }
            }
            catch(Exception e)
            {
                result.ErrorMsg = e.Message;
            }

            //ViewBag.Category = _context.Categories.OrderBy(x => x.ID).ToList();
            ViewBag.Category = _context.Categories.OrderBy(x => x.ID).ToList();
            return View();
        }

        [Authorize(Roles = "Merchant")]
        [HttpGet]
        public async Task<IActionResult> MyShop(string KeyWord)
        {
            var shop = _context.Shops.Include(s => s.Merchant).SingleOrDefault(x => x.Merchant.Name == User.Identity.Name);
            ViewBag.Category = _context.Categories.OrderBy(x => x.ID).ToList();
            if (!string.IsNullOrEmpty(KeyWord))
            {
                ViewBag.Products= _context.Products.Include(x => x.Images).Include(x=>x.Category).Where(x =>(x.Shop.Merchant.Name== User.Identity.Name&&x.Name.Contains(KeyWord) || x.Shop.Merchant.Name == User.Identity.Name && x.Category.Name.Contains(KeyWord))).ToList();
                
            }
            else
                ViewBag.Products = _context.Products.Include(x => x.Images).Include(x=>x.Category).Where(x => (x.Shop.Merchant.Name == User.Identity.Name)).ToList();
            return View(shop);
        }

        [Authorize(Roles = "Merchant")]
        [HttpGet]
        public async Task<IActionResult> Stocks(string KeyWord)
        {
            List<Products> products;
            if (string.IsNullOrEmpty(KeyWord))
            {
                products = _context.Products.Include(p=>p.Category).Where(x => x.Shop.Merchant.Name == User.Identity.Name).OrderByDescending(x => x.Category.ID).ThenByDescending(x=>x.Stocks).ToList();
            }
            else
            {
                products = _context.Products.Include(p => p.Category).Where(x => x.Shop.Merchant.Name == User.Identity.Name&&x.Name.Contains(KeyWord)).OrderByDescending(x => x.Category.ID).ThenByDescending(x => x.Stocks).ToList();
            }
            return View(products);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MerchantRegister()
        {
            return View(_context.Applies.SingleOrDefault(x=>x.Account.Name==User.Identity.Name));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MerchantRegister(Apply apply, [FromServices]IHostingEnvironment environment)
        {
            
            Result result = new Result { Success = false };
            var zips = Request.Form.Files.Where(c => c.Name == "document");
            string path = string.Empty;
            try
            {
                var user = _context.Accounts.Include(x=>x.Apply).SingleOrDefault(x => x.Name == User.Identity.Name);
                if (user == null) throw new Exception("用户信息异常！");
                if (zips != null || zips.Count() >= 0)
                {
                    if (zips.All(x => x.ContentType == "application/x-zip-compressed"))
                    {
                        if (zips.Sum(x => x.Length) <= 1024 * 1024 * 4)
                        {

                            //var shop = _context.Shops.Include(x => x.Products).SingleOrDefault(x => x.Merchant.Name == User.Identity.Name);

                            foreach (var zip in zips)
                            {
                                string strpath = Path.Combine("Apply", DateTime.Now.ToString("MMddHHmmss")+apply.DocumentName+".zip");
                                apply.CreateTime = DateTime.Now;
                                apply.Status = ApplyStatus.Uncheck;
                                path = Path.Combine(environment.WebRootPath, strpath);
                                apply.DocumentUrl = "\\" + strpath;
                                using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                                {
                                    await zip.CopyToAsync(stream);
                                }
                            }
                            if (user.Apply == null)
                            {
                                apply.Account = user;
                                _context.Applies.Add(apply);
                            }
                            else
                            {
                                user.Apply.CreateTime = apply.CreateTime;
                                user.Apply.DocumentName = apply.DocumentName;
                                user.Apply.DocumentUrl = apply.DocumentUrl;
                                user.Apply.Status = ApplyStatus.Uncheck;
                            }
                            

                        }
                        else
                        {
                            throw new Exception("压缩包不能大于4MB");

                        }
                    }
                }
                await _context.SaveChangesAsync();
                result.Success = true;
                return View(apply);
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
            }

            return View(apply);
        }

        // GET: Shop/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shop = await _context.Shops
                .Include(s => s.Merchant)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (shop == null)
            {
                return NotFound();
            }

            return View(shop);
        }



        // GET: Shop/Edit/5
        [Authorize(Roles = "Merchant")]
        [HttpGet]
        public async Task<IActionResult> EditProduct(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.Include(p=>p.Status).SingleOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Category = _context.Categories.OrderBy(x => x.ID).ToList();
            ViewBag.Status = _context.Statuses.OrderBy(x => x.ID).ToList();
            return View(product);
        }

        [Authorize(Roles = "Merchant")]
        [HttpPost]
        public async Task<IActionResult> EditProduct(Products pro, [FromServices]IHostingEnvironment environment)
        {
            Result result = new Result { Success = false };
            var imgs = Request.Form.Files.Where(c => c.Name == "image");

            string path = string.Empty;
            try
            {
                var product = _context.Products.Include(x => x.Images).SingleOrDefault(m => m.ID == pro.ID);
                product.Category = _context.Categories.FirstOrDefault(x => x.ID == pro.Category.ID);
                product.Status = _context.Statuses.FirstOrDefault(x => x.ID == pro.Status.ID);
                if (imgs != null || imgs.Count() >= 0)
                {
                    if (imgs.All(x => x.ContentType == "image/jpeg" || x.ContentType == "image/png"))
                    {
                        if (imgs.Sum(x => x.Length) <= 1024 * 1024 * 4)
                        {
                            
                            //var shop = _context.Shops.Include(x => x.Products).SingleOrDefault(x => x.Merchant.Name == User.Identity.Name);
                            
                            foreach (var img in imgs)
                            {
                                string strpath = Path.Combine("UpLoad", DateTime.Now.ToString("MMddHHmmss") + img.FileName);
                                path = Path.Combine(environment.WebRootPath, strpath);
                                product.Images.Add(new Image { Url = "\\" + strpath });
                                using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                                {
                                    await img.CopyToAsync(stream);
                                }
                            }
                            
                        }
                        else
                        {
                            throw new Exception("图片不能大于4MB");

                        }
                    }
                }
                product.Name = pro.Name;
                product.Price = pro.Price;
                product.Stocks = pro.Stocks;
                product.Description = pro.Description;
                await _context.SaveChangesAsync();
                result.Success = true;
                return RedirectToAction("Stocks", "Shop");
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
            }

            return View(pro);
        }

        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> ManageComment()
        {
            return View(_context.Comments.Include(x=>x.Account).Include(x => x.Product).Where(x => x.Product.Shop == CurrentShop&&!x.Replies.Any(p=>p.Account.Name==User.Identity.Name)).OrderBy(x=>x.CreateTime).ToList());
        }

        [Authorize(Roles = "Merchant")]
        [HttpPost]
        public async Task<IActionResult> ManageComment(long id,string content)
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Reply(long CommentID,string content)
        {
            Result result = new Result { Success = false };
            try
            {
                var account = _context.Accounts.SingleOrDefault(x => x.Name == User.Identity.Name);
                if (account == null) throw new Exception("账号信息异常！");
                var comment = _context.Comments.SingleOrDefault(x => x.ID == CommentID);
                if (comment == null) throw new Exception("评论不存在");
                Reply reply = new Reply { Account = account, Content = content, Comment = comment,CreateTime=DateTime.Now };
                _context.Replies.Add(reply);
                _context.SaveChanges();
                result.Success = true;
            }
            catch(Exception e)
            {
                result.ErrorMsg=e.Message;
            }
            return Json(result);
        }
    }
}
