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
using System.Collections.Generic;
using System.Linq;

namespace GithubReader;

public class FileContentFunction
{
    private readonly IRepositoryService _repositoryService;

    public FileContentFunction(IRepositoryService repositoryService)
    {
        _repositoryService = repositoryService;
    }

    [FunctionName("GetFileContent")]
    [OpenApiOperation(operationId: "getFileContent", tags: new[] { "fileContent" }, Summary = "Get File Content", Description = "This gets the content of multiple files from a repository on a specific branch.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "author", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Author of the repository", Description = "The GitHub username of the repository owner.")]
    [OpenApiParameter(name: "repository", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Repository name", Description = "The name of the repository.")]
    [OpenApiParameter(name: "branch", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "Branch name", Description = "The name of the branch.")]
    [OpenApiParameter(name: "files", In = ParameterLocation.Query, Required = true, Type = typeof(string[]), Summary = "File paths", Description = "The full paths to the files, including filenames and extensions.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Dictionary<string, string>), Summary = "File content", Description = "The contents of the files as a dictionary with file paths as keys.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "File not found", Description = "One or more specified files were not found in the repository.")]
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
            string[] files = req.Query["files"].ToString().Split(',');

            if (files == null || files.Length == 0)
            {
                return new BadRequestObjectResult("At least one file path is required.");
            }

            var contents = new Dictionary<string, string>();
            foreach (var file in files)
            {
                var content = await _repositoryService.GetFileContentAsync(author, repository, branch, file.Trim());
                contents.Add(file, content);
            }

            return new OkObjectResult(contents);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error retrieving file content.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}