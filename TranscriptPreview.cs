using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TheMotleyFool.Transcripts
{
    public class TranscriptPreview
    {
        public string Title {get; set;}
        public DateTime PostedDate {get; set;}
        public string Description {get; set;}
        public string Url {get; set;}
        

        public async Task<Transcript> GetTranscriptAsync()
        {
            if (Url == null)
            {
                throw new Exception("URL is null.");
            }
            else
            {
                if (Url == "")
                {
                    throw new Exception("URL is blank.");
                }
            }
            
            Transcript t = await Transcript.CreateFromUrlAsync(Url);
            return t;
        }

    }
}