using Godot;
using OtherExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

public partial class CommandConsole : Node
{
	public event Action console_opened;
	public event Action console_closed;
	public event Action console_unknown_command;

	static CommandConsole instance;

    class Command
    {
        public bool base_command = false;
        public Callable function;
        public int min_param_count;
        public int max_param_count;
        public Dictionary<string, string> Params = new Dictionary<string, string>();
        public string Description = "Description Not Defined";

        public Command(Callable in_function, int in_min_param_count, int in_max_param_count)
        {
            this.function = in_function;
            this.min_param_count = in_min_param_count;
            this.max_param_count = in_max_param_count;
        }
    }

    void OnTextSubmited(string text)
    {
        ScrollToBottom();
        ResetAutoComplete();
        line_edit.Clear();
        syntaxLabel.Clear();
        AddInputHistory(text);
        PrintLine(text);

        string[] splitText = SplitConsideringQuotes(text);

        if (splitText.Length > 0)
        {
            string commandString = splitText[0].ToLower();

            if (Commands.ContainsKey(commandString))
            {
                Command commandEntry = Commands[commandString];

                if (splitText.Length - 1 < commandEntry.min_param_count)
                {
                    PrintLine($"Not enough parameters. Expected at least {commandEntry.min_param_count}.");
                    return;
                }

                if (commandEntry.max_param_count != -1 && splitText.Length - 1 > commandEntry.max_param_count)
                {
                    PrintLine($"Too many parameters. Expected at most {commandEntry.max_param_count}.");
                    return;
                }

                List<Variant> InGameparams_ = new List<Variant>();

                for (int i = 1; i < splitText.Length; i++)
                {
                    InGameparams_.Add(splitText[i]);
                }

                commandEntry.function.Call(InGameparams_.ToArray());
            }
            else
            {
                console_unknown_command?.Invoke();
                PrintLine("Command not found.", "red");
            }
        }
    }

    Control control;
	RichTextLabel rich_label;
	RichTextLabel syntaxLabel;
    LineEdit line_edit;

	Dictionary<string, Command> Commands = new Dictionary<string, Command>();
	List<string> console_history = new List<string>();
	int console_history_index = 0;


	List<string> Suggestions = new List<string>();
	int CurrentSuggestion = 0;
	bool Suggesting = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        #region Create

        instance = this;
		this.control = new Control();
		this.rich_label = new RichTextLabel();
		this.syntaxLabel = new RichTextLabel();
        this.line_edit = new LineEdit();


		StyleBoxFlat style = new StyleBoxFlat();
		StyleBoxFlat style2 = new StyleBoxFlat();
        style.BgColor = new Color(0, 0, 1, 0.5f);
		style2.BgColor = new Color(0, 0, 0, 0.5f);

		CanvasLayer canvas = new CanvasLayer();
		canvas.Layer = 100;
		AddChild(canvas);
		control.AnchorBottom = 1;
		control.AnchorRight = 1;
		canvas.AddChild(control);

		rich_label.BbcodeEnabled = true;
		rich_label.ScrollFollowing = true;
		rich_label.AnchorRight = 1.0f;
		rich_label.AnchorBottom = 0.5f;
		rich_label.AddThemeStyleboxOverride("normal", style);
		control.AddChild(rich_label);

		rich_label.Text = "Development Console. \n";
		line_edit.PlaceholderText = "Enter \"help\" for instructions";
		line_edit.AnchorTop = 0.54f;
		line_edit.AnchorRight = 1;
		line_edit.AnchorBottom = 0.59f;

		syntaxLabel.AnchorTop = 0.5f;
		syntaxLabel.AnchorRight = 1.0f;
		syntaxLabel.AnchorBottom = 0.54f;

        syntaxLabel.AddThemeStyleboxOverride("normal",style2);
		control.AddChild(syntaxLabel);


