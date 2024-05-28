using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Talabat.APIs.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Services.Interfaces;

namespace Talabat.APIs.Controllers
{
    [Authorize]
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _paymentService;
        const string endpointSecret = "whsec_c32aea36eaaa5965b1f39dd14748ea5258ada5fd5fe1ee17d4c31c0c37346090";
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        [ProducesResponseType(typeof(CustomerBasket),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            if (basket is null) return BadRequest(error: new ApiResponse(statusCode: 400, message: "There is isa problem with your basket!"));
            return Ok(basket);
        }


        [AllowAnonymous]
        [HttpPost(template:"webhook")] //Post: https://localhost:7235/api/payments/webhook
        public async Task<IActionResult> StripeWebhook()
         {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], endpointSecret);
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
                {
                    await _paymentService.UpdatePaymentIntentToSuccessedOrFailed(paymentIntent.Id, flag: false);
                }
                else if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    await _paymentService.UpdatePaymentIntentToSuccessedOrFailed(paymentIntent.Id, flag: true);

                }
                // ... handle other event types
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }
    }
}
