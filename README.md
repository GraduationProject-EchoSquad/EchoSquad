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

**Echo Squad** is a TPS defense game where voice becomes your tactical weapon.

- **Voice-Controlled AI Companions**: Command your squad in real-time using natural speech
- **Local AI Processing**: STT, LLM, and TTS run entirely on your machine—no internet required
- **Strategic Defense Gameplay**: Survive 5 waves of enemies with intelligent AI teammates
- **Custom LoRA Training**: Fine-tuned language model specialized for tactical commands

> 📚 **Academic Project**: Developed as a graduation project for Konkuk University, focusing on AI-driven companion interaction as the core gameplay mechanic.


## ⚙️Installation

### For Players (Game Release)
> 🚧 **Coming Soon**: A standalone executable version of Echo Squad for end-users is currently in development. Stay tuned for future releases!

### For Developers (Unity Project Setup)
This guide will walk you through setting up the Echo Squad project source code on your local machine.

#### Prerequisites

Before you begin, ensure you have the following tools and software installed on your system.

| Requirement | Version | Purpose | Notes |
|-------------|---------|---------|-------|
| **Unity** | `6000.0.42f1` or newer | Game engine | Install via Unity Hub |
| **Git** | Latest | Version control | Git LFS recommended for large files |
| **CUDA Toolkit** | 12.9 or compatible | GPU acceleration for TTS | ⚠️ Optional but **highly recommended** |
| **cuDNN** | 9.12.0 or compatible | Deep learning library | ⚠️ Optional but **highly recommended** |

> **⚡ Performance Note**: Without CUDA/cuDNN, TTS will run on CPU and be significantly slower (several seconds per voice line).

---

#### 1. Project & Core Dependencies Setup

**Step 1: Clone the Repository**
```bash
git clone https://github.com/GraduationProject-EchoSquad/EchoSquad.git
```

**Step 2: Download ONNX Runtime (TTS Dependency)**
1. Download `onnxruntime-win-x64-gpu-1.23.1.zip`
2. Extract and copy all files from the `lib` folder
3. Paste into `Assets/Plugins/` folder in your project

**Step 3: Open Project in Unity**
1. Open **Unity Hub** → Add cloned project folder
2. Wait for automatic **LLM StreamingAssets** download
3. ⚠️ **Troubleshooting**: If LLM errors occur:
   - Delete `llamacpp` folder from project
   - Restart Unity Editor
   - Wait for re-download to complete

---

#### 2. Manual Model Setup

> **Required Downloads**: The following models and assets must be manually downloaded and configured.

| Component | File/Model | Download Source | Installation Path | Notes |
|-----------|------------|-----------------|-------------------|-------|
| **TTS (Spark-TTS)** | `SparkTTS` folder | [Google Drive](httpsManage cookies) | `Assets/StreamingAssets/` | See [Spark-TTS-Unity repo](https://github.com/arghyasur1991/Spark-TTS-Unity) for details |
| **LLM (Qwen3)** | `Qwen3-1.7b` base model | Unity Editor (LLM Object) | Auto-downloaded | Use "Load model" option in LLM Object |
| **LLM (LoRA)** | `qwen3-1.7b-lora-adapter.gguf` | [Google Drive](https://drive.google.com/file/d/1dUdH4YhvF7zO9W-cXgWaVIiGbg1saS_Z/view?usp=sharing) | Link via Unity Editor | Use "Load LoRA" option in LLM Object |
| **STT (Whisper)** | `ggml-large-v3-turbo-q8_0.bin` | [Hugging Face](https://huggingface.co/ggerganov/whisper.cpp/tree/main) | `Assets/StreamingAssets/` | Link in relevant Unity component |
| **UI Assets** | Shift - Complete Sci-fi UI | [Unity Asset Store](https://assetstore.unity.com/packages/2d/gui/shift-complete-sci-fi-ui-157943) | `Assets/ImportedAsset/` | Purchase required |

---

#### 3. Running the Project

> **✅ Ready to Play**: Once all prerequisites, models, and assets are properly configured, press the **Play** button (▶) in the Unity Editor to launch the game.
## 🎮How to Play
### 🕹️ Controls
| Action | Key/Input              | Description |
|--------|------------------------|-------------|
| **Movement** | `W` `A` `S` `D`        | Move forward, left, backward, right |
| **Camera Rotation** | `Mouse Movement`       | Rotate camera view |
| **Camera Zoom** | `Mouse Wheel`          | Scroll up to zoom in, down to zoom out |
| **Jump** | `Spacebar`             | Jump |
| **Shoot** | `Left Mouse Button`    | Fire weapon |
| **Aiming** | `Right Mouse Button`   | Display red aiming line |
| **Aim Up/Down** | `Q` / `E`              | Aim gun upward / downward |
| **Voice Command** | `` ` `` (Backtick)     | Activate microphone |
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
