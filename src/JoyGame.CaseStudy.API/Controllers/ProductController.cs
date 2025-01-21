using JoyGame.CaseStudy.API.Extensions;
using JoyGame.CaseStudy.API.Models;
using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Interfaces.Services;
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
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var productsOperationResult = await _productService.GetAllAsync();
        return HandleResult(productsOperationResult.ToApiResponse());
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var productOperationResult = await _productService.GetByIdAsync(id);
        return HandleResult(productOperationResult.ToApiResponse());
    }

    [HttpGet("detailed/{id:int}")]
    [Authorize(Policy = "ProductView")]
    [ProducesResponseType(typeof(ApiResponse<ProductWithCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductWithCategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdDetailed(int id)
    {
        var productOperationResult = await _productService.GetByIdDetailedAsync(id);
        return HandleResult(productOperationResult.ToApiResponse());
    }

    [HttpGet("{slug}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProductWithCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductWithCategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var productOperationResult = await _productService.GetBySlugAsync(slug);
        return HandleResult(productOperationResult.ToApiResponse());
    }

    [HttpGet("category/{categoryId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var productsOperationResult = await _productService.GetByCategoryIdAsync(categoryId);
        return HandleResult(productsOperationResult.ToApiResponse());
    }

    [HttpGet("search")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string searchTerm, [FromQuery] int? categoryId = null)
    {
        var productsOperationResult = await _productService.SearchAsync(searchTerm, categoryId);
        return HandleResult(productsOperationResult.ToApiResponse());
    }

    [HttpPost]
    [Authorize(Policy = "ProductManagement")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto createProductDto)
    {
        var productOperationResult = await _productService.CreateAsync(createProductDto);

        return HandleResult(productOperationResult.ToApiResponse());
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ProductManagement")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto updateProductDto)
    {
        var productOperationResult = await _productService.UpdateAsync(id, updateProductDto);
        return HandleResult(productOperationResult.ToApiResponse());
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ProductManagement")]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleteOperationResult = await _productService.DeleteAsync(id);
        return HandleResult(deleteOperationResult.ToApiResponse());
    }

    [HttpGet("with-categories")]
    [Authorize(Policy = "ProductView")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductWithCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsWithCategories([FromQuery] int page = 1,
        [FromQuery] int pageSize = 10, [FromQuery] int? categoryId = null, [FromQuery] string? searchText = null)
    {
        var productsDataAndTotalOperationResult =
            await _productService.GetProductsWithCategoriesAsync(page, pageSize, categoryId, searchText);

        var responseOperationResult =
            PaginatedOperationResult<List<ProductWithCategoryDto>>.Success(
                productsDataAndTotalOperationResult.Data.data,
                new PaginatedOperationResult<List<ProductWithCategoryDto>>.PaginationMetadata()
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = productsDataAndTotalOperationResult.Data.total,
                    TotalPages = (int)Math.Ceiling(productsDataAndTotalOperationResult.Data.total / (double)pageSize)
                });

        return HandleResult(responseOperationResult.ToApiResponse());
    }
}