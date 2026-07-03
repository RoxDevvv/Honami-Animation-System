<p align="center">
  <img src="Runtime/Images/HonamiAnimationSystemLogoCropped.png" alt="Honami Animation System" width="480"/>
</p>

<p align="center">
  <a href="https://github.com/loyal-studio/Honami-Animation-System/releases">
    <img src="https://img.shields.io/badge/version-0.1.0--beta.1-ec407a?style=for-the-badge&logo=unity&logoColor=white" alt="Version"/>
  </a>
  <a href="https://docs.unity3d.com/6000.0/Documentation/Manual/WhatsNewUnity6.html">
    <img src="https://img.shields.io/badge/Unity-6000.0%2B-26c6da?style=for-the-badge&logo=unity&logoColor=white" alt="Unity Version"/>
  </a>
  <img src="https://img.shields.io/badge/status-beta-ff7043?style=for-the-badge" alt="Beta"/>
  <img src="https://img.shields.io/badge/license-MIT-66bb6a?style=for-the-badge" alt="License"/>
</p>

<p align="center">
  <b>A high-performance, node-graph driven animation system for Unity 6 - built to replace the built-in Animator entirely.</b><br/>
  Rig-agnostic · Powerful node graph · Procedural by design · Zero-allocation runtime
</p>

## Overview

**Honami Animation System** is a complete, from-scratch alternative to Unity's built-in Animator. Designed for Unity 6, it gives you a powerful node-graph editor, built-in procedural rigging, pseudo-physics bones, and seamless Timeline integration - all at zero runtime allocation cost.

## Why Honami?

Unity's built-in Animator is great - until it isn't. The moment your project grows, graphs become spaghetti, state machines explode, retargeting breaks your non-humanoid creatures, and every new feature means duplicating logic across dozens of controllers. Honami was built to solve exactly that.

**The graph stays readable no matter how complex your character is.** Sub-Nodes keep secondary logic (sounds, VFX, IK hints, events) hidden inside states. Portal and Sequencer nodes eliminate the need for massive flat state machines. Override Controllers let you reuse entire graphs across character variants without copy-pasting.

**Any skeleton. No compromises.** Unity's Humanoid retargeting is a cage - it assumes a specific bipedal structure, costs CPU on every frame, and actively fights you when building anything non-human. Honami is rig-agnostic. Quadrupeds, spiders, mechs, vehicles, modular bosses - all work with the same pipeline, with full control over every bone.

**Procedural where it matters.** Unity has no built-in rigging system inside its Animator (requiring a heavy, separate package with its own performance and setup limitations). Honami comes with native, high-performance rigging constraints: dynamic Pose Constraints to inject procedural postures (like sliding, recoil, or holding weapons), LookAt targeting for bone chains with per-bone weights, and springy pseudo-physics - all running as a final correction pass on top of authored animation, not instead of it.

**One command, many characters.** The Linked Brain system broadcasts state transitions, parameter changes and action IDs to entire squads at once - with tag filtering, radius targeting and propagation waves. Crowd animation without a single loop in your game code.

**Zero runtime cost.** The evaluation loop produces no GC allocations per frame. Distant characters run at a capped FPS with smooth interpolation. Every API method has an integer-hash overload to keep hot paths allocation-free.

| Feature | Built-in Animator | **Honami** |
|---|:---:|:---:|
| Node-graph editor | ✅ | ✅ |
| Blend trees | ✅ | ✅ |
| Avatar masking | ✅ | ✅ |
| Full Timeline track support | ❌ | ✅ |
| Pose & rigging constraints | ❌ | ✅ |
| Pseudo-physics bones | ❌ | ✅ |
| Per-animator FPS cap | ❌ | ✅ |
| Linked animators | ❌ | ✅ |
| Portal / Sequencer nodes | ❌ | ✅ |
| Sub-Nodes architecture | ❌ | ✅ |
| Live visual debugging | ❌ | ✅ |
| Zero-allocation runtime | ❌ | ✅ |
| Works with non-humanoid rigs | ❌ | ✅ |
| Graph inheritance & override | ❌ | ✅ |

