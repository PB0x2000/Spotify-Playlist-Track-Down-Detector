using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Leaf.xNet;
using System.Drawing;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
using Console = Colorful.Console;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.DevTools.V115.Emulation;
using OpenQA.Selenium.DevTools.V115.Network;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Security.Policy;
using OpenQA.Selenium.DevTools.V113.Security;

namespace Spotify_UPC_AIO
{
    internal class Program
    {
        public static List<DB_TRACK> DB_SONGS = new List<DB_TRACK>();
        public static List<DB_ARTIST> DB_ARTISTS = new List<DB_ARTIST>();
        public static List<DB_PLAYLIST> DB_PLAYLISTS = new List<DB_PLAYLIST>();
        public static int added = 0;
        public static int offline = 0;
        public static int progress = 0;
        public static int todo = 0;
        public static int TIME = 0;
        public static string MODE = "BETA";
        public static string offset = "";
        public static ChromeDriver driver;
        public class DB
        {
            public List<DB_TRACK> Tracks { get; set; }
            public List<DB_ARTIST> Artists { get; set; }
            public List<DB_PLAYLIST> Playlists { get; set; }
        }
        public class DB_TRACK
        {
            public string ID { get; set; }
            public string ISRC { get; set; }
            public string UPC { get; set; }
            public string NAME { get; set; }
            public string RELEASE { get; set; }
            public string ADDED { get; set; }
            public string POPULARITY { get; set; }
            public List<string> ARTISTS { get; set; }
            public List<string> PLAYLISTS { get; set; }
        }
        public class DB_ARTIST
        {
            public string ID { get; set; }
            public string NAME { get; set; }
            public string LAST_CHECKED { get; set; }
        }
        public class DB_PLAYLIST
        {
            public string ID { get; set; }
            public string NAME { get; set; }
        }



        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                INIT();

                Thread a = new Thread(stats);
                a.Start();

                Thread b = new Thread(stats);
                b.Start();

                Thread c = new Thread(stats);
                c.Start();

                Helper.addArtistsManual();
                Helper.DB_WRITE();

                //scrapeARTISTS();
                Helper.scrapePLAYLISTS();
                Helper.DB_WRITE();

                Console.WriteLine("Press enter to start scraping");
                Console.ReadKey();




                Console.WriteLine("\nRunning!", Color.Green);
                while(true)
                {
                    Thread.Sleep(1000);
                }
                Console.ReadKey();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadKey();
            }
        }

        public static void scraper()
        {
            while (true)
            {
                try
                {

                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
        }

        public static void INIT()
        {
            Helper.DB_READ();
            Spotify.auth = Spotify.GetBearerToken(Spotify.basic);
            driver = Helper.CHROME_START();
        }
        
        public static void stats()
        {
            while (true)
            {
                Thread.Sleep(1000);
                Console.Title = "Spotify UPC AIO    [ " + MODE + " ]            Progress [ " + progress + " | " + todo + " ]      Added [ "+ added + " ]";
            }
        }
    }
}
