using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Stripe.Climate;
using System.Security.Claims;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.Core;
using Talabat.Core.Entities.Order;
using Talabat.Core.Services.Interfaces;
using Talabat.Repository;
using Talabat.Core.Entities.Order;


namespace Talabat.APIs.Controllers
{

    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public OrdersController(IOrderService orderService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _orderService = orderService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [ProducesResponseType(typeof(OrderToReturnDto),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<OrderToReturnDto>> CreateOrder(OrderDto model)
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var address = _mapper.Map<AddressDto, Address>(model.ShippingAddress);
            var order = await _orderService.CreateOrderAsync(buyerEmail, model.BasketId, model.DeliveryMethodId, address);
            if (order is null) return BadRequest(error: new ApiResponse(statusCode: 400, message: "There is a problem with your order!"));
            var result = _mapper.Map<Core.Entities.Order.Order, OrderToReturnDto>(order);
            return Ok(result);

        }
       
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetOrdersForUser()
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var orders = await _orderService.GetOrdersForSpecificUserAsync(buyerEmail);
            if (orders is null) return NotFound(value: new ApiResponse(statusCode: 404, message: "There is no order for you"));
            return Ok(_mapper.Map<IReadOnlyList<OrderToReturnDto>>(orders));
        }

        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet(template: "{id}")]
        [Authorize]
        public async Task<ActionResult<OrderToReturnDto>> GetOrderByIdForUser(int id)
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var order = await _orderService.GetOrderByIdForSpecificUserAsync(buyerEmail, id);
            if (order is null) return NotFound(value: new ApiResponse(statusCode: 404, message: $"There is No order with id : {id} for you"));
            return Ok(_mapper.Map<OrderToReturnDto>(order));
        }

        [HttpGet(template: "deliverymethods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            var deliveryMethods = await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
            return Ok(deliveryMethods);
        }
       
    }
}