### Clean graphs, not spaghetti

The built-in Animator becomes an unreadable mess as a project grows. Honami keeps graphs clean with **Sub-Nodes** - secondary logic like sounds, VFX, or IK hints lives *inside* a state, invisible in the main flow. You see only what matters.

### Built-in Rig System

In standard Unity development, rigging constraints require the external **Animation Rigging** package. It is completely decoupled from the Animator, requires a complex setup, has significant performance overhead, and struggles with non-humanoid rigs.

Honami includes a high-performance, rig-agnostic constraint pipeline out of the box. No external packages, no humanoid limitations, and zero runtime setup overhead.

All rigs run as a final correction pass after animation sampling, so authored animation stays in control while procedural adjustments handle contacts, aiming, secondary motion and physics.

**Pose Constraint** - Procedurally injects static or dynamic poses directly into bone hierarchies. Allows you to easily enforce offsets and target alignments for specific actions (such as a sliding stance, crouch adjustments, or holding weapons in hands) in either local or world space, with full weight control.

**LookAt Constraint** - Rotates one or a chain of bones toward a target using custom aim/up axes. Handles heads, eyes, turrets, weapon barrels, creature sensors and long necks (spine → neck → head, each with independent weight). Additive mode works on top of authored animation without overwriting it.

**Point Constraint** - Locks a transform to another transform's position and/or rotation with authored offsets. Keeps weapon sockets, armor plates, held objects and split-body pieces perfectly aligned without turning them into Humanoid bones.

**Pivot Fixer** - Solves the "wrong imported pivot" problem. Stabilizes weapon grips, door hinges, tool handles and creature attachment points by picking a different effective anchor than what the FBX provides.

**Pseudo-Physics** - Adds springy inertia and secondary motion to any list of bones: hair, tails, cloth strips, antennae, cables, soft armor, weapon sway. Lightweight by design - no Rigidbody chains, no full physics sim, just the feel of weight and follow-through.

And much more!

### One brain, many characters

The **Linked Brain** system lets you send a single command - a parameter change, a state transition, an action ID - to dozens of animators at once. Tag filtering, radius targeting, propagation waves: orchestrate entire crowds with precision and zero boilerplate.

### Zero cost where it matters

The evaluation loop produces **zero GC allocations** per frame. Distant characters can evaluate at a fixed 15 or 30 FPS with smooth interpolation via the per-animator **FPS Cap** - a free LOD system for animation.

### Works with any rig

Honami is rig-agnostic. Human, dragon, spider, vehicle - all treated with the same high-performance pipeline. No "Humanoid" retargeting tax, no 15-bone minimum. Full control over every bone, every frame.

## Used in Production

Honami powers all animation in **[Daisen](https://store.steampowered.com/app/3702380/Daisen/)** - a fast-paced action game built by LOYAL Studio.

## Installation

### Via Unity Package Manager (Git URL) *(recommended)*

1. Open **Window → Package Manager**.
2. Click the **＋** button → **Add package from git URL…**
3. Paste:
   ```
   https://github.com/loyal-studio/Honami-Animation-System.git
   ```
4. Click **Add** and wait for import to complete.
5. The **Welcome to Honami** window will open automatically on first launch.

## Documentation

Open **Window → Honami → Documentation** directly inside Unity. The built-in docs cover everything: graph authoring, transitions, blend trees, avatars and masks, IK and constraints, the Linked Brain system, the full scripting API, and the optimization guide - with live code examples you can copy with one click.

## Contributing

Contributions, bug reports and feature requests are welcome!

## License

This project is licensed under the **MIT License** - see [LICENSE](LICENSE) for details.

<p align="center">
  Made with Love by <a href="https://github.com/loyal-studio"><b>LOYAL Studio</b></a>
</p>
