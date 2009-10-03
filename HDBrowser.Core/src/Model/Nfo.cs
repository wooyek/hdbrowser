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
using System.Text;
using System.Text.RegularExpressions;
using log4net;

namespace HDBrowser.Core.Model {
    public class Nfo {
        private static readonly ILog log = LogManager.GetLogger(typeof (Nfo));
        public static string GetImdbUrl(FileInfo info) {
            FileStream stream = info.OpenRead();
            byte[] text = new byte[stream.Length];
            stream.Read(text, 0, (int)stream.Length);

            ASCIIEncoding encoding = new ASCIIEncoding();

            string content = encoding.GetString(text);
            MatchCollection matches = Regex.Matches(content, @"http://(www.|us.|former.){0,1}imdb.com/title/tt\d+/");

            foreach (Match match in matches) {
                return match.Value;
            }
            log.Info("GetImdbUrl: Could not find IMDB Url in "+info.FullName);
            return null;
        }

        public static FileInfo GetNfoFile(FileInfo mkv) {
            FileInfo nfo = new FileInfo(mkv.FullName.Replace(".mkv", ".nfo"));
            if (nfo.Exists) {
                return nfo;
            }
            DirectoryInfo dir = mkv.Directory;
            FileInfo[] infos = dir.GetFiles("*.nfo");
            if (infos.Length > 0) {
                if (infos.Length != 1) {
                    log.InfoFormat("GetNfoFile: Found {0} nfo files in {1}, trying the first one", infos.Length, dir.FullName);
                }
                return infos[0];
            }
            log.InfoFormat("GetNfoFile: Could not find NFO for {0}", mkv.FullName);
            return null;
        }
    }
}