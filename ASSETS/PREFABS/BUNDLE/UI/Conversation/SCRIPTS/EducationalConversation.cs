/************************************************************************
 *    Copyright (C) 2025 Code Forge Temple                              *
 *    This file is part of local-llm-npc project                        *
 *    See the LICENSE file in the project root for license details.     *
 ************************************************************************/

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using LllmNpcConversationSystem.Services.Types;
using Newtonsoft.Json;

namespace LllmNpcConversationSystem
{
    /// <summary>
    /// Represents the learning difficulty level for educational conversations.
    /// </summary>
    public enum LearningDifficulty
    {
        Beginner,
        Intermediate,
        Advanced
    }

    /// <summary>
    /// Represents the subject area for educational conversations.
    /// </summary>
    public enum SubjectArea
    {
        SustainableFarming,
        PlantBiology,
        SoilScience,
        WaterManagement,
        Composting,
        Permaculture
    }

    /// <summary>
    /// Handles an educational conversation with the player, tracking progress, adapting difficulty, and managing topics.
    /// </summary>
    public partial class EducationalConversation : Conversation
    {
        /// <summary>
        /// The current subject area of the conversation.
        /// </summary>
        public SubjectArea Subject { get; set; } = SubjectArea.SustainableFarming;

        /// <summary>
        /// Determines whether to use the latest Gemma 3n model.
        /// </summary>
        private bool useGemma3nLatest { get; set; } = true;

        /// <summary>
        /// The current learning difficulty level.
        /// </summary>
        [Export]
        public LearningDifficulty CurrentDifficulty { get; set; } = LearningDifficulty.Beginner;

        /// <summary>
        /// Number of learning sessions completed.
        /// </summary>
        private int learningSessionCount = 0;

        /// <summary>
        /// Tracks learning progress and completed topics.
        /// </summary>
        private LearningProgressTracker progressTracker;

        /// <summary>
        /// List of completed topics in the current session.
        /// </summary>
        private readonly List<string> completedTopics = [];

        /// <summary>
        /// The current learning objective.
        /// </summary>
        private string currentLearningObjective = "";

        /// <summary>
        /// Tracks how often topics are mentioned.
        /// </summary>
        private readonly Dictionary<string, int> topicMentionCount = [];

        /// <summary>
        /// Number of assessments completed.
        /// </summary>
        public int AssessmentsCompleted { get; private set; } = 0;

        /// <summary>
        /// Number of checkpoints reached.
        /// </summary>
        public int CheckpointsReached { get; private set; } = 0;

        /// <summary>
        /// Called when the node is added to the scene. Initializes progress tracker and sets up visibility change handler.
        /// </summary>
        public override void _Ready()
        {
            NpcInitialMessage = NpcInitialMessage.Length > 0 ? NpcInitialMessage : GetEducationalInitialMessage();

            progressTracker = new LearningProgressTracker();

            VisibilityChanged += OnVisibilityChanged;

            base._Ready();
        }

        private int lastSubjectIndex = -1;
        private bool lastUseGemma3nLatest = true;

        /// <summary>
        /// Handles logic when the conversation UI visibility changes, such as subject or model updates.
        /// </summary>
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (!Visible) return;

            var gameData = GameData.getInstance();

            bool subjectChanged = false;

            if (gameData.EducationalSubject != lastSubjectIndex)
            {
                Subject = (SubjectArea)gameData.EducationalSubject;
                lastSubjectIndex = gameData.EducationalSubject;
                subjectChanged = true;

                GD.Print($"EducationalConversation: Subject changed to {Subject}");
            }

            if (gameData.UseGemma3nLatest != lastUseGemma3nLatest)
            {
                useGemma3nLatest = gameData.UseGemma3nLatest;
                lastUseGemma3nLatest = gameData.UseGemma3nLatest;

                GD.Print($"EducationalConversation: UseGemma3nLatest changed to {useGemma3nLatest}");
            }

            if (subjectChanged)
            {
                conversationHistory.Clear();
                ResetEducationalProgress();
                NpcInitialMessage = GetEducationalInitialMessage();
                needsDisplayUpdate = true;

                GD.Print("EducationalConversation: Conversation and progress reset due to subject change.");
            }
        }

        /// <summary>
        /// Returns the initial message for the NPC based on the current subject area.
        /// </summary>
        private string GetEducationalInitialMessage()
        {
            return Subject switch
            {
                SubjectArea.SustainableFarming => $"Welcome to the learning garden! I'm {NpcName}, your agricultural mentor. I see you're interested in sustainable farming practices. What would you like to explore first - soil health, crop rotation, or perhaps companion planting?",
                SubjectArea.PlantBiology => "Greetings! Ready to dive deep into how plants actually work? We can start with photosynthesis, plant anatomy, or how plants communicate with each other!",
                SubjectArea.SoilScience => "The foundation of all life starts beneath our feet! Let's explore the fascinating world of soil - from microorganisms to nutrient cycles.",
                SubjectArea.WaterManagement => "Water is life in the garden. Shall we learn about efficient irrigation, rainwater harvesting, or drought-resistant growing techniques?",
                SubjectArea.Composting => "Let's turn waste into garden gold! Composting is nature's way of recycling. Where would you like to begin?",
                SubjectArea.Permaculture => "Welcome to permaculture - designing agricultural systems that work with nature. Ready to learn about sustainable design principles?",
                _ => "Welcome to our learning garden! What agricultural topic interests you most today?"
            };
        }

