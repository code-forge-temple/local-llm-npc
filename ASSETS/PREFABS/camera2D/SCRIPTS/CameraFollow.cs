/************************************************************************
 *    Copyright (C) 2025 Code Forge Temple                              *
 *    This file is part of local-llm-npc project                        *
 *    See the LICENSE file in the project root for license details.     *
 ************************************************************************/

using Godot;

public partial class CameraFollow : Camera2D
{
	[Export]
	public NodePath playerPath;

	[Export]
	public float followSpeed = 5.0f;

	private Node2D player;

	public override void _Ready()
	{
		player = GetNode<Node2D>(playerPath);
	}

	public override void _Process(double delta)
	{
		if (player != null)
		{
			GlobalPosition = GlobalPosition.Lerp(player.GlobalPosition, (float)delta * followSpeed);
		}
	}
}