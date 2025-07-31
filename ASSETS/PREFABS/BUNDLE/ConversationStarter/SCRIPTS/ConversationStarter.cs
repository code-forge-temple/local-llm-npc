/************************************************************************
 *    Copyright (C) 2025 Code Forge Temple                              *
 *    This file is part of local-llm-npc project                        *
 *    See the LICENSE file in the project root for license details.     *
 ************************************************************************/

using Godot;

namespace LllmNpcConversationSystem
{
    /// <summary>
    /// Handles the logic for starting a conversation when the player enters a designated area.
    /// Shows a talk button and triggers the conversation when pressed.
    /// </summary>
    public partial class ConversationStarter : Area2D
    {
        /// <summary>
        /// Reference to the talk button UI element.
        /// </summary>
        [Export]
        private Button talkButton;

        /// <summary>
        /// Reference to the Conversation object that manages the dialogue.
        /// </summary>
        [Export]
        private Conversation conversation;

        /// <summary>
        /// Called when the node enters the scene tree for the first time.
        /// Initializes references and sets up event handlers.
        /// </summary>
        public override void _Ready()
        {
            talkButton = GetNode<Button>("TalkButton");

            if (talkButton == null)
            {
                GD.PrintErr("TalkButton not found");
            }

            if (conversation == null)
            {
                GD.PrintErr("Conversation not found");
            }

            talkButton.Visible = false;
            talkButton.Pressed += onTalkButtonPressed;

            BodyEntered += OnBodyEntered;
        }

        /// <summary>
        /// Called when a body enters the area.
        /// Shows the talk button if the body is the player.
        /// </summary>
        /// <param name="body">The node that entered the area.</param>
        private void OnBodyEntered(Node2D body)
        {
            if (body is Player)
            {
                talkButton.Visible = true;
            }
        }

        /// <summary>
        /// Called when the talk button is pressed.
        /// Starts the conversation and makes it visible.
        /// </summary>
        private void onTalkButtonPressed()
        {
            conversation.Visible = true;
            conversation.StartConversation();
        }
    }
}