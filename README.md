# EchoSquad

<div align="center">
  <img src="./images/gameplay-demo_boss.gif" alt="Boss Battle" width="800">
  <p><i>Boss monster appears - Face the ultimate challenge</i></p>

  <br>

  <img src="./images/gameplay-demo_move.gif" alt="Voice Command Movement" width="800">
  <p><i>Command your AI companion to move using voice controls</i></p>
</div>


## 📌Table of Contents
- [Highlights](#highlights)
- [Installation](#installation)
- [How to Play](#how-to-play)
- [Trailer](#trailer)
- [System Overview](#system-overview)
- [Reference](#reference)
- [License](#license)
- [Team](#team)


## ⭐Highlights
Echo Squad is a TPS (Third-Person Shooter) defense game being developed as a graduation project for Konkuk University.
Players can communicate with AI companions in real-time using voice commands to execute strategic gameplay. 
The goal is to implement AI-driven companion interaction as the core mechanic.


## ⚙️Installation
This guide will walk you through setting up the Echo Squad project on your local machine.

### Prerequisites

Before you begin, ensure you have the following tools and software installed on your system.

* **Unity:** Version **`6000.0.42f1`** (or newer) via Unity Hub.
* **Git:** Standard Git client (Git LFS is recommended).
* **NVIDIA GPU Dependencies (Required for TTS):**
    * **CUDA Toolkit:** Version 12.9 (or compatible).
    * **cuDNN:** Version 9.12.0 (or compatible).
    * *Note: TTS will run on CPU without these, but it will be very slow.*

---

### 1. Project & Core Dependencies Setup

1.  **Clone the Repository**
    Open your terminal and run the following command:
    ```bash
    git clone [https://github.com/GraduationProject-EchoSquad/EchoSquad.git](https://github.com/GraduationProject-EchoSquad/EchoSquad.git)
    ```

2.  **Download ONNX Runtime (for TTS)**
    * Download **`onnxruntime-win-x64-gpu-1.23.1.zip`**.
    * Unzip the file.
    * Copy all files from inside the `lib` folder into the project's `Assets/Plugins/` folder.

3.  **Open the Project in Unity**
    * Open **Unity Hub** and add the cloned project folder.
    * When the project first opens, it will automatically download the **`LLM - StreamingAssets`**. Wait for this to complete.
    * **Troubleshooting:** If you encounter LLM errors, try deleting the `llamacpp` folder from the project, restart the Unity Editor, and wait for the download to complete again.

---

### 2. Manual Model Setup

You must manually download and place the following models into the project.

**A. TTS (Spark-TTS) Models**

1.  Download the `SparkTTS` folder from this [Google Drive link](httpsManage cookies).
2.  Copy the entire `SparkTTS` folder directly into `Assets/StreamingAssets/`.
3.  (For more info, see the original [Spark-TTS-Unity repo](https://github.com/arghyasur1991/Spark-TTS-Unity?tab=readme-ov-file)).

**B. LLM (Qwen3) Models**

1.  **Base Model:** Inside the Unity Editor, find the "LLM Object" and use its interface to download and **"Load model"** for **`Qwen3-1.7b`**.
2.  **LoRA Adapter:**
    * Download `qwen3-1.7b-lora-adapter.gguf` from this [Google Drive link](https://drive.google.com/file/d/1dUdH4YhvF7zO9W-cXgWaVIiGbg1saS_Z/view?usp=sharing).
    * In the "LLM Object", link this file using the **"Load LoRA"** option.

**C. STT (Whisper) Model**

1.  Go to the [whisper.cpp Hugging Face repository](https://huggingface.co/ggerganov/whisper.cpp/tree/main).
2.  Download the **`ggml-large-v3-turbo-q8_0.bin`** file.
3.  Place this `.bin` file inside the `Assets/StreamingAssets/` folder.
4.  Link this model file within the relevant component in the Unity Editor.

**D. UI Files**

1.  Purchase and download the **"Shift - Complete Sci-fi UI"** asset from the Unity Asset Store:
    * [https://assetstore.unity.com/packages/2d/gui/shift-complete-sci-fi-ui-157943](https://assetstore.unity.com/packages/2d/gui/shift-complete-sci-fi-ui-157943)
2.  Import the downloaded asset package directly into the `Assets/ImportedAsset` folder within your project.

---

### 3. Running the Project

Once all prerequisites are installed and all models are downloaded and placed in their correct folders, press the **Play** button (▶) in the Unity Editor to run the game.
## 🎮How to Play
### 🕹️ Controls
| Action | Key/Input              | Description |
|--------|------------------------|-------------|
| **Movement** | `W` `A` `S` `D`        | Move forward, left, backward, right |
| **Voice Command** | `` ` `` (Backtick)     | Activate microphone |
| **Aiming** | `Mouse`                | Change player direction |
| **Aim Up/Down** | `Q` / `E`              | Aim gun upward / downward |
| **Jump** | `Spacebar`             | Jump |
| **Camera Zoom** | `Mouse Wheel`          | Scroll up to zoom in, down to zoom out |
| **Shop** | `Z` (near center rune) | Open shop to purchase upgrades |

### Gameplay
- **Coins**: Defeating enemies grants coins to buy items in the shop
- **Shop Items**: Fire rate upgrades, ammo refills, health restoration, and max HP increase
- **Waves**: Survive 5 progressively challenging waves (Wave 1–5)

### Mission Objectives
- **Mission Complete**: Successfully survive all five waves
- **Mission Failed**: Both player and AI companion die


## 🎞️Trailer
Click on the image below to watch the game trailer.

[![Echo Squad Trailer](https://img.youtube.com/vi/WveVS0yvggg/hqdefault.jpg)](https://www.youtube.com/watch?v=WveVS0yvggg)


## 💡System Overview

<div align="center">
  <img src="./images/system-diagram.png" alt="System Architecture Diagram" width="900">
</div>

Echo Squad implements a real-time voice command system where players interact with AI companions through natural speech. The entire pipeline—from voice input to AI response—runs locally within Unity, ensuring low latency and seamless gameplay.

### Data Flow Pipeline

| Stage | Component | Description |
|-------|-----------|-------------|
| **1. Voice Capture** | STT (Whisper) | Player speaks into microphone<br>↓<br>Whisper transcribes speech to text in real-time<br>↓<br>Natural language command extracted |
| **2. Intent Recognition** | LLM (Qwen3 + LoRA) | Text processed by Qwen3 LLM with custom LoRA Adapter<br>↓<br>Model interprets player intent from command patterns<br>↓<br>Outputs structured JSON (AI action + dialogue response) |
| **3. Action Execution** | FSM Controller | FSM parses JSON output<br>↓<br>Translates into in-game behaviors (move/attack/support)<br>↓<br>Triggers AI action and vocal response simultaneously |
| **4. Voice Generation** | TTS (Spark-TTS) | AI dialogue text sent to Spark-TTS<br>↓<br>Common phrases pre-cached for instant playback<br>↓<br>Generated voice played through speakers |

### Key Technical Features
- **Local Processing**: All AI inference runs on the player's machine—no cloud dependency
- **LoRA Fine-tuning**: Custom adapter trained on tactical commands for accurate intent recognition
- **Voice Caching**: Pre-computed audio reduces TTS latency from seconds to milliseconds
- **Modular Architecture**: Each component (STT/LLM/TTS) operates independently for maintainability


## 📃Reference
This project was developed using several key open-source libraries. We extend our gratitude to the original authors for their contributions to the community.

* **LLM (Llama):** [LLMUnity](https://github.com/undreamai/LLMUnity)
    * An integration of Large Language Models in Unity, used for AI companion dialogue.
* **STT (OpenAI Whisper):** [whisper.unity](https://github.com/Macoron/whisper.unity)
    * A Unity wrapper for `whisper.cpp` used to implement the real-time voice command (STT) functionality.
* **TTS (Spark TTS):** [Spark-TTS-Unity](https://github.com/arghyasur1991/Spark-TTS-Unity)
    * A high-performance Text-to-Speech library for Unity, providing the AI companion's voice.
* **Asynchronous Programming:** [UniTask](https://github.com/Cysharp/UniTask)
    * An efficient, allocation-free async/await integration for Unity, used to manage various asynchronous operations.


## 💻License
This project (`Echo Squad`) is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for more details.

### Third-Party Licenses

This project utilizes several open-source libraries. We are grateful to the authors for their work. Please find their respective licenses below.

* **LLMUnity:** Licensed under the [MIT License](https://github.com/undreamai/LLMUnity/blob/main/LICENSE).
* **whisper.unity:** Licensed under the [MIT License](https://github.com/Macoron/whisper.unity/blob/master/LICENSE).
* **UniTask:** Licensed under the [MIT License](https://github.com/Cysharp/UniTask/blob/master/LICENSE).
* **Spark-TTS:** The [Spark-TTS-Unity](https://github.com/arghyasur1991/Spark-TTS-Unity) library is a port of the original [Spark-TTS](https://github.com/SparkAudio/Spark-TTS) project, which is licensed under the [Apache 2.0 License](https://github.com/SparkAudio/Spark-TTS/blob/main/LICENSE).

---

**Apache 2.0 License (Spark-TTS) Notices:**

In compliance with the Apache 2.0 License, we acknowledge the following notices from the original Spark-TTS project:

> * Do not use this model for unauthorized voice cloning, impersonation, fraud, scams, deepfakes, or any illegal activities.
> * Ensure compliance with local laws and regulations when using this model and uphold ethical standards.
> * The developers assume no liability for any misuse of this model.


## 👥Team

| Name | Role | GitHub |
|------|------|:---|
| Jimin Lee | Development | [@ljm008jjang](https://github.com/ljm008jjang) |
| Jinhwan Lee | Development | [@Growcompany](https://github.com/Growcompany) |
| Hyeonjeong Kim | Development | [@KHyeonxJ](https://github.com/KHyeonxJ) |
