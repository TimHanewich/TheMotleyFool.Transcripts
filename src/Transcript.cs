using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;

namespace TheMotleyFool.Transcripts
{
    public class Transcript
    {
        public string Title {get; set;}
        public string Description {get; set;}
        public DateTime CallDateTimeStamp {get; set;}
        public CallParticipant[] Participants {get; set;}
        public Remark[] Remarks {get; set;}

        public string TradingSymbol {get; set;}

        public static async Task<Transcript> CreateFromUrlAsync(string url)
        {
        
            HttpClient hc = new HttpClient();
            HttpResponseMessage hrm = await hc.GetAsync(url);
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

            //Get Posted Date/Time
            loc1 = web.IndexOf("<span id=\"date\">");
            loc1 = web.IndexOf(">", loc1 + 1);
            loc2 = web.IndexOf("<", loc1 + 1);
            if (loc2 > loc1)
            {
                string cdtstr = web.Substring(loc1 + 1, loc2 - loc1 - 1);
                if (cdtstr != "")
                {
                    try
                    {
                        ToReturn.CallDateTimeStamp = DateTime.Parse(web.Substring(loc1 + 1, loc2 - loc1 - 1));
                    }
                    catch
                    {

                    }
                }
                else
                {
                    //if we were unable to get the date time from above, try another way.
                    //For example, for this one: https://www.fool.com/earnings/call-transcripts/2020/03/05/bruker-brkr-q4-2019-earnings-call-transcript.aspx
            
                    loc1 = web.IndexOf("<span id=\"date\">");
                    loc1 = web.IndexOf(">", loc1 + 1);
                    loc2 = web.IndexOf("<", loc1 + 1); //close span
                    loc1 = web.IndexOf(">", loc2 + 1); //end of the close span tag
                    loc2 = web.IndexOf("<", loc1 + 1);
                    if (loc2 > loc1)
                    {
                        string tts = web.Substring(loc1 + 1, loc2 - loc1 - 1);
                        try
                        {
                            ToReturn.CallDateTimeStamp = DateTime.Parse(tts);
                        }
                        catch
                        {

                        }
                    }
                }
            }

            

            #region "Get call participants"

            List<CallParticipant> Participants_ = new List<CallParticipant>();

            //Get the call participant area
            loc1 = web.IndexOf(">Call participants:</");
            loc2 = web.IndexOf("<div id=", loc1 + 1);
            string callparticipantdata = web.Substring(loc1 + 1, loc2 - loc1 - 1);

            //Split it into lines, enumerate through and parse.
            Splitter.Clear();
            Splitter.Add("<p>");
            string[] parts = callparticipantdata.Split(Splitter.ToArray(), StringSplitOptions.None);
            for (t=1;t<parts.Length;t++)
            {
                string thisparticipantdata = parts[t];
                loc1 = thisparticipantdata.IndexOf("</p");
                if (loc1 != -1)
                {    
                    string dataonly = thisparticipantdata.Substring(0, loc1);
                    if (dataonly.Contains("<strong>"))
                    {
                        CallParticipant this_participant = new CallParticipant();

                        //Get name
                        loc1 = thisparticipantdata.IndexOf(">");
                        loc2 = thisparticipantdata.IndexOf("<", loc1 + 1);
                        this_participant.Name = thisparticipantdata.Substring(loc1 + 1, loc2 - loc1 - 1).Trim();

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
                    
                }
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
            loc2 = web.IndexOf(">Call participants:</", loc1 + 1);
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
                    
                    //If there is one in the buffer, add it to the list. But only add it to the list if there is content in it!
                    if (BufferReady)
                    {
                        if (BufferRemark.SpokenRemarks != null) //It has to have spoken remarks
                        {
                            if (BufferRemark.SpokenRemarks.Length > 0) //The number of spoken remarks must be > 0
                            {
                                if (BufferRemark.Speaker != null) //The speaker data must be there
                                {
                                    AllRemarks.Add(BufferRemark);
                                }
                            }
                        }
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
                    if (loc1 > 0)
                    {
                        string thisparagraph = this_line.Substring(0, loc1);

                        //Clean it
                        thisparagraph = thisparagraph.Replace("&amp;", "&");
                        thisparagraph = thisparagraph.Trim();

                        //Create the list of remarks so far that we will append to if this is worthy
                        List<string> strs;
                        if (BufferRemark.SpokenRemarks != null)
                        {
                            strs = BufferRemark.SpokenRemarks.ToList();
                        }
                        else
                        {
                            strs = new List<string>();
                        }

                        //Add it if it meets criteria (i.e. not blank)
                        if (thisparagraph != "")
                        {
                            strs.Add(thisparagraph);
                        }
                        
                        //Add all of the spoken remarks (stirng) to the remark class
                        BufferRemark.SpokenRemarks = strs.ToArray();
                    }
                }

                ToReturn.Remarks = AllRemarks.ToArray();
            }

            #endregion

            #region "Try and extract the ticker symbol"
  
            //Main method that works with most earnings call transcripts
            loc1 = web.IndexOf("<span class=\"ticker\"");
            if (loc1 != -1)
            {
                loc1 = web.IndexOf("a href", loc1 + 1);
                if (loc1 != -1)
                {
                    loc1 = web.IndexOf(">", loc1 + 1);
                    loc2 = web.IndexOf("<", loc1 + 1);
                    if (loc2 > loc1)
                    {
                        string symbolpart = web.Substring(loc1 + 1, loc2 - loc1 - 1); //for example, NASDAQ:HQY
                        loc1 = symbolpart.IndexOf(":");
                        if (loc1 != -1)
                        {
                            string symbol = symbolpart.Substring(loc1 + 1);
                            ToReturn.TradingSymbol = symbol;
                        }   
                    }
                }
            }

            //If it didnt find it above, try and find it the alternative way
            //For example: https://www.fool.com/earnings/call-transcripts/2021/03/03/profound-medical-corp-q4-2020-earnings-call-transc/
            if (ToReturn.TradingSymbol == null)
            {
                loc1 = web.IndexOf("WuDkNe");
                if (loc1 != -1)
                {
                    loc1 = web.IndexOf(">", loc1 + 1);
                    loc2 = web.IndexOf("<", loc1 + 1);
                    if (loc2 > loc1)
                    {
                        string symbol = web.Substring(loc1 + 1, loc2 - loc1 - 1);
                        ToReturn.TradingSymbol = symbol;
                    }
                }
            }
            
            //Finally, if the above two did not work, try to extract it from the title
            //Extract the trading symbol and company name from the title
            if (ToReturn.TradingSymbol == null)
            {
                loc2 = ToReturn.Title.LastIndexOf(")");
                loc1 = ToReturn.Title.LastIndexOf("(", loc2);
                if (loc2 > loc1)
                {
                    ToReturn.TradingSymbol = web.Substring(loc1 + 1, loc2 - loc1 - 1).Trim().ToUpper();
                }
            }
            
            #endregion

            return ToReturn;
        }

    }
}