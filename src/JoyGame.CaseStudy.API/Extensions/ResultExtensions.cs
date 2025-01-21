using JoyGame.CaseStudy.API.Models;
using JoyGame.CaseStudy.Application.Common;

namespace JoyGame.CaseStudy.API.Extensions;

public static class ResultExtensions
{
    public static ApiResponse<T> ToApiResponse<T>(this OperationResult<T> result)
    {
        if (result.IsSuccess)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = result.Data
            };
        }

        return new ApiResponse<T>
        {
            Success = false,
            Error = new ApiResponse.ErrorDetails
            {
                Code = (int)result.ErrorCode!,
                Message = result.ErrorMessage!,
                Details = result.ErrorDetails
            }
        };
    }

    public static PaginatedApiResponse<T> ToApiResponse<T>(this PaginatedOperationResult<T> result)
    {
        var response = new PaginatedApiResponse<T>
        {
            Success = result.IsSuccess,
            Data = result.Data
        };

        if (result.IsSuccess)
        {
            response.Pagination = new PaginatedApiResponse<T>.PaginationMetadata
            {
                Page = result.Metadata.Page,
                PageSize = result.Metadata.PageSize,
                TotalCount = result.Metadata.TotalCount,
                TotalPages = result.Metadata.TotalPages,
                HasNext = result.Metadata.HasNext,
                HasPrevious = result.Metadata.HasPrevious
            };
        }
        else
        {
            response.Error = new ApiResponse.ErrorDetails
            {
                Code = (int)result.ErrorCode!,
                Message = result.ErrorMessage!,
                Details = result.ErrorDetails
            };
        }

        return response;
    }
}