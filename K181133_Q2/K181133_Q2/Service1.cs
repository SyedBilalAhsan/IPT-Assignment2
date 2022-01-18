using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Xml.Linq;
using Timer = System.Timers.Timer;

namespace K181133_Q2
{
    public partial class Service1 : ServiceBase
    {
        List<Feeds> postList = new List<Feeds>();
        Timer t;
        public Service1()
        {
            InitializeComponent();
            t = new Timer();
            t.Interval = 5 * 60 * 1000;                            //Timer for 5 min
            t.Elapsed += new System.Timers.ElapsedEventHandler(RssService);
        }
        public void onDebug()
        {
            OnStart(null);
        }
        public class Feeds
        {
            public string title { get; set; }
            public string description { get; set; }
            public string pubDate { get; set; }
            public string NewsChannel { get; set; }
            public DateTime DatetoSort { get; set; }

        }
        private void SortbyDateTime()
        {
            postList.Sort((y, x) => y.DatetoSort.CompareTo(x.DatetoSort));
            saveinXml();
            postList.Clear();
        }
        private void saveinXml()
        {
            try
            {
                string xmlpath = ConfigurationManager.AppSettings["xmlPath"];
                foreach (Feeds i in postList)
                {
                    if (!File.Exists(xmlpath))
                    {
                        XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                        xmlWriterSettings.Indent = true;
                        xmlWriterSettings.NewLineOnAttributes = true;
                        using (XmlWriter xmlWriter = XmlWriter.Create(xmlpath, xmlWriterSettings))
                        {
                            xmlWriter.WriteStartDocument();
                            xmlWriter.WriteStartElement("News");

                            xmlWriter.WriteStartElement("NewsItem");
                            xmlWriter.WriteElementString("Title", i.title);
                            xmlWriter.WriteElementString("Description", i.description);
                            xmlWriter.WriteElementString("PublishedDate", i.pubDate);
                            xmlWriter.WriteElementString("NewsChannel", i.NewsChannel);
                            xmlWriter.WriteEndElement();

                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteEndDocument();
                            xmlWriter.Flush();
                            xmlWriter.Close();
                        }
                    }
                    else
                    {
                        XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                        xmlWriterSettings.Indent = true;
                        xmlWriterSettings.NewLineOnAttributes = true;
                        XDocument xmlDoc = XDocument.Load(xmlpath);
                        XElement root = xmlDoc.Element("News");
                        IEnumerable<XElement> rows = root.Descendants("NewsItem");
                        XElement firstRow = rows.First();
                        firstRow.AddBeforeSelf(new XElement("NewsItem", new XElement("Title", i.title), new XElement("Description", i.description), new XElement("PublishedDate", i.pubDate), new XElement("NewsChannel", i.NewsChannel)));
                        xmlDoc.Save(xmlpath);
                    }
                }
                
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            

        }
        public void RssFeedGeo()
        {
            var reader = XmlReader.Create("https://www.geo.tv/rss/1/2");
            var feed = SyndicationFeed.Load(reader);

            foreach (var i in feed.Items)
            {
                Feeds rss = new Feeds();
                rss.title = i.Title.Text;
                string[] desc = i.Summary.Text.Split('\t');
                rss.description = desc[4];
                rss.pubDate = i.PublishDate.ToString();
                rss.DatetoSort = i.PublishDate.DateTime;
                rss.NewsChannel = i.Id.ToString();
                postList.Add(rss);
            }
        }
        public void RssFeedTheNews()
        {
            var reader = XmlReader.Create("https://www.thenews.com.pk/rss/1/3");
            var feed = SyndicationFeed.Load(reader);

            foreach (var i in feed.Items)
            {
                Feeds rss = new Feeds();
                rss.title = i.Title.Text;
                string[] desc = i.Summary.Text.Split('\t');
                rss.description = desc[4];
                rss.pubDate = i.PublishDate.ToString();
                rss.DatetoSort = i.PublishDate.DateTime;
                rss.NewsChannel = i.Id.ToString();
                postList.Add(rss);
            }
        }
        public void RssService(object sender, ElapsedEventArgs e)
        {
            RssFeedGeo();
            RssFeedTheNews();
            SortbyDateTime();
        }
        protected override void OnStart(string[] args)
        {
            t.Enabled = true;
        }
        
        protected override void OnStop()
        {
            t.Enabled = false;
        }
    }

   
}
