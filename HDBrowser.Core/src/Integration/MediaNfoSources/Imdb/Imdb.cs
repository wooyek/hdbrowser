// 
//    Copyright 2009 Janusz Skonieczny
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
// $Id: $
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HDBrowser.Core.Model;
using log4net;

namespace HDBrowser.Core.Integration.MediaNfoSources.Imdb
{
    public static class Imdb {
        private static readonly ILog log = LogManager.GetLogger(typeof (Imdb));

        public static string GetPage(string url) {
            try {
                return GetPage(new Uri(url));
            } catch (Exception e) {
                throw new Exception("GetPage failed: " + url, e);
            }
        }

        public static string GetPage(Uri url) {
            log.DebugFormat("GetPage: {0}", url);
            try {
                var myRequest = (HttpWebRequest) WebRequest.Create(url);
                myRequest.AllowAutoRedirect = true;
                myRequest.Method = "GET";
                myRequest.Timeout = 6000;
                using (WebResponse myResponse = myRequest.GetResponse()) {
                    using (var sr = new StreamReader(myResponse.GetResponseStream())) {
                        return sr.ReadToEnd();
                    }
                }
            } catch (Exception e) {
                throw new Exception("GetPage failed for: " + url, e);
            }
        }

        public static void Update(TitleInfo titleInfo) {
            string page = GetPage(titleInfo.Url);
            ParseTitlePage(page, titleInfo);
        }

        public static TitleInfo ParseTitlePage(Uri url) {
            string page = GetPage(url);
            return ParseTitlePage(page);
        }

        public static TitleInfo ParseTitlePage(string pageSource) {
            TitleInfo titleInfo = new TitleInfo();
            titleInfo.Type = TitleType.Movie;
            ParseTitlePage(pageSource, titleInfo);
            return titleInfo;
        }

        private static void ParseTitlePage(string pageSource, TitleInfo titleInfo) {
            log.InfoFormat("ParseTitlePage: updating {0}", titleInfo.Name);
            string pat = @"<title>(.*?)\((\d{4}).*?\)</title>";
            var reg = new Regex(pat);
            Match match = reg.Match(pageSource);
            string title = match.Groups[1].Value;
            string year = match.Groups[2].Value;

            pat = @"<h1>(.*?)</h1>";
            reg = new Regex(pat);
            string type = reg.Match(pageSource).Groups[1].Value;
            bool tvSeries = type.Contains("TV series");
            if ((titleInfo.Type == TitleType.Movie && tvSeries) || (titleInfo.Type == TitleType.Series && !tvSeries)) {
                log.InfoFormat("ParseTitlePage: tvSeries={0} titleType={1}", tvSeries, titleInfo.Type);
                return;
            }
            pat = @";id=(tt\d{7});";
            reg = new Regex(pat);
            string link = "http://www.imdb.com/title/" + reg.Match(pageSource).Groups[1].Value + "/";
            titleInfo.Url = link;
            titleInfo.Id = reg.Match(pageSource).Groups[1].Value;

            titleInfo.Name = CleanText(title);
            titleInfo.Year = CleanText(year);
            titleInfo.CoverUrl = GetCoverUrl(pageSource);
            titleInfo.Rating = GetCoverRating(pageSource);
            titleInfo.Directors = GetDirectors(titleInfo, pageSource);
            if (titleInfo.Type == TitleType.Series) {
                SetSeasons(titleInfo, pageSource);
            }

            titleInfo.Genres = GetGenres(pageSource);
            titleInfo.Tagline = GetTagline(pageSource);
            titleInfo.Plot = GetPlot(pageSource);
            titleInfo.Actors = GetActors(pageSource);
            titleInfo.Runtime = GetRuntime(pageSource);
            log.InfoFormat("ParseTitlePage: complete {0}", titleInfo.Name);
        }

        public static string GetRuntime(string pageSource) {
            const string pat = @"<h5>Runtime.*?\n(\d+ min)";
            Regex reg = new Regex(pat);
            Match match = reg.Match(pageSource);

            if (match.Success) {
                return match.Groups[1].Value;
            }
            return null;
        }

