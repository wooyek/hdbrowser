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
using System.IO;
using System.Text.RegularExpressions;
using log4net;
using log4net.Core;
using NUnit.Framework;
using WooYek.Common.Logging;

namespace HDBrowser.Core.Integration.MediaIndex.FileSystem
{
    [TestFixture]
    public class FileEntryTests {
        private static readonly ILog log = LogManager.GetLogger(typeof(FileEntryTests));

        [TestFixtureSetUp]
        public void TestFixtureSetUp(){
            LoggingConfig.ConfigureLoggingWithExecutingAssembly(Level.Debug);
            log.Debug("TestFixtureSetUp: ");
        }

        [Test]
        public void NewTitle() {
            string name = @"g:\filmy\Cass.2008.LIMITED.720p.BluRay.x264-REVEiLLE\rev-cass.mkv";
            FileEntry file = FileEntry.New(name);
            Console.Out.WriteLine("file.TitleInfo = {0}", file.Info);
            Assert.AreEqual("rev-cass.mkv", file.Info.Name);
            //todo: fallback to direcotry name if file name is uninformtive
            //Assert.AreEqual("Cass.2008.LIMITED", file.Info.Name);
        }

        [Test]
        public void UpdateInfoBaseOnFile() {
            FileEntry mf = FileEntry.New(@"g:\filmy\Breaking.and.Entering.2006.BluRay.720p.x264.dts-HDL\Breaking.and.Entering.2006.BluRay.720p.x264.dts-HDL.mkv");
            Assert.AreEqual("Breaking.and.Entering.2006", mf.Info.Name);
        }

        [Test]
        public void GetNfo() {
            string mkv =
                @"g:\filmy\How.to.Lose.Friends.&.Alienate.People.2008.720p.BluRay.DTS.x264-ESiR\How.to.Lose.Friends.&.Alienate.People.2008.720p.BluRay.DTS.x264-ESiR.mkv";
            string nfo =
                @"g:\filmy\How.to.Lose.Friends.&.Alienate.People.2008.720p.BluRay.DTS.x264-ESiR\How.to.Lose.Friends.&.Alienate.People.2008.720p.BluRay.DTS.x264-ESiR .nfo";
            FileEntry mf = FileEntry.New(mkv);
            Assert.AreEqual(nfo, mf.NfoLocation); ;

        }


    }
}