using System;
using System.Runtime.InteropServices;
using System.Text;
class CD
{
    [DllImport("winmm.dll", CharSet = CharSet.Auto)]
    private static extern int mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);
    StringBuilder Buffer = new StringBuilder(128);
    public void Init()
    {
        mciSendString("open cdaudio shareable", null, 0, IntPtr.Zero);
    }
    public void Play(string From, string To)
    {
        mciSendString($"play cdaudio from {From} to {To}", null, 0, IntPtr.Zero);
    }
    public void Pause()
    {
        mciSendString("pause cdaudio", null, 0, IntPtr.Zero);
    }
    public void Resume()
    {
        mciSendString("resume cdaudio", null, 0, IntPtr.Zero);
    }
    public void Stop()
    {
        mciSendString("stop cdaudio", null, 0, IntPtr.Zero);
    }
    public void End()
    {
        mciSendString("close cdaudio", null, 0, IntPtr.Zero);
    }
    public string[] RetrieveTrackLengths()
    {
        mciSendString("status cdaudio number of tracks", Buffer, Buffer.Capacity, IntPtr.Zero);
        int TrackCount = int.Parse(Buffer.ToString());
        List<string> TrackDuration = new List<string>();
        for (int i = 1; i <= TrackCount; i++)
        {
            Buffer.Clear();
            mciSendString($"status cdaudio length track {i}", Buffer, Buffer.Capacity, IntPtr.Zero);
            TrackDuration.Add(Buffer.ToString());
        }
        return (TrackDuration.ToArray());
    }
    public string[] RetrieveTrackPositions(string[] TrackDuration)
    {
        List<string> TrackPositions = new List<string>();
        int Minute = 00;
        int Second = 00;
        int Frame = 00;
        int Indx = 0;
        foreach (string i in TrackDuration)
        {
            Minute += System.Convert.ToInt16(i.Split(":")[0]);
            Second += System.Convert.ToInt16(i.Split(":")[1]);
            Frame += System.Convert.ToInt16(i.Split(":")[2]);
            if (Minute > 80)
            {
                Console.WriteLine("CD is overburned!");
            }
            if (Second > 59)
            {
                Second -= 60;
                Minute += 1;
            }
            if (Frame > 74)
            {
                Frame -= 75;
                Second += 1;
            }
            if (Indx == 0)
            {
                TrackPositions.Add("00:02:00");
            }
            string MinuteS = Minute.ToString();
            string SecondS = Second.ToString();
            string FrameS = Second.ToString();
            if (Minute.ToString().Length == 1)
            {
                MinuteS = $"0{Minute.ToString()}";
            }
            if (Second.ToString().Length == 1)
            {
                SecondS = $"0{Second.ToString()}";
            }
            if (Frame.ToString().Length == 1)
            {
                FrameS = $"0{Frame.ToString()}";
            }
            string Position = $"{MinuteS}:{SecondS}:{FrameS}";
            TrackPositions.Add(Position);
            Indx += 1;
        }
        return (TrackPositions.ToArray());
    }
    public string RetrieveRunTime()
    {
        string[] TrackDurations = RetrieveTrackLengths();
        string LastTrackDuration = TrackDurations[TrackDurations.Length-1];
        string RunTime = RetrieveTrackPositions(TrackDurations)[RetrieveTrackPositions(TrackDurations).Length-1];
        return (RunTime);
    }
}
class Program
{
    static string GetUsrInp(string[] Checkfor, string PrintText)
    {
        while (true)
        {
            Console.WriteLine(PrintText);
            string UsrInp = Console.ReadLine();
            try
            {
                foreach (string i in Checkfor)
                {
                    if (UsrInp == i)
                    {
                        return (UsrInp);
                    }
                }
                Console.WriteLine("Invalid action.\n");
            }
            catch
            {
                Console.WriteLine("Invalid action.\n");
            }
        }
    }
    static void Main()
    {
        CD CD = new CD();
        CD.Init();
        while (true)
        {
            try
            {
                CD.RetrieveRunTime();
                break;
            }
            catch
            {
                Console.WriteLine("No disc was detected, press return to check again.");
            }
            Console.ReadLine();
        }
        string[] CDTracks = CD.RetrieveTrackLengths();
        string[] CDTrackPositions = CD.RetrieveTrackPositions(CDTracks);
        string CDRunTime = CD.RetrieveRunTime();
        List<string> CheckForTrack = new List<string>();
        for (int i = 0; i < CDTracks.Length; i++)
        {
            Console.WriteLine($"Track {i+1}: {CDTracks[i]}");
            CheckForTrack.Add((i+1).ToString());
        }
        Console.WriteLine($"Runtime: {CDRunTime}");
        string[] CheckFor = {"play","stop","quit"};
        string Print = "Play, Stop, Quit?";
        while(true)
        {
            string UsrInp = GetUsrInp(CheckFor, Print);
            if (UsrInp == CheckFor[0])
            {
                string UsrInpFrom = GetUsrInp(CheckForTrack.ToArray(), "Start playing at which track?");
                CheckForTrack.Add("end");
                string UsrInpTo = GetUsrInp(CheckForTrack.ToArray(), "Stop playing at which track? (Enter end to play until the player reaches the end of disc)");
                if (UsrInpTo == "end")
                {
                    CD.Play(CDTrackPositions[System.Convert.ToInt16(UsrInpFrom) - 1], CDRunTime);
                }
                else
                {
                    CD.Play(CDTrackPositions[System.Convert.ToInt16(UsrInpFrom) - 1], CDTrackPositions[System.Convert.ToInt16(UsrInpTo) - 1]);
                }
                CheckForTrack.Remove("end");
            }
            else if (UsrInp == CheckFor[1])
            {
                CD.Stop();
            }
            else if (UsrInp == CheckFor[2])
            {
                CD.Stop();
                CD.End();
                break;
            }
        }
    }
}