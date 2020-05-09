using System;

namespace TheMotleyFool.Transcripts
{
    public class Article
    {
        public string Title {get; set;}
        public string Description {get; set;}
        public DateTime PublicationDateTime {get; set;}
        public CallParticipant[] Participants {get; set;}
        public Remark[] Remarks {get; set;}

    }
}