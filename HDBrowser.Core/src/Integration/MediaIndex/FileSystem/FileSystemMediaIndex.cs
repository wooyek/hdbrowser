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
using System.Linq;
using System.Text;
using log4net;
using WooYek.Common.Doc;

namespace HDBrowser.Core.Integration.MediaIndex.FileSystem
{
    public class FileSystemMediaIndex: DocumentBase<FileSystemMediaIndex> {
        private static readonly ILog log = LogManager.GetLogger(typeof (FileSystemMediaIndex));
        private List<string> movieDirectories = new List<string>();
        private List<FileEntry> titles = new List<FileEntry>();
        public static string IndexPath = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + "HdBrowser.Index.xml";
        private static decimal minSize = 100 * 1024 * 1024;

        public FileSystemMediaIndex() {
            base.Path = IndexPath;
        }

        public static FileSystemMediaIndex Intialize() {
            if (new FileInfo(IndexPath).Exists) {
                try {
                    return Load(IndexPath);
                } catch (Exception e) {
                    string message = string.Format("Loading an existing FileSystemMediaIndex file failed: {0}", IndexPath);
                    throw new Exception(message, e);
                }
            } else {
                return new FileSystemMediaIndex();
            }
        }

        public List<string> MovieDirectories {
            get { return movieDirectories; }
            set { movieDirectories = value; }
        }

        public List<FileEntry> Titles {
            get { return titles; }
            set { titles = value; }
        }

        public void FindTitles() {
            titles = FindTitles(movieDirectories);
        }

        private static List<FileEntry> FindTitles(List<string> movieDirs) {
            List<FileEntry> titles = new List<FileEntry>();
            foreach (string dir in movieDirs) {
                List<FileEntry> files = FindTitles(new DirectoryInfo(dir));
                if (files == null) {
                    return null;
                }
                titles.AddRange(files);
            }
            return titles;
        }

        private static List<FileEntry> FindTitles(DirectoryInfo location){
            if (!location.Exists) {
                log.InfoFormat("FindTitles: Does not exists {0}", location.FullName);
                return null;
            }
            log.DebugFormat("FindTitles: in {0}", location);
            FileInfo[] files = location.GetFiles();
//            log.DebugFormat("FindTitles: files.Length={0}", files.Length);
            IEnumerable<FileInfo> fis = from file in files where file.Extension == ".mkv" && !file.Name.ToLower().Contains("sample") && file.Length > minSize select file;

            List<FileEntry> mediaFiles = new List<FileEntry>();
            foreach (FileInfo fi in fis){
//                log.DebugFormat("FindTitles: fi={0}", fi);
                FileEntry file = FileEntry.New(fi);
//                log.DebugFormat("FindTitles: t={0}", file);
                mediaFiles.Add(file);
            }

            DirectoryInfo[] directories = location.GetDirectories();
            IEnumerable<DirectoryInfo> dirs = from dir in directories where !dir.Name.ToLower().Contains("sample") select dir;
//            log.DebugFormat("FindTitles: dirs.Length={0}", dirs.Length);
            foreach (DirectoryInfo dir in dirs) {
                List<FileEntry> titles = FindTitles(dir);
                mediaFiles.AddRange(titles);
            }

            return mediaFiles;

        }


        public void AddLocation(string directoryPath) {
            movieDirectories.Add(directoryPath);
        }

        public void UpdateAllInfos(bool async) {
            FileEntry.UpdateInfos(async, titles);
        }

        public void CheckForNewMovies(bool updateNewAsync) {
            IEnumerable<FileEntry> enumerable = CheckForNewMovies();
            FileEntry.UpdateInfos(updateNewAsync, enumerable);
        }

        public IEnumerable<FileEntry> CheckForNewMovies(){
            List<FileEntry> files = FindTitles(movieDirectories);
            if (files == null){
                return null;
            }
            List<FileEntry> newTitles = new List<FileEntry>();
            newTitles.AddRange(from file in files where !ContainsMovie(file.Location) select file);
            titles.AddRange(newTitles);
            return newTitles;
        }

        public bool ContainsMovie(string path) {
            return titles.Where(title => title.Location == path).Count() > 0;
        }
    }


}