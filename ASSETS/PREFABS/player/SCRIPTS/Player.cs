/************************************************************************
 *    Copyright (C) 2025 Code Forge Temple                              *
 *    This file is part of local-llm-npc project                        *
 *    See the LICENSE file in the project root for license details.     *
 ************************************************************************/

using Godot;

public partial class Player : CharacterBody2D
{
    [Export]
    public float speed = 300.0f;

    private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    private bool wasOnFloor = false;
    private AnimationPlayer animationPlayer;
    private Sprite2D sprite;

    [Export]
    private float landingImpactThreshold = 200f;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        sprite = GetNode<Sprite2D>("Sprite2D");


        wasOnFloor = IsOnFloor();
    }

    public override void _Process(double delta)
    {
        bool isOnFloor = IsOnFloor();
        bool keyPressed = false;

        Vector2 velocity = Velocity;

        if (!isOnFloor)
        {
            velocity.Y += gravity * (float)delta;
        }

        velocity.X = 0;

        if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right))
        {
            velocity.X = speed;

            if (IsOnFloor())
            {
                animationPlayer.Play("run");
            }
            sprite.FlipH = false;

            keyPressed = true;
        }
        else if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left))
        {
            velocity.X = -speed;
            if (IsOnFloor())
            {
                animationPlayer.Play("run");
            }
            sprite.FlipH = true;
            keyPressed = true;
        }

        if (!keyPressed && IsOnFloor())
        {
            animationPlayer.Play("idle");
        }

        Velocity = velocity;

        MoveAndSlide();
    }
}

