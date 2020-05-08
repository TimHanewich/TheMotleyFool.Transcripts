using System;
using System.Net.Http;
using System.Threading.Tasks;

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

        public async Task<RecentArticle[]> GetRecentArticlesNextPage()
        {
            string search_url = "https://www.fool.com/earnings-call-transcripts/?page=" + search_page.ToString();
            search_page++;

            HttpResponseMessage hrm = await hc.GetAsync(search_url);
            string web = await hrm.Content.ReadAsStringAsync();

        }
    }
}