# Stationeers LLM Player Mod
A BepInEx mod for Stationeers that adds autonomous, LLM-powered bots to the game.

## Features
- **Autonomous Agent Loop**: Bots observe the world, reason using an LLM, and execute actions in a real-time cycle.
- **Visual Perception**: High-performance, non-blocking screenshot capture using Unity's `AsyncGPUReadback`.
- **Structured Awareness**: Automated extraction of inventory contents, player status (health, position), and nearby interactable objects.
- **Multi-Backend Support**:
  - **Ollama**: Optimized for local vision models (e.g., Gemma 3, LLaVA).
  - **OpenAI-Compatible**: Support for OpenRouter, OpenAI, and other standard vision APIs.
  - **Kobold.cpp**: Direct HTTP support for local GGUF vision models.
- **Comprehensive Controls**:
  - **Movement**: Forward, backward, strafing, and jumping.
  - **Looking**: Rotation control for yaw and pitch.
  - **Inventory**: Slot selection and management.
  - **Interaction**: Object interaction and a specialized `CONSTRUCT` action for building.
- **In-Game Integration**:
  - Dedicated **SPAWN LLM BOT** button in the pause menu.
  - Global hotkey (**F9**) to toggle AI control for all bots.
  - Configurable tick rates to balance performance and AI responsiveness.

## Requirements
- **Stationeers**
- **BepInEx 5.x**
- An LLM Backend:
  - [Ollama](https://ollama.com/) (Local)
  - [Kobold.cpp](https://github.com/LostRuins/koboldcpp) (Local)
  - [OpenRouter](https://openrouter.ai/) or OpenAI API Key (Cloud)

## Installation
1. Ensure BepInEx 5.x is installed in your Stationeers game directory.
2. Download and place `LLMPlayer.dll` into the `BepInEx/plugins` folder.
3. Launch Stationeers. The mod will generate a configuration file on the first run.

## Configuration
Settings can be found in `BepInEx/config/com.jules.stationeers.llmplayer.cfg`.

### LLM Setup
- **ProviderType**: Select between `Ollama`, `OpenAICompatible`, or `Kobold`.
- **Endpoint URLs**: Configure the API address for your chosen provider.
- **Model Name**: Specify the vision-capable model you wish to use (e.g., `gemma3:4b`).

### Security (OpenAI/OpenRouter)
To keep your API keys secure, they are **not** stored in the BepInEx config file. Use one of the following methods:
1. Set an environment variable named `STATIONEERS_LLM_OPENAI_KEY`.
2. Create a file named `llm_openai_key.txt` in the same folder as the mod DLL and paste your key inside.

## How to Spawn and Use
1. Start a game session.
2. Press **ESC** to open the pause menu.
3. Click the **SPAWN LLM BOT** button to create a new bot instance at your location.
4. Use the **F9** key to enable or disable AI logic for all spawned bots.
5. Watch the BepInEx console for reasoning logs and action execution details.

## Development
To build the project from source:
1. Open `LLMPlayer.sln` in Visual Studio or use `dotnet build`.
2. Ensure you have the required Stationeers DLLs. The project uses `src/LLMPlayer.props` to manage assembly references. By default, it looks for a Stationeers installation in the parent directory.
3. The project targets .NET Framework 4.7.2 for compatibility with Stationeers' Unity Mono runtime.
