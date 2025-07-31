/************************************************************************
 *    Copyright (C) 2025 Code Forge Temple                              *
 *    This file is part of local-llm-npc project                        *
 *    See the LICENSE file in the project root for license details.     *
 ************************************************************************/

using Godot;
using System;
using LllmNpcConversationSystem.Services.Types;
using LllmNpcConversationSystem;

/// <summary>
/// Handles responses from the conversation system and dispatches signals to the event handler.
/// </summary>
public partial class ConversationResponseHandler : RefCounted
{
    private ConversationEventHandler eventHandler;

    /// <summary>
    /// Initializes a new instance of the ConversationResponseHandler class.
    /// </summary>
    /// <param name="eventHandler">The event handler to dispatch signals to.</param>
    public ConversationResponseHandler(ConversationEventHandler eventHandler)
    {
        this.eventHandler = eventHandler;
    }

    /// <summary>
    /// Handles a structured response by checking its signal and dispatching it if valid.
    /// </summary>
    /// <param name="response">The structured response containing the signal.</param>
    public void HandleStructuredResponse(StructuredResponse response)
    {
        if (response?.Signal == null || string.IsNullOrEmpty(response.Signal.Type)) return;

        string signalType = response.Signal.Type;
        dynamic data = response.Signal.Data;

        GD.Print($"Received signal: {signalType}");

        if (signalType == Conversation.OTHER_SIGNAL) return;

        ProcessSignal(signalType, data);
    }

    /// <summary>
    /// Processes the signal by invoking the corresponding method on the event handler.
    /// </summary>
    /// <param name="signal">The type of signal to process.</param>
    /// <param name="data">The data associated with the signal.</param>
    private void ProcessSignal(string signal, dynamic data)
    {
        if (eventHandler != null)
        {
            string methodName = "On" + ToCamelCase(signal);

            try
            {
                var eventHandlerType = eventHandler.GetType();
                var method = eventHandlerType.GetMethod(methodName);

                method.Invoke(eventHandler, [data]);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error invoking method {methodName}: {ex.Message}");
            }
        }
        else
        {
            GD.PrintErr($"No EventHandler set for conversation - signal '{signal}' processed but no action taken");
        }
    }

    /// <summary>
    /// Converts a string from snake_case to CamelCase.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>The CamelCase version of the input text.</returns>
    private string ToCamelCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var words = text.Split('_');
        var result = "";

        foreach (var word in words)
        {
            if (!string.IsNullOrEmpty(word))
            {
                result += char.ToUpper(word[0]) + word[1..].ToLower();
            }
        }

        return result;
    }
}