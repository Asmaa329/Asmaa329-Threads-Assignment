## Asmaa Diab Abdelsamee Raghep    Groub # 1 .NET


# Answers: MenuViews Concurrency Assignment

## R2: Why Expected and Actual Disagree
The `Views++` operation is not atomic. In C#, incrementing an integer actually involves three distinct steps:
1. **Read** the current value of `Views` from memory into a CPU register.
2. **Increment** the value by 1 in the register.
3. **Write** the new value back to memory.

When multiple threads execute `RegisterView()` concurrently, their execution steps interleave.

---

## R4: When to Protect This Kind of Update Differently
While `Interlocked.Increment` is ideal for a single primitive counter, you would choose a different synchronization mechanism (such as a `lock` statement, `Mutex`, or Semaphore) in scenarios such as:
* **Compound State Changes:** If updating the view count also requires updating other related fields simultaneously (e.g., updating a timestamp, a user log list, or dependent aggregates) and all changes must happen together atomically.
* **Complex Business Logic:** If the update requires conditional checks, validation, or multi-step calculations based on the current state before modifying the data.