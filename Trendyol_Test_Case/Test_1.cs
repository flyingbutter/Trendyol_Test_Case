using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trendyol_Test_Case
{

    
    [TestFixture(typeof(ChromeDriver))]
    [TestFixture(typeof(FirefoxDriver))]
    [Parallelizable(ParallelScope.Fixtures)]
    public class Test_2<TWebDriver> where TWebDriver : IWebDriver, new()
    {
     
                      
        string path = String.Format("{0}credentials_data.csv", AppDomain.CurrentDomain.BaseDirectory);
        private List<IWebDriver> drivers=new List<IWebDriver>();
        private List<string> handles = new List<string>();
        private TWebDriver driver;

        [SetUp]
        public void Set_up()
        {

               this.driver = new TWebDriver();

          
        }

        [Test]
        public  void Test2_Login()
        {
            driver.Close();
            var listOfActions = new List<Action>();

          
            using (var reader = new StreamReader(path))
            {
                int count = 0;
                while (!reader.EndOfStream)
                {
                    string mail, password;
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                   
             //       String handle = driver1.CurrentWindowHandle;//Return a string of alphanumeric window handle
               //     handles.Add(handle);
                    mail = values[0];
                    password = values[1];
                   
                    listOfActions.Add(() => Test_paralel(mail, password,count));
                    //    Task task = new Task(() => test_paralel(mail, password));
                    //   task.Start();
                   
                    count++;
                    
                }
                var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
                Parallel.Invoke(options, listOfActions.ToArray());

            }
            
           
        }
    


        [TearDown]
        public void End_Test2()
        {
          
        }

       public void Test_paralel(string mail,string password,int i)
        {
            IWebDriver driver = new TWebDriver();
          //  drivers.Add(driver1);
          //  IWebDriver driver = drivers[i-1];
          //  driver.SwitchTo().Window(handles[i-1]);
            driver.Navigate().GoToUrl("https://www.trendyol.com/");

          //  var a = driver.FindElements(By.XPath("//*[@title='Kapat']"));

       //     if (a.Count() != 0)
      //      {
                FindElement(By.XPath("//*[@title='Kapat']"),driver).Click();

        //    }

            Actions action = new Actions(driver);
            action.MoveToElement(FindElement(By.XPath("//*[@id=\"accountNavigationRoot\"]/div/ul/li[1]/div[1]/i"),driver)).Perform();

           FindElement(By.XPath("//*[@id=\"accountNavigationRoot\"]/div/ul/li[1]/div[2]/div/div[1]"),driver).Click();
           FindElement(By.XPath("//*[@id=\"email\"]"),driver).SendKeys(mail);
           FindElement(By.XPath("//*[@id=\"password\"]"),driver).SendKeys(password);
           FindElement(By.XPath("//*[@id=\"loginSubmit\"]"),driver).Click();

            Thread.Sleep(10000);
            driver.Quit();
            
        }

         public IWebElement FindElement(By by,IWebDriver driver_)
        {

            var wait = new WebDriverWait(driver_, TimeSpan.FromSeconds(60));
            wait.Until(driver => driver_.FindElement(by));
            return waitElement(by,driver_);

        }

        private IWebElement waitElement(By by,IWebDriver driver_)
        {

            var wait = new WebDriverWait(driver_, TimeSpan.FromSeconds(60));
            var clickableElement = wait.Until(ExpectedConditions.ElementToBeClickable(by));
            return driver_.FindElement(by);

        }

    }

   public class Test_1
    {

        IWebDriver driver;
      
        string path = String.Format("{0}log.txt", AppDomain.CurrentDomain.BaseDirectory);



        [SetUp]
        public void Initialize_Test1()
        {

           // ChromeOptions options = new ChromeOptions();                    //Start selenium
          //  options.AddArguments("--start-maximized");                  //Fullscreen browser
         //   var driverService = ChromeDriverService.CreateDefaultService();
          //  driverService.HideCommandPromptWindow = true;                   //hide the command prompt that opens with browser
          //  driver = new ChromeDriver(driverService, options);
            driver = new FirefoxDriver();
        }




        [Test]
        public void Test1_log_resource_timing()
        {

            driver.Navigate()
                   .GoToUrl("http://www.trendyol.com/");

            var a = driver.FindElements(By.XPath("//*[@title='Kapat']"));        

            if (a.Count()!=0)
            {
                driver.FindElement(By.XPath("//*[@title='Kapat']")).Click();

            }
        
            

           IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
          
            jse.ExecuteScript("performance.setResourceTimingBufferSize(5000);");

            while (driver.FindElements(By.XPath("//*[@id=\"littleCampaigns\"]/div[686]/div[1]/a/img")).Count()==0)
            {
                jse.ExecuteScript("window.scrollBy(0,250)");
                Thread.Sleep(150);
                
            }
            jse.ExecuteScript("window.scrollBy(0,1000)");
            Thread.Sleep(1000);

            List<string> img_src = new List<string>();
            List<string> entry_name = new List<string>();

            var performance_ = (ReadOnlyCollection<Object>)jse.ExecuteScript("var performance = window.performance || window.mozPerformance || window.msPerformance || window.webkitPerformance || {}; var network = performance.getEntriesByType('resource') || {}; return network;");

           var images = driver.FindElements(By.TagName("img"));

            foreach (var element in images)
            {
                try
                {
                    img_src.Add(element.GetAttribute("src"));
                }
                catch (Exception)
                {

                 
                }
            }

            int count = 0;
            StreamWriter writer = new StreamWriter(path, true, Encoding.Unicode);
            writer.AutoFlush = true;

            foreach (Dictionary<string, object> item in performance_) 
                {
                string name, duration;
                if (!item["initiatorType"].Equals("img"))
                {
                    continue;
                }
                    else
                {
                    name = item["name"].ToString();
                    duration = item["duration"].ToString();
                        entry_name.Add(name);
                       

                        writer.WriteLine(count +"   :   "+name + "   :   " + duration
                            , Encoding.Unicode);
                       
                        count++;
                    }
                  
                }

            List<string> failed_entries = img_src.Except(entry_name).ToList();

            foreach (var item in failed_entries)
            {
                writer.WriteLine(count + "   :   " + item + "   :   failed"
                                          , Encoding.Unicode);
            }

            writer.Close();

        }

        [TearDown]
        public void End_Test1()
        {
            driver.Close();
        }

       

    }

   


}
