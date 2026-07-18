using AutoMapper;
using ERP_API.App.IService;
using ERPDto.PaigingDto;
using ERPDto.PurchaseDto;
using Infrastructure.Cache;
using Infrastructure.JWT;
using Infrastructure.ORM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondApi.Controllers;

namespace ERP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : MasterController
    {
        private readonly IPurchaseReceiptService _purchaseService;

        public PurchasesController(
            IPurchaseReceiptService purchaseService,
            IUserService userService,
            IAppMemoryCache cache,
            DBContext context,
            IMapper mapper,
            Jwt jwtService) : base(userService, cache, context, mapper, jwtService)
        {
            _purchaseService = purchaseService;
        }

        [HttpPost("CompleteReceipt")]
        [Authorize(Roles = "FullAccess,CompletePurchaseReceipt")]
        public async Task<IActionResult> CompleteReceipt([FromBody] CompletePurchaseReceiptModel model)
        {
            if (_UserId == 0) GetUserId();
            var receipt = await _purchaseService.CompleteReceiptAsync(model, _UserId);
            return Ok(receipt);
        }

        [HttpGet("LookupProductByBarcode/{barcode}")]
        [Authorize(Roles = "FullAccess,CompletePurchaseReceipt")]
        public async Task<IActionResult> LookupProductByBarcode(string barcode, [FromQuery] int? warehouseId = null)
        {
            var product = await _purchaseService.LookupProductByBarcodeAsync(barcode, warehouseId);
            if (product is null)
                throw new KeyNotFoundException("لم يتم العثور على المادة");
            return Ok(product);
        }

        [HttpGet("SearchProducts")]
        [Authorize(Roles = "FullAccess,CompletePurchaseReceipt")]
        public async Task<IActionResult> SearchProducts([FromQuery] string term, [FromQuery] int take = 12, [FromQuery] int? warehouseId = null)
        {
            var products = await _purchaseService.SearchProductsAsync(term, take, warehouseId);
            return Ok(products);
        }

        [HttpPost("GetReceipts")]
        [Authorize(Roles = "FullAccess,GetPurchaseReceipts")]
        public async Task<IActionResult> GetReceipts([FromBody] PageDto page)
        {
            var result = await _purchaseService.GetReceiptsAsync(page);
            return Ok(result);
        }

        [HttpGet("GetReceiptById/{id:int}")]
        [Authorize(Roles = "FullAccess,GetPurchaseReceiptById")]
        public async Task<IActionResult> GetReceiptById(int id)
        {
            var receipt = await _purchaseService.GetReceiptByIdAsync(id);
            return Ok(receipt);
        }
    }
}
