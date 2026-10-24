using System.Diagnostics;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BankAccountSeleniumTests;

[TestClass]
[DoNotParallelize]
public class BankAccountUiTests
{
    private const string BaseUrl = "http://127.0.0.1:5187";
    private static Process? server;
    private IWebDriver driver = null!;
    private WebDriverWait wait = null!;

    [ClassInitialize]
    public static async Task StartServer(TestContext context)
    {
        string projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "BankAccountWeb", "BankAccountWeb.csproj"));
        var start = new ProcessStartInfo("dotnet")
        {
            Arguments = $"run --no-launch-profile --project \"{projectPath}\" --urls {BaseUrl}",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        server = Process.Start(start) ?? throw new InvalidOperationException("Cannot start web server.");
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        for (int attempt = 0; attempt < 60; attempt++)
        {
            if (server.HasExited) throw new InvalidOperationException("Web server exited unexpectedly.");
            try
            {
                using var response = await http.GetAsync(BaseUrl + "/api/balance");
                if (response.IsSuccessStatusCode) return;
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException) { }
            await Task.Delay(500);
        }
        throw new TimeoutException("Web server did not start within 30 seconds. Check port 5187.");
    }

    [ClassCleanup]
    public static void StopServer()
    {
        if (server is { HasExited: false }) server.Kill(entireProcessTree: true);
        server?.Dispose();
    }

    [TestInitialize]
    public async Task SetUp()
    {
        using var http = new HttpClient();
        using var response = await http.PostAsync(BaseUrl + "/api/reset", null);
        response.EnsureSuccessStatusCode();
        var options = new ChromeOptions();
        // Set to true to hide Chrome during automated tests.
        options.AddArgument("--window-size=1280,900");
        // options.AddArgument("--headless=new");
        driver = new ChromeDriver(options); // Selenium Manager resolves ChromeDriver automatically.
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    [TestCleanup]
    public void TearDown() => driver?.Quit();

    private IWebElement Balance => driver.FindElement(By.CssSelector("[data-testid='balance']"));
    private IWebElement Status => driver.FindElement(By.Id("message"));
    private void WaitForBalance(string expected) => wait.Until(_ => Balance.Text == expected);
    private void WaitForStatus(string expected) => wait.Until(_ => Status.Text == expected);
    private void OpenApp()
    {
        driver.Navigate().GoToUrl(BaseUrl);
        WaitForBalance("100.00 PLN");
    }
    private void EnterAmount(string amount)
    {
        var input = driver.FindElement(By.Id("amount"));
        input.Clear();
        input.SendKeys(amount);
    }
    private void Click(string id) => driver.FindElement(By.Id(id)).Click();

    [TestMethod]
    [TestCategory("Smoke")]
    public void Homepage_DisplaysInitialBalance()
    {
        OpenApp();
        Assert.AreEqual("Bank Account Manager", driver.FindElement(By.TagName("h1")).Text);
    }

    [TestMethod]
    [TestCategory("Smoke")]
    public void Deposit_ValidAmount_IncreasesBalance()
    {
        OpenApp(); EnterAmount("50"); Click("deposit");
        WaitForBalance("150.00 PLN");
        WaitForStatus("Deposit successful.");
    }

    [TestMethod]
    public void Withdraw_ValidAmount_DecreasesBalance()
    {
        OpenApp(); EnterAmount("40"); Click("withdraw");
        WaitForBalance("60.00 PLN");
    }

    [TestMethod]
    public void Withdraw_TooMuch_ShowsErrorAndKeepsBalance()
    {
        OpenApp(); EnterAmount("150"); Click("withdraw");
        WaitForStatus("Insufficient funds.");
        Assert.AreEqual("100.00 PLN", Balance.Text);
    }

    [DataTestMethod]
    [DataRow("0")]
    [DataRow("-5")]
    public void Deposit_NonPositiveAmount_ShowsValidation(string amount)
    {
        OpenApp(); EnterAmount(amount); Click("deposit");
        WaitForStatus("Amount must be positive.");
        Assert.AreEqual("100.00 PLN", Balance.Text);
    }

    [TestMethod]
    public void Balance_AfterReload_RemainsUpdated()
    {
        OpenApp(); EnterAmount("25"); Click("deposit");
        WaitForBalance("125.00 PLN");
        driver.Navigate().Refresh();
        WaitForBalance("125.00 PLN");
    }
}
