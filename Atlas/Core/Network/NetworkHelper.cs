using Atlas.UI;
using Newtonsoft.Json.Linq;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;

namespace Atlas.Core.Network
{
    public class NetworkHelper
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient _httpClient = new HttpClient();

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
                    //stock
                    jsonArray = JArray.Parse(response);

                    /*string fullVersion = jsonArray[0]["name"]!.ToString();
                    string version = jsonArray[0]["name"]!.ToString().Replace("v", "").Split('-')[0];
                    string download_url = jsonArray[0]["assets"][0]["browser_download_url"]!.ToString();

                    return new[] { version, download_url, fullVersion };*/
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return jsonArray;
            }
            return jsonArray;
        }



        public static Task DownloadFileToMemory(string downloadUrl, string outputPath)
        {
            Task task = null;
            /*Uri uriResult;

            if (!Uri.TryCreate(uri, UriKind.Absolute, out uriResult))
                throw new InvalidOperationException("URI is invalid.");*/
            //The percentage needs to be split in half
            WebClient webClient = new WebClient();

            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            webClient.DownloadFileCompleted += (s, e) =>
            {
                webClient.Dispose();
                task = Task.CompletedTask;
                // any other code to process the file
            };

            byte[] fileBytes = null;

            webClient.DownloadDataAsync(new Uri(downloadUrl), fileBytes);
            //webClient.DownloadFileAsync(new Uri(downloadUrl), outputPath);

            //webClient.Dispose();

            while (task == null)
            {

            }

            return task;
            //await _httpClient.
            //byte[] fileBytes = await _httpClient.GetByteArrayAsync(uri);
            //File.WriteAllBytes(outputPath, fileBytes);

        }

        public static Task DownloadFile(string downloadUrl, string outputPath)
        {
            Task task = null;
            /*Uri uriResult;

            if (!Uri.TryCreate(uri, UriKind.Absolute, out uriResult))
                throw new InvalidOperationException("URI is invalid.");*/
            //The percentage needs to be split in half
            WebClient webClient = new WebClient();

            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            webClient.DownloadFileCompleted += (s, e) =>
            {
                webClient.Dispose();
                task = Task.CompletedTask;
                // any other code to process the file
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
            }


            return task;
            //await _httpClient.
            //byte[] fileBytes = await _httpClient.GetByteArrayAsync(uri);
            //File.WriteAllBytes(outputPath, fileBytes);

        }

        public static async Task<Task> DownloadFileAsync(string url, string filename, int delay = 0)
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
                        using (var fileStream = new FileStream(filename, FileMode.Create))
                        {
                            //var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
                            await stream.CopyToAsync(fileStream);

                        }

                        Logger.Info("File downloaded successfully: " + filename);
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

        public static async Task<byte[]> DownloadBytesAsync(string url, string filename, int delay = 0)
        {
            byte[] byteArray = null;
            if (url != "")
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        var response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();

                        using var stream = await response.Content.ReadAsStreamAsync();
                        using (var memoryStream = new MemoryStream())
                        {
                            Logger.Info("File downloaded successfully: " + filename);
                            stream.CopyTo(memoryStream);
                            return memoryStream.ToArray();
                        }


                    }
                    catch (HttpRequestException ex)
                    {
                        Logger.Error("Failed to download File: " + ex.Message);
                    }
                }
            }

            //If we need to give the downloader a delay, this will help. 
            System.Threading.Thread.Sleep(delay);
            return byteArray;
        }
        public async Task<byte[]> DownloadImageAsync(string imageUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
                return imageBytes;
            }
        }
        public static SKBitmap DownloadImage(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                byte[] imageData = webClient.DownloadData(url);
                return SKBitmap.Decode(imageData);
            }
        }
    }
}
