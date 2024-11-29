using Godot;
using System;

public partial class RecycleBinIcon : EnhancedButton
{
	public enum IconType { File, Link}
    [Export] public IconType iconType = IconType.File;
	[Export] public string Link;
}
