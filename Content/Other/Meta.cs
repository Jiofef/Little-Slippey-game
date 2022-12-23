using Godot;
using Godot.Collections;
using System;
using System.Linq;

public class Meta : Node
{
    //Meta is the singleton that having various options for gameplay, video and other. It saves and when you enter the game, you will find the same settings that you set earlier

    //Reserve in case user will click cancel options or will pressed escape button
    public static Meta OptionsReserve = new Meta();
    //And default Meta Instance
    public static Meta Instance = new Meta();

    //SoundsVolume
    //in this array #0 is _master, #1 _interface, #2 _music, #3 _player, #4 _crosssnap, #5 _crossother, %6 _crossexplosion
    public float[] _busvolumes = { -8, 0, 0, 0, -15, 0, 0 };
    public bool[] _dobusmuted = { false, false, false, false, false, false, false };

    //VideoOptions
    public Vector2 Resolution = new Vector2(1280, 720);
    public enum ScreenFormat { Windowed, Borderless, Fullscreen }
    public ScreenFormat _screenFormat;

    //Gameplay
    public byte _dificulty = 0;

    public void ApplyOptions()
    {
        for (int i = 0; i < Instance._busvolumes.Length; i++)
        {
            AudioServer.SetBusVolumeDb(i, Instance._busvolumes[i]);
            AudioServer.SetBusMute(i, Instance._dobusmuted[i]);
        }

        switch (_screenFormat)
        {
            case ScreenFormat.Windowed:
                OS.WindowFullscreen = false;
                OS.WindowBorderless = false;
                break;
            case ScreenFormat.Borderless:
                OS.WindowFullscreen = false;
                OS.WindowBorderless = true;
                break;
            case ScreenFormat.Fullscreen:
                OS.WindowFullscreen = true;
                OS.WindowBorderless = false;
                break;
        }

        OS.WindowSize = Instance.Resolution;
    }
    public Meta Clone()
    {
        Meta returnmeta = new Meta();
        for (int i = 0; i < Instance._busvolumes.Length; i++)
        {
            returnmeta._busvolumes[i] = _busvolumes[i];
            returnmeta._dobusmuted[i] = _dobusmuted[i];
        }
        returnmeta._dificulty = _dificulty;
        returnmeta._screenFormat = _screenFormat;
        returnmeta.Resolution = Resolution;
        return returnmeta;
    }
    public Dictionary<string, object> GetJson()
    {
        return new Dictionary<string, object>()
        {
            {"_busvolumes", _busvolumes},
            {"_dobusmuted0",  _dobusmuted[0]},
            {"_dobusmuted1",  _dobusmuted[1]},
            {"_dobusmuted2",  _dobusmuted[2]},
            {"_dobusmuted3",  _dobusmuted[3]},
            {"_dobusmuted4",  _dobusmuted[4]},
            {"_dobusmuted5",  _dobusmuted[5]},
            {"_dobusmuted6",  _dobusmuted[6]},
            {"ResolutionX", Resolution.x},
            {"ResolutionY", Resolution.y},
            {"screenFormat", _screenFormat},
            {"_dificulty", _dificulty}
        };
    }
    public void SaveToFile()
    {
        const string SavePath = "user://options.json";
        File file = new File();
        var data = Instance.GetJson();
        var jsondata = JSON.Print(data);
        file.Open(SavePath, File.ModeFlags.Write);
        file.StoreString(jsondata);
        file.Close();
    }
    public void LoadOptions()
    {
        try
        {
            const string ReadPath = "user://options.json";

            File file = new File();
            if (!file.FileExists(ReadPath)) return;
            file.Open(ReadPath, File.ModeFlags.Read);
            var text = file.GetAsText();
            var model = JSON.Parse(text).Result as Dictionary;

            var _busvolumesArray = model["_busvolumes"] as Godot.Collections.Array;
            for (int i = 0; i < _busvolumesArray.Count; i++)
                _busvolumes[i] = (float)_busvolumesArray[i];

            for (int i = 0; i < 7; i++)
                _dobusmuted[i] = (bool)model["_dobusmuted" + i];

            Resolution.x = (float)model["ResolutionX"];
            Resolution.y = (float)model["ResolutionY"];

            var screenFormatID = Convert.ToInt32(model["screenFormat"]);
            switch (screenFormatID)
            {
                case 0:
                    _screenFormat = ScreenFormat.Windowed;
                    break;
                case 1:
                    _screenFormat = ScreenFormat.Borderless;
                    break;
                case 2:
                    _screenFormat = ScreenFormat.Fullscreen;
                    break;
            }

            _dificulty = Convert.ToByte(model["_dificulty"]);

            file.Close();
        }
        catch {}
    }
}