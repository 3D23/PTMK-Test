using PTMK_Test.Application.Common;

namespace PTMK_Test.Web.Extensions
{
    public static class RequestResultExtensions
    {
        public static IResult ToHttpResponse(this RequestResult result)
        {
            if (result.IsSuccess)
            {
                return Results.Ok();
            }

            return result.StatusCode switch
            {
                404 => Results.NotFound(new { error = result.ErrorMessage }),
                _ => Results.BadRequest(new { error = result.ErrorMessage })
            };
        }

        public static IResult ToHttpResponse<T>(this RequestResult<T> result)
        {
            if (result.IsSuccess)
            {
                return Results.Ok(result.Value);
            }

            return result.StatusCode switch
            {
                404 => Results.NotFound(new { error = result.ErrorMessage }),
                _ => Results.BadRequest(new { error = result.ErrorMessage })
            };
        }

        public static IResult ToHttpCreated<T>(this RequestResult<T> result, string uriTemplate)
        {
            if (result.IsSuccess)
            {
                var locationUri = string.Format(uriTemplate, result.Value);
                return Results.Created(locationUri, new { Id = result.Value });
            }

            return result.StatusCode switch
            {
                404 => Results.NotFound(new { error = result.ErrorMessage }),
                _ => Results.BadRequest(new { error = result.ErrorMessage })
            };
        }
    }
}
