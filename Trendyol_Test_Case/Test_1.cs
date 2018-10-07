using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trendyol_Test_Case
{


    [TestFixture(typeof(ChromeDriver))]
    [TestFixture(typeof(FirefoxDriver))]            // for parallel running both chrome and firefox
    [Parallelizable(ParallelScope.Fixtures)]
    public class Test_2<TWebDriver> where TWebDriver : IWebDriver, new()
    {



        //  string path = String.Format("{0}credentials_data.csv", AppDomain.CurrentDomain.BaseDirectory);   //Data file as a csv file in base directory, currently holding 5 copies of same credentials
        //Update: embedded the credentials file for easier access.


        [SetUp]
        public void Set_up()
        {
            
          


        }

        [Test]
        public  void Test2_Login()
        {
        

            /*     Test Case for data driven logins 
             *     Here, program reads from csv file that was mentioned earlier and creates an action for each entry.
             *      Login actions are in Test_paralel() function 
             
             */
            var listOfActions = new List<Action>();

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Trendyol_Test_Case.Resources.Credentials.txt";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                int count = 0;
                while (!reader.EndOfStream)
                {
                    string mail, password;
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                   
             
                    mail = values[0];
                    password = values[1];
                   
                    listOfActions.Add(() => Test_paralel(mail, password,count));
                   
                    count++;
                    
                }
                var options = new ParallelOptions { MaxDegreeOfParallelism =8  };//maximum number of parallel running tasks 
                Parallel.Invoke(options, listOfActions.ToArray());

            }
            
           
        }
    


        [TearDown]
        public void End_Test2()
        {
          
        }

       public void Test_paralel(string mail,string password,int i)
        {
            //Create a new instance of webdriver for each call
            IWebDriver driver = new TWebDriver();

            //navigate browser to 
            driver.Navigate().GoToUrl("https://www.trendyol.com/");

            //Close the popup
                FindElement(By.XPath("//*[@title='Kapat']"),driver).Click();

            FindElement(By.XPath("//*[@id=\"accountNavigationRoot\"]/div/ul/li[1]/div[1]/i"),driver).Click();

          

            //Click on login button and enter credentials
         
           FindElement(By.XPath("//*[@id=\"email\"]"),driver).SendKeys(mail);
           FindElement(By.XPath("//*[@id=\"password\"]"),driver).SendKeys(password);
           FindElement(By.XPath("//*[@id=\"loginSubmit\"]"),driver).Click();

            //wait 10 secs before closing browser after successfull login
            Thread.Sleep(10000);
            driver.Quit();
            
        }

         public IWebElement FindElement(By by,IWebDriver driver_)
        {
            //Funciton to wait for an element to exist
            var wait = new WebDriverWait(driver_, TimeSpan.FromSeconds(60));
            wait.Until(driver => driver_.FindElement(by));
            return waitElement(by,driver_);

        }

        private IWebElement waitElement(By by,IWebDriver driver_)
        {
            //Function to wait for an element to be clickable
            var wait = new WebDriverWait(driver_, TimeSpan.FromSeconds(60));
            var clickableElement = wait.Until(ExpectedConditions.ElementToBeClickable(by));
            return driver_.FindElement(by);

        }

    }

   public class Test_1
    {

        /*  Test Case for gettin image loading times and logging them.
         * It is not proper to measure loading times from selenium,browser already has network logs we can access.
         * 
         * 
         */

        IWebDriver driver;
      //log file locatin is the base directory of project 
        string path = String.Format("{0}log.txt", AppDomain.CurrentDomain.BaseDirectory);



        [SetUp]
        public void Initialize_Test1()
        {

         /*Chrome doesnt log failed requests but firefox does. I used chrome for the sake of reliability 
          * For chrome I tried getting all img resources and searched for them in network log entries.If it is not in there it is a failed request.
          */
            driver = new FirefoxDriver();
        }




        [Test]
        public void Test1_log_resource_timing()
        {

            driver.Navigate()
                   .GoToUrl("http://www.trendyol.com/");

            //check if there is a popup
            var a = driver.FindElements(By.XPath("//*[@title='Kapat']"));        

            if (a.Count()!=0)
            {
                driver.FindElement(By.XPath("//*[@title='Kapat']")).Click();

            }
        
           

           IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
          
            //Network entry logs has a buffer limit, since I need all the entries I need to increase that limit,
            jse.ExecuteScript("performance.setResourceTimingBufferSize(5000);");

            //manually added the last element at the trendyol.com to load, at the time I wrote this to know that I reached the end of page.
            //Scrolls down untill we see the last element.
            while (driver.FindElements(By.XPath("//*[@id=\"littleCampaigns\"]/div[686]/div[1]/a/img")).Count()==0)
            {
                jse.ExecuteScript("window.scrollBy(0,200)");
                Thread.Sleep(150);
                
            }
            jse.ExecuteScript("window.scrollBy(0,1000)");
            //added a 1 sec delay for last elements to load
            Thread.Sleep(1000);

            List<string> img_src = new List<string>();//List where I store src attribute of img files
            List<string> entry_name = new List<string>();//List where I store network log entries
            
            //Javascript to get all entries with resource tag
            var performance_ = (ReadOnlyCollection<Object>)jse.ExecuteScript("var performance = window.performance || window.mozPerformance || window.msPerformance || window.webkitPerformance || {}; var network = performance.getEntriesByType('resource') || {}; return network;");


            //Getting all elements with img tag
           var images = driver.FindElements(By.TagName("img"));

            foreach (var element in images)
            {
                try
                {
                    //getting src of img tags if exists
                    img_src.Add(element.GetAttribute("src"));
                }
                catch (Exception)
                {

                 
                }
            }

            int count = 0;

            //writer for writing into log file
            StreamWriter writer = new StreamWriter(path, true, Encoding.Unicode);
            writer.AutoFlush = true;

            foreach (Dictionary<string, object> item in performance_) 
                {
                //iterate over resource network logs we got from browser to find img records.
                //when an img entry found get the name of image and full loading duration.
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

            //Compare img list we got from browser with network logs of img files to find failed logs in chrome
            //firefox doesnt need this , firefox failed entries show 0 loading duration
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
