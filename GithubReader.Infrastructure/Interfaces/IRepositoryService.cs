namespace GithubReader.Infrastructure.Interfaces;

public interface IRepositoryService
{
    Task<IEnumerable<string>> GetBranchesAsync(string author, string repository);
    Task<IEnumerable<string>> GetFilesAsync(string author, string repository, string branch);
    Task<string> GetFileContentAsync(string author, string repository, string branch, string file);
}