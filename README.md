[README.md](https://github.com/user-attachments/files/23548137/README.md)
# 🎮 GigaTap

### A Reflex-Based Arcade Game Built With Unity

GigaTap is a fast-paced arcade reflex game designed as a complete
**gameplay framework** that demonstrates clean architecture, dynamic
difficulty, advanced combo logic, and scalable Unity systems.\
Developed as a **portfolio project**, the game focuses on polished
moment-to-moment gameplay and modular, production-ready systems.

------------------------------------------------------------------------

## 🚀 Features

### 🟡 Core Gameplay

-   Modular **Target Spawner** with adaptive spawn rates\
-   **Dynamic Difficulty Adjustment (DDA)** based on score &
    performance\
-   **Combo System** with multiplier tiers (x2, x3, x5)\
-   **Timed combo decay** using coroutine-driven windows\
-   **Score, punishment & shield logic** with event callbacks\
-   Scalable **GameManager** implementing safe Singleton architecture

------------------------------------------------------------------------

## ⚡ Power-Up System

Each power-up is implemented as a standalone module with clear
structure:

-   🕒 **Slow Motion** --- global timescale slowdown using unscaled
    timers\
-   ✖️ **Double Score** --- dynamic score multiplier\
-   🛡 **Shield** --- one-hit damage negation\
-   🧊 **Time Freeze** --- complete world freeze using
    `Time.timeScale = 0`

All power-ups dispatch: - Activation events\
- Deactivation events\
- UI-ready data (duration/type)

------------------------------------------------------------------------

## 🧠 Technical Highlights

-   **Event-Driven Architecture** decoupling gameplay, UI, and effects\
-   **Coroutine-based loops** for timers, spawns, decay & power-ups\
-   Designer-friendly inspector configuration\
-   **Adaptive difficulty logic** with performance tracking\
-   **SOLID-aligned structure**, clean, simple, extensible\
-   Mobile-ready frame-safe gameplay loops

------------------------------------------------------------------------

## 🏗 Project Architecture

    GameManager
     ├─ Spawn System
     ├─ Score System
     ├─ Combo Manager
     ├─ Dynamic Difficulty Controller
     ├─ Power-Up Manager
     └─ Event Dispatcher

Each subsystem is isolated, readable, and ready for scaling or
refactoring.

------------------------------------------------------------------------

## 🛠 Tech Stack

-   Unity 2022+ / Unity 6.x\
-   C# Gameplay Architecture\
-   Coroutines\
-   Events\
-   Physics Interactions

------------------------------------------------------------------------

## ▶️ How to Play

1.  Clone the repo\
2.  Open in **Unity 2022.3+ or Unity 6.x**\
3.  Press **Play**\
4.  Hit targets, stack combos, survive difficulty spikes

------------------------------------------------------------------------

## 📌 Roadmap

-   Object Pooling\
-   Extra power-ups\
-   Camera shake & improved juice\
-   Mobile UI & haptics\
-   Leaderboards\
-   Achievement system

------------------------------------------------------------------------

## 👤 Author

**Bülent ÜNALAN**\
Unity Gameplay Programmer\
Portfolio Project

------------------------------------------------------------------------

## 📄 License

MIT License -- Free to use and modify.
