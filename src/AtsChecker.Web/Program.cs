using AtsChecker.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IResumeExtractor, ResumeExtractor>();
builder.Services.AddSingleton<IResumeChecker, ResumeChecker>();
var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();

public partial class Program { }