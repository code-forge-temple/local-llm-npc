/************************************************************************
 *    Copyright (C) 2025 Code Forge Temple                              *
 *    This file is part of local-llm-npc project                        *
 *    See the LICENSE file in the project root for license details.     *
 ************************************************************************/

using Godot;
using System;
using System.Collections.Generic;
using LllmNpcConversationSystem.Services;
using LllmNpcConversationSystem.Services.Types;
using Newtonsoft.Json;

namespace LllmNpcConversationSystem
{
    /// <summary>
    /// Enum representing possible pronoun types for NPCs.
    /// </summary>
    public enum NpcPronounType
    {
        TheyThem,
        HeHim,
        SheHer
    }

    /// <summary>
    /// Handles the UI and logic for NPC conversations, including message history,
    /// AI response fetching, and display formatting.
    /// </summary>
    public partial class Conversation : CanvasLayer
    {
        /// <summary>
        /// Signal type for other events.
        /// </summary>
        public const string OTHER_SIGNAL = "other";

        /// <summary>
        /// Color used for user messages in the conversation display.
        /// </summary>
        [Export]
        public Color UserMessageColor { get; set; } = Colors.LightBlue;

        /// <summary>
        /// Color used for NPC messages in the conversation display.
        /// </summary>
        [Export]
        public Color NpcMessageColor { get; set; } = Colors.LightGreen;

        /// <summary>
        /// Name of the NPC.
        /// </summary>
        [Export]
        public string NpcName { get; set; } = "NPC";

        /// <summary>
        /// Path to the NPC backstory text file.
        /// </summary>
        [Export]
        public string NpcBackStoryPath { get; set; } = "res://ASSETS/PREFABS/BUNDLE/UI/Conversation/TEMPLATES/npcBackStory.txt";

        /// <summary>
        /// Path to the NPC response schema JSON file.
        /// </summary>
        [Export]
        public string NpcResponseSchemaPath { get; set; } = "res://ASSETS/PREFABS/BUNDLE/UI/Conversation/TEMPLATES/npcResponseSchema.json";

        /// <summary>
        /// Initial message shown by the NPC at the start of the conversation.
        /// </summary>
        [Export]
        public string NpcInitialMessage { get; set; } = "";

        /// <summary>
        /// Pronouns used by the NPC.
        /// </summary>
        [Export]
        public NpcPronounType NpcPronouns { get; set; } = NpcPronounType.TheyThem;

        /// <summary>
        /// Event handler for conversation events.
        /// </summary>
        [Export]
        public ConversationEventHandler EventHandler { get; set; }

        /// <summary>
        /// Handles structured responses from the AI.
        /// </summary>
        protected ConversationResponseHandler responseHandler;

        /// <summary>
        /// Format for AI responses, loaded from schema.
        /// </summary>
        protected object responseFormat;

        /// <summary>
        /// NPC backstory loaded from file.
        /// </summary>
        protected string npcBackStory = "";

        /// <summary>
        /// RichTextLabel node for displaying conversation.
        /// </summary>
        private RichTextLabel conversationRichTextLabel;

        /// <summary>
        /// Button to close the conversation UI.
        /// </summary>
        private TextureButton closeButton;

        /// <summary>
        /// LineEdit node for player input.
        /// </summary>
        protected LineEdit playerResponseLineEdit;

        /// <summary>
        /// Button to submit player input.
        /// </summary>
        protected Button playerResponseButton;

        /// <summary>
        /// Service for fetching AI responses.
        /// </summary>
        protected OllamaService ollamaService;

        /// <summary>
        /// List of messages exchanged in the conversation.
        /// </summary>
        protected List<Message> conversationHistory;

        /// <summary>
        /// Indicates if the system is waiting for an AI response.
        /// </summary>
        protected bool isWaitingForResponse = false;

        /// <summary>
        /// Indicates if the conversation display needs updating.
        /// </summary>
        protected bool needsDisplayUpdate = false;

        /// <summary>
        /// Handles logic when the conversation UI visibility changes.
        /// </summary>
        protected virtual void OnVisibilityChanged()
        {
            if (Visible)
                GetTree().Paused = true;
            else
                GetTree().Paused = false;
        }

