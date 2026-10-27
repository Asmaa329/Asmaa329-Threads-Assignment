# Asmaa Diab Abdelsamee Raghep
# Answers - Invoice Export Refactoring

## 1. Problems Identified
1. **Sequential Execution Bottleneck (Sequential Awaiting):** 
   - Processing pending orders sequentially inside a `foreach` loop (awaiting each order, customer file read, and invoice write operation one after another) caused cumulative delays, preventing any concurrency benefits.
2. **Redundant Database Query (N+1 Query Anti-Pattern):** 
   - Calling `_db.GetOrderByIdAsync(order.Id)` inside the loop was completely redundant since the full list of pending orders was already fetched upfront.

## 2. Changes Made & Why
1. **Removed Redundant Queries:** Eliminated the unnecessary `GetOrderByIdAsync` call inside the loop, relying directly on the order data already retrieved from `GetPendingOrdersAsync()`.
2. **Concurrent Processing:** Replaced sequential execution with a `foreach` loop that adds background tasks to a list, and then used `Task.WhenAll` to process and export all invoice files simultaneously in parallel, significantly improving I/O performance.

## 3. Execution Time Comparison (Console Output)
- **Before Modification (Sequential):** 
  - Total execution time took around **6.6 s** due to cumulative blocking delays.
- **After Modification (Concurrent):** 
  - Total execution time dropped significantly to **1.4 s** because all file I/O and database operations ran concurrently.