        public static List<Person> GetActors(string pageSource) {
            const int actorLimit = 5;
            string pat = @"<h3>Cast.*?(<a href=.*?)<a class=";
            Regex reg = new Regex(pat);
            Match match = reg.Match(pageSource);
            if (match.Success) {
                List<Person> actors = new List<Person>();
                string temp = match.Groups[1].Value;
                //pat = @"<a href="".*?<img src=""(.*?)"".*?<a href=""(.*?)"">(.*?)</a>.*?(href=""/character/.*?"">(.*?))?</a>";
                pat = @"<a href="".*?<img src=""(.*?)"".*?<a href=""(.*?)"">(.*?)</a>.*?(<td class=""char"">(.*?))?</td></tr>";
                reg = new Regex(pat);
                MatchCollection matches = reg.Matches(temp);
                int count = 0;
                foreach (Match m in matches) {
                    if (actorLimit == -1 || (count < actorLimit)) {
                        var actor = new Person();
                        actor.Name = CleanText(m.Groups[3].Value);
                        string caract = m.Groups[5].Value;
                        if (!string.IsNullOrEmpty(caract)) {
                            if (caract.Contains("<a href=")) {
                                pat = @"href=""/character/.*?"">(.*?)</a>";
                                reg = new Regex(pat);
                                caract = reg.Match(caract).Groups[1].Value;
                            }
                        }
                        actor.Character = CleanText(caract);

                        if (m.Groups[1].Value != "http://i.media-imdb.com/images/tn15/addtiny.gif") {
                            actor.PhotoUrl = m.Groups[1].Value;
                        }

                        actor.Url = "http://www.imdb.com" + m.Groups[2].Value;
                        actors.Add(actor);
                        count++;
                    }
                }
                return actors;
            }
            return null;
        }

        public static string GetPlot(string pageSource) {
            string pat = @"<h5>Plot.*?\n(.*?)\n?<";
            Regex reg = new Regex(pat);
            return CleanText(reg.Match(pageSource).Groups[1].Value.Trim());
        }

        public static string GetTagline(string pageSource) {
            string pat = @"<h5>Tagline.*?\n(.*?)\n?<";
            Regex reg = new Regex(pat);
            return CleanText(reg.Match(pageSource).Groups[1].Value.Trim());
        }

