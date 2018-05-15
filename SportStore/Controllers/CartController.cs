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
    [Authorize]
    public class CartController : Controller
    {
        private readonly SportStoreDbContext _context;

        public CartController(SportStoreDbContext context)
        {
            _context = context;
        }

        private Cart _CurrentShoppingCart;

        public Cart CurrentShoppingCart
        {
            get
            {
                if (_CurrentShoppingCart == null)
                {
                    _CurrentShoppingCart = _context.Carts.Include(x=>x.Item).ThenInclude(c=>c.Product).ThenInclude(p=>p.Shop).SingleOrDefault(x => x.Account.Name == User.Identity.Name);
                    if (_CurrentShoppingCart == null) _CurrentShoppingCart = new Cart { Account = _context.Accounts.SingleOrDefault(x => x.Name == User.Identity.Name) };
                    _context.SaveChanges();
                    foreach(var i in _CurrentShoppingCart.Item)
                    {
                        i.Product.Images = _context.Images.Where(x => x.Product == i.Product).ToList();
                    }
                    if (_CurrentShoppingCart == null)
                    {
                        var account=_context.Accounts.Include(x=>x.ShoppingCart).SingleOrDefault(x => x.Name == User.Identity.Name);
                        if (account.ShoppingCart == null)
                        {
                            account.ShoppingCart = new Cart();
                            _context.SaveChanges();
                            return account.ShoppingCart;
                        }
                        throw new Exception("账号信息异常！");
                    }
                    return _CurrentShoppingCart;
                }
                else
                    return _CurrentShoppingCart;
            
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.MallDiscount = _context.MallInfo.FirstOrDefault().Discount;
            return View(CurrentShoppingCart.Item);
            //return View();
        }

        

        [HttpPost]
        public async Task<IActionResult> SelectedItem(long? id,bool selected)
        {
            Result result = new Result { Success = false };
            try
            {
                if (!id.HasValue) throw new Exception("id为空！");
                var item = CurrentShoppingCart.Item.SingleOrDefault(x => x.ID == id);
                if (item == null) throw new Exception("无效id");
                item.Selected = selected;
                await _context.SaveChangesAsync();
                result.Success = true;
                
            }
            catch(Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> SelectedAllItem()
        {
            Result result = new Result { Success = false };
            try
            {
                var items = CurrentShoppingCart.Item;
                if (items == null) throw new Exception("购物车为空！");
                foreach(var item in items)
                    item.Selected = true;
                await _context.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> UnSelectedAllItem()
        {
            Result result = new Result { Success = false };
            try
            {
                var items = CurrentShoppingCart.Item;
                if (items == null) throw new Exception("购物车为空！");
                foreach (var item in items)
                    item.Selected = false;
                await _context.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return Json(result);
        }


        [HttpGet]
        public async Task<IActionResult> CheckOut()
        {
            ViewBag.MallDiscount = _context.MallInfo.FirstOrDefault().Discount;
            Result result = new Result { Success = false };
            try
            {
                var itemList = CurrentShoppingCart.Item.Where(x => x.Selected).ToList();
                ViewBag.ItemList = itemList.Count>0?itemList:throw new Exception("请选择要结算的商品");
                ViewBag.Receivers = _context.Receivers.Where(x => x.Account.Name == User.Identity.Name).ToList();
                result.Success = true;
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut(long receiverId)
        {
            var MallDiscount = _context.MallInfo.FirstOrDefault().Discount;
            Result result = new Result { Success = false };
            try
            {
                var receiver = _context.Receivers.SingleOrDefault(x => x.ID == receiverId);
                if (receiver == null) throw new Exception("收货人信息异常！");
                var selectedItems=CurrentShoppingCart.Item.Where(x => x.Selected);
                var shops = selectedItems.GroupBy(x=>x.Product.Shop);
                foreach(var shop in shops)
                {
                    var order = new Order()
                    {
                        Shop=shop.Key,
                        OrderItems = new List<OrderItem>(),
                        CreateTime = DateTime.Now,
                        Status = _context.OrderStatuses.SingleOrDefault(x => x.ID == (long)OrdStatus.New),
                        Receiver=receiver,
                        Customer= _context.Accounts.SingleOrDefault(x => x.Name == User.Identity.Name),
                        
                    };
                    foreach (var item in shop)
                    {
                        order.OrderItems.Add(new OrderItem
                        {
                            Count = item.Count,
                            Price = item.Product.Price * (decimal)(item.Product.Shop.ShopDiscount*MallDiscount) / 100,
                            TotalPrice = item.DiscountTotal*MallDiscount/10,
                            Product = item.Product,
                        });
                    }
                    order.TotalPrice = order.OrderItems.Sum(x => x.TotalPrice);
                    _context.Orders.Add(order);
                    _context.SaveChanges();
                }


            
            _context.CartItems.RemoveRange(selectedItems);
            result.Success = true;
            _context.SaveChanges();
            return RedirectToAction("ConfirmOrder", "Order");


        }
            catch(Exception e)
            {
                result.ErrorMsg = e.Message;
                return Content(e.Message);
            }

            //return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add(long productId,int count=1)
        {
            Result result = new Result { Success = false };
            try
            {
                var item = CurrentShoppingCart.Item.SingleOrDefault(x => x.Product.ID == productId);
                if (item == null)
                {
                    item = new CartItem { Product = _context.Products.SingleOrDefault(x => x.ID == productId)};
                    if (item.Product.Stocks < count) throw new Exception($"库存不足,库存为{item.Product.Stocks}");
                    item.Count = count;
                    CurrentShoppingCart.Item.Add(item);
                }
                else
                {
                    if (item.Product.Stocks - item.Count < count) throw new Exception($"库存不足,库存为{item.Product.Stocks}");
                        item.Count+=count;
                }
                await _context.SaveChangesAsync();
                result.Success = true;
            }
            catch(Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            Result result = new Result { Success = false };
            try
            {
                CurrentShoppingCart.Item.Clear();
                await _context.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Reduce(long productId)
        {
            Result result = new Result { Success = false };
            try
            {
                var item = CurrentShoppingCart.Item.SingleOrDefault(x => x.Product.ID == productId);
                if (item == null)
                {
                    throw new Exception("删除商品信息异常，请重试！");
                }
                else
                {
                    if (item.Count > 1)
                        item.Count--;
                    else
                        throw new Exception("商品数量最小为1！");
                }
                await _context.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeCount(long productId,int count)
        {
            Result result = new Result { Success = false };
            try
            {
                var item = CurrentShoppingCart.Item.SingleOrDefault(x => x.Product.ID == productId&&x.Product.Stocks>=count);
                if (item == null)
                {
                    throw new Exception("商品信息异常或数量超出库存，请重试！");
                }
                else
                {

                    if (count < 1)
                        item.Count=1;
                    else
                        item.Count=count;
                }
                await _context.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            Result result = new Result { Success = false };
            try
            {
                var item = _context.CartItems.SingleOrDefault(x => x.ID == id);
                if (item == null)
                {
                    throw new Exception("删除商品信息异常，请重试！");
                }
                else
                {
                    _context.CartItems.Remove(item);
                }
                await _context.SaveChangesAsync();
                result.Success = true;
            }
            catch (Exception e)
            {
                result.ErrorMsg = e.Message;
            }
            return Json(result);
        }
    }
}