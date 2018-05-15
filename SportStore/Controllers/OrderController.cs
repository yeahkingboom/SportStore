using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportStore.Models;

namespace SportStore.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {

        private readonly SportStoreDbContext _context;

        public OrderController(SportStoreDbContext context)
        {
            _context = context;
        }

        private  Account _account; 

        public Account CurrentAccount { get
            {
                if (_account == null)
                {
                    _account = _context.Accounts.SingleOrDefault(x => x.Name == User.Identity.Name);
                    return _account;
                }
                else
                    return _account;
            } }



        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Orders.Include(x=>x.Receiver).Include(x=>x.Shop).Include(x=>x.Status).Include(x=>x.OrderItems).ThenInclude(x=>x.Product).ThenInclude(x=>x.Images).Where(x=>x.Customer==CurrentAccount).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmOrder()
        {
            Result result = new Result { Success = false };
            List<Order> order;
            try
            {

                var status= _context.OrderStatuses.SingleOrDefault(x => x.ID == (long)OrdStatus.UnPay);
                order = _context.Orders.Include(x=>x.OrderItems).ThenInclude(x=>x.Product).Where(x => x.Customer == CurrentAccount && x.Status.ID == (long)OrdStatus.New).ToList();
                if(order.Count==0) throw new Exception("暂无需确认订单！");
                foreach (var item in order)
                {
                    
                    foreach(var pro in item.OrderItems) {
                        if (_context.Database.ExecuteSqlCommand($"EXECUTE [dbo].[CheckOut] @ID={pro.Product.ID},@Count={pro.Count}") == -1)
                            throw new Exception("库存数小于购买数");
                    }
                    item.Status = status;
                    _context.SaveChanges();
                }
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
                return Content(result.ErrorMsg);
            }
            return View("Payment",order);
        }


