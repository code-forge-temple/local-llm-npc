/************************************************************************
 *    Copyright (C) 2025 Code Forge Temple                              *
 *    This file is part of local-llm-npc project                        *
 *    See the LICENSE file in the project root for license details.     *
 ************************************************************************/

using Godot;

public partial class Npc : CharacterBody2D
{
    private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private bool wasOnFloor = false;
    private AnimationPlayer animationPlayer;
    private Sprite2D sprite;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        sprite = GetNode<Sprite2D>("Sprite2D");

        wasOnFloor = IsOnFloor();
    }

    public override void _Process(double delta)
    {
        bool isOnFloor = IsOnFloor();
        Vector2 velocity = Velocity;

        if (!isOnFloor)
        {
            velocity.Y += gravity * (float)delta;
        }

        velocity.X = 0;

        if (IsOnFloor())
        {
            animationPlayer.Play("idle");
        }

        Velocity = velocity;
        
        MoveAndSlide();
    }
}
