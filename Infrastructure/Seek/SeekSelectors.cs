using OpenQA.Selenium;

namespace JobFlowAutomation.Infrastructure.Seek;

public static class SeekSelectors
{
    public static readonly By JobCards = By.CssSelector("article");

    public static class Card
    {
        public static readonly By Title = By.CssSelector("a[data-automation='jobTitle']");
        public static readonly By Company = By.CssSelector("[data-automation='jobCompany']");
        public static readonly By Location = By.CssSelector("[data-automation='jobLocation']");
    }

    public static class Detail
    {
        public static readonly By Title = By.CssSelector("h1");
        public static readonly By Description = By.CssSelector("[data-automation='jobAdDetails']");
        public static readonly By Classifications = By.CssSelector("[data-automation='job-detail-classifications']");
        public static readonly By WorkType = By.CssSelector("[data-automation='job-detail-work-type']");
        public static readonly By Salary = By.CssSelector("[data-automation='job-detail-salary']");
        public static readonly By IsQuickApply = By.CssSelector("[data-automation='job-detail-apply']");
    }
}
