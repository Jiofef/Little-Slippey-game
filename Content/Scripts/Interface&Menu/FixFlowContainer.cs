using Godot;
using System.Linq;

// Custom FlowContainer to keep a specific element at the end of the first row
public partial class FixFlowContainer : FlowContainer
{
    // The element to fix at the end of the first row (assign in editor)
    [Export]
    public Control FixedElement;


    public override void _Ready()
    {
        // Connect to the resized signal
        Resized += OnResized;

        // Initial position update
        UpdateElementPosition();
    }

    private void OnResized()
    {
        UpdateElementPosition();
    }

    private void UpdateElementPosition()
    {
        if (FixedElement == null || !IsNodeReady()) return;

        // Ensure fixed element is last in the child list
        MoveChild(FixedElement, GetChildCount() - 1);

        int firstRowEndIndex = GetFirstRowEndIndex();

        // If element is not in first row, move it to first row's end
        if (FixedElement.GetIndex() > firstRowEndIndex)
        {
            MoveChild(FixedElement, firstRowEndIndex - 1);
        }
    }

    private int GetFirstRowEndIndex()
    {
        float currentWidth = 0;
        int index = 0;
        float hSeparation = GetThemeConstant("h_separation");

        foreach (var child in GetChildren().OfType<Control>())
        {
            if (child == FixedElement) continue;

            // Calculate cumulative width with spacing
            currentWidth += child.Size.X + hSeparation;

            // Check if we exceeded container width
            if (currentWidth > Size.X)
            {
                break;
            }

            index++;
        }

        return index;
    }
}