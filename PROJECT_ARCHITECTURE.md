# Project Architecture

This document describes the core files and their responsibilities in the conversation system.

## Main Components

- **Conversation.cs**  
  Handles the core logic for NPC conversations, signal dispatching, and state management.

- **EducationalConversation.cs**  
  Manages educational content, tracks progress, and interfaces with learning modules.

- **ConversationStarter.cs**  
  Initializes and triggers new conversations with NPCs.

- **ConversationEventHandler.cs**  
  Receives conversation signals and triggers UI updates, progress tracking, and content unlocking.

- **ConversationResponseHandler.cs**  
  Processes structured responses from the AI and routes signals to the appropriate event handler methods.

- **OllamaService.cs**  
  Provides communication with the Ollama server, sending prompts and receiving AI responses.

- **GameData.cs**  
  Manages persistent game data, including Ollama host configuration and educational subject selection.

- **npcBackStory.txt**  
  Contains the NPC’s background story, used for context in conversations.

- **npcResponseSchema.json**  
  Defines the expected schema for AI responses, ensuring structured data exchange.

## File Locations

```plaintext
ASSETS/
├── SCRIPTS/
│   └── Global/
│       └── GameData.cs
└── PREFABS/
    └── BUNDLE/
        ├── ConversationStarter/
        │   └── SCRIPTS/
        │       └── ConversationStarter.cs
        └── UI/
            └── Conversation/
                ├── SCRIPTS/
                │   ├── Conversation.cs
                │   ├── EducationalConversation.cs
                │   ├── ConversationResponseHandler.cs
                │   └── OllamaService/
                │       └── OllamaService.cs
                └── TEMPLATES/
                    ├── ConversationEventHandler.cs
                    ├── npcBackStory.txt
                    └── npcResponseSchema.json
```

**Linked files:**
- [GameData.cs](ASSETS/SCRIPTS/Global/GameData.cs)
- [ConversationStarter.cs](ASSETS/PREFABS/BUNDLE/ConversationStarter/SCRIPTS/ConversationStarter.cs)
- [Conversation.cs](ASSETS/PREFABS/BUNDLE/UI/Conversation/SCRIPTS/Conversation.cs)
- [EducationalConversation.cs](ASSETS/PREFABS/BUNDLE/UI/Conversation/SCRIPTS/EducationalConversation.cs)
- [ConversationResponseHandler.cs](ASSETS/PREFABS/BUNDLE/UI/Conversation/SCRIPTS/ConversationResponseHandler.cs)
- [OllamaService.cs](ASSETS/PREFABS/BUNDLE/UI/Conversation/SCRIPTS/OllamaService/OllamaService.cs)
- [ConversationEventHandler.cs](ASSETS/PREFABS/BUNDLE/UI/Conversation/TEMPLATES/ConversationEventHandler.cs)
- [npcBackStory.txt](ASSETS/PREFABS/BUNDLE/UI/Conversation/TEMPLATES/npcBackStory.txt)
- [npcResponseSchema.json](ASSETS/PREFABS/BUNDLE/UI/Conversation/TEMPLATES/npcResponseSchema.json)