        /// <summary>
        /// Loads the response schema from the specified path.
        /// </summary>
        private void LoadResponseSchema()
        {
            try
            {
                var file = FileAccess.Open(NpcResponseSchemaPath, FileAccess.ModeFlags.Read);
                if (file != null)
                {
                    var jsonContent = file.GetAsText();
                    file.Close();

                    responseFormat = JsonConvert.DeserializeObject(jsonContent);

                    GD.Print("Loaded response schema for structured output");
                }
                else
                {
                    responseFormat = null;

                    GD.PrintErr("Could not load response schema file");
                }
            }
            catch (Exception ex)
            {
                responseFormat = null;

                GD.PrintErr($"Error loading response schema: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the NPC backstory from the specified path.
        /// </summary>
        private void LoadNpcBackStory()
        {
            try
            {
                if (!string.IsNullOrEmpty(NpcBackStoryPath))
                {
                    var file = FileAccess.Open(NpcBackStoryPath, FileAccess.ModeFlags.Read);

                    if (file != null)
                    {
                        npcBackStory = file.GetAsText();

                        file.Close();

                        GD.Print("Loaded NPC backstory from file");
                    }
                    else
                    {
                        npcBackStory = "";

                        GD.PrintErr("Could not load NPC backstory file");
                    }
                }
            }
            catch (Exception ex)
            {
                npcBackStory = "";

                GD.PrintErr($"Error loading NPC backstory: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when the node is added to the scene. Initializes UI and loads resources.
        /// </summary>
        public override void _Ready()
        {
            conversationRichTextLabel = GetNode<RichTextLabel>("Background/ConversationRichTextLabel");
            closeButton = GetNode<TextureButton>("Background/CloseButton");
            playerResponseLineEdit = GetNode<LineEdit>("Background/PlayerResponseLineEdit");
            playerResponseButton = GetNode<Button>("Background/PlayerResponseButton");

            conversationRichTextLabel.BbcodeEnabled = true;

            conversationHistory = new List<Message>();

            ollamaService = OllamaService.Instance;

            LoadResponseSchema();
            LoadNpcBackStory();

            responseHandler = new ConversationResponseHandler(EventHandler);

            closeButton.Pressed += () =>
            {
                Visible = false;
            };

            playerResponseButton.Pressed += OnPlayerResponsePressed;
            playerResponseLineEdit.TextSubmitted += OnPlayerResponseSubmitted;
        }

        /// <summary>
        /// Called every frame. Updates the conversation display if needed.
        /// </summary>
        /// <param name="delta">Time since last frame.</param>
        public override void _Process(double delta)
        {
            if (needsDisplayUpdate)
            {
                UpdateConversationDisplay();

                needsDisplayUpdate = false;
            }
        }

        /// <summary>
        /// Handles the player response button press event.
        /// </summary>
        private async void OnPlayerResponsePressed()
        {
            await SendPlayerMessage();
        }

        /// <summary>
        /// Handles the player response line edit submit event.
        /// </summary>
        /// <param name="text">Submitted text.</param>
        private async void OnPlayerResponseSubmitted(string text)
        {
            await SendPlayerMessage();
        }

        protected async System.Threading.Tasks.Task SendPlayerMessageCore(
            string playerMessage,
            Func<System.Threading.Tasks.Task> beforeSend = null,
            Func<System.Threading.Tasks.Task> afterSend = null,
            string modelName = null,
            Func<List<Message>> buildPrompt = null)
        {
            if (isWaitingForResponse || string.IsNullOrWhiteSpace(playerMessage)) return;

            if (beforeSend != null) await beforeSend();

            isWaitingForResponse = true;
            playerResponseLineEdit.Text = "";
            playerResponseLineEdit.Editable = false;
            playerResponseLineEdit.ReleaseFocus();
            playerResponseButton.Disabled = true;

            conversationHistory.Add(new Message
            {
                Role = MessageType.User,
                Content = playerMessage
            });

            needsDisplayUpdate = true;

            conversationHistory.Add(new Message
            {
                Role = MessageType.Assistant,
                Content = ""
            });

            try
            {
                var prompt = buildPrompt != null ? buildPrompt() : conversationHistory;
                var model = modelName ?? "gemma3n:e4b";

                await foreach (var response in ollamaService.FetchAIResponseAsync(
                    prompt,
                    model,
                    responseFormat))
                {
                    if (response.Success)
                    {
                        conversationHistory[^1].Content = response.Reply;

                        if (response.Final && response.StructuredData != null && responseHandler != null)
                        {
                            responseHandler.HandleStructuredResponse(response.StructuredData);
                        }

                        needsDisplayUpdate = true;

                        if (response.Final)
                        {
                            break;
                        }
                    }
                    else
                    {
                        conversationHistory[^1].Content = $"Error: {response.Error}";
                        conversationHistory[^1].Role = MessageType.System;
                        needsDisplayUpdate = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                conversationHistory[^1].Content = $"Error: {ex.Message}";
                conversationHistory[^1].Role = MessageType.System;
                needsDisplayUpdate = true;
                GD.PrintErr($"Error in conversation: {ex.Message}");
            }
            finally
            {
                isWaitingForResponse = false;
                playerResponseLineEdit.Editable = true;
                playerResponseLineEdit.GrabFocus();
                playerResponseButton.Disabled = false;

                if (afterSend != null) await afterSend();
            }
        }


        /// <summary>
        /// Sends the player's message to the conversation and fetches the AI response.
        /// </summary>
        protected virtual async System.Threading.Tasks.Task SendPlayerMessage()
        {
            var playerMessage = playerResponseLineEdit.Text.Trim();

            // NOTE: gemma3n:e4b failed to run on Jetson Nano initially due to insufficient memory. 
            // The device’s physical RAM (~7.4 GB) plus zram swap (~3.8 GB compressed RAM swap) was not enough to handle the model’s large memory demands (which can require 16+ GB RAM). 
            // This caused the Linux OOM killer to terminate the Ollama process during model loading. After adding an 8 GB on-disk swap file, the total available swap increased to ~11.8 GB, improving stability.
            await SendPlayerMessageCore(
                playerMessage,
                beforeSend: null,
                afterSend: null,
                modelName: "gemma3n:e4b",
                buildPrompt: () => conversationHistory.GetRange(0, conversationHistory.Count - 1)
            );
        }

        /// <summary>
        /// Updates the conversation display with the current message history.
        /// </summary>
        private void UpdateConversationDisplay()
        {
            var conversationText = "";

            foreach (var message in conversationHistory)
            {
                if (message.Role == MessageType.User)
                {
                    conversationText += $"[color=#{UserMessageColor.ToHtml()}]You:[/color] {ProcessFormatting(message.Content)}\n\n";
                }
                else if (message.Role == MessageType.Assistant)
                {
                    string displayContent = ExtractMessageFromPartialJson(message.Content);

                    conversationText += $"[color=#{NpcMessageColor.ToHtml()}]{NpcName}:[/color] {ProcessFormatting(displayContent)}\n\n";
                }
            }

            conversationRichTextLabel.Text = conversationText;

            CallDeferred(nameof(ScrollToBottom));
        }

        /// <summary>
        /// Extracts the message from a partial JSON response, handling structured and unstructured formats.
        /// </summary>
        /// <param name="content">Content string from the AI response.</param>
        /// <returns>Extracted message string.</returns>
        private string ExtractMessageFromPartialJson(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return "";
            }

            if (content.Trim().StartsWith("{") && content.Trim().EndsWith("}"))
            {
                try
                {
                    var structuredResponse = JsonConvert.DeserializeObject<StructuredResponse>(content);
                    if (structuredResponse?.Message != null)
                    {
                        return structuredResponse.Message;
                    }
                }
                catch { }
            }

            if (content.Contains("\"message\":\""))
            {
                var messageStart = content.IndexOf("\"message\":\"") + "\"message\":\"".Length;
                var messageEnd = content.Length;
                var inEscape = false;

                for (int i = messageStart; i < content.Length; i++)
                {
                    if (inEscape)
                    {
                        inEscape = false;
                        continue;
                    }

                    if (content[i] == '\\')
                    {
                        inEscape = true;
                        continue;
                    }

                    if (content[i] == '"')
                    {
                        messageEnd = i;
                        break;
                    }
                }

                if (messageEnd > messageStart)
                {
                    var extractedMessage = content[messageStart..messageEnd];

                    extractedMessage = extractedMessage.Replace("\\\"", "\"")
                                                     .Replace("\\n", "\n")
                                                     .Replace("\\r", "\r")
                                                     .Replace("\\t", "\t")
                                                     .Replace("\\\\", "\\");

                    return extractedMessage;
                }
            }

            if (content.Trim().StartsWith("{"))
            {
                return "";
            }

            return content;
        }

        /// <summary>
        /// Processes formatting in the text, converting markdown-like syntax to BBCode.
        /// </summary>
        /// <param name="text">Input text.</param>
        /// <returns>Formatted text.</returns>
        private string ProcessFormatting(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // for Bold
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\*\*([^\*]+)\*\*", "[b]$1[/b]");
            // for Italics
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\*([^\*]+)\*", "[i]$1[/i]");

            return text;
        }

        /// <summary>
        /// Scrolls the conversation display to the bottom.
        /// </summary>
        private void ScrollToBottom()
        {
            conversationRichTextLabel.ScrollToLine(conversationRichTextLabel.GetLineCount() - 1);
        }

        /// <summary>
        /// Gets the pronoun string for the NPC based on the selected type.
        /// </summary>
        /// <returns>Pronoun string.</returns>
        protected string GetPronounString()
        {
            return NpcPronouns switch
            {
                NpcPronounType.HeHim => "he/him",
                NpcPronounType.SheHer => "she/her",
                NpcPronounType.TheyThem => "they/them",
                _ => "they/them"
            };
        }

        /// <summary>
        /// Starts a new conversation or continues an existing one.
        /// Initializes the system prompt and NPC initial message.
        /// </summary>
        public virtual void StartConversation()
        {
            if (conversationHistory.Count == 0)
            {
                if (!string.IsNullOrWhiteSpace(npcBackStory))
                {
                    var systemPrompt = $"Your name is {NpcName}. Your pronouns are {GetPronounString()}. {npcBackStory}";

                    conversationHistory.Add(new Message
                    {
                        Role = MessageType.System,
                        Content = systemPrompt
                    });
                }

                conversationHistory.Add(new Message
                {
                    Role = MessageType.Assistant,
                    Content = JsonConvert.SerializeObject(new StructuredResponse
                    {
                        Message = NpcInitialMessage,
                        Signal = new SignalData { Type = OTHER_SIGNAL }
                    })
                });

                GD.Print("Started new conversation");
            }
            else
            {
                GD.Print("Continuing existing conversation");
            }

            needsDisplayUpdate = true;
            playerResponseLineEdit.GrabFocus();
        }
    }
}