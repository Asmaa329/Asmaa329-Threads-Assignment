# Asmaa Diab Abdelsamee Raghep   Groub # 1 .NET

# Assignment 03 — Locking Research

## Q1: What is `System.Threading.Monitor` used for?
**Answer:**
`System.Threading.Monitor` is a utility class in C# used to provide synchronization for access to shared resources in a multithreaded environment. It ensures thread safety through mutual exclusion (allowing only one thread to access a critical section at a time) and provides methods to control thread interaction, such as waiting for a resource or signaling other threads (`Wait`, `Pulse`, `TryEnter`).

---

## Q2: What does the C# `lock` statement compile to?
**Answer:**
The C# `lock` statement is syntactic sugar that the compiler translates into a safe `try-finally` block utilizing the `Monitor` class. Specifically, it compiles down to acquiring the lock using `Monitor.Enter` (often with a `ref bool lockTaken` flag for thread-safety against asynchronous exceptions) and ensuring the lock is reliably released inside the `finally` block using `Monitor.Exit`.

Under the hood, it roughly translates to:
```csharp
bool lockTaken = false;
try
{
    Monitor.Enter(obj, ref lockTaken);
    // Critical section code
}
finally
{
    if (lockTaken)
    {
        Monitor.Exit(obj);
    }
}

## Q3: List at least two differences between using lock and calling Monitor APIs directly.
**Answer:**

Simplicity vs. Control: The lock statement is simple, concise, and inherently safe because it handles acquisition and release automatically. Calling Monitor APIs directly requires manual management (writing your own try-finally blocks and managing lockTaken flags), which gives you finer control at the cost of higher risk of bugs (like forgetting to release the lock).
Timeouts and Advanced Features: The lock statement blocks indefinitely until the lock is acquired. By calling Monitor APIs directly (such as Monitor.TryEnter), you can specify a timeout period or check for the lock non-blocking, allowing a thread to execute alternative logic if the lock isn't immediately available.

---

## Q4: When might a developer use Monitor.TryEnter, Monitor.Pulse, or Monitor.Wait instead of a plain lock?
**Answer:**

Monitor.TryEnter: Used when a developer wants to attempt acquiring a lock with a specific timeout or non-blocking check, helping to prevent deadlocks or allowing a thread to do alternative work instead of hanging indefinitely if the resource is busy.
Monitor.Wait and Monitor.Pulse (PulseAll): Used in advanced producer-consumer or signaling scenarios where threads need to coordinate and communicate states. A thread calls Monitor.Wait to release the lock and sleep until another thread performs work and calls Monitor.Pulse to wake it up — a workflow that a simple lock statement cannot handle on its own.

---


 ### Q5. Code Comparison: `lock` vs `Monitor`


#### Using `lock`:
```csharp
private readonly object _lockObj = new object();

public void DoWorkWithLock()
{
    lock (_lockObj)
    {
        // Critical section
    }
}
#### Using `Monitor`:
```csharp
private readonly object _lockObj = new object();

public void DoWorkWithMonitor()
{
    bool lockTaken = false;
    try
    {
        Monitor.Enter(_lockObj, ref lockTaken);
        // Critical section
    }
    finally
    {
        if (lockTaken)
        {
            Monitor.Exit(_lockObj);
        }
    }
}
## References

* [Microsoft Learn: lock statement](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock)
* [Microsoft Learn: Monitor Class](https://learn.microsoft.com/en-us/dotnet/api/system.threading.monitor)
* [Threading in C# by Joseph Albahari](http://www.albahari.com/threading/part2.aspx)