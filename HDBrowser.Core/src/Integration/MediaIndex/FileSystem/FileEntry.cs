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
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using HDBrowser.Core.Integration.MediaNfoSources.Imdb;
using HDBrowser.Core.Model;
using HdBrowser.Integration;
using log4net;

namespace HDBrowser.Core.Integration.MediaIndex.FileSystem
{
    public class FileEntry : IndexEntry    {
        private static ILog log = LogManager.GetLogger(typeof(FileEntry));
        private string location;
        private string nfoLocation;

        public static FileEntry New(string path) {
            FileInfo fileInfo = new FileInfo(path);
            return New(fileInfo);
        }

        public static FileEntry New(FileInfo fileInfo) {
            if (!fileInfo.Exists) {
                throw new ArgumentException("File does not exist " + fileInfo.FullName);
            }
            log.DebugFormat("New: {0}", fileInfo);
            FileEntry file = new FileEntry();
            file.title = fileInfo.Name;
            file.location = fileInfo.FullName;
            file.info = file.GetInfoBasedOnFile();
            FileInfo nfo = Nfo.GetNfoFile(fileInfo);
            if (nfo != null && updateInfoFromInternet){
                file.nfoLocation = nfo.FullName;
                file.info.Url = Nfo.GetImdbUrl(nfo);                
            }
            return file;
        }

        public TitleInfo GetInfoBasedOnFile() {
            Regex reg = new Regex(@"(.+?)\.?(BluRay|720p|1080p)");
            string name = reg.Match(this.title).Groups[1].Value;
            if (String.IsNullOrEmpty(name)) {
                name = this.title;
            }
            return new TitleInfo { Name = name };
        }

        public override string ToString() {
            return String.Format("Title: {0}, MediaLocation: {1}, TitleInfo: {2}, NfoLocation: {3}", title, location, info, nfoLocation);
        }

        public string Location {
            get { return location; }
            set { location = value; }
        }

        public string NfoLocation {
            get { return nfoLocation; }
            set { nfoLocation = value; }
        }

        public static void UpdateInfos(bool async, IEnumerable<FileEntry> files) {
            if (files == null) {
                return;
            }
            foreach (FileEntry file in files) {
                file.Update(async);
            }
        }
    }
}