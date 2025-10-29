# EchoSquad
[프로젝트를 gif 파일, 사진을 기반으로 소개합니다.]


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
- **Movement** : Use W / A / S / D to move (Up, Left, Down, Right).
- **Aiming** : Move your mouse to change the player’s direction. Press Q to aim the gun up, and E to aim it down.
- **Jump** : Press Spacebar to jump.
- **Camera Zoom** : Scroll the mouse wheel up to zoom in, and scroll down to zoom out.
- **Shop** : Approach the center rune and press Z to open the shop. In the shop, you can purchase fire rate upgrades, ammo, and health.
- **Coins** : Defeating enemies grants you coins, which can be used to buy items in the shop.
- **Waves** : The game consists of 5 waves (Waves 1–5).
- **Mission Complete** : Successfully surviving all five waves will trigger Mission Complete.
- **Mission Failed** : If both the player and the AI companion die, the mission will fail.


## 🎞️Trailer
Click on the image below to watch the game trailer.

[![Echo Squad Trailer](https://img.youtube.com/vi/WveVS0yvggg/hqdefault.jpg)](https://www.youtube.com/watch?v=WveVS0yvggg)


## 💡System Overview
[프로젝트의 기술적인 구조와 작동 원리에 대해 설명합니다.]


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
[프로젝트의 라이선스 정보를 안내합니다.]


## 👥Team

| Name | Role | GitHub |
|------|------|:---|
| Jimin Lee | Development | [@ljm008jjang](https://github.com/ljm008jjang) |
| Jinhwan Lee | Development | [@Growcompany](https://github.com/Growcompany) |
| Hyeonjeong Kim | Development | [@KHyeonxJ](https://github.com/KHyeonxJ) |
