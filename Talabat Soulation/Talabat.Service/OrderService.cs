using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order;
using Talabat.Core.Repositories.Interfaces;
using Talabat.Core.Services.Interfaces;
using Talabat.Core.Specifications.Order_Specs;

namespace Talabat.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        public OrderService(IBasketRepository basketRepository,
            IUnitOfWork unitOfWork,
            IPaymentService paymentService
            )
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
        }
        public async Task<Order?> CreateOrderAsync(string BuyerEmail, string basketId, int DeliveryMethodId, Address ShippingAddress)
        {
            //1- get basket from basket repo
            var basket = await _basketRepository.GetBasketAsync(basketId);
            //1- get select items from basket 
            var OrderItem = new List<OrderItem>();
            if (basket?.Items.Count() > 0)
            {
                foreach(var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetAsync(item.Id);
                    var productItemOrderd = new ProductItemOrdered(product.Id, product.Name, product.PictureUrl);
                    var orderItem = new OrderItem(productItemOrderd, item.Price, item.Quantity);
                    OrderItem.Add(orderItem);
                }
            }
            //3- calculate subtotal
            var subTotal = OrderItem.Sum(OI => OI.Price * OI.Quantity);
            //4- get delivery method from database
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAsync(DeliveryMethodId);
            //5- create order
            //ckeck if paymentintentid exist from another order
            var spec = new OrderWithPaymentIntentSpecifications(basket.PaymentIntentId);
            var ExOrder = await _unitOfWork.Repository<Order>().GetWithSpecAsync(spec);
            if (ExOrder is not null)
            {
                _unitOfWork.Repository<Order>().DeleteAsync(ExOrder);
                //update paymentitentid with amount of basket if changed
                basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            }
            var order = new Order(BuyerEmail, ShippingAddress, deliveryMethod, OrderItem, subTotal, basket.PaymentIntentId);
            //6- add order locally
            await _unitOfWork.Repository<Order>().AddAsync(order);
            //7- save order in database
            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0) { return null; }
            else
            {
                return order;
            }

        }

        public async Task<Order?> GetOrderByIdForSpecificUserAsync(string BuyerEmail, int OrderId)
        {
            var spec = new OrderSpecifications(BuyerEmail, OrderId);
            var order = await _unitOfWork.Repository<Order>().GetWithSpecAsync(spec);
            if (order is null) return null;
            return order;
        }

        public async Task<IReadOnlyList<Order>?> GetOrdersForSpecificUserAsync(string BuyerEmail)
        {
            var spec = new OrderSpecifications(BuyerEmail);
            var orders = await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(spec);
            return orders;
        }
    }
}
