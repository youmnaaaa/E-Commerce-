﻿using Microsoft.Extensions.Configuration;
using Stripe;
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
using Product = Talabat.Core.Entities.Product;

namespace Talabat.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public PaymentService(IBasketRepository basketRepository
            ,IUnitOfWork unitOfWork
            ,IConfiguration configuration
            )
        {
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<CustomerBasket?> CreateOrUpdatePaymentIntent(string basketId)
        {
            //Get Basket
           var basket = await _basketRepository.GetBasketAsync(basketId);
            if (basket is null) return null;
            //total price 
            if (basket.Items.Count > 0)
            {
                foreach(var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetAsync(item.Id);
                    if(item.Price != product.Price)
                    {
                        item.Price = product.Price;
                    }
                }
            }
            //subtotal
            var subTotal = basket.Items.Sum(I => I.Price * I.Quantity);

            var ShippingPrice = 0m;
            if (basket.DeliveryMethodId.HasValue)
            {
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAsync(basket.DeliveryMethodId.Value);
                ShippingPrice = deliveryMethod.Cost;
            }
            //call stripe
            StripeConfiguration.ApiKey = _configuration[key: "StripeKeys:Secretkey"];
            var service = new PaymentIntentService();
            PaymentIntent paymentIntent;
            if (string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                //create new paymentintentid
                var Options = new PaymentIntentCreateOptions()
                {
                    Amount = (long)(subTotal * 100 + ShippingPrice * 100),
                    Currency = "usd",
                    PaymentMethodTypes = new List<string>() { "card" },
                };
                paymentIntent = await service.CreateAsync(Options);
                basket.PaymentIntentId = paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;
            }
            else
            {
                //update paymentintentid
                var Options = new PaymentIntentUpdateOptions()
                {
                    Amount = (long)(subTotal * 100 + ShippingPrice * 100),
           
                };
                paymentIntent = await service.UpdateAsync(basket.PaymentIntentId, Options);
                basket.PaymentIntentId = paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;

            }
            //return basket included paymentintentid and ckient secret
            await _basketRepository.UpdateBasketAsync(basket);
            return basket;

            


        }

        public async Task<Order> UpdatePaymentIntentToSuccessedOrFailed(string paymentIntentId, bool flag)
        {
            var spec = new OrderWithPaymentIntentSpecifications(paymentIntentId);
            var order = await _unitOfWork.Repository<Order>().GetWithSpecAsync(spec);
            if (flag)
            {
                order.Status = OrderStatus.PaymentReceived;
            }
            else
            {
                order.Status = OrderStatus.PaymentFailed;
            }
            _unitOfWork.Repository<Order>().UpdateAsync(order);
            await _unitOfWork.CompleteAsync();
            return order;
        }
    }
}
