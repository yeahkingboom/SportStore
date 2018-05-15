using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportStore;
using SportStore.Models;

namespace SportStore.Controllers
{
    //[Authorize]
    public class AccountController : Controller
    {
        private readonly SportStoreDbContext _context;

        public AccountController(SportStoreDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password,string ReturnUrl)
        {
            //Result result = new Result { Success = false };
            try
            {
                var account = _context.Accounts.SingleOrDefault(x => x.Name == username.Trim() && x.Password == password.Trim());
                if (account == null)
                    throw new Exception("用户名或密码错误！");
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, account.Name));
                identity.AddClaim(new Claim(ClaimTypes.SerialNumber, account.ID.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Role, account.GetType().ToString().Split('.').Last()));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            }
            catch(Exception e)
            {
                ViewBag.Msg = e.Message;
                return View();
            }
            if (ReturnUrl == null)
                return RedirectToAction("Index", "Home");
            else
                return  Redirect(ReturnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
              
        }
        // GET: Account
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(_context.Accounts.SingleOrDefault(x=>x.Name==User.Identity.Name));
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            try
            {
                if (CheckUser(user))
                {
                    user = CompleteUser(user);
                    _context.Accounts.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception e)
            {
                ViewBag.ErrorMsg = e.Message;
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Update(User user)
        {
            try
            {
                user.Email = user.Email.Trim();
                user.Phone = user.Phone.Trim();
                if (CheckUser(user))
                {
                    //user = CompleteUser(user);
                    _context.Accounts.Update(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Account");
                }
            }
            catch (Exception e)
            {
                ViewBag.ErrorMsg = e.Message;
            }

            return View("Index",user);
        }



        public User CompleteUser(User user)
        {
            user.Name = user.Name.Trim();
            user.Phone = user.Phone.Trim();
            user.Email = user.Phone.Trim();
            user.CreateTime = DateTime.Now;
            user.ShoppingCart = new Cart();
            return user;
        }

        public bool CheckUser(User user)
        {
            if (user.ID == 0)
            {
                if (_context.Accounts.Any(x => x.Name == user.Name.Trim())) throw new Exception("该用户名已被注册！");
                if (_context.Accounts.Any(x => x.Email == user.Email.Trim())) throw new Exception("该邮箱已被注册！");

            }
            else
            {
                if(_context.Accounts.Any(x=>x.ID!=user.ID&&x.Email==user.Email.Trim())) throw new Exception("该邮箱已被注册！");
            }
            
            return true;
        }
        private bool AccountExists(long id)
        {
            return _context.Accounts.Any(e => e.ID == id);
        }
    }
}
