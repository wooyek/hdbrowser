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
using System.Text.RegularExpressions;
using HDBrowser.Core.Integration.MediaNfoSources.Imdb;
using log4net.Core;
using NUnit.Framework;
using WooYek.Common.Logging;

namespace HdBrowser.Integration {
    [TestFixture]
    public class ImdbTest {
        [TestFixtureSetUp]
        public void TestFixtureSetUp(){
            LoggingConfig.ConfigureLoggingWithExecutingAssembly(Level.Debug);
        }
        [Test]
        public void CoverUrl() {
            string page = Imdb.GetPage("http://www.imdb.com/title/tt0381505/");
            string coverUrl = Imdb.GetCoverUrl(page);
            //lowres
            //string expected = "http://ia.media-imdb.com/images/M/MV5BMTI0NTA1MDkyOF5BMl5BanBnXkFtZTcwOTg3NTAzMQ@@._V1._SX94_SY140_.jpg";
            //normal
            const string expected = "http://ia.media-imdb.com/images/M/MV5BMTI0NTA1MDkyOF5BMl5BanBnXkFtZTcwOTg3NTAzMQ@@._V1._SX270_SY400_.jpg";
            Assert.AreEqual(expected, coverUrl);
        }

        [Test]
        public void Re1() {
            string s =
                "<a name=\"poster\" href=\"/rg/action-box-title/primary-photo/media/rm3475021056/tt0381505\" title=\"Pretty Persuasion\"><img alt=\"Pretty Persuasion\" title=\"Pretty Persuasion\" src=\"http://ia.media-imdb.com/images/M/MV5BMTI0NTA1MDkyOF5BMl5BanBnXkFtZTcwOTg3NTAzMQ@@._V1._SX94_SY140_.jpg\" border=\"0\"></a>";
            const string pat = @"<a name=""poster"".*?href=""(.*?)"".*?src=""(.*?)"".*?><";
            Regex reg = new Regex(pat);
            Match match = reg.Match(s);
            Console.Out.WriteLine("match.Groups[1].Value = {0}", match.Groups[1].Value);
            Console.Out.WriteLine("match.Groups[2].Value = {0}", match.Groups[2].Value);
        }        
        
        [Test]
        public void Re2() {
            const string s = @"<center><table id=""principal"">
<tr><td valign=""middle"" align=""center""><img oncontextmenu=""return false;"" galleryimg=""no"" onmousedown=""return false;"" onmousemove=""return false;"" src=""http://ia.media-imdb.com/images/M/MV5BMTI0NTA1MDkyOF5BMl5BanBnXkFtZTcwOTg3NTAzMQ@@._V1._SX270_SY400_.jpg""></td></tr>
</table></center>
";
            const string pat = @"<table id=""principal"">.*?<img.*?src=""(.*?)"">";
            Regex reg = new Regex(pat,RegexOptions.Singleline);
            Match match = reg.Match(s);
            Console.Out.WriteLine("match.Groups[1].Value = {0}", match.Groups[1].Value);
        }

        [Test]
        public void CleanForSearch() {
            string q = Imdb.CleanForSearch("Forgetting Sarah Marshall 2008 Unrated");
            Assert.AreEqual("Forgetting Sarah Marshall ",q);
            q = Imdb.CleanForSearch("9.rota.2005");
            Assert.AreEqual("9 rota ",q);
            q = Imdb.CleanForSearch("In.My.Fathers.Den");
            Assert.AreEqual("In My Fathers Den", q);
            q = Imdb.CleanForSearch("Star Trek Insurrection.mkv");
            Assert.AreEqual("Star Trek Insurrection", q);
            q = Imdb.CleanForSearch("Star Trek Insurrection.REPACK");
            Assert.AreEqual("Star Trek Insurrection", q);
        }
    }
}