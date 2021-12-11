# Advent of Code 2021

Going to do these in C# this year so I can (hopefully) finish.  I'll be using the new .NET 6 minimal console app templates.
 
https://adventofcode.com/2021/

## Commentary

I am adding this commentary section because everyone loves my opinions.

### Day 1:

  * Making use of `.Skip()` and `.Take()` LINQ operators [here](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec01/Program.cs#L41).  This is probably slower than aggregating the elements directly, but the code is really tidy and semantic.  It'd be interesting to see how much slower this would be with BenchmarkDotNet or something once I'm caught up and the YEYS crunch at work settles down.  I was enough of a [clever-clogs](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec01/Program.cs#L49) to re-use the `current` sum on each iteration so we don't have to sum it twice.
  * I'm using [Debug.Assert()](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec01/Program.cs#L9) to check my results, and this is nice because it will break the debugger there if the assertion is not true.
  * This is the first time I'm using the C# minimal console app template for anything other than Hello World and it seems pretty nice overall.  I only had to [import two things](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec01/Program.cs#L1-L2) and all of my code is aligned to the left margin which I'm very not used-to with C#.  I have a feeling I'm really going to like `global using` (not used in this project) except for all the cases where it will be a pain (such as four of the NuGet packages we use at work having its own `Metadata` class).

### Day 2:

  * I sort of casually threw in PLINQ `.AsParallel()` in the first part and reminded myself that `.AsOrdered()` is required if you need to guarantee the ordering, so it was kind of not going to be useful in the second part since each iteration depended on knowing the `aim` from the previous one, so didn't really make sense to use it at all.  I think I really will have to benchmark everything once this is all complete.  They've done so much work on LINQ in .NET 5 and 6 that it'd be interesting to see how much gap there truly is these days in memory allocation and performance.  From a programmer productivity standpoint LINQ is probably worth using regardless of any performance differences except for in like games or your business app's hottest of hot paths.
  * [This code](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec02/Program.cs#L54) is making good use of value tuples which we love at work.  The code (`(depth: 0, position: 0, aim: 0)`) becomes effectively an anonymous struct with three `int` properties that has memberwise value equality.  [This code](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec02/Program.cs#L53) parses up the space-delimited strings into an enum using the built-in enum parsing with case-insensitivity turned on via the `true`, even though the data does not appear to require this.  I don't like the idea that I should feel compelled to make my program match the data, but again if this was a super hot path I could see doing so.  This solution demonstrates the LINQ `Aggregate()` method which is effectively the same as `Reduce()` in JS.  Lastly, the solution is  using C# 9 switch expressions to set each property on the returned aggregate in a single statement. For example, [this code](https://github.com/nycdotnet/advent-of-code-2021/blob/main/adventofcode2021-dec02/Program.cs#L60-L64) is adding a number to the position on every aggregate; the number is either the current `amount` from the element when the direction is `forward`, or else it's 0. 

### Day 3:

  * Well, that escalated quickly!  Right off the bat on Day 3 we needed to parse some binary.  There's two helper data structures in .NET for holding bit arrays with helper methods, `BitArray` and `BitVector32`.  `BitArray` uses an `int[]` as its [backing store](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Collections/src/System/Collections/BitArray.cs#L19) and has helper methods like Get/Set/And/Or/Xor, etc. that work on the arbitrarily long set of bits.  I thought it was interesting that the docs sort of [discouraged its use](https://docs.microsoft.com/en-us/dotnet/api/system.collections.bitarray?view=net-6.0#remarks) and pushed you toward `BitVector32` which is effectively a [wrapper for a uint](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Collections.Specialized/src/System/Collections/Specialized/BitVector32.cs#L15) that requires you to write some [boilerplate code](https://docs.microsoft.com/en-us/dotnet/api/system.collections.specialized.bitvector32.createsection?view=net-6.0#examples) to carve up the 32 bits into arbitrarily-sized `short`s or smaller chunks down to even 1 bit.  `BitVector32` seems like a great choice for game code where a sprite's stats might fit into various small unsigned integers - `BitVector32` provides an API that makes packing such data into a `uint` convenient and very fast.
  * The data in the challenge was only 12 bits wide, but I don't think we can guarantee that future binary stuff won't be wider, so I decided to go with `BitArray`.  The next step was to figure out how to turn strings of 0s and 1s into a `BitArray`.  The built-in method looks something like this: `Convert.ToInt32("01010001", 2)` (2 meaning it's binary).  This limits us to 32 bits of data, though.  Since I wanted to support arbitrary-length binary, I thought that a `BitArray.TryParse()` method would work, but then quickly realized that this would mean a static extension method, which isn't a thing.  So googling around lead me to the latest proposal is [Extension Function Members](https://github.com/dotnet/csharplang/issues/192), the name of which feels a bit like George Carlin's old bit about "PTSD" vs "Shell Shock".  I prefer its immediate ancestor [Extension Everything](https://github.com/dotnet/roslyn/issues/11159) on the Roslyn issues. Ultimately it would be really nice if we got this, but there are many of ways to get the functionality I want, anyway, if not in as pleasing a form.
  * My selected unelegant workaround was to write a `TryParseToBitArray` extension on `string`.  Being that when dotting off a string you effectively NEVER want to do this, except in this specific case, I hid it in a `common.BitArrayExtensionMethods` namespace.  I don't care for this from a discoverability perspective.  It would be so much nicer to have been able to extend BitArray with a `BitArray.TryParse` static extension.  If you agree, upvote both of the issues above.  I also put together a `Format` extension method for `BitArray`; at least this was able to be on the `BitArray` instance.
  * I added some tests for these extension methods using the setup we often do at Namely: xunit latest, theories when possible, FluentAssertions for the assertions part, and keep it short and simple.  I don't think it's worth doing codecov for this as of yet, but I may change my mind later when things get more complex.  My method for doing these daily challenges so far has been to get them working using the example data and the expected, and then just running my code against the bigger data set.  I suppose this is a form of forced moral equivalent of TDD, but I have not been doing using formal unit tests, just stepping through the code in the debugger and making sure it shows what I expect for the main part.  I did write a bunch of tests to poke around the edges of `BitVector32` and `BitArray`.
  * Wow - does the new Test Explorer <i>POP</i> on Visual Studio 2022 with a toy project like this one!!!  It feels great.  Too bad it's still so slow on a real solution with thousands of tests and dozens of projects.  I have been meaning to reach out to the Test Explorer team about this.  I suspect that talking to users and having a friendly person with a real project on which they could run a profiler might make a huge difference.
  * I was disappointed to find that when using FluentAssertions, calling `x.Should().NotBeNull();` does not work with the C# nullable reference types feature and make `x` not be null after that call.  I wonder if that's a limitation of the nullable reference types feature or of FluentAssertions?  (edit: seems they're [considering it](https://github.com/fluentassertions/fluentassertions/issues/1672) for v7.  Too bad v6 has so many breaking changes - the good kind where at least your build breaks - so it'll be a while before we upgrade from v5.  I think it's awesome that some aspects of TypeScript type inference have made it into C#.
  * Day 3 part 2 led me into more fun with `BitArray` - this time I wanted to count the set bits, which led me to a `Cardinality` extension method.  The example I liked most uses the `BitOperations.PopCount` method.  I even tried to use [stackalloc](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/stackalloc) on the array, which appears in the position of the `new` keyword and requires explicitly typing the variable as `Span<T>` (or else it would be a pointer and require `unsafe`).  Would have been great, except `BitArray` only supports `CopyTo` which requires a proper array for the destination.  Ultimately, I didn't use the Cardinality extension since I needed to count bits in a position across multiple `BitArrays` rather than bits in a single BitArray.

### Day 4:
  * Funny how when there's more of a business problem I get away from `BitArray` and friends and go back to the usual suspects such as generic `List<T>`/`IEnumerable<T>`, Linq, `bool[]`, etc..  This one was solved mostly via a `BingoBoard` class with methods like `.Play()` and `.IsWinner()`.  I tried a bit of parallelization with both `Parallel.ForEach()` and Plinq, but removed it as it was slower with the data and scale involved.  On my laptop in Release configuraion, the second solution ran in about 2.5 ms with parallelization and 1.5 ms without.  This is with a crummy `Stopwatch` and running in a loop 100x.  Would be good to set up a [BenchmarkDotNet](https://benchmarkdotnet.org/) harness in the future to get a more accurate reading.

### Day 5:
  * This was an interesting one with the diagonals.  I decided to normalize the line segments and only added support for drawing "points", South, East, and diagonals going North East or South East.  I did this by swapping the coordinates when they went the other way which helped the rasterizing code be a bit simpler.  There is probably a better way to do this.  I used tuples to do the switching.
  * This one had the same program for part 1 and part 2 - they just asked you to filter out diagonals in part 1 and I did this by making the solution method take a predicate to apply to the input data.
  * This is also the first time I've worked with named RegEx capture groups.  I'm curious of the performance when getting by name vs index, but it certainly seemed fast to me.  The colorization of RegEx strings in VS 2019/2022 is pretty nice.

## Day 6:
  * This was the first time the most obvious approach had scaling problems once the number of iterations grew.  Since simulating the fish just required knowing the count of each fish in a certain state, it was straight-forward to refactor the implementation to use a `Dictionary<int,long>`.  Using the naive `List<int>` approach, the debugger console was showing major allocations and the fans definitely spun up on my laptop!