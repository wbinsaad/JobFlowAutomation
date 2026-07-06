using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace JobFlowAutomation.Infrastructure.Selenium;

public static class WebDriverFactory
{
    public static IWebDriver Create()
    {
        var options = new ChromeOptions();

        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--start-maximized");
        options.AddExcludedArgument("enable-automation");

        return new ChromeDriver(options);
    }
}
