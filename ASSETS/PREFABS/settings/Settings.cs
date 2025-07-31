/************************************************************************
 *    Copyright (C) 2025 Code Forge Temple                              *
 *    This file is part of local-llm-npc project                        *
 *    See the LICENSE file in the project root for license details.     *
 ************************************************************************/

using Godot;
using LllmNpcConversationSystem;
using LllmNpcConversationSystem.Services;
using System;

/// <summary>
/// Settings UI component for configuring Ollama host, subject area, and Gemma model usage.
/// </summary>
public partial class Settings : NinePatchRect
{
    [Export]
    private TextureButton openSettingsButton; // Button to open the settings panel

    private LineEdit ollamaHostLineEdit;      // Input field for Ollama host URL
    private GameData gameData;                // Singleton holding game settings
    private Button saveButton;                // Button to save settings
    private TextureButton closeButton;        // Button to close the settings panel
    private OptionButton subjectOptionButton; // Dropdown for selecting subject area
    private CheckBox gemmaModelCheckBox;      // Checkbox for Gemma model usage

    /// <summary>
    /// Called when the node is added to the scene. Initializes UI and loads settings.
    /// </summary>
    public override void _Ready()
    {
        ollamaHostLineEdit = GetNode<LineEdit>("OllamaHostLineEdit");
        subjectOptionButton = GetNode<OptionButton>("SubjectOptionButton");
        gemmaModelCheckBox = GetNode<CheckBox>("GemmaModelCheckBox");
        closeButton = GetNode<TextureButton>("CloseButton");

        // Connect close button to hide settings
        closeButton.Pressed += HideSettings;

        if (openSettingsButton == null)
        {
            GD.PrintErr("OpenSettingsButton is not set in Settings.");
            return;
        }

        // Connect open button to show settings
        openSettingsButton.Pressed += ShowSettings;

        saveButton = GetNode<Button>("SaveButton");
        saveButton.Pressed += OnSaveButtonPressed;

        gameData = GameData.getInstance();

        // Populate subject dropdown with enum values
        subjectOptionButton.Clear();
        foreach (var subject in Enum.GetValues(typeof(SubjectArea)))
        {
            subjectOptionButton.AddItem(subject.ToString());
        }

        // Load saved settings into UI
        ollamaHostLineEdit.Text = gameData.ollamaHostUrl;
        subjectOptionButton.Selected = gameData.EducationalSubject;
        gemmaModelCheckBox.ButtonPressed = gameData.UseGemma3nLatest;

        // Set Ollama service host
        OllamaService.Instance.SetHost(gameData.ollamaHostUrl);

        // Show settings if host URL is not set, otherwise hide
        if (string.IsNullOrEmpty(gameData.ollamaHostUrl))
        {
            ShowSettings();
        }
        else
        {
            HideSettings();
        }
    }

    /// <summary>
    /// Saves settings from UI to GameData and hides the settings panel.
    /// </summary>
    private void OnSaveButtonPressed()
    {
        gameData.ollamaHostUrl = ollamaHostLineEdit.Text.Trim();
        gameData.EducationalSubject = subjectOptionButton.Selected;
        gameData.UseGemma3nLatest = gemmaModelCheckBox.ButtonPressed;

        OllamaService.Instance.SetHost(gameData.ollamaHostUrl);

        gameData.Save();

        HideSettings();

        GD.Print("Settings saved successfully.");
    }

    /// <summary>
    /// Shows the settings panel and pauses the game.
    /// </summary>
    private void ShowSettings()
    {
        Visible = true;
        GetTree().Paused = true;
    }

    /// <summary>
    /// Hides the settings panel and resumes the game.
    /// </summary>
    private void HideSettings()
    {
        Visible = false;
        GetTree().Paused = false;
    }
}