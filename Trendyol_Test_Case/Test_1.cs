using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Trendyol_Test_Case
{
    class Test_1
    {

        IWebDriver driver;

        [SetUp]
        public void Initialize()
        {

            ChromeOptions options = new ChromeOptions();                    //Start selenium
            options.AddArguments("--start-maximized");                  //Fullscreen browser

      //      options.PerformanceLoggingPreferences.IsCollectingNetworkEvents = true;


            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;                   //hide the command prompt that opens with browser
            driver = new ChromeDriver(driverService, options);
        }

        [Test]
        public void Open_Page()
        {
            string scriptToExecute = "var performance = window.performance || window.mozPerformance || window.msPerformance || window.webkitPerformance || {}; var network = performance.getEntriesByType('resource') || {}; return network;";

            driver.Navigate()
                   .GoToUrl("http://www.trendyol.com/");

            var a = driver.FindElements(By.XPath("//*[@title='Kapat']"));        

            if (a.Count()!=0)
            {
                driver.FindElement(By.XPath("//*[@title='Kapat']")).Click();

            }
        
            

            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;

            /*
                      do
                      {

                          jse.ExecuteScript("window.scrollBy(0,500)", "");


                          Thread.Sleep(500);


                      }   
                      while (driver.FindElements(By.XPath("//*[@data-id='196783']")).Count() == 0);





                      jse.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");


                      while (driver.FindElements(By.XPath("//*[@data-id='196783']")).Count()==0)
                      {
                          Thread.Sleep(2000);
                      }
                      */

            //       ReadOnlyCollection<Object> netData = (ReadOnlyCollection<Object>)jse.ExecuteScript(scriptToExecute);
            //    ReadOnlyCollection<Object> netData_copy = (ReadOnlyCollection<Object>)jse.ExecuteScript(scriptToExecute);


            IList<IWebElement> list = driver.FindElements(By.TagName("img"));
            IList<IWebElement> temp_list = new List<IWebElement>();
            IList<IWebElement> dif_list = list.Except(temp_list).ToList();
            List<string> source_list = new List<string>();
            temp_list=temp_list.Union(list).ToList();

            int count = 0;
            int source_count = 0;
            IList<IWebElement> bad_list = driver.FindElements(By.TagName("img"));

            do
            {
                foreach (var item in dif_list)
                {

                                 try
                              {
                        string item_xpath = getElementXPath(item);

                        if (item_xpath.Contains("ecaesliderkadin") || item_xpath.Contains("ilhamverenslider"))
                            continue;

                       
                            Actions builder = new Actions(driver);
                            builder.MoveToElement(item).Perform();
                       

                        

                        string source = item.GetAttribute("src");
                        source_list.Add(source);
                        source_count++;
                    }
                    catch (Exception)
                    {


                    }

                    if(source_count>9)
                    {
                        var asdf = (ReadOnlyCollection<Object>)jse.ExecuteScript("var performance = window.performance || window.mozPerformance || window.msPerformance || window.webkitPerformance || {}; var network = performance.getEntriesByName('" + source_list[source_count-9].Replace("\r", "").Replace("\n", "") + "') || {}; return network;");
                       


                    }





                    count++;
                    
                }
               
                Thread.Sleep(100);
                list = driver.FindElements(By.TagName("img"));
                dif_list = list.Except(temp_list).ToList();
                temp_list = temp_list.Union(list).ToList();

            } while (dif_list.Count()!=0 || !driver.Url.Equals("https://www.trendyol.com/?pi=34"));

           





            /*

                        do
                        {

                            jse.ExecuteScript("window.scrollBy(0,500)", "");



                             netData =(ReadOnlyCollection<Object>)jse.ExecuteScript(scriptToExecute);

                             var diff = netData.Except(netData_copy);

                            netData_copy = netData;

                        }
                        while (driver.FindElements(By.XPath("//*[@data-id='196783']")).Count()==0);

                */
            jse.ExecuteScript("window.scrollBy(0,400)", "");


            
            


        }

        [TearDown]
        public void End_Test()
        {
            driver.Close();
        }




        public String getElementXPath( IWebElement element)
        {
            return (String)((IJavaScriptExecutor)driver).ExecuteScript("gPt=function(c){if(c.id!==''){return'id(\"'+c.id+'\")'}if(c===document.body){return c.tagName}var a=0;var e=c.parentNode.childNodes;for(var b=0;b<e.length;b++){var d=e[b];if(d===c){return gPt(c.parentNode)+'/'+c.tagName+'['+(a+1)+']'}if(d.nodeType===1&&d.tagName===c.tagName){a++}}};return gPt(arguments[0]).toLowerCase();", element);
        }
    }
}