        /// <summary>
        /// Sends the player's message to the AI, tracks progress, and updates the conversation.
        /// </summary>
        protected virtual async System.Threading.Tasks.Task SendEducationalPlayerMessage()
        {
            var playerMessage = playerResponseLineEdit.Text.Trim();

            await SendPlayerMessageCore(
                playerMessage,
                beforeSend: async () =>
                {
                    progressTracker?.RecordInteraction(playerMessage, Subject.ToString());
                    AdjustDifficultyIfNeeded(playerMessage);
                    TrackTopicMentions(playerMessage);
                    await System.Threading.Tasks.Task.CompletedTask;
                },
                afterSend: async () =>
                {
                    learningSessionCount++;
                    await System.Threading.Tasks.Task.CompletedTask;
                },
                modelName: useGemma3nLatest ? "gemma3n:e4b" : "gemma3n:e2b",
                buildPrompt: BuildEducationalPrompt
            );
        }

        /// <summary>
        /// Builds the prompt for the AI model, including context and learning progress.
        /// </summary>
        private List<Message> BuildEducationalPrompt()
        {
            var educationalPrompt = new List<Message>(conversationHistory);

            if (educationalPrompt.Count > 0 && educationalPrompt[0].Role == MessageType.System)
            {
                educationalPrompt[0].Content = $"{GetBaseEducationalSystemPrompt()}\n\nCurrent Learning Context:\n" +
                    $"- Subject Focus: {Subject}\n" +
                    $"- Student Level: {CurrentDifficulty}\n" +
                    $"- Topics Mastered: {string.Join(", ", completedTopics)}\n" +
                    $"- Session Count: {learningSessionCount}\n" +
                    $"- Current Objective: {currentLearningObjective}\n" +
                    $"- Frequently Mentioned Topics: {GetFrequentTopics()}\n\n" +
                    "IMPORTANT: Adapt your teaching style to the learner's demonstrated level. " +
                    "Use practical examples from the garden environment. Ask follow-up questions to assess understanding. " +
                    "Provide hands-on activities when appropriate. Always respond with the structured JSON format.";
            }

            return educationalPrompt.GetRange(0, educationalPrompt.Count - 1);
        }

        /// <summary>
        /// Adds a topic to the list of completed topics if not already present.
        /// </summary>
        public void AddCompletedTopic(string topicName)
        {
            if (!string.IsNullOrEmpty(topicName) && !completedTopics.Contains(topicName))
            {
                completedTopics.Add(topicName);

                GD.Print($"🎯 Topic added to educational conversation: {topicName}");
            }
        }

        /// <summary>
        /// Gets the current progress tracker.
        /// </summary>
        public LearningProgressTracker GetProgressTracker() => progressTracker;

        /// <summary>
        /// Gets a copy of the completed topics list.
        /// </summary>
        public List<string> GetCompletedTopics() => completedTopics.ToList();

        /// <summary>
        /// Gets the number of learning sessions completed.
        /// </summary>
        public int GetLearningSessionCount() => learningSessionCount;

        /// <summary>
        /// Tracks mentions of key topics in the player's message.
        /// </summary>
        private void TrackTopicMentions(string message)
        {
            var topics = new[] { "soil", "water", "h2o", "co2", "carbon dioxide", "nitrogen", "compost", "plants", "seeds", "pest", "organic", "sustainable", "permaculture", "ph" };

            foreach (var topic in topics)
            {
                if (message.ToLower().Contains(topic))
                {
                    if (!topicMentionCount.ContainsKey(topic))
                    {
                        topicMentionCount[topic] = 0;
                    }

                    topicMentionCount[topic]++;
                }
            }
        }

        /// <summary>
        /// Returns a string of the three most frequently mentioned topics.
        /// </summary>
        private string GetFrequentTopics()
        {
            return string.Join(", ", topicMentionCount
                .OrderByDescending(kvp => kvp.Value)
                .Take(3)
                .Select(kvp => $"{kvp.Key}({kvp.Value})"));
        }

        /// <summary>
        /// Adjusts the learning difficulty based on player progress and message complexity.
        /// </summary>
        private void AdjustDifficultyIfNeeded(string playerMessage)
        {
            var wordCount = playerMessage.Split(' ').Length;
            var hasComplexTerms = ContainsComplexTerms(playerMessage);

            if (learningSessionCount > 5 && wordCount > 15 && hasComplexTerms && CurrentDifficulty == LearningDifficulty.Beginner)
            {
                CurrentDifficulty = LearningDifficulty.Intermediate;

                GD.Print("🌱 Difficulty adjusted to Intermediate - student showing progress!");
            }
            else if (learningSessionCount > 12 && wordCount > 20 && CurrentDifficulty == LearningDifficulty.Intermediate)
            {
                CurrentDifficulty = LearningDifficulty.Advanced;

                GD.Print("🌿 Difficulty adjusted to Advanced - excellent learning progress!");
            }
        }

