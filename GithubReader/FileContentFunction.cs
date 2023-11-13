using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using GithubReader.Infrastructure;
using GithubReader.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace GithubReader;

public class FileContentFunction
{
    private readonly IRepositoryService _repositoryService;

    public FileContentFunction(IRepositoryService repositoryService)
    {
        _repositoryService = repositoryService;
    }

    [FunctionName("GetFileContent")]
    [OpenApiOperation(operationId: "getFileContent", tags: new[] { "fileContent" }, Summary = "Get File Content", Description = "This gets the content of a file from a repository on a specific branch.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "author", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Author of the repository", Description = "The GitHub username of the repository owner.")]
    [OpenApiParameter(name: "repository", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Repository name", Description = "The name of the repository.")]
    [OpenApiParameter(name: "branch", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Branch name", Description = "The name of the branch.")]
    [OpenApiParameter(name: "file", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "File path", Description = "The full path to the file including filename and extension.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "File content", Description = "The content of the file as a string.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "File not found", Description = "The specified file was not found in the repository.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "Bad Request", Description = "The request was invalid or cannot be served.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Summary = "Unauthorized", Description = "The request is not authorized, possibly missing the function key if required.")]
    public async Task<IActionResult> GetFileContent(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "authors/{author}/repositories/{repository}/branches/{branch}/fileContent")] HttpRequest req,
        string author,
        string repository,
        string branch,
        ILogger log)
    {
        try
        {
            string file = req.Query["file"];

            if (string.IsNullOrEmpty(file))
            {
                return new BadRequestObjectResult("File path is required.");
            }

            var content = await _repositoryService.GetFileContentAsync(author, repository, branch, file);
            return new OkObjectResult(content);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error retrieving file content.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
