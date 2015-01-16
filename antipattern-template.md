# Name of Anti-Pattern (e.g. Synchronous I/O)

Here we give a concise description of the anti-pattern. We should answer the following questions as appropriate:
- Why is this anti-pattern is a problem in the context of the cloud (assuming that it is not a problem in other contexts)?
- What might cause a developer to follow the anti-pattern (instead of _doing the right thing_ by default)?
- What are the symptoms that the anti-pattern exhibits in a system? 

``` C#
// if possible, we should provide inline snippets here demonstrating the anti-pattern
// the snippet should be clearly indicate where the problem spots are

for (var message in batch) {
 someSynchronousOperation(message); // <- this is blocking and could be parallelized!
}
```
[Link to the related sample][fullDemonstrationOfProblem]

## How to detect the problem
In the context of the anti-pattern, we want to discuss how to capture meaningful data about the impact of the problem on the system. The emphasis here should be on the type of work and not necessarily the tools.
In the case of sychrononous I/O, we might take about the overall time to process a 

## How to correct the problem

## How to validate the solution

## What problems will this uncover?


[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
