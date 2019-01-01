using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace ssl_requester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var hosts = File.ReadAllLines("hosts.txt").Select(s => s.Trim());

            foreach(var host in hosts)
                requestCertificate(host);

            using(IWebDriver driver = new ChromeDriver())
            {
                foreach(var host in hosts)
                    submitCertificateRequest(driver, host);
            }

        }


        static void requestCertificate(string hostname)
        {
            var dirname = hostname.Replace(".","_");

            if(Directory.Exists($"output/{dirname}")) {
                Console.WriteLine($"Output folder {hostname} already exists.");
                return;
            }

            Directory.CreateDirectory($"output/{dirname}");
            var cmd = $"/bin/bash";
            var args = $"-c \"openssl req -utf8 -nodes -sha256 -newkey rsa:2048 -keyout output/{dirname}/{dirname}.key -out output/{dirname}/{dirname}.csr -subj '/CN={hostname}/O=Universiteit van Amsterdam/C=NL'\"";
            var startInfo = new ProcessStartInfo(cmd, args);
            startInfo.CreateNoWindow = true;
            Process.Start(startInfo).WaitForExit();
        }

        static void submitCertificateRequest(IWebDriver driver, string hostname)
        {
            var escaped_hostname = hostname.Replace(".","_");

            if(File.Exists($"output/{escaped_hostname}/requested.txt"))
            {
                Console.WriteLine($"{hostname} already requested");
                return;
            }

            var csr = File.ReadAllText($"output/{escaped_hostname}/{escaped_hostname}.csr");

            driver.Navigate().GoToUrl("https://secret-url.com/");

            driver.FindElement(By.Name("guest_requester_first_name")).SendKeys("You");
            driver.FindElement(By.Name("guest_requester_last_name")).SendKeys("You");
            driver.FindElement(By.Name("guest_requester_email")).SendKeys("Your e-mail");

            driver.FindElement(By.Id("csr")).Clear();
            driver.FindElement(By.Id("csr")).SendKeys(csr.Trim());

            Thread.Sleep(1000);

            driver.FindElement(By.LinkText("Add Organization")).Click();

            Thread.Sleep(1000);
            driver.FindElement(By.Id("add-org-btn")).Click();

            Thread.Sleep(2000);
            driver.FindElement(By.Id("submit-request-button")).Click();

            File.WriteAllText($"output/{escaped_hostname}/requested.txt",":-)");
            Thread.Sleep(10000);
        }
    }
}