        public async Task<IActionResult> ConfirmDelivery(long id)
        {
            try
            {
                var order = _context.Orders.Include(x => x.Status).SingleOrDefault(x => x.ID == id && x.Customer.Name == User.Identity.Name);
                if (order == null)
                {
                    throw new Exception("订单信息有误！");
                }

                order.Status = _context.OrderStatuses.SingleOrDefault(x => x.ID == (long)OrdStatus.AlreadyDone);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
            return RedirectToAction("Index", "Order");
        }

        [HttpGet]
        public async Task<IActionResult> Payment(long []id)
        {
            List<Order> OrderList = new List<Order>();
            foreach(var i in id)
            {
                var order = _context.Orders.SingleOrDefault(x => x.ID == i&&x.PayMent.UnPay);
                if (order != null) OrderList.Add(order);
            }
            if (OrderList.Count == 0) return Content("暂无要支付订单");
            return View(OrderList);
        }

        [HttpPost]
        public async Task<IActionResult> Payment(long []id,string payway)
        {
            Result result = new Result { Success = false };
            Payment payment = new Payment();
            payment.Orders = new List<Order>();
            payment.UnPay = true;
            payment.Account = _context.Accounts.SingleOrDefault(x => x.Name == User.Identity.Name);
            //var delPayment = _context.Payments.Where(x => x.Account.Name == User.Identity.Name);
            //_context.Payments.RemoveRange(delPayment);
            //_context.SaveChanges();
            if (payway == "货到付款")
            {
                payment.PayWay = PayWay.Cash;
            }
            else
            {
                payment.PayWay = PayWay.Online;
            }
            try
            {
                Order order;
                foreach (var i in id)
                {
                    order = _context.Orders.SingleOrDefault(x => x.ID == i);
                    if (order == null) throw new Exception("订单ID有误!");
                    payment.Orders.Add(order);
                    payment.Total += order.TotalPrice;
                }
                _context.Payments.Add(payment);
                _context.SaveChanges();
                result.Success = true;
                result.ErrorMsg = "/Order/GetQrCode/" + payment.ID;
                return Json(result);
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Pay(long id)
        {
            var payment=_context.Payments.Include(x=>x.Orders).SingleOrDefault(x => x.ID == id && x.UnPay);
            if (payment == null)
            {
                return Content("暂无要支付订单");
            }
            if (payment.Total > payment.Orders.Sum(x => x.TotalPrice))
            {
                return Content("包含了已支付订单！");
            }
            return View(payment);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmPay(long id)
        {
            var payment = _context.Payments.Include(x=>x.Orders).SingleOrDefault(x => x.ID == id && x.UnPay);
            if (payment == null)
            {
                return Content("暂无要支付订单");
            }
            var status = _context.OrderStatuses.SingleOrDefault(x => x.ID == (long)OrdStatus.UnDelivery);
            foreach(var item in payment.Orders)
            {
                item.Status = status;
            }
            payment.UnPay = false;
            _context.SaveChanges();
            return RedirectToAction("Index","Order");
        }

        [HttpGet]
        public async Task<IActionResult> GetQrCode(long id, [FromServices]IHostingEnvironment environment)
        {
            string domain;
            if (environment.IsDevelopment())
            {
                domain = "localhost:63687";
            }
            else
            {
                domain = "123.207.97.94";
            }
            var QrBitmap = OrderQRCode.OrderQRCode.GetQrCode(domain, "Order", "Pay",id);
            var ms = new MemoryStream();
            QrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            QrBitmap.Dispose();
            return File(ms.ToArray(),"image/jpeg");
        }

        public async Task<IActionResult> Cancel(long id)
        {
            try
            {
                var order = _context.Orders.Include(x=>x.Status).SingleOrDefault(x => x.ID == id && x.Customer.Name == User.Identity.Name);
                if (order == null)
                {
                    throw new Exception("订单信息有误！");
                }
                    
                order.Status = _context.OrderStatuses.SingleOrDefault(x => x.ID == (long)OrdStatus.Closing);
                var orderitem = _context.OrderItems.Include(x=>x.Product).Where(x => x.Order == order);
                foreach(var item in orderitem)
                {
                    item.Product.Stocks += item.Count;
                }
                _context.SaveChanges();
            }
            catch(Exception e)
            {
                return Content(e.Message);
            }
            return RedirectToAction("Index","Order");
        }

        public async Task<IActionResult> Delivery(long? id)
        {
            try
            {
                if (id.HasValue)
                {
                    var order = _context.Orders.Include(x => x.Status).SingleOrDefault(x => x.ID == id && x.Shop.Merchant.Name == User.Identity.Name);
                    if (order == null)
                        throw new Exception("订单信息异常!");
                    order.Status = _context.OrderStatuses.SingleOrDefault(x => x.ID == (long)OrdStatus.Delivering);
                    _context.SaveChanges();
                }
                else
                    throw new Exception("订单信息异常!");
                return RedirectToAction("ManageOrder", "Shop");
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
        }

        public async Task<IActionResult> ConfirmClose(long? id)
        {
            try
            {
                if (id.HasValue)
                {
                    var order = _context.Orders.Include(x => x.Status).SingleOrDefault(x => x.ID == id && x.Shop.Merchant.Name == User.Identity.Name);
                    if (order == null)
                        throw new Exception("订单信息异常!");
                    order.Status = _context.OrderStatuses.SingleOrDefault(x => x.ID == (long)OrdStatus.Closed);
                    _context.SaveChanges();
                }
                else
                    throw new Exception("订单信息异常!");
                return RedirectToAction("ManageOrder", "Shop");
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
        }

        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var order = _context.Orders.Include(x=>x.OrderItems).Include(x => x.Status).SingleOrDefault(x => x.ID == id && x.Customer.Name == User.Identity.Name);
                if (order == null)
                {
                    order = _context.Orders.Include(x => x.OrderItems).Include(x => x.Status).SingleOrDefault(x => x.ID == id && x.Shop.Merchant.Name == User.Identity.Name);
                    if (order == null)
                    throw new Exception("订单信息有误！");
                }
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
            return RedirectToAction("Index", "Order");
        }

    }
}