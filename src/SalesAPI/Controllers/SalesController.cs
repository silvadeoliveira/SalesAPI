using Microsoft.AspNetCore.Mvc;
using SalesAPI.Data;
using SalesAPI.Domain.Entities;
using SalesAPI.DTOs;

namespace SalesAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SalesController : ControllerBase
{

    private readonly SalesDbContext _context;
    private readonly ILogger<SalesController> _logger;

    public SalesController(SalesDbContext context, ILogger<SalesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova venda.
    /// </summary>
    /// <param name="dto">Dados da venda.</param>
    /// <returns>Venda criada.</returns>
    [HttpPost]
    public IActionResult CreateSale([FromBody] CreateSaleDto dto)
    {
        var sale = new Sale
        {
            SaleNumber = dto.SaleNumber,
            SaleDate = dto.SaleDate,
            CustomerName = dto.CustomerName,
            Branch = dto.Branch,
            Items = [.. dto.Items.Select(item => new SaleItem
                {
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                })]
        };

        _context.Sales.Add(sale);
        _context.SaveChanges();

        _logger.LogInformation($"[EVENT] SaleCreated - SaleId: {sale.Id}");

        return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
    }

    /// <summary>
    /// Obtém os detalhes de uma venda.
    /// </summary>
    /// <param name="id">ID da venda.</param>
    /// <returns>Venda encontrada.</returns>
    [HttpGet("{id}")]
    public IActionResult GetSale(Guid id)
    {
        var sale = _context.Sales
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.SaleNumber,
                s.SaleDate,
                s.CustomerName,
                s.Branch,
                s.IsCancelled,
                Items = s.Items.Select(i => new
                {
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.Discount,
                    i.Total
                }),
                s.TotalAmount
            })
            .FirstOrDefault();

        if (sale == null)
            return NotFound();

        return Ok(sale);
    }

    /// <summary>
    /// Atualiza os dados de uma venda.
    /// </summary>
    /// <param name="id">ID da venda.</param>
    /// <param name="dto">Dados atualizados.</param>
    [HttpPut("{id}")]
    public IActionResult UpdateSale(Guid id, [FromBody] CreateSaleDto dto)
    {
        var sale = _context.Sales.FirstOrDefault(s => s.Id == id);
        if (sale == null) return NotFound();

        sale.SaleNumber = dto.SaleNumber;
        sale.SaleDate = dto.SaleDate;
        sale.CustomerName = dto.CustomerName;
        sale.Branch = dto.Branch;
        sale.Items = dto.Items.Select(item => new SaleItem
        {
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        }).ToList();

        _context.SaveChanges();

        _logger.LogInformation($"[EVENT] SaleModified - SaleId: {sale.Id}");

        return NoContent();
    }

    /// <summary>
    /// Cancela uma venda.
    /// </summary>
    /// <param name="id">ID da venda.</param>
    [HttpPost("{id}/cancel")]
    public IActionResult CancelSale(Guid id)
    {
        var sale = _context.Sales.FirstOrDefault(s => s.Id == id);
        if (sale == null) return NotFound();

        if (sale.IsCancelled)
            return BadRequest("Sale is already cancelled.");

        sale.IsCancelled = true;
        _context.SaveChanges();

        _logger.LogInformation($"[EVENT] SaleCancelled - SaleId: {sale.Id}");

        return NoContent();
    }

    /// <summary>
    /// Cancela um item da venda.
    /// </summary>
    /// <param name="saleId">ID da venda.</param>
    /// <param name="productName">Nome do produto a cancelar.</param>
    [HttpPost("{saleId}/items/{productName}/cancel")]
    public IActionResult CancelItem(Guid saleId, string productName)
    {
        var sale = _context.Sales.FirstOrDefault(s => s.Id == saleId);
        if (sale == null) return NotFound();

        var item = sale.Items.FirstOrDefault(i => i.ProductName == productName);
        if (item == null) return NotFound();

        sale.Items.Remove(item);
        _context.SaveChanges();

        _logger.LogInformation($"[EVENT] ItemCancelled - SaleId: {sale.Id}, Product: {productName}");

        return NoContent();
    }
}