        control.AddChild(line_edit);
		line_edit.TextSubmitted += OnTextSubmited;
		line_edit.TextChanged += OnTextChanged;
		control.Visible = false;
		this.ProcessMode = ProcessModeEnum.Always;
        #endregion

        #region add commands
        AddCommand("quit", Quit);
		Commands["quit"].base_command = true;
		AddCommandDescription("quit", "Quits the game.");

		AddCommand("help", Help);
		Commands["help"].base_command = true;
		AddCommandDescription("help", "Shows a list of all the currently registered commands.");

		AddCommand("clear", clear);
		Commands["clear"].base_command = true;
		AddCommandDescription("clear", "Clears the current registry view.");

		AddCommand("delete_history", DeleteHistory);
		Commands["delete_history"].base_command = true;
		AddCommandDescription("delete_history", "Deletes the commands history.");

		AddCommand("command_list", ShowExternalCommands);
		Commands["command_list"].base_command = true;
		AddCommandDescription("command_list", "Shows a list of all the currently registered commands.");

        // Jiofef's commands
        AddCommand("print", new Action<string, string>(PrintLine), 2);
        AddCommand("get", new Action<string>(PrintGet), 1);
        AddCommand("print_tree", new Action(print_tree));

        AddCommand("select_node", new Action<string>(select_node), 1);
        AddCommand("node_call", new Action<string>(node_call), 1);
        AddCommand("node_set", new Action<string, Variant>(node_set), 2);
        AddCommand("node_get", new Action<string>(node_get), 1);

        AddCommand("get_achievement", new Action<string>(get_achievement));
        AddCommand("remove_achievement", new Action<string>(remove_achievement));

        AddCommand("set_level_completed", new Action<int, byte>(set_level_completed));

        AddCommand("add_golden_crosses", new Action<int>(AddGoldenCrosses));

        AddCommand("cheats", new Action(Cheats));

