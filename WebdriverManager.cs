using System;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SaveTheWorldRewards
{
    class WebdriverManager
    {
        public static async Task<string> GetAuthCode()
        {
            string link = "https://www.epicgames.com/id/api/redirect?clientId=ec684b8c687f479fadea3cb2ad83f5c6&responseType=code";

            var options = new ChromeOptions();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                options.AddArgument($"user-data-dir={Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/Google/Chrome/User Data");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                options.AddArgument($"user-data-dir=~/.config/google-chrome");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                options.AddArgument($"user-data-dir=~/Library/Application Support/Google/Chrome");
            }
            else
            {
                Console.WriteLine("Unsupported platform!");
                return string.Empty;
            }

            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalCapability("useAutomationExtension", false);

            options.AddArgument("disable-gpu");
            options.AddArgument("disable-software-rasterizer");
            options.AddArgument("no-sandbox");

            // options.AddArgument("headless");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            chromeDriverService.SuppressInitialDiagnosticInformation = true;

            ChromeDriver driver;

            try
            {
                driver = new ChromeDriver(chromeDriverService, options);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return string.Empty;
            }

            await Task.Delay(2000);

            driver.Url = link;

            string text = new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("/html/body/pre"))).Text;
            driver.Close();
            driver.Quit();

            driver.Dispose();
            driver = null;

            Process[] killChrome = Process.GetProcessesByName("chromedriver.exe");

            foreach (var process in killChrome)
            {
                process.Kill();
            }


            var codeDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
            if (codeDict.ContainsKey("redirectUrl"))
            {
                try
                {
                    return codeDict["redirectUrl"][43..];
                }
                catch
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }
    }
}