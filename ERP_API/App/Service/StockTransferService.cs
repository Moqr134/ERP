using AutoMapper;
using ERP_API.App.Inventory;
using ERP_API.App.IService;
using ERP_API.Domin.ProductEntity;
using ERP_API.Domin.StockTransactionsEntity;
using ERP_API.Domin.StockTransferEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.StockTransferDto;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class StockTransferService : MasterService, IScopped, IStockTransferService
    {
        public StockTransferService(DBContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<List<StockTransferDto>> GetTransfersAsync()
        {
            return await _context.StockTransfers
                .AsNoTracking()
                .OrderByDescending(t => t.Id)
                .Take(200)
                .Select(t => new StockTransferDto
                {
                    Id = t.Id,
                    TransferNumber = t.TransferNumber,
                    FromWarehouseId = t.FromWarehouseId,
                    FromWarehouseName = t.FromWarehouse.Name,
                    ToWarehouseId = t.ToWarehouseId,
                    ToWarehouseName = t.ToWarehouse.Name,
                    Status = t.Status,
                    Notes = t.Notes,
                    CreateDate = t.CreateDate,
                    Lines = t.Lines.Select(l => new StockTransferLineDto
                    {
                        ProductId = l.ProductId,
                        ProductName = l.Product.Name,
                        Barcode = l.Product.Barcode,
                        Quantity = l.Quantity
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<StockTransferDto> GetTransferByIdAsync(int id)
        {
            var transfer = await _context.StockTransfers
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new StockTransferDto
                {
                    Id = t.Id,
                    TransferNumber = t.TransferNumber,
                    FromWarehouseId = t.FromWarehouseId,
                    FromWarehouseName = t.FromWarehouse.Name,
                    ToWarehouseId = t.ToWarehouseId,
                    ToWarehouseName = t.ToWarehouse.Name,
                    Status = t.Status,
                    Notes = t.Notes,
                    CreateDate = t.CreateDate,
                    Lines = t.Lines.Select(l => new StockTransferLineDto
                    {
                        ProductId = l.ProductId,
                        ProductName = l.Product.Name,
                        Barcode = l.Product.Barcode,
                        Quantity = l.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (transfer is null)
                throw new KeyNotFoundException("التحويل غير موجود");

            return transfer;
        }

        public async Task<StockTransferDto> CreateTransferAsync(CreateStockTransferModel model, int userId)
        {
            if (model.FromWarehouseId == model.ToWarehouseId)
                throw new InvalidOperationException("مخزن المصدر والوجهة يجب أن يختلفا");

            if (model.Lines is null || model.Lines.Count == 0)
                throw new InvalidOperationException("يجب إضافة منتج واحد على الأقل");

            var merged = model.Lines
                .GroupBy(l => l.ProductId)
                .Select(g => new CreateStockTransferLineDto
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            if (merged.Any(l => l.Quantity <= 0))
                throw new InvalidOperationException("كمية التحويل يجب أن تكون أكبر من صفر");

            // Lock warehouses in ascending id order to reduce deadlock risk
            var firstWh = Math.Min(model.FromWarehouseId, model.ToWarehouseId);
            var secondWh = Math.Max(model.FromWarehouseId, model.ToWarehouseId);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await WarehouseStockHelper.EnsureWarehouseActiveAsync(_context, firstWh);
                await WarehouseStockHelper.EnsureWarehouseActiveAsync(_context, secondWh);

                var productIds = merged.Select(l => l.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id) && !p.IsRemoved)
                    .ToListAsync();

                if (products.Count != productIds.Count)
                    throw new KeyNotFoundException("أحد المنتجات غير موجود");

                var now = DateTime.UtcNow.AddHours(3);
                var transferNumber = await GenerateTransferNumberAsync(now);

                var transfer = new StockTransfer
                {
                    TransferNumber = transferNumber,
                    FromWarehouseId = model.FromWarehouseId,
                    ToWarehouseId = model.ToWarehouseId,
                    Status = "Completed",
                    Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                    CreateDate = now,
                    CreateUserId = userId
                };

                foreach (var line in merged)
                {
                    var product = products.First(p => p.Id == line.ProductId);

                    await WarehouseStockHelper.ApplyDeltaAsync(
                        _context, product, model.FromWarehouseId, -line.Quantity, userId, now, product.Name);
                    await WarehouseStockHelper.ApplyDeltaAsync(
                        _context, product, model.ToWarehouseId, line.Quantity, userId, now, product.Name);

                    transfer.Lines.Add(new StockTransferLine
                    {
                        ProductId = product.Id,
                        Quantity = line.Quantity,
                        CreateDate = now,
                        CreateUserId = userId
                    });

                    _context.StockTransactions.Add(new StockTransactions
                    {
                        ProductId = product.Id,
                        WarehouseId = model.FromWarehouseId,
                        RelatedWarehouseId = model.ToWarehouseId,
                        Quantity = line.Quantity,
                        TransactionType = "TransferOut",
                        ReferenceId = transferNumber,
                        Notes = "تحويل مخزون صادر",
                        CreateDate = now,
                        CreateUserId = userId
                    });

                    _context.StockTransactions.Add(new StockTransactions
                    {
                        ProductId = product.Id,
                        WarehouseId = model.ToWarehouseId,
                        RelatedWarehouseId = model.FromWarehouseId,
                        Quantity = line.Quantity,
                        TransactionType = "TransferIn",
                        ReferenceId = transferNumber,
                        Notes = "تحويل مخزون وارد",
                        CreateDate = now,
                        CreateUserId = userId
                    });
                }

                _context.StockTransfers.Add(transfer);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var fromName = await _context.Warehouses.AsNoTracking()
                    .Where(w => w.Id == transfer.FromWarehouseId)
                    .Select(w => w.Name)
                    .FirstAsync();
                var toName = await _context.Warehouses.AsNoTracking()
                    .Where(w => w.Id == transfer.ToWarehouseId)
                    .Select(w => w.Name)
                    .FirstAsync();

                return new StockTransferDto
                {
                    Id = transfer.Id,
                    TransferNumber = transfer.TransferNumber,
                    FromWarehouseId = transfer.FromWarehouseId,
                    FromWarehouseName = fromName,
                    ToWarehouseId = transfer.ToWarehouseId,
                    ToWarehouseName = toName,
                    Status = transfer.Status,
                    Notes = transfer.Notes,
                    CreateDate = transfer.CreateDate,
                    Lines = products
                        .Where(p => merged.Any(l => l.ProductId == p.Id))
                        .Select(p =>
                        {
                            var line = merged.First(l => l.ProductId == p.Id);
                            return new StockTransferLineDto
                            {
                                ProductId = p.Id,
                                ProductName = p.Name,
                                Barcode = p.Barcode,
                                Quantity = line.Quantity
                            };
                        })
                        .ToList()
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("تم تعديل المخزون من عملية أخرى، أعد المحاولة");
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException(
                    "تعذر حفظ التحويل. تأكد من أن الكمية متوفرة في مخزن المصدر وأن المخازن صحيحة.", ex);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<string> GenerateTransferNumberAsync(DateTime now)
        {
            var prefix = $"TRF-{now:yyyyMMdd}-";
            var last = await _context.StockTransfers
                .IgnoreQueryFilters()
                .Where(t => t.TransferNumber.StartsWith(prefix))
                .OrderByDescending(t => t.TransferNumber)
                .Select(t => t.TransferNumber)
                .FirstOrDefaultAsync();

            var seq = 1;
            if (!string.IsNullOrEmpty(last) && last.Length > prefix.Length
                && int.TryParse(last[prefix.Length..], out var n))
                seq = n + 1;

            return $"{prefix}{seq:D4}";
        }
    }
}
