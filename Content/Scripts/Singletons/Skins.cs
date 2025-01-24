using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Skins : Node
{
    public static readonly string[] VanillaSkinNames = ["Slippey", "Samey", "Sanboy", "Strawman", "Pineplum", "Bondey", "Sleepy", "Daley", "Hostey", "CompressMass", "JioYobaFefski", "Chad", "MISSINGNULL", "Corey"];

    public static Dictionary<string, bool> IsSkinBoughtDic = VanillaSkinNames.ToDictionary(key => key, key => false);

    public static int GetBoughtSkinsCount()
    {
        int count = 0;
        foreach (var isSkinBoughtObj in IsSkinBoughtDic)
        {
            if (isSkinBoughtObj.Value)
            {
                count++;
            }
        }
        return count;
    }

    public static int GetSkinsCount()
    {
        return VanillaSkinNames.Length + ModSkinNames.Length;
    }

    // Mod skins
    public static string[] ModSkinNames = [];
}
