using Atlas.UI;
using Newtonsoft.Json.Linq;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;

namespace Atlas.Core.Networking
{
    public class NetworkHelper
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        private static void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                {
                    InterfaceHelper.LauncherProgressBar.Value = InterfaceHelper.ProgressBarStartValue + e.ProgressPercentage / 2;
                    Logger.Info(InterfaceHelper.LauncherProgressBar.Value);
                }));
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
            }
        }

        public static JArray RequestJSON(string url)
        {
            JArray jsonArray = new JArray();
            string response = string.Empty;
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            try
            {
                HttpResponseMessage rspmsg = client.GetAsync(url).Result;
                rspmsg.EnsureSuccessStatusCode();
                response = rspmsg.Content.ReadAsStringAsync().Result;


                if (response != string.Empty)
                {
                    jsonArray = JArray.Parse(response);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return jsonArray;
            }
            return jsonArray;
        }
        public static Task DownloadFileWithProgress(string downloadUrl, string outputPath)
        {
            Task task = null;
            WebClient webClient = new WebClient();

            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            webClient.DownloadFileCompleted += (s, e) =>
            {
                webClient.Dispose();
                task = Task.CompletedTask;
            };

            try
            {
                webClient.DownloadFileAsync(new Uri(downloadUrl), outputPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            //webClient.Dispose();

            while (task == null)
            {
               // System.Threading.Thread.Sleep(50);
            }


            return task;
        }

        public static async Task<Task> DownloadFileAsync(string url, string outputPath, int delay = 0)
        {
            if (url != "")
            {

                using (var client = new HttpClient())
                {
                    try
                    {
                        var response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();

                        var contentLength = response.Content.Headers.ContentLength;

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(outputPath, FileMode.Create))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                        Logger.Info("File downloaded successfully: " + outputPath);
                    }
                    catch (HttpRequestException ex)
                    {
                        Logger.Error("Failed to download File: " + ex.Message);
                    }
                }
            }
            else
            {
                Logger.Warn("Unable to get a valid image URL");
            }

            //If we need to give the downloader a delay, this will help. 
            System.Threading.Thread.Sleep(delay);
            return Task.CompletedTask;
        }

        public static async Task<byte[]> DownloadImageBytesAsync(string url)
        {
            Logger.Info($"Downloading image from: {url}");
            using (var httpClient = new HttpClient())
            {
                // Download the image bytes
                try
                {
                    byte[] imageBytes = await httpClient.GetByteArrayAsync(url);
                    return imageBytes;
                }
                catch (Exception ex)
                {
                    Logger.Error($"{ex}");
                    return null;
                }
            }
        }



    }
}
