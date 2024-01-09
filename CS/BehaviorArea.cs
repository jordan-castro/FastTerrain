using Godot;
using System;

public partial class BehaviorArea : Area2D
{
    // Field for behavior name
    private string behaviorName = "default";

    // Constructor
    public BehaviorArea(string name = "default")
    {
        behaviorName = name;
    }

    // Property for behaviorName
    public string BehaviorName
    {
        get { return behaviorName; }
        set { behaviorName = value; }
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
