using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
class CD
{
    string PauseFrom = "00:00:00";
    string PauseTo = "00:00:00";
    [DllImport("winmm.dll", CharSet = CharSet.Auto)]
    private static extern int mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);
    StringBuilder Buffer = new StringBuilder(128);
    public void Init()
    {
        mciSendString("open cdaudio shareable", null, 0, IntPtr.Zero);
    }
    public void Play(string From, string To)
    {
        try
        {
            From = RetrieveTrackPositions()[System.Convert.ToInt16(From)-1];
        }
        catch
        {
            try
            {
                From = $"{System.Convert.ToInt16(From.Split(':')[0])}:{System.Convert.ToInt16(From.Split(':')[1])}:{System.Convert.ToInt16(From.Split(':')[2])}";
            }
            catch
            {
                From = RetrieveTrackPositions()[0];
            }
        }
        try
        {
            To = RetrieveTrackPositions()[System.Convert.ToInt16(To) - 1];
        }
        catch
        {
            try
            {
                To = $"{System.Convert.ToInt16(To.Split(':')[0])}:{System.Convert.ToInt16(To.Split(':')[1])}:{System.Convert.ToInt16(To.Split(':')[2])}";
            }
            catch
            {
                if (To == "end")
                {
                    To = RetrieveRunTime();
                }
                else
                {
                    To = RetrieveRunTime();
                }
            }
        }
        mciSendString($"play cdaudio from {From} to {To}", null, 0, IntPtr.Zero);
        PauseTo = To;
    }
    public void Pause()
    {
        mciSendString("status cdaudio position", Buffer, Buffer.Capacity, IntPtr.Zero);
        PauseFrom = Buffer.ToString();
        Stop();
    }
    public void Resume()
    {
        Play(PauseFrom, PauseTo);
    }
    public void Stop()
    {
        mciSendString("stop cdaudio", null, 0, IntPtr.Zero);
    }
    public void End()
    {
        Stop();
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
    public string[] RetrieveTrackPositions()
    {
        List<string> TrackPositions = new List<string>();
        for (int i = 1; i <= RetrieveTrackLengths().Length; i++)
        {
            Buffer.Clear();
            mciSendString($"status cdaudio track {i} position", Buffer, Buffer.Capacity, IntPtr.Zero);
            TrackPositions.Add(Buffer.ToString());
        }
        return (TrackPositions.ToArray());
    }
    public string RetrieveRunTime()
    {
        string LastTrackPosition = RetrieveTrackPositions()[RetrieveTrackPositions().Length-1];
        string LastTrackDuration = RetrieveTrackLengths()[RetrieveTrackLengths().Length-1];
        int MinutePos = System.Convert.ToInt16(LastTrackPosition.Split(":")[0]);
        int SecondPos = System.Convert.ToInt16(LastTrackPosition.Split(":")[1]);
        int FramePos = System.Convert.ToInt16(LastTrackPosition.Split(":")[2]);
        int MinuteDur = System.Convert.ToInt16(LastTrackDuration.Split(":")[0]);
        int SecondDur = System.Convert.ToInt16(LastTrackDuration.Split(":")[1]);
        int FrameDur = System.Convert.ToInt16(LastTrackDuration.Split(":")[2]);
        int MinuteFin = MinutePos + MinuteDur;
        int SecondFin = SecondPos + SecondDur;
        int FrameFin = FramePos + FrameDur;
        if (MinuteFin > 80)
        {
            Console.WriteLine("CD is overburned!");
        }
        if (FrameFin > 74)
        {
            FrameFin -= 75;
            SecondFin += 1;
        }
        if (SecondFin > 59)
        {
            SecondFin -= 60;
            MinuteFin += 1;
        }
        return ($"{MinuteFin}:{SecondFin}:{FrameFin}");
    }
    public void ListDiscProp()
    {
        string[] CDTracks = RetrieveTrackLengths();
        string[] CDTrackPositions = RetrieveTrackPositions();
        string CDRunTime = RetrieveRunTime();
        for (int i = 0; i < CDTracks.Length; i++)
        {
            Console.WriteLine($"Track {i + 1}: {CDTracks[i]} ({CDTrackPositions[i]})");
        }
        Console.WriteLine($"Runtime: {CDRunTime}");
    }
    public void CheckForDisc()
    {
        while (true)
        {
            try
            {
                RetrieveRunTime();
                break;
            }
            catch
            {
                Console.WriteLine("No disc was detected, press return to check again.");
            }
            Console.ReadLine();
        }
    }
    public string[] GetFuncFromInp(string i, string[] Commands)
    {
        if (i == "play" | i == "ply")
        {
            string[] Return = { "play", "yes" };
            return (Return);
        }
        else if (i == "discinfo" | i == "dscinf")
        {
            string[] Return = { "dscinf", "no" };
            return (Return);
        }
        else if (i == "quit" | i == "qt")
        {
            string[] Return = { "quit", "no" };
            return (Return);
        }
        else if (i == "stop" | i == "stp")
        {
            string[] Return = { "stop", "no" };
            return (Return);
        }
        else if (i == "help" | i == "hlp")
        {
            string[] Return = { "help", "no" };
            return (Return);
        }
        else if (i == "pause" | i == "ps")
        {
            string[] Return = { "pause", "no" };
            return (Return);
        }
        else if (i == "resume" | i == "rsm")
        {
            string[] Return = { "resume", "no" };
            return (Return);
        }
        else
        {
            string[] Return = { "none", "no" };
            return (Return);
        }
    }
    public void Help()
    {
        string Help =
            "Separate all arguments with '-'. Invalid commands and arguments will be ignored.\n" +
            "Time format is MM:SS:FF\n\n" +
            "play/ply: Plays the disc. You can specify tracks or times to start and end at.\n" +
            "stop/stp: Stops playback, can not resume after stopping.\n" +
            "pause/ps: Pauses playback, saves the current timestamp to be resumed from later.\n" +
            "resume/rsm: Resumes playback from the timestamp saved by the pause command.\n\n" +
            "*fastforward/ff: Fast forwards a specified amount of seconds.\n" +
            "*rewind/rw: Rewinds a specified amount of seconds.\n\n" +
            "*next/nxt: Skips to the next track.\n" +
            "*previous/prv: Seeks to the start of the previous track.\n\n" +
            "*seekdisc/skdsc: Seeks to a specified time on the disk\n" +
            "*seek/sk: Seeks to a specified time within the currently playing track.\n" +
            "*seektrack/sktrk: Seeks to a specified track.\n\n" +
            "*eject/ejct: Ejects the disc if your drive supports it.\n" +
            "discinfo/dscinf: Displays info about the disc.\n" +
            "*playerinfo/plyinf: Displays current info about the player.\n" +
            "help/hlp: Prints this message.\n" +
            "quit/qt: Exits CSharp Disc Player.\n" +
            "*Feature not yet implemented.";
        Console.WriteLine(Help);
    }
}
class Program
{
    static void Main()
    {
        string[] Commands = {"play","ply","discinfo","dscinf","quit","qt","stop","stp","help","hlp","pause","ps","resume","rsm"};
        CD CD = new CD();
        CD.Init();
        CD.CheckForDisc();
        CD.Help();
        while (true)
        {
            CD.CheckForDisc();
            string UsrInp = Console.ReadLine();
            foreach (string i in Commands)
            {
                string FuncType = "none";
                if (UsrInp.Split('-')[0].Trim() == i)
                {
                    FuncType = CD.GetFuncFromInp(UsrInp.Split("-")[0].Trim(), Commands)[0];
                    string HasArgs = CD.GetFuncFromInp(UsrInp.Split("-")[0].Trim(), Commands)[1];
                    string Arg1;
                    string Arg2;
                    if (HasArgs == "yes")
                    {
                        try
                        {
                            Arg1 = UsrInp.Split('-')[1].Trim();
                        }
                        catch
                        {
                            if (FuncType == "play")
                            {
                                Arg1 = CD.RetrieveTrackPositions()[0];
                            }
                            else
                            {
                                Arg1 = "0";
                            }
                        }
                        try
                        {
                            Arg2 = UsrInp.Split('-')[2].Trim();
                        }
                        catch
                        {
                            Arg2 = "end";
                        }
                    }
                    else
                    {
                        Arg1 = "please stop whining compiler";
                        Arg2 = "pretty please!";
                    }
                    if (FuncType == "play")
                    {
                        CD.Play(Arg1, Arg2);
                    }
                    if (FuncType == "dscinf")
                    {
                        CD.ListDiscProp();
                    }
                    if (FuncType == "stop")
                    {
                        CD.Stop();
                    }
                    if (FuncType == "help")
                    {
                        CD.Help();
                    }
                    if (FuncType == "pause")
                    {
                        CD.Pause();
                    }
                    if (FuncType == "resume")
                    {
                        CD.Resume();
                    }
                    if (FuncType == "quit")
                    {
                        CD.End();
                        Environment.Exit(1);
                    }
                }
            }
        }
    }
}
