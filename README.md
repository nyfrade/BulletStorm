# 🎮 BulletStorm

BulletStorm is a **roguelike** game developed with **MonoGame** (.NET 8), focused on fast-paced action, level-based progression, and a performance-based scoring system. Face waves of enemies, survive, upgrade your skills, and defeat the final boss.

## 🧠 Developers
- Anthony Frade (31477)
- Valezka Naia (31481)

## 🕹️ Controls

| Key | Action |
|-----|--------|
| W | Move Up |
| S | Move Down |
| A | Move Left |
| D | Move Right |
| E | Special Ability |
| F11 | Fullscreen |

---

## 📦 General Project Structure

- Developed using **MonoGame (.NET 8)**.
- **State-based** architecture (`Menu`, `InputName`, `Playing`, `Scores`, `Credits`).
- The main loop (`Game1`) manages logic, input handling, rendering, and transitions.

---

## 🧭 Game Flow

1. **Main Menu**
   - Custom interface drawn using primitives (rectangles) and text.
   - Three options: `Start`, `Scores`, `Credits`.
   - Navigation via keyboard and mouse.

2. **Name Input**
   - The player enters a name (max. 12 characters).
   - The name is used to save the score at the end of the session.

3. **Gameplay**
   - The player faces various **phases** with waves of enemies and increasing difficulty.
   - Power-ups available in specific phases (Damage, Health, Crit Rate, Speed).
   - The final boss appears in the last phase.

4. **Score**
   - Formula: `score = enemies killed * 100 - time (in seconds)`
   - Minimum score: 0
   - Results (Name, Score, Enemies, Time) are displayed on the "Scores" screen (with persistence via a local `.txt` file).

---

## 🛠️ Implementation Decisions

- **Simple and Functional**: UI built without external libraries.
- **Separation of Concerns**: Distinct classes for `Player`, `Enemy`, `Weapon`, `LevelManager`, etc.
- **Clean State System**: Intuitive management using enums (`GameState`, `Phase`).
- **Input Handling**: Basic text input system (letters, numbers, and spaces).
- **Fair Scoring**: A system that rewards performance and efficiency.

---

## 🚀 Future Improvements

- Improved HUD and GUI.
- Animations and visual effects for the menu.
- Persistent upgrade system.
- Visual character customization.
- More enemy types and phases/levels.
- Enhanced sprite work.
- Multiple characters with unique abilities/weapons and a character editor.
- Weapon crafting system.

---

## 🧪 How to Play

1. Compile the project with .NET 8 and MonoGame installed.
2. Run the generated binary or execute directly from your IDE (Visual Studio/JetBrains Rider).
3. Use the controls to navigate and play.

---

## 🏁 Requirements

- .NET 8 SDK
- MonoGame Framework
- Windows OS (Linux/Mac may require additional configuration)

---

## 📂 Code Organization

```plaintext
├── BulletStorm/
│   ├── Game1.cs              # Main game loop
│   ├── States/               # Menu, Scores, Credits, Input logic
│   ├── Entities/             # Player, Enemy, Weapon, etc.
│   ├── Levels/               # LevelManager, Phase logic
│   ├── Content/              # Game assets (images, audio, fonts)
│   └── Utils/                # Helpers and scoring logic
```
---
This project was developed as part of an academic assignment for the game engineering and development course at IPCA.
