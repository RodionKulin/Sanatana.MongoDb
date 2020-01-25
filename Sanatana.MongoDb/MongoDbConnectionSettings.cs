using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.MongoDb
{ 
    public class MongoDbConnectionSettings
    {
        //properties
        public string Host { get; set; }
        public int Port { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionsPrefix { get; set; }
        public MongoCredential Credential { get; set; }
    }
}
