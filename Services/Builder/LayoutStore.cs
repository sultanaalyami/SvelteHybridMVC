using System.Text.Json;

namespace HRCE.Services.Builder;

public sealed class LayoutStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _storePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public LayoutStore(IWebHostEnvironment environment)
    {
        var dataPath = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataPath);
        _storePath = Path.Combine(dataPath, "builder-layout.json");
    }

    public async Task<LayoutDefinition> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var state = await LoadAsync(cancellationToken);
        if (state.CurrentVersionId == null)
        {
            return LayoutDefinition.Empty;
        }

        var version = state.Versions.FirstOrDefault(v => v.Id == state.CurrentVersionId.Value);
        return version?.Layout ?? LayoutDefinition.Empty;
    }

    public async Task<IReadOnlyList<LayoutVersionSummary>> GetVersionsAsync(CancellationToken cancellationToken)
    {
        var state = await LoadAsync(cancellationToken);
        return state.Versions
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new LayoutVersionSummary(
                v.Id,
                v.Name,
                v.CreatedAt,
                state.CurrentVersionId == v.Id))
            .ToList();
    }

    public async Task<LayoutVersionSummary> SaveAsync(string name, LayoutDefinition layout, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Auto";
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var state = await LoadAsync(cancellationToken);
            var version = new LayoutVersion(Guid.NewGuid(), name.Trim(), DateTimeOffset.UtcNow, layout);

            state.Versions.Insert(0, version);
            state.CurrentVersionId = version.Id;

            if (state.Versions.Count > 25)
            {
                state.Versions.RemoveRange(25, state.Versions.Count - 25);
            }

            await SaveAsync(state, cancellationToken);
            return new LayoutVersionSummary(version.Id, version.Name, version.CreatedAt, true);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RestoreAsync(Guid versionId, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var state = await LoadAsync(cancellationToken);
            if (state.Versions.All(v => v.Id != versionId))
            {
                return false;
            }

            state.CurrentVersionId = versionId;
            await SaveAsync(state, cancellationToken);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<LayoutStoreState> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_storePath))
        {
            return new LayoutStoreState();
        }

        await using var stream = File.OpenRead(_storePath);
        var state = await JsonSerializer.DeserializeAsync<LayoutStoreState>(stream, SerializerOptions, cancellationToken);
        return state ?? new LayoutStoreState();
    }

    private async Task SaveAsync(LayoutStoreState state, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(_storePath);
        await JsonSerializer.SerializeAsync(stream, state, SerializerOptions, cancellationToken);
    }
}

public sealed record LayoutDefinition(IReadOnlyList<LayoutBlock> Blocks)
{
    public static LayoutDefinition Empty => new(Array.Empty<LayoutBlock>());
}

public sealed record LayoutBlock(string Id, string Type, string Label, string? Url, string? CssClass);

public sealed record LayoutVersion(Guid Id, string Name, DateTimeOffset CreatedAt, LayoutDefinition Layout);

public sealed record LayoutVersionSummary(Guid Id, string Name, DateTimeOffset CreatedAt, bool IsCurrent);

public sealed class LayoutStoreState
{
    public Guid? CurrentVersionId { get; set; }

    public List<LayoutVersion> Versions { get; set; } = new();
}
