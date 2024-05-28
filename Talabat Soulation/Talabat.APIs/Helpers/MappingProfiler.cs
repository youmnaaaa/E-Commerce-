using AutoMapper;
using Talabat.APIs.Dtos;
using Talabat.Core.Entities;
using UserAddress= Talabat.Core.Entities.Identity.Address;
using OrderAddress= Talabat.Core.Entities.Order.Address;
using Talabat.Core.Entities.Order;


namespace Talabat.APIs.Helpers
{
    public class MappingProfiler : Profile
    {
        public MappingProfiler()
        {
            CreateMap<Product, ProductToReturnDto>()
                .ForMember(d => d.Brand, O => O.MapFrom(s => s.Brand.Name))
                .ForMember(d => d.Category, O => O.MapFrom(s => s.Category.Name))
                .ForMember(d => d.PictureUrl, O => O.MapFrom<ProductPictureUrlResolver>());
            CreateMap<UserAddress, AddressDto>().ReverseMap();
            CreateMap<OrderAddress, AddressDto>().ReverseMap()
                .ForMember(d => d.FirstName, O => O.MapFrom(s => s.FName))
                .ForMember(d => d.LastName, O => O.MapFrom(s => s.LName));

            CreateMap<Order, OrderToReturnDto>()
                .ForMember(d => d.DeliveryMethod, O => O.MapFrom(S => S.DeliveryMethod.ShortName))
                .ForMember(d => d.DeliveryMethodCost, O => O.MapFrom(S => S.DeliveryMethod.Cost));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.ProductId, O => O.MapFrom(S => S.Product.ProductId))
                .ForMember(d => d.ProductName, O => O.MapFrom(S => S.Product.ProductName))
                .ForMember(d => d.ProductName, O => O.MapFrom(S => S.Product.PictureUrl))
                .ForMember(d => d.PictureUrl, O => O.MapFrom<OrderItemPictureUrlResolver>());

            CreateMap<CustomerBasket, CustomerBasketDto>().ReverseMap();
        }

    }
}
