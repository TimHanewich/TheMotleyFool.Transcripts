# TheMotleyFool.Transcripts
A lightweight library for parsing and reading earnings call transcripts from The Motley Fool.  
This is not an official library and is not created or owned by The Motley Fool.  
Install with Nuget by running the following command 
```
dotnet add package TheMotleyFool.Transcripts
```

## To Use
Place the following `using` statement at the top of your code file.
```
using TheMotleyFool.Transcripts;
```

## Finding Latest Transcripts
You can get a list of the most recent transcripts by using the `TranscriptSearch` class.
```
TranscriptSource ts = new TranscriptSource();
TranscriptPreview[] transcript_previews = await ts.GetRecentTranscriptPreviewsNextPageAsync();
```
The `GetRecentTranscriptPreviewsNextPageAsync` method will return the next twenty latest transcripts. If you use the same method again, the following twenty (next page) will be returned.  
The `TranscriptPreview` class (an array will be returned) contains details like title, description, URL, and more that you can use to preview the transcript.

## Parsing Transcripts
You can parse an earnings call transcript by providing the URL to the static `CreateFromUrlAsync` method of the `Transcript` class. For exampe:
```
Transcript t = await Transcript.CreateFromUrlAsync("https://www.fool.com/earnings/call-transcripts/2021/01/27/microsoft-msft-q2-2021-earnings-call-transcript/");
```
Hint: Use the `URL` property from the `TranscriptPreview` class that was returned from the search above!
## Anatomy of the `Transcript` class
| Property | Description |
| --- | --- |
| `Title` | Title of the Transcript |
| `Description` | Transcript Description |
| `CallDateTimeStamp` | The date and time the earnings call was held |
| `Participants` | Array of `CallParticipant` class, containing each participant's name and position |
| `Remarks` | Array of `Remark` class |
The `Remarks` property will contain the call transcript (spoken remarks by the participants). Each `Remark` class references the speaker (participant who said it) and an array of strings, representing each paragraph or phrase.
