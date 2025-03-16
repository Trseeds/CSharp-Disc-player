using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TagLib.Matroska;
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
    public void CheckForDisc()
    {
        while (true)
        {
            //Tray("close");
            try
            {
                GetRunTime();
                break;
            }
            catch
            {
                Console.WriteLine("No disc was detected, press return to check again.");
            }
            Console.ReadLine();
        }
    }
    public void Tray(string Type)
    {
        if (Type == "open")
        {
            mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
        }
        if (Type == "close")
        {
            mciSendString("set cdaudio door closed", null, 0, IntPtr.Zero);
        }
    }
    public void End()
    {
        Stop();
        mciSendString("close cdaudio", null, 0, IntPtr.Zero);
    }
    public void Play(string From, string To)
    {
        try
        {
            From = GetTrackPositions()[System.Convert.ToInt16(From) - 1];
        }
        catch
        {
            try
            {
                From = $"{System.Convert.ToInt16(From.Split(':')[0]):00}:{System.Convert.ToInt16(From.Split(':')[1]):00}:{System.Convert.ToInt16(From.Split(':')[2]):00}";
            }
            catch
            {
                From = GetTrackPositions()[0];
            }
        }
        try
        {
            To = GetTrackPositions()[System.Convert.ToInt16(To) - 1];
        }
        catch
        {
            try
            {
                To = $"{System.Convert.ToInt16(To.Split(':')[0]):00}:{System.Convert.ToInt16(To.Split(':')[1]):00}:{System.Convert.ToInt16(To.Split(':')[2]):00}";
            }
            catch
            {
                if (To == "end")
                {
                    To = GetRunTime();
                }
                else
                {
                    To = GetRunTime();
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
    public void Seek(string UsrInp)
    {
        try
        {
            string From = GetTrackPositions()[System.Convert.ToInt16(UsrInp) - 1];
            Play(From, PauseTo);
        }
        catch
        {
            try
            {
                string From = $"{System.Convert.ToInt16(UsrInp.Split(':')[0]):00}:{System.Convert.ToInt16(UsrInp.Split(':')[1]):00}:{System.Convert.ToInt16(UsrInp.Split(':')[2]):00}";
                Play(From, PauseTo);
            }
            catch
            {
                ;
            }
        }
    }
    public void SeekTrack(string UsrInp)
    {
        try
        {
            string From = $"{System.Convert.ToInt16(UsrInp.Split(':')[0]):00}:{System.Convert.ToInt16(UsrInp.Split(':')[1]):00}:{System.Convert.ToInt16(UsrInp.Split(':')[2]):00}";
        }
        catch
        {
            ;
        }
        int Track = System.Convert.ToInt16(GetPlayHeadPosition()[1]);
        int MinuteTrack = System.Convert.ToInt16(GetTrackPositions()[Track - 1].Split(":")[0]);
        int SecondTrack = System.Convert.ToInt16(GetTrackPositions()[Track - 1].Split(":")[1]) + MinuteTrack * 60;
        int FrameTrack = System.Convert.ToInt16(GetTrackPositions()[Track - 1].Split(":")[2]) + SecondTrack * 75;
        int MinuteFrom = System.Convert.ToInt16(UsrInp.Split(":")[0]);
        int SecondFrom = System.Convert.ToInt16(UsrInp.Split(":")[1]) + MinuteFrom * 60;
        int FrameFrom = System.Convert.ToInt16(UsrInp.Split(":")[2]) + SecondFrom * 75;
        int MinuteFin = 0;
        int SecondFin = 0;
        int FrameFin = 0;
        FrameFin = FrameFrom + FrameTrack;
        while (FrameFin > 74)
        {
            FrameFin -= 75;
            SecondFin++;
        }
        while (SecondFin > 59)
        {
            SecondFin -= 60;
            MinuteFin++;
        }
        Console.WriteLine($"{MinuteFin:00}:{SecondFin:00}:{FrameFin:00}");
        Seek($"{MinuteFin:00}:{SecondFin:00}:{FrameFin:00}");
    }
    public void DragPlayHead(string UsrInp)
    {
        int MinuteFrom = System.Convert.ToInt16(GetPlayHeadPosition()[0].Split(":")[0]);
        int SecondFrom = System.Convert.ToInt16(GetPlayHeadPosition()[0].Split(":")[1]);
        int FrameFrom = System.Convert.ToInt16(GetPlayHeadPosition()[0].Split(":")[2]);
        int SecondTo = System.Convert.ToInt16(UsrInp);
        SecondTo += SecondFrom;
        while (SecondTo > 59)
        {
            SecondTo -= 60;
            MinuteFrom += 1;
        }
        while (SecondTo < 1)
        {
            SecondTo += 60;
            MinuteFrom -= 1;
        }
        Seek($"{MinuteFrom:00}:{SecondTo:00}:{FrameFrom:00}");
    }
    public string[] GetTrackLengths()
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
    public string[] GetTrackPositions()
    {
        List<string> TrackPositions = new List<string>();
        for (int i = 1; i <= GetTrackLengths().Length; i++)
        {
            Buffer.Clear();
            mciSendString($"status cdaudio track {i} position", Buffer, Buffer.Capacity, IntPtr.Zero);
            TrackPositions.Add(Buffer.ToString());
        }
        return (TrackPositions.ToArray());
    }
    public string GetRunTime()
    {
        string LastTrackPosition = GetTrackPositions()[GetTrackPositions().Length - 1];
        string LastTrackDuration = GetTrackLengths()[GetTrackPositions().Length - 1];
        int MinutePos = System.Convert.ToInt16(LastTrackPosition.Split(":")[0]);
        int SecondPos = System.Convert.ToInt16(LastTrackPosition.Split(":")[1]);
        int FramePos = System.Convert.ToInt16(LastTrackPosition.Split(":")[2]);
        int MinuteDur = System.Convert.ToInt16(LastTrackDuration.Split(":")[0]);
        int SecondDur = System.Convert.ToInt16(LastTrackDuration.Split(":")[1]);
        int FrameDur = System.Convert.ToInt16(LastTrackDuration.Split(":")[2]);
        int MinuteFin = MinutePos + MinuteDur;
        int SecondFin = SecondPos + SecondDur;
        int FrameFin = FramePos + FrameDur;
        if (MinuteFin > 79)
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
        return ($"{MinuteFin:00}:{SecondFin:00}:{FrameFin:00}");
    }
    public string[] GetPlayHeadPosition()
    {
        List<string> Return = new List<string>();
        mciSendString("status cdaudio position", Buffer, Buffer.Capacity, IntPtr.Zero);
        Return.Add(Buffer.ToString());
        int Minute = System.Convert.ToInt16(Return[0].Split(":")[0]);
        int Second = System.Convert.ToInt16(Return[0].Split(":")[1]) + Minute * 60;
        int Frame = System.Convert.ToInt16(Return[0].Split(":")[2]) + Second * 75;
        mciSendString("status cdaudio current track", Buffer, Buffer.Capacity, IntPtr.Zero);
        int Track = System.Convert.ToInt16(Buffer.ToString());
        Return.Add(Track.ToString());
        int MinuteTrack = System.Convert.ToInt16(GetTrackPositions()[Track-1].Split(":")[0]);
        int SecondTrack = System.Convert.ToInt16(GetTrackPositions()[Track - 1].Split(":")[1]) + MinuteTrack * 60;
        int FrameTrack = System.Convert.ToInt16(GetTrackPositions()[Track - 1].Split(":")[2]) + SecondTrack * 75;
        int MinuteFin = 0;
        int SecondFin = 0;
        int FrameFin = Frame - FrameTrack;
        while (FrameFin > 74)
        {
            FrameFin -= 75;
            SecondFin++;
        }
        while (SecondFin > 59)
        {
            SecondFin -= 60;
            MinuteFin++;
        }
        Return.Add($"{MinuteFin:00}:{SecondFin:00}:{FrameFin:00}");
        return (Return.ToArray());
    }
    public void ListDiscProp()
    {
        string[] CDTracks = GetTrackLengths();
        string[] CDTrackPositions = GetTrackPositions();
        string CDRunTime = GetRunTime();
        for (int i = 0; i < CDTracks.Length; i++)
        {
            Console.WriteLine($"Track {i + 1}: {CDTracks[i]} ({CDTrackPositions[i]})");
        }
        Console.WriteLine($"Runtime: {CDRunTime}");
    }
    public void ListPlayHeadProp()
    {
        Console.WriteLine($"Track: {GetPlayHeadPosition()[1]}\nTime on track: {GetPlayHeadPosition()[2]}\nTime on disc: {GetPlayHeadPosition()[0]}");
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
        else if (i == "seekdisc" | i == "skdsc")
        {
            string[] Return = { "seekdisc", "yes" };
            return (Return);
        }
        else if (i == "seek" | i == "sk")
        {
            string[] Return = { "seek", "yes" };
            return (Return);
        }
        else if (i == "next" | i == "nxt")
        {
            string[] Return = { "next", "no" };
            return (Return);
        }
        else if (i == "previous" | i == "prv")
        {
            string[] Return = { "previous", "no" };
            return (Return);
        }
        else if (i == "fastforward" | i == "ff")
        {
            string[] Return = { "ff", "yes" };
            return (Return);
        }
        else if (i == "rewind" | i == "rw")
        {
            string[] Return = { "rw", "yes" };
            return (Return);
        }
        else if (i == "info" | i == "inf")
        {
            string[] Return = { "plyhdprp", "no" };
            return (Return);
        }
        else if (i == "eject" | i == "ejct")
        {
            string[] Return = { "open", "no" };
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
            "fastforward/ff: Fast forwards a specified amount of seconds.\n" +
            "rewind/rw: Rewinds a specified amount of seconds.\n\n" +
            "next/nxt: Skips to the next track.\n" +
            "previous/prv: Seeks to the start of the previous track.\n\n" +
            "seekdisc/skdsc: Seeks to a specified time or track on the disc\n" +
            "seek/sk: Seeks to a specified time within the currently playing track.\n\n" +
            "eject/ejct: Ejects the disc if your drive supports it.\n" +
            "discinfo/dscinf: Displays info about the disc.\n" +
            "info/inf: Displays information about the position on disc.\n" +
            "help/hlp: Prints this message.\n" +
            "quit/qt: Exits CSharp Disc Player.\n";
        Console.WriteLine(Help);
    }
}
class Program
{
    static void Main()
    {
        string[] Commands = { "play", "ply", "discinfo", "dscinf", "quit", "qt", "stop", "stp", "help", "hlp", "pause", "ps", "resume", "rsm", "seekdisc", "skdsc", "next", "nxt", "previous", "prv", "fastforward", "ff", "rewind", "rw", "info", "inf", "seek", "sk","eject","ejct"};
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
                                Arg1 = CD.GetTrackPositions()[0];
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
                    if (FuncType == "seekdisc")
                    {
                        CD.Seek(Arg1);
                    }
                    if (FuncType == "seek")
                    {
                        CD.SeekTrack(Arg1);
                    }
                    if (FuncType == "next")
                    {
                        try
                        {
                            CD.Seek(CD.GetTrackPositions()[System.Convert.ToInt16(CD.GetPlayHeadPosition()[1])]);
                        }
                        catch
                        {
                            ;
                        }
                    }
                    if (FuncType == "previous")
                    {
                        try
                        {
                            CD.Seek(CD.GetTrackPositions()[System.Convert.ToInt16(CD.GetPlayHeadPosition()[1]) - 2]);
                        }
                        catch
                        {
                            ;
                        }
                    }
                    if (FuncType == "ff")
                    {
                        CD.DragPlayHead(Arg1);
                    }
                    if (FuncType == "rw")
                    {
                        CD.DragPlayHead($"-{Arg1}");
                    }
                    if (FuncType == "plyhdprp")
                    {
                        CD.ListPlayHeadProp();
                    }
                    if (FuncType == "open")
                    {
                        CD.Tray("open");
                    }
                    if (FuncType == "quit")
                    {
                        CD.End();
                        return;
                    }
                }
            }
        }
    }
}
