using AutoMapper;
using CarvedRock.Core;
using CarvedRock.Data.Entities;

namespace CarvedRock.Domain.Mapping;
public class ProductMappingProfile : Profile
{
	public ProductMappingProfile()
	{
        CreateMap<Product, ProductModel>()           
            .ReverseMap().MaxDepth(64);

        CreateMap<NewProductModel, Product>().MaxDepth(64);
    }
}
