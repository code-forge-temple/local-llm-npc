/************************************************************************
 *    Copyright (C) 2025 Code Forge Temple                              *
 *    This file is part of local-llm-npc project                        *
 *    See the LICENSE file in the project root for license details.     *
 ************************************************************************/

using Godot;
using System;
using System.Text.Json;

/// <summary>
/// Manages persistent game settings and data using Godot's FileAccess and JSON serialization.
/// Implements a singleton pattern for global access.
/// </summary>
public partial class GameData : Node
{
    /// <summary>
    /// Serializable container for game settings.
    /// </summary>
    private GameDataSerializable data;

    /// <summary>
    /// Path to the JSON file storing game data.
    /// </summary>
    static readonly string GAME_DATA_FILE = "user://gamedata.json";

    /// <summary>
    /// Singleton instance of GameData.
    /// </summary>
    private static GameData instance;

    /// <summary>
    /// Gets or sets the Ollama host URL.
    /// </summary>
    public string ollamaHostUrl { get => data.ollamaHostUrl; set => data.ollamaHostUrl = value; }

    /// <summary>
    /// Gets or sets the educational subject.
    /// </summary>
    public int EducationalSubject { get => data.educationalSubject; set => data.educationalSubject = value; }

    /// <summary>
    /// Gets or sets whether to use Gemma3n latest version.
    /// </summary>
    public bool UseGemma3nLatest { get => data.useGemma3nLatest; set => data.useGemma3nLatest = value; }

    /// <summary>
    /// Returns the singleton instance of GameData.
    /// </summary>
    public static GameData getInstance()
    {
        if (instance == null)
        {
            instance = new GameData();
        }
        return instance;
    }

    /// <summary>
    /// Private constructor initializes data and loads from file.
    /// </summary>
    private GameData()
    {
        data = new GameDataSerializable();
        Load();
    }

    /// <summary>
    /// Saves the current game data to disk as JSON.
    /// </summary>
    public void Save()
    {
        // Add this to the Save method to debug the path:
        string userDir = ProjectSettings.GlobalizePath("user://");
        string actualPath = userDir + "gamedata.json";
        GD.Print($"Saving to actual path: {actualPath}");

        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(data, options);

        using var file = Godot.FileAccess.Open(GAME_DATA_FILE, Godot.FileAccess.ModeFlags.Write);
        file.StoreString(jsonString);
    }

    /// <summary>
    /// Loads game data from disk if the file exists.
    /// </summary>
    public void Load()
    {
        if (Godot.FileAccess.FileExists(GAME_DATA_FILE))
        {
            using var file = Godot.FileAccess.Open(GAME_DATA_FILE, Godot.FileAccess.ModeFlags.Read);
            string jsonString = file.GetAsText();
            data = JsonSerializer.Deserialize<GameDataSerializable>(jsonString);
        }
    }

    /// <summary>
    /// Serializable class for storing game settings.
    /// </summary>
    [Serializable]
    public class GameDataSerializable
    {
        /// <summary>
        /// Ollama host URL.
        /// </summary>
        public string ollamaHostUrl { get; set; }

        /// <summary>
        /// Educational subject.
        /// </summary>
        public int educationalSubject { get; set; }

        /// <summary>
        /// Use Gemma3n latest version.
        /// </summary>
        public bool useGemma3nLatest { get; set; }

        /// <summary>
        /// Initializes default values.
        /// </summary>
        public GameDataSerializable()
        {
            ollamaHostUrl = "";
            educationalSubject = 0;
            useGemma3nLatest = true;
        }
    }
}