# Advent of Code 2021

Going to do these in C# this year so I can (hopefully) finish.  Using the new .NET 6 minimal boilerplate console app templates
 
https://adventofcode.com/2021/

## Commentary

I am adding this commentary section because everyone loves my opinions.

### Day 1:

  * Making use of .Skip() and .Take() LINQ operators [here](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec01/Program.cs#L41).  This is probably slower than aggregating the elements directly, but the code is really tidy and semantic.  It'd be interesting to see how much slower this would be with BenchmarkDotNet or something once I'm caught up and the YEYS crunch at work settles down.
  * I'm using [Debug.Assert()](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec01/Program.cs#L9) to check my results, and this is nice because it will break the debugger there if the assertion is not true.
  * This is the first time I'm using the C# minimal console app template for anything other than Hello World and it seems pretty nice overall.  I only had to [import two things](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec01/Program.cs#L1-L2) and all of my code is aligned to the left margin which I'm very not used-to with C#.  I have a feeling I'm really going to like `global using` (not used in this project) except for all the cases where it will be a pain (such as four of the NuGet packages we use at work having its own `Metadata` class).

### Day 2:

  * I sort of casually threw in PLINQ `.AsParallel()` in the first part and reminded myself that `.AsOrdered()` is required if you need to guarantee the ordering, so it was kind of not going to be useful in the second part since each iteration depended on knowing the `aim` from the previous one, so didn't really make sense to use it at all.  I think I really will have to benchmark everything once this is all complete.  They've done so much work on LINQ in .NET 5 and 6 that it'd be interesting to see how much gap there truly is these days in memory allocation and performance.  From a programmer productivity standpoint LINQ is probably worth using regardless of any performance differences except for in like games or your business app's hottest of hot paths.
  * [This code](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec02/Program.cs#L54) is making good use of value tuples which we love at work.  The code (`(depth: 0, position: 0, aim: 0)`) becomes effectively an anonymous struct with three `int` properties that has memberwise value equality.  [This code](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec02/Program.cs#L53) parses up the space-delimited strings into an enum using the built-in enum parsing with case-insensitivity turned on via the `true`, even though the data does not appear to require this.  I don't like the idea that I should feel compelled to make my program match the data, but again if this was a super hot path I could see doing so.  This solution demonstrates the LINQ `Aggregate()` method which is effectively the same as `Reduce()` in JS.  Lastly, the solution is  using C# 9 switch expressions to set each property on the returned aggregate in a single statement. For example, [this code](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec02/Program.cs#L60-L64) is adding a number to the position on every aggregate; the number is either the `amount` from the element when the direction is `forward`, or else it's 0. 

