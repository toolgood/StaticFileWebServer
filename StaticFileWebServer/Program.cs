using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System;
using System.Text.RegularExpressions;

namespace StaticFileWebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            var env = app.Services.GetService<IWebHostEnvironment>();
            app.UseStaticFiles(new StaticFileOptions() {
                FileProvider = new PhysicalFileProvider(env.ContentRootPath),
                ServeUnknownFileTypes = true
            });
            app.Run(async h => {
                var path = Path.GetFullPath(System.IO.Path.Combine(env.ContentRootPath, (h.Request.Path.Value + "/index.html").TrimStart('/')));
                if (File.Exists(path)) {
                    await h.Response.WriteAsync(ReplaceHtml(await File.ReadAllTextAsync(path)));
                    return;
                }
                path = Path.GetFullPath(System.IO.Path.Combine(env.ContentRootPath, (h.Request.Path.Value + "/index.htm").TrimStart('/')));
                if (File.Exists(path)) {
                    await h.Response.WriteAsync(ReplaceHtml(await File.ReadAllTextAsync(path)));
                    return;
                }
            });
            Thread thread = new Thread(() => {
                Thread.Sleep(500);
                Process.Start(new ProcessStartInfo("http://localhost:5000") { UseShellExecute = true });
            });
            thread.Start();
            app.Run();
        }
        private static string ReplaceHtml(string html)
        {
            html = Regex.Replace(html, @"https://[^\.]*?\.github\.io/", "/");
            // chinese view url error
            return html.Replace("https://cdn.jsdelivr.net/", "https://fastly.jsdelivr.net/");
        }
    }
}
