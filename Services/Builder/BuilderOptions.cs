namespace HRCE.Services.Builder;

public sealed class BuilderOptions
{
    public string RootPath { get; set; } = string.Empty;
    public string ViewsPath { get; set; } = "Views";
    public string ControllersPath { get; set; } = "Controllers";
}
