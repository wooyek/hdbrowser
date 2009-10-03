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
using System.Reflection;
using System.Threading;
using HdBrowser.Integration;
using log4net.Core;
using NUnit.Framework;
using WooYek.Common.Logging;

namespace HDBrowser.Core.Integration.MediaIndex.FileSystem
{
    [TestFixture]
    public class FileSystemMediaIndexTests {
        const string moviesLocation = @"g:\filmy\21.Grams.2003.720p.DTheater.DTS.x264-ESiR\";
//        const string moviesLocation = @"g:\filmy\";
        
        [TestFixtureSetUp]
        public void TestFixtureSetUp(){
            LoggingConfig.ConfigureLoggingWithExecutingAssembly(Level.Debug);
        }

        [Test]
        public void UpdateAndStore() {
            FileSystemMediaIndex i = new FileSystemMediaIndex();
            i.AddLocation(moviesLocation);
            i.FindTitles();
            i.UpdateAllInfos(false);
            i.Store();

        }       
        
        [Test,Ignore("TODO")]
        public void UpdateAndStoreAsync() {
            FileSystemMediaIndex i = new FileSystemMediaIndex();
            i.AddLocation(moviesLocation);
            i.FindTitles();
            i.UpdateAllInfos(true);
            i.Store();
            ImdbAsync.UpdateComplete += delegate(object sender, ImdbAsync.UpdateCompleteEventArgs args){
                                                                                                           if (ImdbAsync.WorkerCount <1) {
                                                                                                               i.Store();
                                                                                                               lock(i) {
                                                                                                                   Monitor.Pulse(i);
                                                                                                               }
                                                                                                           } 
            }; 
            lock(i) {
                Monitor.Wait(i);
            }
        }

    }
}