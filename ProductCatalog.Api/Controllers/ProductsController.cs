using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Interfaces;

namespace ProductCatalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Gets a paginated list of products with optional name and stock filters.
    /// </summary>
    /// <param name="parameters">The query string filters and pagination values.</param>
    /// <response code="200">Returns the paginated list of products.</response>
    /// <response code="400">Returned when page or pageSize contain invalid values.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<ProductListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDto<ProductListItemDto>>> GetProductsAsync(
        [FromQuery] ProductQueryParameters parameters)
    {
        var validationError = ValidatePagination(parameters);
        if (validationError is not null)
        {
            return BadRequest(CreateErrorResponse(validationError));
        }

        var queryParameters = new ProductQueryParametersDto
        {
            Search = parameters.Name,
            InStock = parameters.InStock,
            Page = parameters.Page,
            PageSize = parameters.PageSize
        };

        var products = await _productService.GetProductsAsync(queryParameters);

        return Ok(products);
    }

    /// <summary>
    /// Gets the full detail of a product by its identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <response code="200">Returns the requested product detail.</response>
    /// <response code="404">Returned when the product does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailDto>> GetProductByIdAsync(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product is null)
        {
            return NotFound(CreateErrorResponse($"The product with Id {id} does not exist."));
        }

        return Ok(product);
    }

    /// <summary>
    /// Gets products whose stock is less than or equal to the provided threshold.
    /// </summary>
    /// <param name="threshold">The maximum stock threshold. Defaults to 5 when an invalid value is provided.</param>
    /// <response code="200">Returns the products with low stock.</response>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(IEnumerable<ProductListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductListItemDto>>> GetLowStockAsync([FromQuery] int threshold = 5)
    {
        var products = await _productService.GetLowStockAsync(threshold);

        return Ok(products);
    }

    private static string? ValidatePagination(ProductQueryParameters parameters)
    {
        if (parameters.Page < 1)
        {
            return "The page query parameter must be greater than or equal to 1.";
        }

        if (parameters.PageSize < 1 || parameters.PageSize > 100)
        {
            return "The pageSize query parameter must be between 1 and 100.";
        }

        return null;
    }

    private static ErrorResponseDto CreateErrorResponse(string message)
    {
        return new ErrorResponseDto
        {
            Message = message
        };
    }
}
