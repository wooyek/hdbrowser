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
using log4net;
using NUnit.Framework;

namespace HDBrowser.Core.Model {
    [TestFixture]
    public class NfoTests {
        private static readonly ILog log = LogManager.GetLogger(typeof (NfoTests));

        [Test]
        public void GetImdbUrl() {
            string name = @"g:\filmy\Cass.2008.LIMITED.720p.BluRay.x264-REVEiLLE\rev-cass.nfo";
            string url = Nfo.GetImdbUrl(new FileInfo(name));
            log.DebugFormat("GetImdbUrl: url={0}", url);
            Console.Out.WriteLine("url = {0}", url);
        }
    }
}