using Godot;
using System;

public partial class BehaviorArea : Area2D
{
    // Field for behavior name
    private string behavior_name = "default";

    // Constructor
    public BehaviorArea(string name = "default")
    {
        behavior_name = name;
    }

    // Property for behaviorName
    public string BehaviorName
    {
        get { return behavior_name; }
        set { behavior_name = value; }
    }

    // Ready method
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    // Signal handler for body_entered
    private void OnBodyEntered(Node body)
    {
        EmitSignal(nameof(BehaviorBodyEntered), body, this);
    }

    // Define the signal
    [Signal]
    public delegate void BehaviorBodyEnteredEventHandler(Node body, BehaviorArea behaviorArea);
}
