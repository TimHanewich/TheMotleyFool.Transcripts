using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TheMotleyFool.Transcripts
{
    public class TranscriptSource
    {
        private int search_page {get; set;}
        private HttpClient hc {get; set;}

        public TranscriptSource()
        {
            search_page = 1;
            hc = new HttpClient();
        }

        public async Task<TranscriptPreview[]> GetRecentArticlesNextPageAsync()
        {
            string search_url = "https://www.fool.com/earnings-call-transcripts/?page=" + search_page.ToString();
            search_page++;

            HttpResponseMessage hrm = await hc.GetAsync(search_url);
            string web = await hrm.Content.ReadAsStringAsync();

            int loc1 = 0;
            int loc2 = 0;
            List<string> Splitter = new List<string>();

            loc1 = web.IndexOf("wagtail-aggregator-list-content");
            loc2 = web.IndexOf("load-more", loc1 + 1);
            string article_data = web.Substring(loc1, loc2 - loc1);
            
            //Split to articles
            Splitter.Clear();
            Splitter.Add("<a href");
            string[] parts = article_data.Split(Splitter.ToArray(), StringSplitOptions.None);

            //parse the articles
            List<TranscriptPreview> recentArticles = new List<TranscriptPreview>();
            int t = 0;
            for (t=1;t<parts.Length;t++)
            {
                string thisData = parts[t];
                TranscriptPreview ra = new TranscriptPreview();

                //Get link
                loc1 = thisData.IndexOf("href");
                loc1 = thisData.IndexOf("\"", loc1 + 1);
                loc2 = thisData.IndexOf("\"", loc1 + 1);
                ra.Url = thisData.Substring(loc1 + 1, loc2 - loc1 - 1);
                ra.Url = "https://www.fool.com" + ra.Url;

                //Get title
                loc1 = thisData.IndexOf("title=");
                loc1 = thisData.IndexOf("\"", loc1 + 1);
                loc2 = thisData.IndexOf("\"", loc1 + 1);
                ra.Title = thisData.Substring(loc1 + 1, loc2 - loc1 - 1);

                //Get date
                loc1 = thisData.IndexOf("story-date-author");
                loc1 = thisData.IndexOf(">", loc1 + 1);
                loc2 = thisData.IndexOf("<", loc1 + 1);
                string sta = thisData.Substring(loc1 + 1, loc2 - loc1 - 1);
                loc1 = sta.IndexOf("|");
                string dte = sta.Substring(loc1 + 1);
                dte = dte.Trim();
                ra.PostedDate = DateTime.Parse(dte);

                //Get description ("promo");
                loc1 = thisData.IndexOf("article-promo");
                loc1 = thisData.IndexOf(">", loc1 + 1);
                loc2 = thisData.IndexOf("<", loc1 + 1);
                ra.Description = thisData.Substring(loc1 + 1, loc2 - loc1 - 1);

                recentArticles.Add(ra);
            }


            return recentArticles.ToArray();
        }
    }
}