using JoyGame.CaseStudy.API.Extensions;
using JoyGame.CaseStudy.API.Models;
using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JoyGame.CaseStudy.API.Controllers;

[Authorize]
public class CategoryController(
    ICategoryService categoryService,
    ILogger<CategoryController> logger)
    : BaseApiController
{
    private readonly ICategoryService _categoryService = categoryService;
    private readonly ILogger<CategoryController> _logger = logger;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var categoriesOperationResult = await _categoryService.GetAllAsync();
        return HandleResult(categoriesOperationResult.ToApiResponse());
    }

    [HttpGet("tree")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryTreeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree([FromQuery] string? slug)
    {
        var categoryTreeOperationResult = await _categoryService.GetCategoryTreeAsync();
        return HandleResult(categoryTreeOperationResult.ToApiResponse());
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "CategoryView")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var categoryOperationResult = await _categoryService.GetByIdAsync(id);
        return HandleResult(categoryOperationResult.ToApiResponse());
    }

    [HttpGet("{slug}")]
    [Authorize(Policy = "CategoryView")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var categoryOperationResult = await _categoryService.GetBySlugAsync(slug);
        return HandleResult(categoryOperationResult.ToApiResponse());
    }

    [HttpPost]
    [Authorize(Policy = "CategoryManagement")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto createCategoryDto)
    {
        var categoryOperationResult = await _categoryService.CreateAsync(createCategoryDto);

        return HandleResult(categoryOperationResult.ToApiResponse());
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CategoryManagement")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
    {
        var categoryOperationResult = await _categoryService.UpdateAsync(id, updateCategoryDto);
        return HandleResult(categoryOperationResult.ToApiResponse());
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CategoryManagement")]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleteOperationResult = await _categoryService.DeleteAsync(id);
        return HandleResult(deleteOperationResult.ToApiResponse());
    }

    [HttpGet("hierarchy")]
    [Authorize(Policy = "CategoryView")]
    public async Task<IActionResult> GetHierarchy()
    {
        var hierarchyOperationResult = await _categoryService.GetCategoryHierarchyAsync();
        return HandleResult(hierarchyOperationResult.ToApiResponse());
    }
}