using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces;
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
    [ProducesResponseType(typeof(Result<List<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return HandleResult(Result<List<CategoryDto>>.Success(categories));
    }

    [HttpGet("tree")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<List<CategoryTreeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree()
    {
        var categoryTree = await _categoryService.GetCategoryTreeAsync();
        return HandleResult(Result<List<CategoryTreeDto>>.Success(categoryTree));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return HandleResult(Result<CategoryDto>.Success(category));
    }

    [HttpPost]
    [Authorize(Policy = "CategoryManagement")]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto createCategoryDto)
    {
        try
        {
            var category = await _categoryService.CreateAsync(createCategoryDto);
            return CreatedAtAction(
                nameof(GetById),
                new { id = category.Id },
                Result<CategoryDto>.Success(category));
        }
        catch (BusinessRuleException ex)
        {
            return HandleResult(Result<object>.Failure(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CategoryManagement")]
    [ProducesResponseType(typeof(Result<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
    {
        try
        {
            var category = await _categoryService.UpdateAsync(id, updateCategoryDto);
            return HandleResult(Result<CategoryDto>.Success(category));
        }
        catch (EntityNotFoundException)
        {
            return HandleResult(Result<object>.Failure($"Category with ID {id} not found"));
        }
        catch (BusinessRuleException ex)
        {
            return HandleResult(Result<object>.Failure(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CategoryManagement")]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _categoryService.DeleteAsync(id);
            return HandleResult(Result<string>.Success("Category deleted successfully"));
        }
        catch (EntityNotFoundException)
        {
            return HandleResult(Result<object>.Failure($"Category with ID {id} not found"));
        }
        catch (BusinessRuleException ex)
        {
            return HandleResult(Result<object>.Failure(ex.Message));
        }
    }

    [HttpGet("hierarchy")]
    [Authorize(Policy = "CategoryView")]
    public async Task<IActionResult> GetHierarchy()
    {
        var hierarchy = await _categoryService.GetCategoryHierarchyAsync();
        return HandleResult(Result<List<CategoryHierarchyDto>>.Success(hierarchy));
    }
}