# Local LLM NPC: Private, On-Device Educational NPCs Powered by Gemma 3n

<!-- TOC -->

- [Local LLM NPC: Private, On-Device Educational NPCs Powered by Gemma 3n](#local-llm-npc-private-on-device-educational-npcs-powered-by-gemma-3n)
    - [Overview](#overview)
    - [Features](#features)
    - [Installation](#installation)
        - [Prerequisites](#prerequisites)
            - [Required Software](#required-software)
            - [Ollama Host Installation & Configuration](#ollama-host-installation--configuration)
            - [Gemma 3n Model Installation](#gemma-3n-model-installation)
        - [Steps](#steps)
    - [Presentation Video](#presentation-video)
    - [Project Architecture](#project-architecture)
    - [Attribution](#attribution)
    - [License](#license)

<!-- /TOC -->

## Overview

**local-llm-npc** is an interactive educational game built for the Google Gemma 3n Impact Challenge. It leverages Gemma 3n’s on-device, multimodal AI to deliver
private, offline-first learning experiences in a beautiful garden setting. The project demonstrates how next-generation AI can revolutionize education, accessibility, and sustainability—especially in low-connectivity regions.

- **Impact:** Real-time, personalized agricultural education with privacy-first
  AI.
- **Gemma 3n Integration:** Uses Gemma 3n (via Ollama) for on-device, structured educational conversations.
- **Text-based:** Supports rich, hands-on learning through interactive text dialogue.
- **Offline-Ready:** Runs locally—no internet required for core features.

## Features

- **Educational NPC:** Teaches sustainable farming, botany, and more using Socratic dialogue and hands-on activities.
- **Progress Tracking:** Tracks learning checkpoints, completed topics, and assessments.
- **Customizable AI Host:** Easily set your Ollama/Gemma 3n endpoint in settings.
- **Gemma 3n Model:** Supports dynamic model selection and mix’n’match capabilities (set at build time; not changeable by the player).

## Installation

### Prerequisites

#### Required Software

- **Godot Engine:** Version **4.4.1** (Mono/C# build required)  
  [Download Godot 4.4.1 Mono](https://godotengine.org/download)
- **.NET SDK:** Version **8.0** or higher  
  [Download .NET SDK](https://dotnet.microsoft.com/en-us/download)
- **Ollama:** Installed locally or on a LAN host  
  [Ollama Setup](https://ollama.com/)

#### Ollama Host Installation & Configuration

Ensure that the [Ollama](https://ollama.com) host is installed on your local machine or available on your LAN.

**Linux**

1. Install Ollama on your host ([Ollama Setup](https://ollama.com/)).
2. Edit the systemd service file by running:
   ```sh
   sudo nano /etc/systemd/system/ollama.service
   ```
3. Add the following environment variables in the `[Service]` section:
   ```
   Environment="OLLAMA_HOST=0.0.0.0"
   ```
   > **Note:** The `OLLAMA_HOST=0.0.0.0` setting is optional if the Ollama server is running on localhost and you do not need the Ollama server to be accessed from LAN.

4. Save the file, then reload and restart the service:
   ```sh
   sudo systemctl daemon-reload
   sudo systemctl restart ollama.service
   ```

**Windows**

1. Install Ollama on your host ([Ollama Setup](https://ollama.com/)).
2. On the machine running Ollama, set the environment variables:
   ```
   OLLAMA_HOST=0.0.0.0
   ```
   You can do this via the System Properties or using PowerShell.
   > **Note:** The `OLLAMA_HOST=0.0.0.0` setting is optional if the Ollama server is running on localhost and you do not need the Ollama server to be accessed from LAN.

3. Restart Ollama app.

#### Gemma 3n Model Installation

After Ollama is installed and running, open a terminal and run:

```sh
ollama pull gemma3n:e4b   # for the larger model
ollama pull gemma3n:e2b   # for the smaller model
```

> For **Jetson Orin Nano** and other resource-constrained devices, the larger Gemma 3n model (`gemma3n:e4b`) will require additional swap space.  
> **Recommendation:** For best performance, run Ollama from an SSD rather than an SD card.
>> NOTE: I have tested this setup on a Jetson Orin Nano Super Developer Kit with the OS installed on the SSD + the updated swap space.
>
> To add 8GB swap:
>
> ```sh
> sudo fallocate -l 8G /swapfile        # create 8GB swap file
> sudo chmod 600 /swapfile              # restrict permissions
> sudo mkswap /swapfile                 # mark file as swap
> sudo swapon /swapfile                 # enable swap immediately
> swapon --show                         # verify swap is active
> sudo nano /etc/fstab                  # add '/swapfile swap swap defaults 0 0' line to auto-enable at boot
> ```
>
> To install OS on the SSD:
>
> See [installation instructions here](https://youtu.be/BaRdpSXU6EM?si=ERLC0FnS_OZqxLR6&t=1407).


Set your Ollama host URL in the game settings (e.g., `http://localhost:11434` or the network IP where the Ollama is installed).

### Steps

**Option 1: Build from Source**

1. **Clone the Repository**
   ```sh
   git clone https://github.com/code-forge-temple/local-llm-npc.git
   cd local-llm-npc
   ```

2. **Restore .NET Dependencies**
   ```sh
   dotnet restore
   ```

3. **Open the Project in Godot**
   - Launch Godot 4.4.1 (Mono/C#).
   - Open the project folder.

4. **Build and Run**
   - Press **Play** in the Godot editor.
   - Configure your Ollama host URL in the game

**Option 2: Run Prebuilt Executable**

1. **Clone the Repository**
   ```sh
   git clone https://github.com/code-forge-temple/local-llm-npc.git
   cd local-llm-npc
   ```
2. **Run the Executable**
   - For Windows:  
     Open `BIN/WINDOWS/local-llm-npc (4.4).exe`
   - For Linux:  
     Open `BIN/LINUX/local-llm-npc (4.4).x86_64`


## Presentation Video

Watch the project presentation here:

[![kGyafSgyRWA](https://img.youtube.com/vi/kGyafSgyRWA/0.jpg)](https://www.youtube.com/watch?v=kGyafSgyRWA)

## Project Architecture

See [PROJECT_ARCHITECTURE.md](PROJECT_ARCHITECTURE.md) for a detailed overview of the main components and their responsibilities.


## Attribution

Sprites used in this project are from [Kenney's asset store](https://kenney.nl/assets), released under [Creative Commons CC0](https://creativecommons.org/publicdomain/zero/1.0/).

Audio files were taken from the [Yellowstone National Park Sound Library](https://www.nps.gov/yell/learn/photosmultimedia/soundlibrary.htm?utm_source=chatgpt.com), which are in the public domain:  
> "The files available here were recorded in the park and are in the public domain."


## License

See [LICENSE](LICENSE) for details.