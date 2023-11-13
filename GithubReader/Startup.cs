using System.Reflection;
using GithubReader.Infrastructure.Interfaces;
using GithubReader.Services.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(GithubReader.Startup))]

namespace GithubReader;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddTransient<IRepositoryService, RepositoryService>();
    }
}