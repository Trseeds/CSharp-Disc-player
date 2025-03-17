using System;
using Main;

class TUI
{
    static void Main()
    {
        string[] Commands = { "play", "ply", "discinfo", "dscinf", "quit", "qt", "stop", "stp", "help", "hlp", "pause", "ps", "resume", "rsm", "seekdisc", "skdsc", "next", "nxt", "previous", "prv", "fastforward", "ff", "rewind", "rw", "info", "inf", "seek", "sk", "eject", "ejct" };
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
