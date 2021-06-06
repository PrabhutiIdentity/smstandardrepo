using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;
using System.IO;
using SMEnterprise.Models;

namespace SMEnterprise.Repository
{
    public static class GioData
    {
        public static string _luceneDir
        {
            get
            {
                if (!System.IO.Directory.Exists(HttpRuntime.AppDomainAppPath + CommonUsage.GioIndexBasePath))
                {
                    System.IO.Directory.CreateDirectory(HttpRuntime.AppDomainAppPath + CommonUsage.GioIndexBasePath);
                    
                }
                return HttpRuntime.AppDomainAppPath + CommonUsage.GioIndexBasePath;
            }
        }
        private static FSDirectory _directoryTemp;
        private static FSDirectory _directory
        {
            get
            {
                if (_directoryTemp == null) _directoryTemp = FSDirectory.Open(new DirectoryInfo(_luceneDir));
                if (IndexWriter.IsLocked(_directoryTemp)) IndexWriter.Unlock(_directoryTemp);
                var lockFilePath = Path.Combine(_luceneDir, "write.lock");
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return _directoryTemp;
            }
        }

        // search methods
        public static IEnumerable<TransportGeoModel> GetAllIndexRecords()
        {
            // validate search index
            if (!System.IO.Directory.EnumerateFiles(_luceneDir).Any()) return new List<TransportGeoModel>();

            // set up lucene searcher
            var searcher = new IndexSearcher(_directory, false);
            var reader = IndexReader.Open(_directory, false);
            var docs = new List<Document>();
            var term = reader.TermDocs();
            // v 2.9.4: use 'hit.Doc()'
            // v 3.0.3: use 'hit.Doc'
            while (term.Next()) docs.Add(searcher.Doc(term.Doc));
            reader.Dispose();
            searcher.Dispose();
            return _mapLuceneToDataList(docs);
        }
        public static IEnumerable<TransportGeoModel> Search(string input, string fieldName = "")
        {
            if (string.IsNullOrEmpty(input)) return new List<TransportGeoModel>();

            var terms = input.Trim().Replace("-", " ").Split(' ')
                .Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim());
            input = string.Join(" ", terms);

            return _search(input, fieldName);
        }
        public static IEnumerable<TransportGeoModel> SearchDefault(string input, string GioDate = "",string VehicleID="")
        {
            return  _search(input, GioDate,VehicleID);//string.IsNullOrEmpty(input) ? new List<TransportGeoModel>() :
        }

        // main search method
        private static IEnumerable<TransportGeoModel> _search(string searchQuery, string GioDate = "",string VehicleID="")
        {
            // validation
            int PageIndex = 0;
            int PageSize = 50;
           // if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", ""))) return new List<TransportGeoModel>();

            // set up lucene searcher
            using (var searcher = new IndexSearcher(_directory, false))
            {
                var hits_limit = 1000;
                var analyzer = new StandardAnalyzer(Version.LUCENE_30);


                BooleanQuery bq = new BooleanQuery();
                if (VehicleID != "")
                {
                    Query qry = new QueryParser(Version.LUCENE_CURRENT, "VehicleID", analyzer).Parse(VehicleID);
                    bq.Add(qry, Occur.MUST);
                }
                Query qry2 = new QueryParser(Version.LUCENE_CURRENT, "GioDate", analyzer).Parse(GioDate);

                bq.Add(qry2, Occur.MUST);

                TopDocs topDocs = searcher.Search(bq, null, ((PageIndex + 1) * PageSize), Sort.RELEVANCE);
                ScoreDoc[] scoreDocs = topDocs.ScoreDocs;
                var results = _mapLuceneToDataList(scoreDocs, searcher);
                analyzer.Close();
                searcher.Dispose();
                return results;

                //// search by single field
                //if (!string.IsNullOrEmpty(searchField))
                //{
                //    var parser = new QueryParser(Version.LUCENE_30, searchField, analyzer);
                //    var query = parseQuery(searchQuery, parser);
                //    var hits = searcher.Search(query, hits_limit).ScoreDocs;
                //    var results = _mapLuceneToDataList(hits, searcher);
                //    analyzer.Close();
                //    searcher.Dispose();
                //    return results;
                //}
                //// search by multiple fields (ordered by RELEVANCE)
                //else
                //{

                //}
            }
        }
        private static Query parseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        // map Lucene search index to data
        private static IEnumerable<TransportGeoModel> _mapLuceneToDataList(IEnumerable<Document> hits)
        {
            return hits.Select(_mapLuceneDocumentToData).ToList();
        }
        private static IEnumerable<TransportGeoModel> _mapLuceneToDataList(IEnumerable<ScoreDoc> hits, IndexSearcher searcher)
        {
            // v 2.9.4: use 'hit.doc'
            // v 3.0.3: use 'hit.Doc'
            return hits.Select(hit => _mapLuceneDocumentToData(searcher.Doc(hit.Doc))).ToList();
        }
        private static TransportGeoModel _mapLuceneDocumentToData(Document doc)
        {
            return new TransportGeoModel
            {
                GioDate = doc.Get("GioDate"),
                GioTime = doc.Get("GioTime"),
                VehicleID = Convert.ToInt32(doc.Get("VehicleID")),
                Longitude =doc.Get("Longitude"),
                Latitude = doc.Get("Latitude"),
                Angle = doc.Get("Angle")
            };
        }

        // add/update/clear search index data 
        public static void AddUpdateLuceneIndex(TransportGeoModel sampleData)
        {
            AddUpdateLuceneIndex(new List<TransportGeoModel> { sampleData });
        }
        public static void AddUpdateLuceneIndex(IEnumerable<TransportGeoModel> sampleDatas)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add data to lucene search index (replaces older entries if any)
                foreach (var sampleData in sampleDatas) _addToLuceneIndex(sampleData, writer);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }
        public static void ClearLuceneIndexRecord(string record_UUID)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // remove older index entry
                var searchQuery = new TermQuery(new Term("UUID", record_UUID));
                writer.DeleteDocuments(searchQuery);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }
        public static bool ClearLuceneIndex()
        {
            try
            {
                var analyzer = new StandardAnalyzer(Version.LUCENE_30);
                using (var writer = new IndexWriter(_directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    // remove older index entries
                    writer.DeleteAll();

                    // close handles
                    analyzer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public static void Optimize()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }
        private static void _addToLuceneIndex(TransportGeoModel sampleData, IndexWriter writer)
        {
            // remove older index entry
            //var searchQuery = new TermQuery(new Term("VehicleID", sampleData.VehicleID));
            //writer.DeleteDocuments(searchQuery);

            // add new index entry
            var doc = new Document();

            // add lucene fields mapped to db fields
            doc.Add(new Field("VehicleID", sampleData.VehicleID.ToString(), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("GioDate", sampleData.GioDate, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("GioTime", sampleData.GioTime, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Longitude", sampleData.Longitude, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Latitude", sampleData.Latitude, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Angle", sampleData.Angle, Field.Store.YES, Field.Index.NOT_ANALYZED));



            //if (Search(sampleData.UUID, "UUID").ToList().Count > 0)
            //{
            //    writer.UpdateDocument(new Term("UUID", sampleData.UUID.ToString()), doc);
            //}
            //else
            //{
            // add entry to index
            writer.AddDocument(doc);
            //}
        }

    }
}