        public static List<string> GetGenres(string pageSource) {
            string pat = @"<h5>Genre.*?\n(<a href=.*?)<a class=";
            Regex reg = new Regex(pat);
            Match match = reg.Match(pageSource);
            if (match.Success) {
                var genres = new List<string>();
                string temp = match.Groups[1].Value;
                pat = @""">(.*?)</a>";
                reg = new Regex(pat);
                MatchCollection matches = reg.Matches(temp);
                foreach (Match m in matches) {
                    genres.Add(CleanText(m.Groups[1].Value));
                }
                return genres;
            }
            return null;
        }

        public static void SetSeasons(TitleInfo titleInfo, string pageSource) {
            const int season = 1;
            int eSeas;
            string pat = @"<h5>Seasons.*?(<a href=.*?)</a>\n{1,2}<a class";
            Regex reg = new Regex(pat, RegexOptions.Singleline);
            Match match = reg.Match(pageSource);

            if (match.Success) {
                string startSeas = "episodes#season-";
                if (season == -1) {
                    startSeas += "1";
                } else {
                    startSeas += season;
                }
                string temp = match.Groups[1].Value;
                reg = new Regex(startSeas, RegexOptions.Singleline);
                match = reg.Match(temp);
                if (match.Success) {
                    //parseSeason(titleInfo.Url + startSeas, eSeas, titleInfo);
                }
            }
        }

        public static List<Link> GetDirectors(TitleInfo titleInfo, string pageSource) {
            string pat;
            Regex reg;
            Match match;
            if (titleInfo.Type == TitleType.Movie) //directors
            {
                var directors = new List<Link>();
                pat = @"<h5>Director.*?\n(<a href=.*?</a>)<br/>\n{1,2}</div>";
                reg = new Regex(pat, RegexOptions.Singleline);
                match = reg.Match(pageSource);
                if (match.Success) {
                    string temp = match.Groups[1].Value;
                    pat = @"<a href=""(.{16})"">(.*?)</a>";
                    reg = new Regex(pat);
                    MatchCollection matches = reg.Matches(temp);
                    foreach (Match m in matches) {
                        var director = new Link();
                        director.Type = 0;
                        director.Name = CleanText(m.Groups[2].Value);
                        director.Url = "http://www.imdb.com" + m.Groups[1].Value;
                        directors.Add(director);
                    }
                    return directors;
                }
            } else if (titleInfo.Type == TitleType.Series) //creators
            {
                var creators = new List<Link>();
                pat = @"<h5>Creator.*?\n(<a href=.*?</a>)<br/>\n{1,2}<a class";
                reg = new Regex(pat, RegexOptions.Singleline);
                match = reg.Match(pageSource);
                if (match.Success) {
                    string temp = match.Groups[1].Value;
                    pat = @"<a href=""(.{16})"">(.*?)</a>";
                    reg = new Regex(pat, RegexOptions.Singleline);
                    MatchCollection matches = reg.Matches(temp);
                    foreach (Match m in matches) {
                        var creator = new Link();
                        creator.Type = LinkType.ShowCreator;
                        creator.Name = CleanText(m.Groups[2].Value);
                        creator.Url = "http://www.imdb.com" + m.Groups[1].Value;
                        creators.Add(creator);
                    }
                    return creators;
                }
            }
            return null;
        }

        public static string GetCoverRating(string pageSource) {
            const string pat = @"<b>([0-9/\.]+)*.?</b>";
            Regex reg = new Regex(pat);
            string rating = reg.Match(pageSource).Groups[1].Value;
            return CleanText(rating);
        }

        public static string GetCoverUrl(string pageSource) {
            // check it does have an image.
            string noImage = "http://ia.media-imdb.com/media/imdb/01/I/37/89/15/10.gif";
            string addPoster = "http://i.media-imdb.com/images/SFe8dc8ed3d786ce2d03c60bd6bc16d4c8/intl/en/title_addposter.jpg";
            if (!pageSource.Contains(noImage) && !pageSource.Contains(addPoster)) {
                const string pat = @"<a name=""poster"".*?href=""(.*?)"".*?src=""(.*?)"".*?><";
                Regex reg = new Regex(pat);
                Match match = reg.Match(pageSource);
                string lowRes = match.Groups[2].Value;
                log.DebugFormat("GetCoverUrl: lowRes={0}", lowRes);
                string posterPage = match.Groups[1].Value;
                return GetImageUrl("http://www.imdb.com" + posterPage);
            }
            return null;
        }

        private static string GetImageUrl(string imageUrl) {
            string page = GetPage(imageUrl);
            const string pat = @"<table id=""principal"">.*?<img.*?src=""(.*?)"">";
            Regex reg = new Regex(pat, RegexOptions.Singleline);
            return reg.Match(page).Groups[1].Value;
        }

        public static string CleanText(string line) {
            //TODO: this is lame, look for by-the-book way to replace codes to characters.
            line = line.Replace("&#38;", "&").Replace("&#233;", "é").Replace("&#225;", "á").Replace("&#237;", "í");
            line = line.Replace("&#243;", "ó").Replace("&#250;", "ú").Replace("&#224;", "à").Replace("&#232;", "è");
            line = line.Replace("&#236;", "ì").Replace("&#242;", "ò").Replace("&#249;", "ù").Replace("&#193;", "Á");
            line = line.Replace("&#201;", "É").Replace("&#205;", "Í").Replace("&#211;", "Ó").Replace("&#218;", "Ú");
            line = line.Replace("&#192;", "À").Replace("&#200;", "È").Replace("&#204;", "Ì").Replace("&#210;", "Ò");
            line = line.Replace("&#217;", "Ù").Replace("&#227;", "ã").Replace("&#245;", "õ").Replace("&#241;", "ñ");
            line = line.Replace("&#195;", "Ã").Replace("&#213;", "Õ").Replace("&#209;", "Ñ").Replace("&#226;", "â");
            line = line.Replace("&#234;", "ê").Replace("&#194;", "Â").Replace("&#202;", "Ê").Replace("&#34;", "\"");
            return line;
        }

        public static List<TitleInfo> SearchDirty(String query) {
            query = CleanForSearch(query);
            return Search(query);
        }

        public static string CleanForSearch(string query) {
            log.DebugFormat("CleanForSearch: {0}", query);
            query = Remove(query, @".?(\d{4}.*)");
            query = Remove(query, @".?(\.mkv*)");
            query = Remove(query, @".?(\.REPACK*)");
            query = query.Replace('.', ' ');
            return query;
        }

        private static string Remove(string query, string pattern) {
            Regex reg = new Regex(pattern);
            Match match = reg.Match(query);
            if (match.Success) {
                query = query.Replace(match.Groups[1].Value, "");
            }
            return query;
        }

        public static List<TitleInfo> Search(String query) {
            log.DebugFormat("Search: {0}", query);
            string url = "http://www.imdb.com/find?s=tt&q=" + query;
            TitleType media = TitleType.Movie;
            string page = GetPage(url);
            PageType pageType = GetPageType(page, query);
            if (pageType == PageType.Uknown) {
                return null;
            }
            log.DebugFormat("Search: pageType={0}", pageType);
            List<TitleInfo> resutls = new List<TitleInfo>();
            if(pageType == PageType.TitlePage) {
                TitleInfo info = ParseTitlePage(page);
                resutls.Add(info);
                return resutls;
            }
            string line = "";
            string pat = "(<p><b>.*?)\n";
            Regex reg = new Regex(pat);
            Match match = reg.Match(page);
            if (match.Success) {
                line = match.Groups[1].Value;
            }
            resutls.AddRange(ParseLinks(line, media, @"<b>Popular Titles</b>(.*?)<p><b>", "Popular Titles"));
            resutls.AddRange(ParseLinks(line, media, @"<b>Titles \(Exact Matches\)</b>(.*?)<p><b>", "Exact Matches"));
            resutls.AddRange(ParseLinks(line, media, @"<b>Titles \(Approx Matches\)</b>(.*?)<p><b>", "Approximated Matches"));
            resutls.AddRange(ParseLinks(line, media, @"<b>Titles \(Partial Matches\)</b>(.*?)(<p><b>|</table> )", "Partial Matches"));
            resutls.AddRange(ParseLinks(line, media, @"<b>Popular Titles</b>(.*?)<p><b>", "Popular Titles"));
            log.InfoFormat("Search: Found {0}", resutls.Count);
            return resutls;
        }

        private static List<TitleInfo> ParseLinks(string line, TitleType media, string pattern, string titles) {
            Regex reg = new Regex(pattern);
            Match match = reg.Match(line);
            if (match.Success) {
                return ParseLinks(match.Groups[1].Value, titles, media);
            }
            return new List<TitleInfo>();
        }

        private static List<TitleInfo> ParseLinks(string section, string cat, TitleType type) {
            List<TitleInfo> links = new List<TitleInfo>();
            string pat = @"(<a href=.*?)</td></tr>";
            Regex reg = new Regex(pat);
            MatchCollection matches = reg.Matches(section);

            if (matches.Count > 0) {
                foreach (Match m in matches) {
                    string line = m.Groups[1].Value;
                    line = CleanText(line);
                    pat = @"<a href=""(.{17})";
                    reg = new Regex(pat);
                    string url = reg.Match(line).Groups[1].Value;
                    pat = @"<img src=""(.*?)""";
                    reg = new Regex(pat);
                    string cover = reg.Match(line).Groups[1].Value;
                    pat = @";"">(&#34;|"")?(['&,áéíóúàèìòùÁÉÍÓÚÀÈÌÒÙãÃõÕâêÊÂñÑA-Za-z0-9 :\(\)!]*)(&#34;|"")?</a>\s*?\((\d{4}).*?\)";
                    reg = new Regex(pat);
                    Match mat = reg.Match(line);
                    TitleInfo l = new TitleInfo();
                    l.Name = mat.Groups[2].Value;
                    l.Year = mat.Groups[4].Value;
                    l.Url = "http://www.imdb.com" + url;
                    l.CoverUrl = cover;
                    //l.Category = cat;
                    pat = @"<small>(.*?)</small>";
                    reg = new Regex(pat);
                    if (reg.Match(line).Groups[1].Value != null && reg.Match(line).Groups[1].Value != "") {
                        l.Type = TitleType.Series;
                    } else {
                        l.Type = TitleType.Movie;
                    }

                    if (l.Type == type) {
                        links.Add(l);
                    }
                }
            }
            return links;
        }

        public static PageType GetPageType(string page, String query) {
            Regex reg = new Regex("<title>(.*?)</title>");
            string title = reg.Match(page).Groups[1].Value;
            log.DebugFormat("GetPageType: title={0}", title);
            if (title.StartsWith("IMDb")){
                return PageType.SearchResultsPage;
            }

            if (title.ToLower().Contains(query.ToLower())) {
                return PageType.TitlePage;
            }
            return PageType.Uknown;
        }

        public enum PageType {
            Uknown,
            SearchResultsPage,
            TitlePage,
            ShowPage,
            ImagePage,
        }
    }
}