# Unity C# Coding Guidelines & Rules

This rule document dictates coding conventions, patterns, and guidelines for **Escape The Lava** project. Always strictly follow these practices when writing or modifying Unity 3D C# code.

---

## 🧠 Core Software Principles

### 1. SOLID Design Principles
*   **Single Responsibility (SRP)**: Each class must have exactly one reason to change. Separate gameplay logic, input handling, level generation, and UI representation into distinct classes.
*   **Open/Closed (OCP)**: Software entities should be open for extension but closed for modification. Use interfaces or base classes to define extensible behaviors (e.g., custom tile behaviors, achievements, sound handlers).
*   **Liskov Substitution (LSP)**: Derived classes must be fully substitutable for their base classes without breaking functionality.
*   **Interface Segregation (ISP)**: Keep interfaces small and specific. Avoid creating bloated "god interfaces."
*   **Dependency Inversion (DIP)**: Depend on abstractions rather than concrete classes. Use script references, ScriptableObjects, or event channels instead of hard-coupling systems.

### 2. KISS (Keep It Simple, Stupid)
*   Do not over-engineer solutions. Start with the simplest implementation that fulfills the functional requirements, is readable, and compiles cleanly.
*   Avoid adding speculative code or "future-proofing" mechanisms that are not immediately required by the task.

---

## 🚀 Unity Best Practices (DO's)

*   **Cache Component References**: Always cache references retrieved via `GetComponent<T>()` in `Awake()` or `Start()`.
*   **Separation of Concerns**: Use ScriptableObjects (`LevelData`, `GameSettings`) for static configuration, definitions, and level setups. Keep configurations separated from scene state.
*   **Event-Driven Communication**: Use C# events/actions or ScriptableObject-based event channels to notify systems (e.g., UI, Sound) of gameplay events (e.g., collecting diamonds, hitting lava, game over). This avoids tight coupling.
*   **Frame-Rate Independent Movement**: Multiply translation offsets by `Time.deltaTime` in `Update()` or use `FixedUpdate()` for physics-based adjustments.
*   **Clean Hierarchy and Cleanup**: When spawning GameObjects in editor or runtime, always parent them under a root transform, name them logically, and clean up previous instances.
*   **Safe Editor Coding**: Wrap all Unity Editor API calls (e.g. `UnityEditor` namespace, `Undo`, `PrefabUtility`) inside `#if UNITY_EDITOR` blocks, or keep them strictly inside files placed in folders named `Editor`.
*   **Execution in Edit Mode**: Use `[ExecuteAlways]` or `[ExecuteInEditMode]` with extreme caution to avoid dirtying or modifying scene files during normal editing.

---

## 🚫 Unity Anti-Patterns (DON'Ts)

*   **No Find Calls in Updates**: Never call `GameObject.Find`, `FindObjectOfType`, or `GetComponent` inside `Update()`, `LateUpdate()`, or `FixedUpdate()`.
*   **No Empty Callbacks**: Do not leave empty lifecycle callbacks (like `Update()`, `Start()`, `OnEnable()`) inside MonoBehaviours, as Unity still registers and calls them, incurring native-to-managed bridge overhead.
*   **Avoid Garbage Collection Spikes**:
    *   Avoid frequent string concatenation (e.g. `scoreText.text = "Score: " + score;` in every `Update`). Cache strings or only update text UI elements when the values actually change.
    *   Avoid using `Instantiate` and `Destroy` at high frequencies; use **Object Pooling** for frequently spawned items (like bullets, impact effects, or popups).
*   **No Physics in Update**: Do not apply constant force or physics adjustments inside `Update()`; always perform physics calculations and `Rigidbody` updates inside `FixedUpdate()`.
*   **No Hardcoded Offsets**: Avoid using magic numbers or hardcoded grid coordinates. Define them as serialized fields or configuration structures.
