
## Asmaa Diab Abdelsamee Raghep   Group # 1 .Net
## R2: What is Wrong and Why
- **Race Condition (Check-Then-Act):** In a multi-threaded environment where multiple customer threads execute `Book()` simultaneously, checking `if (TablesLeft <= 0)` 
  and subsequently modifying `TablesLeft` and `Booked` are not atomic operations. Multiple threads can evaluate `TablesLeft` as greater than $0$ at the same time before any thread decrements it,
  leading to overselling (negative table counts or incorrect totals).
- **Lock Contention / Performance Bottleneck:** If a `lock` is used but includes blocking operations like `Thread.Sleep(20)` directly inside the critical section, all threads are forced to run sequentially one-by-one. 
  While this prevents race conditions, it creates lock contention and serializes the execution.

## R4: Explanation of the Fix
We can solve this issue using two different approaches depending on whether we use synchronous methods or modern asynchronous patterns:

1. **Approach 1: Using `lock` (Synchronous Mutex)**
   - Encapsulated the check-then-act logic, state modifications, and execution delay inside a `lock` block using a dedicated `_lock` object.
   - This ensures that only one thread can access the critical section at a time, completely eliminating race conditions and keeping `Booked` and `TablesLeft` accurate.

2. **Approach 2: Using `SemaphoreSlim` (Asynchronous Mutual Exclusion)**
   - For `async/await` patterns (e.g., in web APIs), we can use a `SemaphoreSlim` initialized with a count of `1` (`new SemaphoreSlim(1, 1)`).
   - By awaiting `_semaphore.WaitAsync()` and wrapping state changes in a `try-finally` block (`_semaphore.Release()`), 
	we achieve thread safety without blocking the underlying thread during asynchronous delays (`Task.Delay`), which optimizes resource utilization.