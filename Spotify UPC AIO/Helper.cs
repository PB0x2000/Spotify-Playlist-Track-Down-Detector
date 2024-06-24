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
using static Spotify_UPC_AIO.Program;
using System.Diagnostics;

namespace Spotify_UPC_AIO
{
    internal class Helper
    {
        
        public static void DB_READ()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            if(File.ReadAllText("DB.json") == "") { return; }
            Program.DB db = JsonConvert.DeserializeObject<Program.DB>(File.ReadAllText("DB.json"), settings);
            Program.DB_SONGS = db.Tracks;
            Program.DB_ARTISTS = db.Artists;
            Program.DB_PLAYLISTS = db.Playlists;
        }
        public static void DB_WRITE()
        {
            Program.DB db = new Program.DB();
            db.Tracks = Program.DB_SONGS;
            db.Artists = Program.DB_ARTISTS;
            db.Playlists = Program.DB_PLAYLISTS;
            string output = JsonConvert.SerializeObject(db);
            File.WriteAllText("DB.json", output);
        }
        public static ChromeDriver CHROME_START()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("mute-audio");
            chromeOptions.AddArgument("--disable-notifications");
            //chromeOptions.AddArgument("--headless");
            chromeOptions.AddArgument(String.Concat(@"--user-data-dir=", Directory.GetCurrentDirectory(), @"\TEMP"));
            chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);
            chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.notifications", 2);
            chromeOptions.PageLoadStrategy = PageLoadStrategy.Eager;
            var chromeDriverService = ChromeDriverService.CreateDefaultService("Chrome");
            chromeDriverService.HideCommandPromptWindow = true;
            chromeDriverService.SuppressInitialDiagnosticInformation = true;
            return new ChromeDriver(chromeDriverService, chromeOptions);
        }
        public static void ExplicitWaitXpath(ChromeDriver driver, string xpath, int seconds)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
            wait.Until(ExpectedConditions.ElementExists(By.XPath(xpath)));
        }
        public static void ExplicitWaitId(ChromeDriver driver, string idname, int seconds)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
            wait.Until(ExpectedConditions.ElementExists(By.Id(idname)));
        }
        public static void ExplicitWaitClass(ChromeDriver driver, string classname, int seconds)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
            wait.Until(ExpectedConditions.ElementExists(By.ClassName(classname)));
        }
        public static void scrapeARTISTS()
        {
            List<DB_ARTIST> tmp_artists = DB_ARTISTS.ToList();
            todo = tmp_artists.Count;
            foreach (DB_ARTIST artist in tmp_artists)
            {
                if (artist.LAST_CHECKED == "null" || (Int32.Parse(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()) - Int32.Parse(artist.LAST_CHECKED)) > 3600)
                {
                    Spotify.GetAndAddRelatedArtists(artist.ID);
                }
                progress++;
            }
        }
        public static void scrapePLAYLISTS()
        {
            List<DB_ARTIST> tmp_artists = DB_ARTISTS;
            todo = tmp_artists.Count;
            foreach (DB_ARTIST artist in tmp_artists)
            {
                if (artist.LAST_CHECKED != "null" && (Int32.Parse(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()) - Int32.Parse(artist.LAST_CHECKED)) < 3600)
                {
                    continue;
                }
                driver.Navigate().GoToUrl("https://open.spotify.com/intl-de/artist/" + artist.ID + "/discovered-on");

                try
                {
                    Helper.ExplicitWaitXpath(driver, "//section[@data-testid='component-shelf']", 30);
                }
                catch { continue; }


                IWebElement box = driver.FindElements(By.XPath("//div[@data-testid='grid-container']"))[0];

                List<IWebElement> featured_playlists = box.FindElements(By.CssSelector("div:first-child")).ToList();

                foreach (IWebElement element in featured_playlists)
                {
                    try
                    {
                        string link = element.FindElement(By.XPath(".//a")).GetAttribute("href");
                        string id = Regex.Match(link, @"playlist\/(.*)").Groups[1].Value;

                        if (link.Contains("playlist") && !DB_PLAYLISTS.Exists(x => x.ID == id))
                        {
                            string name = element.FindElement(By.XPath(".//a")).GetAttribute("title");
                            DB_PLAYLIST playlist = new DB_PLAYLIST();
                            playlist.ID = id;
                            playlist.NAME = name;
                            DB_PLAYLISTS.Add(playlist);
                            added++;
                            File.AppendAllText("playlist_ids_out.txt", (id + "\n"));
                        }
                        else if (link.Contains("playlist") && DB_PLAYLISTS.Exists(x => x.ID == id))
                        {
                            DB_PLAYLIST playlist = DB_PLAYLISTS.First(x => x.ID == id);
                            string name = element.FindElement(By.XPath(".//a")).GetAttribute("title");
                            if (playlist.NAME != name)
                            {
                                playlist.NAME = name;
                            }
                        }
                    }
                    catch (Exception e) { }
                }
                progress++;
            }
        }
        public static void addArtistsManual()
        {
            List<string> lines = File.ReadAllLines(@"Data\add_artists.txt").ToList();
            foreach (string line in lines)
            {
                string id = "";
                if (line.Contains("http"))
                {
                    id = Regex.Match(line, @"artist\/(.*)").Groups[1].Value;
                }
                else
                {
                    id = line;
                }
                if (!DB_ARTISTS.Exists(x => x.ID == id))
                {
                    DB_ARTIST artist = new DB_ARTIST();
                    artist.ID = id;
                    artist.NAME = "unknown";
                    artist.LAST_CHECKED = "null";
                    DB_ARTISTS.Add(artist);
                }
            }
        }
    }
}
