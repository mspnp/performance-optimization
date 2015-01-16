<comment>making a change to demonstrate content collaboration</comment>

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
In the context of the anti-pattern, we want to discuss how to capture meaningful data about the impact of the problem on the system. The emphasis here should be on the type of work and the data we're collecting; not necessarily about tools and how to use them.
We should favor showing real measurable data, preferrably numeric. The data should set a baseline so that we can validate the solution.

## How to correct the problem
Here we discuss how to replace the anti-pattern with something more approriate. There may be more than one solution, but we do not to be comprehensive. The solution should be practical. The solution should be relative limited in scope. If the solution requires a "big design change", the anti-pattern in question may not be a good fit for this guidance.

``` C#
// We want to show the counter sample to the demonstration of the anti-pattern earlier

for (var message in batch) {
 await someOperationAsync(message); // <- this is non blocking, but still not parallel!
}
```
[Link to the related sample][fullDemonstrationOfSolution]

## How to validate the solution
This section builds on the data collected above. After implementing the fix, we should be able to demonstrate a significant and value change in the data. We should provide some metrics about the expected change in data. This should be practical actionable information, but we should recognize that it cannot be overly specific.

## What problems will this uncover?
This section is very contextual and may not exist for every pattern. The idea is that fixing one problem in a system will likely reveal other problems that were not visible before. We want to give the reader a sense of what they can expect.
For example, increasing the the throughput of your front-end web service may result in overwhelming a downstream service that was previously thought to "run fine".

[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
