using System.Text;

using Newtonsoft.Json;

using OVRLighthouseManager.Core.Contracts.Services;

namespace OVRLighthouseManager.Core.Services;

public class FileService : IFileService
{
    private readonly object _lockObject = new();

    public T Read<T>(string folderPath, string fileName)
    {
        var path = Path.Combine(folderPath, fileName);
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        return default;
    }

    public void Save<T>(string folderPath, string fileName, T content)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileContent = JsonConvert.SerializeObject(content);
        lock (_lockObject)
        {
            File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
        }
    }

    public void Delete(string folderPath, string fileName)
    {
        if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
        {
            lock (_lockObject)
            {
                File.Delete(Path.Combine(folderPath, fileName));
            }
        }
    }
}
