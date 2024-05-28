using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks.Dataflow;

namespace Talabat.APIs.Dtos
{
    public class OrderDto
    {
        [Required]
        public string BasketId { get; set; }
        [Required]
        public int DeliveryMethodId { get; set; }
        [Required]
        public AddressDto ShippingAddress { get; set; } 
    }
}
