# Name of Anti-Pattern (e.g. Synchronous I/O)

Here we give a concise description of the anti-pattern. We should answer the following questions as appropriate:
- Why is this anti-pattern is a problem in the context of the cloud (assuming that it is not a problem in other contexts)?
- What might cause a developer to follow the anti-pattern (instead of _doing the right thing_ by default)?
- What are the symptoms that the anti-pattern exhibits in a system? 

``` C#
\\ if possible, we should provide inline snippets here demonstrating the anti-pattern
\\ the snippet should be clearly indicate where the problem spots are

for (var i = 0; i < 99999; i++) {
 someSynchronousOperation(); // <- this is blocking and could be parallelized!
}
```
[Link to the related sample][codeWithProblem]

## How to detect the problem

## How to correct the problem

## How to validate the solution

## What problems will this uncover?


[codeWithProblem]: http://github.com/mspnp/performance-optimization/xyz
