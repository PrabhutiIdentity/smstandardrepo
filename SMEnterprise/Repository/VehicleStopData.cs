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
    public static class VehicleStopData
    {
        public static string _luceneDir
        {
            get
            {
                if (!System.IO.Directory.Exists(HttpRuntime.AppDomainAppPath + CommonUsage.StopIndexBasePath))
                {
                    System.IO.Directory.CreateDirectory(HttpRuntime.AppDomainAppPath + CommonUsage.StopIndexBasePath);
                    
                }
                return HttpRuntime.AppDomainAppPath + CommonUsage.StopIndexBasePath;
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
        public static IEnumerable<TransportStopUpdateModel> GetAllIndexRecords()
        {
            // validate search index
            if (!System.IO.Directory.EnumerateFiles(_luceneDir).Any()) return new List<TransportStopUpdateModel>();

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
        public static IEnumerable<TransportStopUpdateModel> Search(string input, string fieldName = "")
        {
            if (string.IsNullOrEmpty(input)) return new List<TransportStopUpdateModel>();

            var terms = input.Trim().Replace("-", " ").Split(' ')
                .Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim());
            input = string.Join(" ", terms);

            return _search(input, fieldName);
        }
        public static IEnumerable<TransportStopUpdateModel> SearchDefault(string input, string GioDate = "",string VehicleID="")
        {
            return  _search(input, GioDate,VehicleID);//string.IsNullOrEmpty(input) ? new List<TransportStopUpdateModel>() :
        }

        // main search method
        private static IEnumerable<TransportStopUpdateModel> _search(string searchQuery, string UDate = "",string VehicleRouteID="")
        {
            // validation
            int PageIndex = 0;
            int PageSize = 50;
           // if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", ""))) return new List<TransportStopUpdateModel>();

            // set up lucene searcher
            using (var searcher = new IndexSearcher(_directory, false))
            {
                var hits_limit = 1000;
                var analyzer = new StandardAnalyzer(Version.LUCENE_30);


                BooleanQuery bq = new BooleanQuery();
                Query qry = new QueryParser(Version.LUCENE_CURRENT, "VehicleRouteID", analyzer).Parse(VehicleRouteID);
                Query qry2 = new QueryParser(Version.LUCENE_CURRENT, "UDateOnly", analyzer).Parse(UDate);

                bq.Add(qry, Occur.MUST);
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
        private static IEnumerable<TransportStopUpdateModel> _mapLuceneToDataList(IEnumerable<Document> hits)
        {
            return hits.Select(_mapLuceneDocumentToData).ToList();
        }
        private static IEnumerable<TransportStopUpdateModel> _mapLuceneToDataList(IEnumerable<ScoreDoc> hits, IndexSearcher searcher)
        {
            // v 2.9.4: use 'hit.doc'
            // v 3.0.3: use 'hit.Doc'
            return hits.Select(hit => _mapLuceneDocumentToData(searcher.Doc(hit.Doc))).ToList();
        }
        private static TransportStopUpdateModel _mapLuceneDocumentToData(Document doc)
        {
            return new TransportStopUpdateModel
            {
                UDate = Convert.ToDateTime(doc.Get("UDate")),
                StopID = Convert.ToInt32(doc.Get("StopID")),
                VehicleRouteID = Convert.ToInt32(doc.Get("VehicleRouteID"))
            };
        }

        // add/update/clear search index data 
        public static void AddUpdateLuceneIndex(TransportStopUpdateModel sampleData)
        {
            AddUpdateLuceneIndex(new List<TransportStopUpdateModel> { sampleData });
        }
        public static void AddUpdateLuceneIndex(IEnumerable<TransportStopUpdateModel> sampleDatas)
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
        private static void _addToLuceneIndex(TransportStopUpdateModel sampleData, IndexWriter writer)
        {
            // remove older index entry
            //var searchQuery = new TermQuery(new Term("VehicleID", sampleData.VehicleID));
            //writer.DeleteDocuments(searchQuery);

            // add new index entry
            var doc = new Document();

            // add lucene fields mapped to db fields
            doc.Add(new Field("VehicleRouteID", sampleData.VehicleRouteID.ToString(), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("UDate", sampleData.UDate.ToString("yyyy-MM-dd HH:mm:ss"), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("UDateOnly", sampleData.UDate.ToString("yyyy-MM-dd"), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("StopID", sampleData.StopID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
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