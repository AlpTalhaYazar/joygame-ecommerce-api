using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JoyGame.CaseStudy.API.Controllers;

[Authorize]
public class ProductController(
    IProductService productService,
    ILogger<ProductController> logger)
    : BaseApiController
{
    private readonly IProductService _productService = productService;
    private readonly ILogger<ProductController> _logger = logger;

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(Result<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return HandleResult(Result<List<ProductDto>>.Success(products));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(Result<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        return HandleResult(Result<ProductDto>.Success(product));
    }

    [HttpGet("by-slug/{slug}")]
    [Authorize]
    [ProducesResponseType(typeof(Result<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var product = await _productService.GetBySlugAsync(slug);
        return HandleResult(Result<ProductDto>.Success(product));
    }

    [HttpGet("category/{categoryId}")]
    [Authorize]
    [ProducesResponseType(typeof(Result<List<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        try
        {
            var products = await _productService.GetByCategoryIdAsync(categoryId);
            return HandleResult(Result<List<ProductDto>>.Success(products));
        }
        catch (EntityNotFoundException)
        {
            return HandleResult(Result<object>.Failure($"Category with ID {categoryId} not found"));
        }
    }

    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(typeof(Result<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string searchTerm, [FromQuery] int? categoryId = null)
    {
        var products = await _productService.SearchAsync(searchTerm, categoryId);
        return HandleResult(Result<List<ProductDto>>.Success(products));
    }

    [HttpPost]
    [Authorize(Policy = "ProductManagement")]
    [ProducesResponseType(typeof(Result<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto createProductDto)
    {
        try
        {
            var product = await _productService.CreateAsync(createProductDto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                Result<ProductDto>.Success(product));
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Failed to create product");
            return HandleResult(Result<object>.Failure(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ProductManagement")]
    [ProducesResponseType(typeof(Result<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto updateProductDto)
    {
        try
        {
            var product = await _productService.UpdateAsync(id, updateProductDto);
            return HandleResult(Result<ProductDto>.Success(product));
        }
        catch (EntityNotFoundException)
        {
            return HandleResult(Result<object>.Failure($"Product with ID {id} not found"));
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Failed to update product {ProductId}", id);
            return HandleResult(Result<object>.Failure(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ProductManagement")]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _productService.DeleteAsync(id);
            return HandleResult(Result<string>.Success("Product deleted successfully"));
        }
        catch (EntityNotFoundException)
        {
            return HandleResult(Result<object>.Failure($"Product with ID {id} not found"));
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Failed to delete product {ProductId}", id);
            return HandleResult(Result<object>.Failure(ex.Message));
        }
    }

    [HttpGet("with-categories")]
    [Authorize(Policy = "ProductView")]
    [ProducesResponseType(typeof(Result<List<ProductWithCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsWithCategories([FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var productsDataAndTotal = await _productService.GetProductsWithCategoriesAsync(page, pageSize);
        return HandleResult(PaginationResult<List<ProductWithCategoryDto>>.Success(productsDataAndTotal.data,
            page, pageSize,
            productsDataAndTotal.total));
    }
}