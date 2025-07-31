/************************************************************************
 *    Copyright (C) 2025 Code Forge Temple                              *
 *    This file is part of local-llm-npc project                        *
 *    See the LICENSE file in the project root for license details.     *
 ************************************************************************/

using Godot;
using System;
using LllmNpcConversationSystem;
using Newtonsoft.Json;

public interface IConversationEventHandler
{
    void OnLearningCheckpoint(dynamic data);        // learning_checkpoint
    void OnTopicCompleted(dynamic data);            // topic_completed
    void OnAssessmentQuestion(dynamic data);        // assessment_question
    void OnEncouragement(dynamic data);             // encouragement
    void OnDifficultyAdjustment(dynamic data);      // difficulty_adjustment
    void OnObservationPrompt(dynamic data);         // observation_prompt
    void OnReflectionMoment(dynamic data);          // reflection_moment
    void OnGardenInteraction(dynamic data);         // garden_interaction
    void OnKnowledgeUnlocked(dynamic data);         // knowledge_unlocked
}


/// <summary>
/// Handles conversation events and updates the UI and educational progress accordingly.
/// </summary>
public partial class ConversationEventHandler : Node, IConversationEventHandler
{
    /// <summary>
    /// Label displaying the user's progress.
    /// </summary>
    [Export]
    private Label progressLabel;

    /// <summary>
    /// Reference to the EducationalConversation instance managing progress and state.
    /// </summary>
    [Export]
    private EducationalConversation educationalConversation;

    /// <summary>
    /// Called when the node is added to the scene. Initializes references and updates progress display.
    /// </summary>
    public override void _Ready()
    {
        if (progressLabel == null)
        {
            GD.PrintErr("Progress label is not assigned in the inspector!");
            return;
        }

        if (educationalConversation == null)
        {
            GD.PrintErr("EducationalConversationRef is not assigned in the inspector!");
            return;
        }

        UpdateProgressDisplay();
    }

    /// <summary>
    /// Handles the learning checkpoint event.
    /// </summary>
    public void OnLearningCheckpoint(dynamic data)
    {
        GD.Print($"📍 Learning checkpoint reached: {JsonConvert.SerializeObject(data)}");
        educationalConversation.IncrementCheckpointsReached();
        UpdateProgressDisplay();
    }

    /// <summary>
    /// Handles the topic completed event.
    /// </summary>
    public void OnTopicCompleted(dynamic data)
    {
        var topicData = data?.topic_name;
        string topic = topicData != null ? topicData.ToString() : "(unknown)";

        GD.Print($"✅ Topic completed: {topic}");

        educationalConversation.AddCompletedTopic(topic);
        educationalConversation.GetProgressTracker()?.MarkTopicCompleted(topic, educationalConversation.Subject.ToString());

        GD.Print($"🎯 New topic mastered: {topic}");

        UpdateProgressDisplay();
        UnlockNewContent();
    }

    /// <summary>
    /// Handles the assessment question event.
    /// </summary>
    public void OnAssessmentQuestion(dynamic data)
    {
        GD.Print($"📝 Assessment question: {JsonConvert.SerializeObject(data)}");
        educationalConversation.IncrementAssessmentsCompleted();
        UpdateProgressDisplay();
    }

    /// <summary>
    /// Handles the encouragement event.
    /// </summary>
    public void OnEncouragement(dynamic data)
    {
        GD.Print($"💪 Encouragement: {JsonConvert.SerializeObject(data)}");
    }

    /// <summary>
    /// Handles the difficulty adjustment event.
    /// </summary>
    public void OnDifficultyAdjustment(dynamic data)
    {
        var level = data?.difficulty_level ?? "";

        GD.Print($"⚡ Difficulty adjustment: {level}");

        if (!string.IsNullOrEmpty(level))
        {
            if (Enum.TryParse<LearningDifficulty>(level, true, out LearningDifficulty difficulty))
            {
                educationalConversation.CurrentDifficulty = difficulty;
                GD.Print($"⚡ AI suggested difficulty adjustment to: {difficulty}");
            }
        }
    }

    /// <summary>
    /// Handles the observation prompt event.
    /// </summary>
    public void OnObservationPrompt(dynamic data)
    {
        GD.Print($"👀 Observation prompt: {JsonConvert.SerializeObject(data)}");
    }

    /// <summary>
    /// Handles the reflection moment event.
    /// </summary>
    public void OnReflectionMoment(dynamic data)
    {
        GD.Print($"🤔 Reflection moment: {JsonConvert.SerializeObject(data)}");
    }

    /// <summary>
    /// Handles the garden interaction event.
    /// </summary>
    public void OnGardenInteraction(dynamic data)
    {
        GD.Print($"🌿 Garden interaction: {JsonConvert.SerializeObject(data)}");
    }

    /// <summary>
    /// Handles the knowledge unlocked event.
    /// </summary>
    public void OnKnowledgeUnlocked(dynamic data)
    {
        GD.Print($"🔓 Knowledge unlocked: {JsonConvert.SerializeObject(data)}");
        UnlockNewContent();
    }

    /// <summary>
    /// Updates the progress label with the current educational progress.
    /// </summary>
    private void UpdateProgressDisplay()
    {
        if (progressLabel != null)
        {
            var topicsCount = educationalConversation.GetCompletedTopics().Count;
            progressLabel.Text = $"🌱 Topics: {topicsCount} | 📝 Assessments: {educationalConversation.AssessmentsCompleted} | 📍 Checkpoints: {educationalConversation.CheckpointsReached}";
        }
    }

    /// <summary>
    /// Unlocks new educational content based on the user's progress.
    /// </summary>
    private void UnlockNewContent()
    {
        GD.Print("🔓 Unlocking new educational content based on progress");

        var topicsCount = educationalConversation.GetCompletedTopics().Count;

        if (topicsCount >= 3)
        {
            GD.Print($"🚀 Advanced learning modules unlocked! Completed topics: {string.Join(", ", educationalConversation.GetCompletedTopics())}");
        }
    }

    /// <summary>
    /// Resets the educational progress for a new session.
    /// </summary>
    public void ResetEducationalProgress()
    {
        educationalConversation.ResetEducationalProgress();
        UpdateProgressDisplay();
        GD.Print("🔄 Educational progress reset for new session");
    }
}