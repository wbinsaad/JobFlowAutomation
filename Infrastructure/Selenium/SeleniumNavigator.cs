using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace JobFlowAutomation.Infrastructure.Selenium;

public sealed class SeleniumNavigator
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private readonly ILogger<SeleniumNavigator> _logger;

    public SeleniumNavigator(IWebDriver driver, ILogger<SeleniumNavigator> logger)
    {
        _driver = driver;
        _logger = logger;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    public IWebDriver Driver => _driver;

    public void NavigateTo(string url, By? readySelector = null)
    {
        _logger.LogInformation("Navigating to {Url}", url);

        _driver.Navigate().GoToUrl(url);

        if (readySelector is null)
        {
            return;
        }

        _logger.LogDebug("Waiting for selector: {Selector}", readySelector);
        _wait.Until(driver => driver.FindElements(readySelector).Count > 0);
    }
}
