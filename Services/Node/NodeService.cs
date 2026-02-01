using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace HRCE.Services.Node;

public class NodeService : INodeService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NodeService> _logger;
    private Process? _nodeProcess;
    private const string NodeUrl = "http://localhost:3000/render";

    public NodeService(HttpClient httpClient, ILogger<NodeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        StartNodeProcess();
    }

    private void StartNodeProcess()
    {
        try
        {
            // في بيئة الإنتاج، يجب إدارة العملية بشكل أفضل (مثلاً كخدمة منفصلة)
            var startInfo = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = "Node/server.js",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AppContext.BaseDirectory // أو مسار المشروع
            };

            // _nodeProcess = Process.Start(startInfo);
            _logger.LogInformation("Node.js SSR process started (simulated for now)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Node.js process");
        }
    }

    public async Task<NodeRenderResult> RenderComponentAsync(string componentName, object props)
    {
        try
        {
            var payload = new { componentName, props };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // محاولة الاتصال بالخادم
            // في حالة عدم وجود خادم حقيقي، سنعيد نتيجة وهمية للتجربة
            try 
            {
                var response = await _httpClient.PostAsync(NodeUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
                    return new NodeRenderResult(
                        result.GetProperty("html").GetString() ?? "",
                        result.GetProperty("css").GetProperty("code").GetString() ?? "",
                        result.GetProperty("head").GetString() ?? ""
                    );
                }
            }
            catch (HttpRequestException)
            {
                _logger.LogWarning("Node.js server not reachable. Using fallback simulation.");
            }

            // Fallback simulation if Node is not running
            return new NodeRenderResult(
                $"<div data-ssr='simulated'>Simulated SSR for {componentName}</div>",
                "", ""
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering component {ComponentName}", componentName);
            throw;
        }
    }

    public void Dispose()
    {
        if (_nodeProcess != null && !_nodeProcess.HasExited)
        {
            _nodeProcess.Kill();
            _nodeProcess.Dispose();
        }
    }
}
