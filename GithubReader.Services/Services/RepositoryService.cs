using GithubReader.Infrastructure.Interfaces;
using Newtonsoft.Json;

namespace GithubReader.Services.Services;

public class RepositoryService : IRepositoryService
{
    public async Task<IEnumerable<string>> GetBranchesAsync(string author, string repository)
    {
        using (var client = new HttpClient())
        {
            var url = $"https://api.github.com/repos/{author}/{repository}/branches";
            client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("AppName", "1.0"));
            var response = await client.GetStringAsync(url);
            var branches = JsonConvert.DeserializeObject<IEnumerable<dynamic>>(response);

            return branches.Select(branch => (string)branch.name);
        }
    }

    public async Task<IEnumerable<string>> GetFilesAsync(string author, string repository, string branch)
    {
        using (var client = new HttpClient())
        {
            var url = $"https://api.github.com/repos/{author}/{repository}/git/trees/{branch}?recursive=1";
            client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("AppName", "1.0"));
            var response = await client.GetStringAsync(url);
            dynamic treeInfo = JsonConvert.DeserializeObject(response);

            return ((IEnumerable<dynamic>)treeInfo.tree)
                .Where(item => item.type == "blob")
                .Select(item => (string)item.path);
        }
    }

    public async Task<string> GetFileContentAsync(string author, string repository, string branch, string file)
    {
        using (var client = new HttpClient())
        {
            var url = $"https://api.github.com/repos/{author}/{repository}/contents/{file}?ref={branch}";
            client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("AppName", "1.0"));
            var response = await client.GetStringAsync(url);
            dynamic fileContent = JsonConvert.DeserializeObject(response);

            if (fileContent.encoding == "base64")
            {
                var contentData = Convert.FromBase64String((string)fileContent.content);
                return System.Text.Encoding.UTF8.GetString(contentData);
            }
            else
            {
                // Handle other encodings as needed
                return string.Empty;
            }
        }
    }
}