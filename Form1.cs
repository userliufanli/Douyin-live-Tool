using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SpeechLib;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        #region key
        private static string StapiKey = "火山API";
        private static string secretKey = "火山API";       
        #endregion
        AIapi AIapi = new AIapi(StapiKey, secretKey);
        private string HtmlMessage;
        private Thread thread;
        private IWebDriver driver;
        private bool isFetching = false;
        private bool IsStart = false;
        private static string question = "";



        public Form1()
        {
            InitializeComponent();
            InitializeWebView2Async();


        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {

            HtmlMessage = e.TryGetWebMessageAsString();
            数据解析(HtmlMessage);


        }

        private async void InitializeWebView2Async()
        {
            await webView22.EnsureCoreWebView2Async(null);
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Newhtml.html");
            string uri = new Uri(filePath).AbsoluteUri;
            webView22.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            webView22.CoreWebView2.Navigate(uri);


        }

        private void 数据解析(string data)
        {

            switch (data)
            {
                case "开始监控":
                    开始监控();
                    break;
                case "开始回答":
                    开始回答();
                    break;
                case"测试":
                    测试();
                    break;
                default:
                    MessageBox.Show("！");
                    break;
            }

        }
        private void 测试()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(async () =>
                {
                    // 在 UI 线程中执行 JavaScript 代码
                     webView22.CoreWebView2.PostWebMessageAsString("666");
                }));
            }
            else
            {
                for (int i = 0; i < 100; i++)
                {
                    // 已经在 UI 线程中，直接执行 JavaScript 代码
                    webView22.CoreWebView2.PostWebMessageAsString(i.ToString());
                }
              
            }

        }
        private void 开始监控()
        {

            isFetching = true;
            thread = new Thread(评论获取线程);
            thread.Start();

        }
        private async void 开始回答()
        {


                IsStart = true;
                responses = await AIapi.ChatWithDoubao(question);
            

          

        }

        private HashSet<string> 评论储存 = new HashSet<string>();
        object Timerlock = new object();


        private void 评论获取线程()
        {

            try
            {

                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--enable-unsafe-swiftshader");
                options.AddArgument("--no-sandbox");
                driver = new ChromeDriver(Path.Combine(AppDomain.CurrentDomain.BaseDirectory)+"chromedriver.exe", options);
                string liveUrl = "https://anchor.douyin.com/anchor/dashboard?from=blcenter";
                driver.Navigate().GoToUrl(liveUrl);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                while (isFetching)
                {
                    try
                    {

                        var commentElements = driver.FindElements(By.CssSelector("span.chatItemDesc--KHShO.undefined[elementtiming='element-timing']"));
                        foreach (var commentElement in commentElements)
                        {
                            string commentText = commentElement.Text;
                            if (commentText == "")
                            {
                            }
                            else
                            {
                                if (!评论储存.Contains(commentText))
                                {
                                    lock (Timerlock)
                                    {
                                        // 检查是否需要调度到 UI 线程
                                        if (this.InvokeRequired)
                                        {
                                            this.Invoke(new Action(async () =>
                                            {
                                               // 在 UI 线程中执行 JavaScript 代码
                                                webView22.CoreWebView2.PostWebMessageAsString("评论:" + commentText+"\r\n");
                                            }));
                                        }
                                        else
                                        {
                                            // 已经在 UI 线程中，直接执行 JavaScript 代码
                                            webView22.CoreWebView2.PostWebMessageAsString("评论:"+commentText + "\r\n");
                                        }
                                       
                                        question = commentText;
                                        字段解析();
                                        评论储存.Add(commentText);
                                        commentText = "";
                                    }

                                }


                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        isFetching = false;
                    }
                    Thread.Sleep(2000);
                }

            }
            catch (Exception e)
            {

                MessageBox.Show($"发生错误: {e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);


            }
            finally
            {
                if (driver != null)
                {
                    driver.Quit();
                }
            }
        }


        string responses = "";
        SpVoice spVoice = new SpVoice();

        private async void 字段解析()
        {

            if (IsStart && question != "")
            {
                try
                {
                    string response = await AIapi.ChatWithDoubaos(question, responses);
                    // 解析 JSON 字符串
                    JObject jObject = JObject.Parse(response);
                    // 获取 choices 数组
                    JArray choices = (JArray)jObject["choices"];
                    // 通常 choices 数组里只有一个元素
                    JObject choice = (JObject)choices[0];
                    // 获取 message 对象
                    JObject message = (JObject)choice["message"];
                    // 提取 content 字段
                    string sontent = (string)message["content"];
                    // 检查是否需要调度到 UI 线程
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(async () =>
                        {
                            // 在 UI 线程中执行 JavaScript 代码
                             webView22.CoreWebView2.PostWebMessageAsString("回复:"+sontent+"\n\r");
                        }));
                    }
                    else
                    {
                        // 已经在 UI 线程中，直接执行 JavaScript 代码
                        webView22.CoreWebView2.PostWebMessageAsString("回复"+sontent+"\n\r");
                    }
              
                    question = "";
                    spVoice.Rate = 0;
                    spVoice.Volume = 100;
                    spVoice.Voice = spVoice.GetVoices().Item(0);
                    spVoice.Speak(sontent);


                }
                catch (Exception)
                {

                    question = "";

                }


            }



        }
    }
}

