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
    [Authorize]
    public class AddressController : Controller
    {
        private readonly SportStoreDbContext _context;

        public AddressController(SportStoreDbContext context)
        {
            _context = context;
        }

        // GET: Address
        public async Task<IActionResult> Index()
        {
            return View(await _context.Receivers.Where(x=>x.Account.Name==User.Identity.Name).ToListAsync());
        }

        // GET: Address/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receiver = await _context.Receivers
                .SingleOrDefaultAsync(m => m.ID == id);
            if (receiver == null)
            {
                return NotFound();
            }

            return View(receiver);
        }

        // GET: Address/Create
        public IActionResult Create(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Address/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PhoneNumber,Name")] Receiver receiver,string Address,string Street,string returnUrl)
        {

            try
            {
                var ad = Address.Trim().Split('/');
                receiver.Address = ad[0].Trim() + ad[1].Trim() + ad[2].Trim() + Street.Trim();
                var account = _context.Accounts.SingleOrDefault(x => x.Name == User.Identity.Name);
                receiver.Account = account;
                _context.Receivers.Add(receiver);
                await _context.SaveChangesAsync();
                if (string.IsNullOrEmpty(returnUrl))
                    return RedirectToAction(nameof(Index));
                else
                    return Redirect(returnUrl);
            }
            catch (Exception e)
            {

            }
            return View(receiver);
        }

        // GET: Address/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receiver = await _context.Receivers.SingleOrDefaultAsync(m => m.ID == id);
            if (receiver == null)
            {
                return NotFound();
            }
            return View(receiver);
        }

        // POST: Address/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("ID,PhoneNumber,Address,Name")] Receiver receiver)
        {
            if (id != receiver.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(receiver);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReceiverExists(receiver.ID))
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
            return View(receiver);
        }


        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var receiver = await _context.Receivers.SingleOrDefaultAsync(m => m.ID == id && m.Account.Name == User.Identity.Name);
                if (receiver == null) throw new Exception("删除信息异常！");
                _context.Receivers.Remove(receiver);
                await _context.SaveChangesAsync();
                
            }
            catch(Exception e)
            {
                return Content(e.Message);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ReceiverExists(long id)
        {
            return _context.Receivers.Any(e => e.ID == id);
        }
    }
}
