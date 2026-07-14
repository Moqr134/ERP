using AutoMapper;
using ERP_API.Domin.CategoriesEntity;
using ERP_API.Domin.ProductEntity;
using ERP_API.Domin.RoleEntity;
using ERP_API.Domin.StockTransactionsEntity;
using ERP_API.Domin.SuppliersEntity;
using ERP_API.Domin.UsersEntity;
using ERP_API.Domin.WarehouseEntity;
using ERPDto.CategoriesDto;
using ERPDto.WarehouseDto;
using ERPDto.ProductsDto;
using ERPDto.RolesDto;
using ERPDto.StockTransactionDto;
using ERPDto.Suppliers;
using ERPDto.UserDto;
using SherdProject.DTO;
namespace Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Users, UserOut>();
        CreateMap<LoginModel, Users>();
        CreateMap<Users, UserTokenDto>();
        CreateMap<UserTokenDto, UserOut>();
        CreateMap<Role, RoleDto>();
        CreateMap<RoleDto, Role>();
        CreateMap<UserPermissions, UserPermissionDto>();
        CreateMap<UserPermissionDto, UserPermissions>();
        CreateMap<CategoryDto, Categories>();
        CreateMap<Categories, CategoryDto>();
        CreateMap<Product,ProductDto>();
        CreateMap<ProductDto,Product>();
        CreateMap<CreateProductModel, Product>();
        CreateMap<UpdateProductModel, Product>();
        CreateMap<CreateStockTransactionsModel,StockTransactions>();
        CreateMap<StockTransactions, CreateStockTransactionsModel>();
        CreateMap<Suppliers, SuppliersDto>();
        CreateMap<SuppliersDto, Suppliers>();
        CreateMap<SuppliersModel, Suppliers>();
        CreateMap<Suppliers, SuppliersModel>();
        CreateMap<Warehouse, WarehouseDto>();
        CreateMap<WarehouseModel, Warehouse>();
    }
}
