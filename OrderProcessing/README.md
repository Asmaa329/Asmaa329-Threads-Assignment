# Order Processing & Concurrency Engine (.NET 10.0)

This repository contains the complete implementation of the multi-phase order processing, concurrency, and scale-up system (.NET 10.0), featuring atomic stock management, asynchronous pipeline orchestration, parallel data dashboards, competitive shipping quote races, and CPU-bound fraud scoring.

---

 ## Engineering Challenges & Solutions

1. **Immutable `OrderItem` Constraints:**
   * *Challenge:* Initial attempts to inject `ProductName` directly into `OrderItem` violated architectural constraints.
   * *Solution:* Refactored `InventoryService` to query stock using product identifiers and repository lookups, preserving domain integrity.
2. **Atomic Rollback in Orchestrator:**
   * *Challenge:* When payment failed after inventory reservation, stock remained locked.
   * *Solution:* Implemented a robust `try-catch` block in `OrderProcessor` that invokes `ReleaseStock` if `stockReserved` evaluates to true.
3. **CPU-Bound Thread Pool Freezing:**
   * *Challenge:* Scoring 10,000 orders synchronously blocked the thread pool.
   * *Solution:* Offloaded processing using `Task.Run` combined with PLINQ (`AsParallel()`) to leverage multi-core CPU efficiency safely.

---

##  Phase Questions & Answers

### Phase 1 Questions
1. **Why are the sync and async times almost the same when orders are awaited one after another?**
   * Because when tasks are awaited sequentially one by one (`await Task1; await Task2;`), the execution is effectively blocked on each task completion, eliminating the benefit of parallel concurrency.
2. **Is the thread ID before an await always the same as after it? Why? Use your own log lines as evidence.**
   * No. After an `await`, the continuation is scheduled on the synchronization context or thread pool. If no specific context forces the same thread, any available thread resumes execution. *(Evidence: `[Thread 5] Starting checkout` -> `[Thread 9] Resumed after await`)*.
3. **What does the thread do while Task.Delay() is waiting? And while Thread.Sleep() is waiting?**
   * `Task.Delay()` is non-blocking; it sets a timer and immediately releases the thread back to the ThreadPool. `Thread.Sleep()` blocks the current thread entirely, keeping it frozen until the time expires.
4. **Why should a repository method return Task<T> instead of T?**
   * Returning `Task<T>` indicates an asynchronous operation (like disk/DB access) that yields control while waiting, keeping the calling thread free.
5. **What is the difference between File.ReadAllText and File.ReadAllTextAsync? Does the async version occupy a thread while the disk is reading?**
   * `File.ReadAllText` blocks synchronously. `File.ReadAllTextAsync` uses asynchronous I/O completion ports. **No**, the async version does not occupy a thread while the disk reads.

### Phase 2 Questions
1. **What is the difference between starting a task and awaiting it?**
   * Starting a task schedules its execution in the background. Awaiting it pauses the current method until completion and retrieves its result or exception.
2. **Does every Task create a new thread? Does Task.Delay() use a thread while waiting?**
   * No. Tasks represent asynchronous work units on shared ThreadPool threads. `Task.Delay()` does not use a thread while waiting.
3. **What happens if you call .Result or .Wait() on an unfinished task instead of await? Why is it dangerous?**
   * It blocks the calling thread synchronously, frequently causing deadlocks in context-bound environments.
   * *Demo Method:*
     ```csharp
     public static void UnsafeBlockDemo()
     {
         Task<int> task = Task.Run(async () => { await Task.Delay(100); return 42; });
         int result = task.Result; 
         Console.WriteLine($"Result: {result}");
     }
     ```
4. **What happens to an exception thrown inside a task that is never awaited?**
   * It becomes an unobserved task exception, surfaced via `TaskScheduler.UnobservedTaskException`.
5. **When should you use Task.Run(), and when should you avoid it?**
   * Use for CPU-bound work. Avoid for I/O-bound operations as it wastes ThreadPool threads.
6. **What is the difference between Task and Task<T>?**
   * `Task` represents a non-value-returning operation (`void`), while `Task<T>` returns a result of type `T`.
7. **Why can a lock block not contain an await?**
   * `lock` relies on monitor ownership bound to a specific OS thread. Since `await` can resume on a different thread, it violates monitor rules.
8. **Why did the unprotected stock reservation oversell, and how did you prevent it?**
   * Unprotected concurrent reads caused race conditions. We prevented it using `ConcurrentDictionary` and `lock` statements for atomic updates.

### Task 9.7 — Fire-and-Forget Investigation
1. **Find the problem in this code:**
   * The call to `_emailService.SendConfirmationAsync(order, customer)` is **not awaited**. Unhandled exceptions become unobserved, and the caller method finishes prematurely before the email sends.

 Why async void is Dangerous
Cannot Be Awaited: The calling code cannot track when an async void method completes.

Uncatchable Exceptions: Exceptions thrown inside an async void method cannot be caught by a try-catch block in the calling method. 
Instead, they propagate directly to the Synchronization Context (often crashing the entire application or bringing down the thread pool/process).
Exception: The only valid and standard use case for async void is for UI Event Handlers (e.g., button_Click), because the event handler signature in UI frameworks expects void and cannot be changed to return a Task. 
However, even inside event handlers, developers must wrap the body in a try-catch block to handle exceptions safely.
### Phase 4 Questions
1. **For each of Tasks 10.1–10.4: which tool did you use, and why can't "start, then await one by one" meet the requirement?**
   * **10.1:** `Task.WhenAll` (one-by-one blocks faster completions).
   * **10.2:** `Task.WhenAll` (one-by-one sums up wait times).
   * **10.3:** `Task.WhenAny` (one-by-one ignores response speed race).
   * **10.4:** `Task.Run` + PLINQ (one-by-one bottlenecks on a single core).
2. **In 10.2, when several loads fail, how do you get every exception? What does a plain await give you?**
   * A plain await throws only the first exception. To get all, we inspect `task.Exception.InnerExceptions` on each faulted task and bundle them into an `AggregateException`.
3. **In 10.3, what happens to the losing carriers if you never cancel them?**
   * They continue consuming network and system resources in the background until completion.
4. **Why does 10.4 A get faster with more CPU cores, while 10.4 B does not depend on the number of cores?**
   * 10.4 A is CPU-bound math distribution across cores. 10.4 B is I/O-bound network throttling (`SemaphoreSlim`) bounded by API response rates.
5. **What is the difference between parallel work (10.4 A) and concurrent asynchronous work (10.1)?**
   * Parallel work executes heavy computations across multiple CPU cores simultaneously. Concurrent asynchronous work manages non-blocking I/O tasks on shared threads.
6. **Show a cancelled run (--cancel-after) and explain what happened to the orders, the stock and the payments:**
   * Triggering cancellation signals pending tasks to abort safely, triggers `ReleaseStock` rollbacks for any reserved inventory, and prevents payments to maintain complete system integrity.