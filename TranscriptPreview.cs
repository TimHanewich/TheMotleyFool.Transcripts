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
            //Error checking
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
        
            HttpClient hc = new HttpClient();
            HttpResponseMessage hrm = await hc.GetAsync(Url);
            string web = await hrm.Content.ReadAsStringAsync();

            Transcript ToReturn = new Transcript();
            int loc1 = 0;
            int loc2 = 0;
            List<string> Splitter = new List<string>();
            int t = 0;
            
            //Get heading
            loc1 = web.IndexOf("adv-heading");
            loc1 = web.IndexOf("<h1", loc1 + 1);
            loc1 = web.IndexOf(">", loc1 + 1);
            loc2 = web.IndexOf("<", loc1 + 1);
            ToReturn.Title = web.Substring(loc1 + 1, loc2 - loc1 - 1);

            //Get description
            loc1 = web.IndexOf("adv-heading");
            loc1 = web.IndexOf("<h2", loc1 + 1);
            loc1 = web.IndexOf(">", loc1 + 1);
            loc2 = web.IndexOf("<", loc1 + 1);
            ToReturn.Description = web.Substring(loc1 + 1, loc2 - loc1 - 1);

            #region "Get call participants"

            List<CallParticipant> Participants_ = new List<CallParticipant>();

            //Get the call participant area
            loc1 = web.IndexOf("<h2>Call participants:</h2>");
            loc2 = web.IndexOf("<div id=", loc1 + 1);
            string callparticipantdata = web.Substring(loc1 + 1, loc2 - loc1 - 1);

            //Split it into lines, enumerate through and parse.
            Splitter.Clear();
            Splitter.Add("<strong>");
            string[] parts = callparticipantdata.Split(Splitter.ToArray(), StringSplitOptions.None);
            for (t=1;t<parts.Length;t++)
            {
                string thisparticipantdata = parts[t];
                CallParticipant this_participant = new CallParticipant();

                //Get name
                loc1 = thisparticipantdata.IndexOf("</strong>");
                this_participant.Name = thisparticipantdata.Substring(0, loc1);

                //Get title
                loc1 = thisparticipantdata.IndexOf("<em>");
                if (loc1 != -1)
                {
                    loc1 = thisparticipantdata.IndexOf(">", loc1 + 1);
                    loc2 = thisparticipantdata.IndexOf("</em>", loc1 + 1);
                    this_participant.Title = thisparticipantdata.Substring(loc1 + 1, loc2 - loc1 - 1);
                    this_participant.Title = this_participant.Title.Replace("&amp;", "&");
                }

                Participants_.Add(this_participant);
            }

            //Add the operator as a participant
            CallParticipant cp = new CallParticipant();
            cp.Name = "Operator";
            cp.Title = "Operator";
            Participants_.Add(cp);

            ToReturn.Participants = Participants_.ToArray();
            #endregion

            #region "Get Remarks"

            //Get remark content (paragraph content) and then split it up by line.
            loc1 = web.IndexOf("<h2>Contents:</h2>");
            loc2 = web.IndexOf("<h2>Call participants:</h2>", loc1 + 1);
            string remarkdata = web.Substring(loc1, loc2 - loc1);
            Splitter.Clear();
            Splitter.Add("<p>");
            string[] remark_parts = remarkdata.Split(Splitter.ToArray(), StringSplitOptions.None);

            //Enumerate through all of the pieces and turn it into a remark
            List<Remark> AllRemarks = new List<Remark>();
            Remark BufferRemark = new Remark();
            bool BufferReady = false;
            t = 0;
            for (t=1;t<remark_parts.Length;t++)
            {
                string this_line = remark_parts[t];
                if (this_line.Contains("<strong>"))
                {
                    
                    //If there is one in the buffer, add it to the list
                    if (BufferReady)
                    {
                        AllRemarks.Add(BufferRemark);
                    }
                    
                    BufferRemark = new Remark();
                    
                    //Get their name
                    loc1 = this_line.IndexOf("<strong>");
                    loc1 = this_line.IndexOf(">", loc1 + 1);
                    loc2 = this_line.IndexOf("<", loc1 + 1);
                    string speaker_name = this_line.Substring(loc1 + 1, loc2 - loc1 - 1);

                    //Find the call participant the corresponds to this name
                    foreach (CallParticipant call_part in ToReturn.Participants)
                    {
                        if (call_part.Name.ToLower().Trim() == speaker_name.ToLower().Trim())
                        {
                            BufferRemark.Speaker = call_part;
                        }
                    }

                    BufferReady = true;
                }
                else //This is a paragraph that the speaker said
                {
                    //Get the paragraph
                    loc1 = this_line.IndexOf("</p>");
                    string thisparagraph = this_line.Substring(0, loc1);

                    //Clean it
                    thisparagraph = thisparagraph.Replace("&amp;", "&");
                    thisparagraph = thisparagraph.Trim();

                    //Add it to te remark
                    List<string> strs;
                    if (BufferRemark.SpokenRemarks != null)
                    {
                        strs = BufferRemark.SpokenRemarks.ToList();
                    }
                    else
                    {
                        strs = new List<string>();
                    }
                    strs.Add(thisparagraph);
                    BufferRemark.SpokenRemarks = strs.ToArray();
                }


                ToReturn.Remarks = AllRemarks.ToArray();
            }

            #endregion

            return ToReturn;
        }

    }
}