        GetCommandsWithAttribute();
        #endregion
    }
    #region base Command Console
    private void OnTextChanged(string newText)
	{
		ShowDescription(newText);
		ResetAutoComplete();

    }


    string[] SplitConsideringQuotes(string input)
    {
        var matches = Regex.Matches(input, @"""[^""]+""|\S+");
        var parts = new string[matches.Count];

        for (int i = 0; i < matches.Count; i++)
        {
            parts[i] = matches[i].Value.Trim('"');
        }

        return parts;
    }

    public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
            // ~ key
            if (eventKey.Keycode == Key.Quoteleft)
			{
				if (eventKey.Pressed)
				{
					ToggleConsole();
				}
				GetTree().Root.SetInputAsHandled();
			}
			//toggle console size
			else if (eventKey.Keycode == Key.Quoteleft && eventKey.IsCommandOrControlPressed())
			{
				if (eventKey.Pressed)
				{
					if (control.Visible)
						ToggleSize();
					else
					{
						ToggleConsole();
						ToggleSize();
					}
					GetTree().Root.SetInputAsHandled();
				}
			}
			//disable control on ESC
			else if (eventKey.Keycode == Key.Escape && control.Visible)
			{
				if (eventKey.Pressed)
					ToggleConsole();
				GetTree().Root.SetInputAsHandled();
			}

			if (control.Visible && eventKey.Pressed)
			{
				if (eventKey.GetKeycodeWithModifiers() == Key.Up)
				{
                    GetTree().Root.SetInputAsHandled();
                    if (console_history_index > 0)
                    {
                        console_history_index--;
                        if (console_history_index >= 0)
                        {
                            line_edit.Text = console_history[console_history_index];
                            line_edit.CaretColumn = line_edit.Text.Length;
                            ResetAutoComplete();
                        }
                    }
                }

				if (eventKey.GetKeycodeWithModifiers() == Key.Down)
				{
                    GetTree().Root.SetInputAsHandled();
					if(console_history_index < console_history.Count)
					{
						console_history_index++;
						if(console_history_index < console_history.Count)
						{
                            line_edit.Text = console_history[console_history_index];
                            line_edit.CaretColumn = line_edit.Text.Length;
                            ResetAutoComplete();
                        }
                        else
						{
                            line_edit.Clear();
                            ResetAutoComplete();
                        }
					}
                }

                if (eventKey.Keycode == Key.Pageup)
				{
					VScrollBar scroll = rich_label.GetVScrollBar();
					scroll.Value -= scroll.Page - scroll.Page * 0.1;
					GetTree().Root.SetInputAsHandled();
				}
				if (eventKey.Keycode == Key.Pagedown)
				{
					VScrollBar scroll = rich_label.GetVScrollBar();
					scroll.Value += scroll.Page - scroll.Page * 0.1;
					GetTree().Root.SetInputAsHandled();
				}
				if (eventKey.Keycode == Key.Tab)
				{
					AutoComplete();
					GetTree().Root.SetInputAsHandled();
				}
			}
		}
	}

	void ToggleConsole()
	{
		control.Visible = !control.Visible;
		if (control.Visible)
		{
			//GetTree().Paused = true;
			line_edit.GrabFocus();
			console_opened?.Invoke();
		}
		else
		{
			control.AnchorBottom = 1f;
			//GetTree().Paused = false;
			ScrollToBottom();
			ResetAutoComplete();
			console_closed?.Invoke();
		}
	}
	void ToggleSize()
	{
		if (control.AnchorBottom == 1.0f)
		{
			control.AnchorBottom = 1.9f;
		}
		else
		{
			control.AnchorBottom = 1.0f;
		}
	}
	void AutoComplete()
	{
		if (Suggesting)
		{
			for (int i = 0; i < Suggestions.Count; i++)
			{
				if (CurrentSuggestion == i)
				{
					line_edit.Text = Suggestions[i];
					line_edit.CaretColumn = line_edit.Text.Length;
					if (CurrentSuggestion == Suggestions.Count -1)
					{
						CurrentSuggestion = 0;
					}
					else
					{
						CurrentSuggestion++;
					}
					return;
				}
			}
		}
		else
		{
			Suggesting = true;
			List<string> commands = new List<string>();
			foreach (var command in Commands)
			{
				commands.Add(command.Key);
			}
			commands.Sort();
			commands.Reverse();

			int PrevIndex = 0;
			foreach (var command in commands)
			{
				if (command.Contains(line_edit.Text))
				{
					int index = command.Find(line_edit.Text);
					if (index <= PrevIndex)
					{
						Suggestions.Insert(0, command);
					}
					else
					{
						Suggestions.Add(command);
					}
					PrevIndex = index;
				}
			}
			AutoComplete();

		}
	}

	void ScrollToBottom()
	{
		ScrollBar scroll = rich_label.GetVScrollBar();
		scroll.Value = scroll.MaxValue - scroll.Page;
	}
	void ResetAutoComplete()
	{
		this.Suggestions.Clear();
		CurrentSuggestion = 0;
		Suggesting = false;
	}

	void ShowDescription(string input)
	{
        syntaxLabel.Clear();
        var matches = Regex.Matches(input, @"""[^""]+""|\S+");
		string _function = "";
		string _params = "";

        foreach (var match in matches)
		{
			foreach (var command in Commands.Keys)
			{
				if(command == match.ToString())
				{
					_function = command;
					
					foreach(string _param in Commands[command].Params.Keys) 
					{
						_params += " [color=yellow]" + _param + "[/color]"; 

					}
                }
			}
		}
		
		syntaxLabel.AppendText(_function);
		syntaxLabel.AppendText(_params);
    }

	void PrintLine(string text, string color = "white")
	{
		if (rich_label == null)
		{
			CallDeferred("PrintLine", text);
		}
		else
		{
			rich_label.AppendText($"[color={color}]{text}[/color]\n");
		}
	}

	void AddInputHistory(string text)
	{
		if (console_history.Count == 0 || text != console_history.Last())
		{
			console_history.Add(text);
		}
		console_history_index = console_history.Count;
	}

    void RemoveCommand(string CommandName)
	{
        Commands.Remove(CommandName);
    }

	public void Quit()
	{
		GetTree().Quit();
	}
	
	void clear()
	{
		rich_label.Clear();
	}

	void DeleteHistory()
	{
		console_history.Clear();
		console_history_index = 0;
		DirAccess.RemoveAbsolute(GetPath()+"/Commands.log");
	}
	void Help()
	{
		rich_label.AddText("Commands:\n");
        foreach (var command in Commands.OrderBy(d => d.Key))
        {
			if(command.Value.base_command)
				rich_label.AppendText(string.Format("\t[color=light_green]{0}[/color]: {1}\n", command.Key, command.Value.Description));
        }
        rich_label.AppendText("" +
            "\r\n\t\t[color=light_blue]Up[/color] and [color=light_blue]Down[/color] arrow keys to navigate commands history" +
            "\r\n\t\t[color=light_blue]PageUp[/color] and [color=light_blue]PageDown[/color] to navigate registry history" +
            "\r\n\t\t[color=light_blue]Ctr+Tilde[/color] to change console size between half screen and full creen" +
            "\r\n\t\t[color=light_blue]Tilde[/color] or [color=light_blue]Esc[/color] to close the console" +
            "\r\n\t\t[color=light_blue]Tab[/color] for basic autocomplete\n");
	}

	void ShowExternalCommands()
	{
        foreach (var command in Commands.OrderBy(d => d.Key))
        {
            if (!command.Value.base_command)
                rich_label.AppendText(string.Format("\t[color=light_green]{0}[/color]: {1}\n", command.Key, command.Value.Description));
        }
    }

    // Jiofef's methods
    void PrintGet(string value)
    {
        PrintLine(Get(value).ToString());
    }
	void print_tree()
	{
		string tree = GodotExtensions.GetTreePretty(G.Root);

		PrintLine(tree);
	}

	Node _selectedNode;
    void select_node(string path_from_root)
    {
		_selectedNode = G.Root.GetNodeOrNull(path_from_root);

		if (_selectedNode != null)
			PrintLine(_selectedNode.Name + " selected. You can use node_call, node_set or node_get on it");
		else
			PrintLine("Node not found", "red");
    }

	void node_call(string method)
	{
        if (_selectedNode == null)
		{
            PrintLine("Node isn't selected", "red");
			return;
        }
        if (!_selectedNode.HasMethod(method))
		{
            PrintLine("Method not found", "red");
			return;
        }

		_selectedNode.CallDeferred(method);
	}

    void node_set(string property_name, Variant value)
    {
        if (_selectedNode == null)
        {
            PrintLine("Node isn't selected", "red");
            return;
        } 

        _selectedNode.SetDeferred(property_name, value);
    }

    void node_get(string property_name)
    {
        if (_selectedNode == null)
        {
            PrintLine("Node isn't selected", "red");
            return;
        }

		PrintLine(_selectedNode.Get(property_name).ToString());
    }

	void get_achievement(string achievement_name)
	{
		if (!Achievements.AllTheAchievements.ContainsKey(achievement_name))
		{
			PrintLine("This achievement does not exists", "red");
			return;
		}

		Achievements.GetAchievement(achievement_name);
	}
	void remove_achievement(string achievement_name)
	{
        if (!Achievements.AllTheAchievements.ContainsKey(achievement_name))
        {
            PrintLine("This achievement does not exists", "red");
            return;
        }

        Achievements.RemoveAchievement(achievement_name);
    }

    void set_level_completed(int level_number, byte level_complete_Status)
    {
		if (level_number < 1 || level_number > G.LevelsInGameTotal)
		{
			PrintLine("This level does not exist", "red");
			return;
		}

		if (level_complete_Status < 0  && level_complete_Status <= G.DificultiesInGameTotal)
		{
			PrintLine("This dificulty does not exist", "red");
			return;
		}

		UnchangableMeta.LevelCompleteStatus[level_number] = level_complete_Status;

		Achievements.UpdateLevelAchievements(level_number);
    }
	void AddGoldenCrosses(int amount)
	{
		UnchangableMeta.GoldenCrossesAmount += amount;
	}

	void Cheats()
	{
		G.IsDebugEnabled = !G.IsDebugEnabled;

		if (G.IsDebugEnabled)
		{
			PrintLine("Cheats on. \n" +
                "Immortality - Alt+Z.\n" +
                "Player's physics - Alt+X.\n" +
				"Disable crosses - Alt+C disables the crosses.\n" +
				"Get or remove scores - Alt + mouse wheel scroll up or down\n" +
				"Teleport to mouse - Alt + LBM");
		}
		else
		{
			PrintLine("Cheats off");
		}
	}

    #endregion

    /// <summary>
    /// add a command to the console ingame, active console with ~ button.
    /// <code>
    /// CommandConsole.AddCommand("testing", test);
    /// </code>
    /// any opinion is accepted.
    /// </summary>
    /// <param name="CommandName">name of the function called in game console</param>
    /// <param name="function">reference to the method</param>
    public static void AddCommand(string CommandName, Delegate function, int minParamCount = 0, int maxParamCount = -1)
    {
        try
        {
            instance.Commands.Add(CommandName, new Command(new Callable((GodotObject)function.Target, function.Method.Name), minParamCount, maxParamCount));

            foreach (var param in function.Method.GetParameters())
            {
                instance.Commands[CommandName].Params.Add(param.Name, null);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr(e);
        }
    }

    /// <summary>
	/// work in progress
	/// </summary>
	/// <param name="CommandName"></param>
	/// <param name="param"></param>
	/// <param name="description"></param>
    public static void AddParameterDescription(string CommandName, string param, string description)
    {
        try
        {
            instance.Commands[CommandName].Params[param] = description;
        }
        catch (Exception e)
        {
            GD.PrintErr(e);
        }
    }

	/// <summary>
	/// shows a description of the command in the console with commandlist
	/// </summary>
	/// <param name="CommandName"></param>
	/// <param name="description"></param>
    public static void AddCommandDescription(string CommandName, string description)
    {
        try
        {
            instance.Commands[CommandName].Description = description;
        }
        catch (Exception e)
        {
            GD.PrintErr(e);
        }
    }

    void GetCommandsWithAttribute()
    {
        var Methods =
            Assembly.GetExecutingAssembly()
            .GetTypes().
            SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            ;


        foreach (var Method in Methods)
        {
            var commandAttributes = Method.GetCustomAttributes(typeof(AddCommandAttribute), false);
            AddCommandDescriptionAttribute commandDescriptionAttribute = (AddCommandDescriptionAttribute)Method.GetCustomAttribute(typeof(AddCommandDescriptionAttribute), false);

            foreach (AddCommandAttribute att in commandAttributes)
            {
                try
                {
                    Delegate delegateInstance = null;

                    if (Method.IsStatic)
                    {
                        
                        return;
                        /*
						delegateInstance = Delegate.CreateDelegate(
							System.Linq.Expressions.Expression.GetActionType(
								Method.GetParameters().Select(p => p.ParameterType).ToArray()),
							null,
							Method);*/
                    }

                    else
                    {
                        var targetType = Method.DeclaringType;
                        var targetInstance = Activator.CreateInstance(targetType); // This assumes the type has a parameterless constructor
                        delegateInstance = Delegate.CreateDelegate(
                            System.Linq.Expressions.Expression.GetActionType(
                                Method.GetParameters().Select(p => p.ParameterType).ToArray()),
                            targetInstance,
                            Method);
                    }

                    AddCommand(att.CommandName, delegateInstance);

                    if (commandDescriptionAttribute != null)
                    {
                        AddCommandDescription(att.CommandName, commandDescriptionAttribute.Description);
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr(ex.ToString());
                }
            }
        }
    }
}
