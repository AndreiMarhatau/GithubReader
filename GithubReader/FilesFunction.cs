using System;
using System.Collections.Generic;
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

public class FilesFunction
{
    private readonly IRepositoryService _repositoryService;

    public FilesFunction(IRepositoryService repositoryService)
    {
        _repositoryService = repositoryService;
    }

    [FunctionName("GetFiles")]
    [OpenApiOperation(operationId: "getFiles", tags: new[] { "files" }, Summary = "Get Files", Description = "Retrieves a list of file paths in the specified branch of a repository.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "author", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Author of the repository", Description = "The GitHub username of the repository owner.")]
    [OpenApiParameter(name: "repository", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Repository name", Description = "The name of the GitHub repository.")]
    [OpenApiParameter(name: "branch", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Branch name", Description = "The branch of the repository.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<string>), Summary = "List of file paths", Description = "An array of paths to the files in the repository.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "Not Found", Description = "The specified author or repository or branch does not exist.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Summary = "Unauthorized", Description = "The request did not include a valid function key, if required.")]
    public async Task<IActionResult> GetFiles(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "authors/{author}/repositories/{repository}/branches/{branch}/files")] HttpRequest req,
        string author,
        string repository,
        string branch,
        ILogger log)
    {
        var files = await _repositoryService.GetFilesAsync(author, repository, branch);
        return new OkObjectResult(files);
    }
}