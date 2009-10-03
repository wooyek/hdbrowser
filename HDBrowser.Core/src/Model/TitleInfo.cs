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
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace HDBrowser.Core.Model {

    public class TitleInfo : INotifyPropertyChanged {
        private TitleType type;
        private string name;
        private string year;
        private string id;
        private string url;
        private string coverUrl;
        private string rating;
        private string tagline;
        private string plot;
        private string runtime;
        private List<Link> directors;
        private List<string> genres;
        private List<Person> actors;
        public static string NoCoverUrl = "http://ia.media-imdb.com/media/imdb/01/I/47/58/83/10.gif";

        public TitleInfo() {
            this.name = "Unknown";
            this.Year = "?";
            this.Id = null;
            this.url = null;
            this.coverUrl = NoCoverUrl;
            this.rating = "?";
            this.tagline = "?";
            this.plot = "?";
            this.runtime = "?";    
        }


        [XmlAttribute]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute]
        public TitleType Type {
            get { return type; }
            set {
                if (value != this.type){
                    type = value;
                    NotifyPropertyChanged("Type");
                }
            }
        }

        [XmlAttribute]
        public string Name {
            get { return name; }
            set {
                if (value != this.name) {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public string Year {
            get { return year; }
            set
            {
                if (value != this.year)
                {
                    year = value;
                    NotifyPropertyChanged("Year");
                }
            }
        }
        public string Url {
            get { return url; }
            set
            {
                if (value != this.url)
                {
                    url = value;
                    NotifyPropertyChanged("Url");
                }
            }
        }

        public string CoverUrl {
            get { return coverUrl; }
            set {
                if (value != this.coverUrl){
                    coverUrl = value;
                    if (string.IsNullOrEmpty(coverUrl)) {
                        coverUrl = NoCoverUrl;
                    }
                    NotifyPropertyChanged("CoverUrl");
                }
            }
        }
        [XmlAttribute]
        public string Rating {
            get { return rating; }
            set {
                if (value != this.rating)
                {
                    rating = value;
                    NotifyPropertyChanged("Rating");
                } 
            }
        }

        public string Tagline {
            get { return tagline; }
            set {
                if (value != this.tagline)
                {
                    tagline = value;
                    NotifyPropertyChanged("Tagline");
                } 
            }
        }

        public string Plot {
            get { return plot; }
            set
            {
                if (value != this.plot)
                {
                    plot = value;
                    NotifyPropertyChanged("Plot");
                }
            }
        }
        [XmlAttribute]
        public string Runtime {
            get { return runtime; }
            set
            {
                if (value != this.runtime)
                {
                    runtime = value;
                    NotifyPropertyChanged("Runtime");
                }
            }
        }

        public List<Link> Directors {
            get { return directors; }
            set { directors = value; }
        }

        public List<string> Genres {
            get { return genres; }
            set { genres = value; }
        }

        public List<Person> Actors {
            get { return actors; }
            set { actors = value; }
        }

        public override string ToString() {
            return string.Format("Type: {0}, Name: {1}, Year: {2}, Id: {3}, Url: {4}, CoverUrl: {5}, Rating: {6}, Tagline: {7}, Plot: {8}, Runtime: {9}, Directors: {10}, Genres: {11}, Actors: {12}", type, name, year, id, url, coverUrl, rating, tagline, plot, runtime, directors, genres, actors);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void UpdateProperty(ref object property, object valueToSet, string propertyName) {
            if (property != valueToSet) {
                property = valueToSet;
                NotifyPropertyChanged(propertyName);
            }
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

    }

    public enum TitleType : int {
        Movie = 0, Series = 1
    }

    public class Link {
        [XmlAttribute]
        public LinkType Type;
        [XmlAttribute]
        public string Name;
        public string Url;
    }


    public class Person : Link {
        public string PhotoUrl;
        [XmlAttribute]
        public string Character;
    }

    public enum LinkType : int {
        Director, ShowCreator
    }
}