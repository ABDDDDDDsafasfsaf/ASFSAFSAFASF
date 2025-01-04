using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string logFilePath = @"C:\CS2Server\csgo\logs\server.log"; // Log dosyanızın yolu
        if (!File.Exists(logFilePath))
        {
            Console.WriteLine("Log dosyası bulunamadı: " + logFilePath);
            return;
        }

        Console.WriteLine("Log dosyası dinleniyor...");
        using (FileStream fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader reader = new StreamReader(fileStream))
        {
            while (true)
            {
                string? line = await reader.ReadLineAsync();
                if (!string.IsNullOrEmpty(line))
                {
                    if (line.Contains("say")) // Oyuncu mesajlarını ayıklamak için filtre
                    {
                        string message = ParsePlayerMessage(line);
                        if (!string.IsNullOrEmpty(message))
                        {
                            await SendToDiscord(message);
                        }
                    }
                }
                await Task.Delay(100); // CPU kullanımını azaltmak için küçük bir gecikme
            }
        }
    }

    static string ParsePlayerMessage(string logLine)
    {
        // Örnek log satırı: "L 01/04/2025 - 19:00: PlayerName: say Hello Discord!"
        int startIndex = logLine.IndexOf(": say ") + 6;
        if (startIndex > 5)
        {
            return logLine[startIndex..].Trim();
        }
        return string.Empty;
    }

    static async Task SendToDiscord(string message)
    {
        string webhookUrl = "YOUR_DISCORD_WEBHOOK_URL"; // Discord Webhook URL'sini buraya yapıştırın
        using (HttpClient client = new HttpClient())
        {
            var payload = new
            {
                content = message
            };

            string json = JsonSerializer.Serialize(payload);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(webhookUrl, content);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Mesaj Discord'a gönderildi: " + message);
            }
            else
            {
                Console.WriteLine("Mesaj gönderimi başarısız: " + response.StatusCode);
            }
        }
    }
}
