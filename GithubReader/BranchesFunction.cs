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

public class BranchesFunction
{
    private readonly IRepositoryService _repositoryService;

    public BranchesFunction(IRepositoryService repositoryService)
    {
        _repositoryService = repositoryService;
    }

    [FunctionName("GetBranches")]
    [OpenApiOperation(operationId: "getBranches", tags: new[] { "branch" }, Summary = "Get Branches", Description = "This gets a list of all branches in a given repository.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "author", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Author of the repository", Description = "The GitHub username of the repository owner.")]
    [OpenApiParameter(name: "repository", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Repository name", Description = "The name of the repository.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<string>), Summary = "List of branches", Description = "The list of all branch names in the repository.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "Repository not found", Description = "The specified repository was not found.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Summary = "Unauthorized", Description = "The request is not authorized, possibly missing the function key if required.")]
    public async Task<IActionResult> GetBranches(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "authors/{author}/repositories/{repository}/branches")] HttpRequest req,
        string author,
        string repository,
        ILogger log)
    {
        try
        {
            var branches = await _repositoryService.GetBranchesAsync(author, repository);
            return new OkObjectResult(branches);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error retrieving branches.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}