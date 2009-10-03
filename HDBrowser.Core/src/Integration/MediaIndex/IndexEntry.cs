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
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using HDBrowser.Core.Integration.MediaIndex.FileSystem;
using HDBrowser.Core.Integration.MediaNfoSources.Imdb;
using HDBrowser.Core.Model;
using HdBrowser.Integration;
using log4net;

namespace HDBrowser.Core.Integration.MediaIndex {
    /// <summary>
    /// Media index entry abstraction.
    /// </summary>
    public class IndexEntry {
        private static readonly ILog log = LogManager.GetLogger(typeof (IndexEntry));
        protected const bool updateInfoFromInternet = true;
        protected TitleInfo info;
        protected string title;

        public TitleInfo Info {
            get { return info; }
            set { info = value; }
        }

        [XmlAttribute]
        public string Title {
            get { return title; }
            set { title = value; }
        }

        public void Update(bool async) {
            FileEntry.log.InfoFormat("Update: trying to update {0}", (object) this.Title);
            if (this.info.Url != null){
                if (async) {
                    ImdbAsync.Update(this.info);
                } else {
                    Imdb.Update(this.info);
                }
            } else {
                FileEntry.log.Info("Update: No URL to update from");
            }

        }
    }


}