using Api.Dtos.Products;
using Application.Abstractions;
using Application.UseCase.Products;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;
[EnableRateLimiting("ipLimiter")] // toda la clase usa la política
public sealed class ProductsController : BaseApiController
{
    private readonly IUnitOfWork _uow;
    private readonly ISender _sender;

    public ProductsController(IUnitOfWork uow, ISender sender)
    {
        _uow = uow;
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken ct)
    {
        var products = await _uow.Products.GetAllAsync(ct);
        var result = products.Adapt<IReadOnlyList<ProductDto>>();
        return Ok(result);
    }

    [HttpGet("paged")]
    [DisableRateLimiting] // este endpoint queda excluido si se desea
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        var products = await _uow.Products.GetPagedAsync(page, pageSize, search, ct);
        var total = await _uow.Products.CountAsync(search, ct);
        var items = products.Adapt<IReadOnlyList<ProductDto>>();

        return Ok(new
        {
            page,
            pageSize,
            total,
            items
        });
    }

    [HttpGet("count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Count([FromQuery] string? search = null, CancellationToken ct = default)
    {
        var total = await _uow.Products.CountAsync(search, ct);
        return Ok(new { total });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var product = await _uow.Products.GetByIdAsync(id, ct);
        if (product is null)
        {
            return NotFound();
        }

        var result = product.Adapt<ProductDto>();
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var command = request.Adapt<CreateProduct>();
        var id = await _sender.Send(command, ct);
        var product = await _uow.Products.GetByIdAsync(id, ct);
        if (product is null)
        {
            return NotFound();
        }

        var result = product.Adapt<ProductDto>();
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var command = request.Adapt<UpdateProduct>() with { Id = id };
        await _sender.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "OtherOPremium")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var product = await _uow.Products.GetByIdAsync(id, ct);
        if (product is null)
        {
            return NotFound();
        }

        await _uow.Products.RemoveAsync(product, ct);
        await _uow.SaveChangesAsync(ct);
        return NoContent();
    }
}
