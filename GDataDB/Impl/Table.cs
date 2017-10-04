using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GDataDB.Impl {
    public class Table<T> : ITable<T> where T: new() {
        private readonly DatabaseClient client;
        private readonly Uri listFeedUri;
        private readonly Uri worksheetUri;
        private readonly Serializer<T> serializer = new Serializer<T>();

        public Table(DatabaseClient client, Uri listFeedUri, Uri worksheetUri) {
            if (listFeedUri == null)
                throw new ArgumentNullException("listFeedUri");
            if (client == null)
                throw new ArgumentNullException("client");
            if (worksheetUri == null)
                throw new ArgumentNullException("worksheetUri");
            this.client = client;
            this.listFeedUri = listFeedUri;
            this.worksheetUri = worksheetUri;
        }

        public void Delete() {
            var http = client.RequestFactory.CreateRequest();
            http.UploadString(worksheetUri, method: "DELETE", data: "");
        }

        public void Clear() {
            // https://developers.google.com/google-apps/spreadsheets/#deleting_a_list_row_1
            var rows = Find(new Query {
                Order = new Order {
                    Descending = true,
                }
            });
            foreach (var row in rows)
                row.Delete();
        }

        public void Rename(string newName) {
            var http = client.RequestFactory.CreateRequest();
            var response = http.DownloadString(worksheetUri);
            var xmlResponse = XDocument.Parse(response);
            var title = xmlResponse.Root.Element(Utils.AtomNs + "title");
            if (title == null)
                throw new Exception("Title was null in worksheet feed entry");
            title.Value = newName;
            http = client.RequestFactory.CreateRequest();
            http.UploadString(worksheetUri, method: "PUT", data: xmlResponse.Root.ToString());
        }

        public IRow<T> Add(T e) {
            // https://developers.google.com/google-apps/spreadsheets/#adding_a_list_row_1
            var xml = serializer.SerializeNewRow(e);
            var http = client.RequestFactory.CreateRequest();
            var response = http.UploadString(listFeedUri, xml.ToString());
            var xmlResponse = XDocument.Parse(response);
            var row = serializer.DeserializeRow(xmlResponse.Root, client);
            return row;
        }

        public IRow<T> Get(int rowNumber) {
            var q = new Query {
                Count = 1,
                Start = rowNumber,
            };
            var results = Find(q);
            if (results.Count == 0)
                return null;
            return results[0];
        }

        public IList<IRow<T>> FindAll() {
            return Find(new Query());
        }

        public IList<IRow<T>> FindAll(int start, int count) {
            return Find(new Query {
                Start = start,
                Count = count,
            });
        }

        public IList<IRow<T>> Find(string query) {
            return Find(new Query {FreeQuery = query});
        }


        public static string SerializeQuery(Query q) {
            var b = new StringBuilder();

            if (q.FreeQuery != null)
                b.Append("q=" + Utils.UrlEncode(q.FreeQuery) + "&");
            if (q.Start > 0)
                b.Append("start-index=" + q.Start + "&");
            if (q.Count > 0)
                b.Append("max-results=" + q.Count + "&");
            if (q.Order != null) {
                if (q.Order.ColumnName != null)
                    b.Append("orderby=column:" + Utils.UrlEncode(q.Order.ColumnName) + "&");
                if (q.Order.Descending)
                    b.Append("reverse=true&");
            }

            return b.ToString();
        }

        public IList<IRow<T>> Find(Query q) {
            var http = client.RequestFactory.CreateRequest();
            var uri = listFeedUri + "?" + SerializeQuery(q);
            var rawResponse = http.DownloadString(uri);
            var xmlResponse = XDocument.Parse(rawResponse);
            return xmlResponse.Root.Elements(Utils.AtomNs + "entry")
                .Select(e => serializer.DeserializeRow(e, client))
                .ToList();
        }
    }
}