        /// <summary>
        /// Checks if the player's message contains complex terms.
        /// </summary>
        private bool ContainsComplexTerms(string message)
        {
            var complexTerms = new[] {
                "photosynthesis", "nitrogen", "h2o", "co2", "carbon dioxide", "ph", "microorganisms", "symbiosis",
                "mycorrhizae", "permaculture", "biodiversity", "sustainability",
                "composting", "nutrients", "ecosystem"
            };

            return complexTerms.Any(term => message.ToLower().Contains(term));
        }

        /// <summary>
        /// Returns the base system prompt for the educational AI.
        /// </summary>
        private string GetBaseEducationalSystemPrompt()
        {
            return $"Your name is {NpcName}. Your pronouns are {GetPronounString()}. {npcBackStory}\n\n" +
                        $"EDUCATIONAL CONTEXT: You are teaching {Subject} at {CurrentDifficulty} level. " +
                        $"This is session #{learningSessionCount + 1}. Focus on hands-on, practical learning in a garden environment. " +
                        $"Use Gemma 3n's on-device capabilities to provide private, personalized education.";
        }

        /// <summary>
        /// Starts the educational conversation, initializing prompts and session state.
        /// </summary>
        public override void StartConversation()
        {
            if (conversationHistory.Count == 0)
            {
                if (!string.IsNullOrWhiteSpace(npcBackStory))
                {
                    var educationalSystemPrompt = GetBaseEducationalSystemPrompt();

                    conversationHistory.Add(new Message
                    {
                        Role = MessageType.System,
                        Content = educationalSystemPrompt
                    });
                }

                conversationHistory.Add(new Message
                {
                    Role = MessageType.Assistant,
                    Content = JsonConvert.SerializeObject(new StructuredResponse
                    {
                        Message = NpcInitialMessage,
                        Signal = new SignalData { Type = "" }
                    })
                });

                GD.Print($"🌱 Started educational conversation: {Subject} at {CurrentDifficulty} level");
            }
            else
            {
                GD.Print("📚 Continuing educational session - building on previous learning");
            }

            needsDisplayUpdate = true;
            playerResponseLineEdit.GrabFocus();
        }

        /// <summary>
        /// Increments the number of completed assessments.
        /// </summary>
        public void IncrementAssessmentsCompleted()
        {
            AssessmentsCompleted++;
        }

        /// <summary>
        /// Increments the number of checkpoints reached.
        /// </summary>
        public void IncrementCheckpointsReached()
        {
            CheckpointsReached++;
        }

        /// <summary>
        /// Resets all educational progress and session state.
        /// </summary>
        public void ResetEducationalProgress()
        {
            AssessmentsCompleted = 0;
            CheckpointsReached = 0;
            completedTopics.Clear();
            learningSessionCount = 0;
            currentLearningObjective = "";
            topicMentionCount.Clear();
            progressTracker = new LearningProgressTracker();

            GD.Print("🔄 EducationalConversation: Progress reset");
        }

        /// <summary>
        /// Sends the player's message using the educational message handler.
        /// </summary>
        protected override async System.Threading.Tasks.Task SendPlayerMessage()
        {
            await SendEducationalPlayerMessage();
        }
    }

    /// <summary>
    /// Tracks learning progress, completed topics, and interactions for educational sessions.
    /// </summary>
    public class LearningProgressTracker
    {
        /// <summary>
        /// Stores completed topics by subject.
        /// </summary>
        private readonly Dictionary<string, List<string>> completedTopicsBySubject = [];

        /// <summary>
        /// Stores all interactions as timestamped strings.
        /// </summary>
        private readonly List<string> interactions = [];

        /// <summary>
        /// Stores the last completion time for each topic.
        /// </summary>
        private readonly Dictionary<string, DateTime> lastTopicTime = [];

        /// <summary>
        /// Records a player interaction for a given subject.
        /// </summary>
        public void RecordInteraction(string message, string subject)
        {
            var timestamp = DateTime.Now;

            interactions.Add($"{timestamp:HH:mm:ss} [{subject}]: {message}");

            GD.Print($"📝 Learning interaction recorded: {subject} - {message[..Math.Min(50, message.Length)]}...");
        }

        /// <summary>
        /// Marks a topic as completed for a given subject.
        /// </summary>
        public void MarkTopicCompleted(string topic, string subject)
        {
            if (!completedTopicsBySubject.ContainsKey(subject))
            {
                completedTopicsBySubject[subject] = [];
            }

            if (!completedTopicsBySubject[subject].Contains(topic))
            {
                completedTopicsBySubject[subject].Add(topic);
                lastTopicTime[topic] = DateTime.Now;

                GD.Print($"✅ Topic mastered: {topic} in {subject}");
            }
        }

        /// <summary>
        /// Gets the list of completed topics for a given subject.
        /// </summary>
        public List<string> GetCompletedTopics(string subject)
        {
            return completedTopicsBySubject.TryGetValue(subject, out List<string> value)
                ? value : [];
        }
    }
}