﻿using Selenium.Tests.Internals;
using A = NUnit.Framework.Assert;
using SetUp = NUnit.Framework.SetUpAttribute;
using TearDown = NUnit.Framework.TearDownAttribute;
using TestCase = NUnit.Framework.TestCaseAttribute;
using TestFixture = NUnit.Framework.TestFixtureAttribute;

namespace Selenium.Tests {

    [TestFixture(Browser.Firefox)]
    [TestFixture(Browser.Opera)]
    [TestFixture(Browser.Chrome)]
    [TestFixture(Browser.IE)]
    //[TestFixture(Browser.PhantomJS)]
    class TS_Windows : BaseBrowsers {

        public TS_Windows(Browser browser)
            : base(browser) { }

        [TearDown]
        public void TearDown() {
            driver.Quit();
        }

        [TestCase]
        public void ShouldSwitchToNextWindows() {
            driver.Get("/win1.html");
            A.AreEqual("Window1", driver.Title);

            driver.FindElementByLinkText("Window2").Click();
            driver.SwitchToNextWindow();
            A.AreEqual("Window2", driver.Title);

            driver.FindElementByLinkText("Window3").Click();
            driver.SwitchToNextWindow();
            A.AreEqual("Window3", driver.Title);

            driver.SwitchToPreviousWindow();
            A.AreEqual("Window2", driver.Title);
        }

        [TestCase]
        public void ShouldSwitchToWindows() {
            driver.Get("/win1.html");
            A.AreEqual("Window1", driver.Title);

            driver.FindElementByLinkText("Window2").Click();
            driver.SwitchToWindowByTitle("Window2");
            A.AreEqual("Window2", driver.Title);

            driver.FindElementByLinkText("Window3").Click();
            driver.SwitchToWindowByTitle("Window3");
            A.AreEqual("Window3", driver.Title);

            driver.SwitchToWindowByName("win2");
            A.AreEqual("Window2", driver.Title);

            driver.SwitchToWindowByName("win3");
            A.AreEqual("Window3", driver.Title);
        }

        [TestCase]
        [IgnoreFixture(Browser.IE, "Not supported")]
        public void ShouldOpenLinkInNewWindow() {
            driver.Get("/links.html");
            driver.FindElementByLinkText("Win1", 5000).Click(Keys.Shift);
            driver.SwitchToNextWindow();
            A.AreEqual("Window1", driver.Title);
        }

    }

}
