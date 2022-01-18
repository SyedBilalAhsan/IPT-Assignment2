using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace K181133_Q1
{
    public partial class Service1 : ServiceBase
    {
        Timer t;
        public Service1()
        {
            InitializeComponent();
            t = new Timer();
            t.Interval = 15 * 60 * 1000;                            //Timer for 15 min
            t.Elapsed += new System.Timers.ElapsedEventHandler(sendMail);
        }

        public void onDebug()
        {
            OnStart(null);
        }
        public void sendMail(object sender, ElapsedEventArgs e)
        {
            try
            {
                string AllfilesPath = ConfigurationManager.AppSettings["AllFilesPath"];               
                var files = Directory.GetFiles(AllfilesPath, "*.json", SearchOption.AllDirectories);
                JsonModel data = JsonConvert.DeserializeObject<JsonModel>(File.ReadAllText(files[0]));

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("ba1906313@gmail.com");
                mail.To.Add(data.To);
                mail.Subject = data.Subject;
                mail.Body = data.MessageBody;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("ba1906313@gmail.com", "qwerty123/");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                    
                File.Delete(files[0]);
                //Thread.Sleep(15 * 60 * 1000);            ///Timer for 15 min
                //sendMail();                        
            }
            catch (Exception)
            {
                string ErrorLog = ConfigurationManager.AppSettings["ErrorFile"];
                if (File.Exists(ErrorLog))
                {
                    TextWriter tw = new StreamWriter(ErrorLog);
                    tw.Write("File Not Exists");
                    tw.Close();
                }
                else
                {
                    File.Create(ErrorLog);
                    TextWriter tw = new StreamWriter(ErrorLog);
                    tw.Write("File Not Exists");
                    tw.Close();
                }
            }
        }
        protected override void OnStart(string[] args)
        {
            t.Enabled = true;
            //sendMail(null,null);
        }

        protected override void OnStop()
        {
            t.Enabled = false;
        }
    }
}
