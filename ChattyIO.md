# Chatty I/O

Network calls and other I/O operations are inherently slow compared to compute tasks. Each I/O request typically incorporates significant overhead, and the cumulative effect of a large number of requests can have a significant impact on the performance and responsiveness of the system.

Common examples of chatty I/O operations include:

- Sending a series of requests that constitute a single logical operation to a web service. This often occurs when a system attempts to follow an object-oriented paradigm and handle remote objects as though they were local items held in application memory. This approach can result in an excessive number of network round-trips. For example, the following web API exposes the individual properties of *User* objects through a REST interface. Each method in the REST interface takes a parameter that identifies a user. While this approach is efficient if an application only needs to obtain one selected piece of information, in many cases it is likely that the application will actually require more than one property of a *User* object, resulting in multiple requests as shown in the C# client code snippet.

**C# web API**

```C#
public class User
{
    public int UserID { get; set; }
    public string UserName { get; set; }
    public char? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    ...
}
...
public class UserController : ApiController
{
    ...
    // GET: /Users/{id}/UserName
    [HttpGet]
    [Route("users/{id:int}/username")]
    public HttpResponseMessage GetUserName(int id)
    {
        ...
    }

    // GET: /Users/{id}/Gender
    [HttpGet]
    [Route("users/{id:int}/gender")]
    public HttpResponseMessage GetGender(int id)
    {
        ...
    }

    // GET: /Users/{id}/DateOfBirth
    [HttpGet]
    [Route("users/{id:int}/dateofbirth")]
    public HttpResponseMessage GetDateOfBirth(int id)
    {
        ...
    }
}
```

**C# client**

```C#
var client = new HttpClient();
client.BaseAddress = new Uri("...");
...
// Fetch the data for user 1
HttpResponseMessage response = await client.GetAsync("users/1/username");
response.EnsureSuccessStatusCode();
var userName = await response.Content.ReadAsStringAsync();

response = await client.GetAsync("users/1/gender");
response.EnsureSuccessStatusCode();
var gender = await response.Content.ReadAsStringAsync();

response = await client.GetAsync("users/1/dateofbirth");
response.EnsureSuccessStatusCode();
var dob = await response.Content.ReadAsStringAsync();
...
```

- Saving individual records to a database. For example, an application that enables a user to modify multiple records might save changes made to each record individually. The next example shows part of an application built using the Entity Framework. The application stores information about customers' orders in a SQL database. An order can comprise multiple line items. To create a new order, the application creates a `SalesOrderHeader` record together with `SalesOrderDetail` records for each line item. The application saves the changes back to the database after each record has been added. Each time a save operation occurs, the Entity Framework performs the following tasks, each of can incur further network and/or file I/O:
	- Open a connection to the database,
	- Start a new transaction,
	- Insert the new data into the appropriate table in the database,
	- Commit the transaction (assuming that the insert succeeds), and
	- Close the connection to the database.

**C#**

```C#
var context = ...;

// Create the sales order header
var header = new SalesOrderHeader()
{
    OrderDate = DateTime.Now,
    CustomerID = 29825,
    ...
};

// Save the new sales order header to the database
context.SalesOrderHeaders.Add(header);
await context.SaveChangesAsync();

// Create the first line item for this sales order
var details1 = new SalesOrderDetail()
{
    ProductID = 776,
    OrderQty = 3,
    ...
};

// Save the line item to the database
header.SalesOrderDetails.Add(details1);
await context.SaveChangesAsync();

// Create the second line item
var details2 = new SalesOrderDetail()
{
    ProductID = 778,
    OrderQty = 2,
    ...
};

// Save the second line item
header.SalesOrderDetails.Add(details2);
await context.SaveChangesAsync();
```

- Reading and writing to a file on disk. Performing file I/O involves opening a file and moving to the appropriate point before reading or writing data. When the operation is complete the file might be closed to save operating system resources. An application that continually reads and writes small amounts of information to a file will generate significant I/O overhead. Note that repeated small write requests can also lead to file fragmentation, slowing subsequent I/O operations still further. The following example shows part of an application that creates new *Customer* objects and writes customer information to a file. The details of each customer are stored by using the *SaveCustomerToFileAsync* method immediately after it is created. Note that the *FileStream* object utilized for writing to the file is created and destroyed automatically by virtue of being managed by a *using* block. Each time the *FileStream* object is created the specified file is opened, and when the *FileStream* object is destroyed the file is closed. If this method is invoked repeatedly as new customers are added, the I/O overhead can quickly accumulate.

**C#**

```C#
[Serializable]
public struct Customer
{
    public int Id;
    public string Name;
    ...
}
...
// Create a new customer and save it to a file
var cust = new Customer(...);
await SaveCustomerToFileAsync(cust);
...
// Save a single customer object to a file
private async Task SaveCustomerToFileAsync(Customer cust)
{
    using (Stream fileStream = new FileStream(CustomersFileName, FileMode.Append))
    {
        BinaryFormatter formatter = new BinaryFormatter();
        byte [] data = null;
        using (MemoryStream memStream = new MemoryStream())
        {
            formatter.Serialize(memStream, cust);
            data = memStream.ToArray();
        }
        await fileStream.WriteAsync(data, 0, data.Length);
    }
}
```

[Link to the related sample][fullDemonstrationOfProblem]

## How to detect the problem

End-users are likely to report extended response times and possible failures (caused by services timing out due to increased contention for I/O resources). An operator monitoring the system might observe the following symptoms:

*NOTE: NEED TO QUANTIFY THESE - WHICH PERF COUNTERS TO USE, WHAT THRESHOLDS TO LOOK FOR, ...*
- Applications and services becoming I/O bound.
- A large number of network requests made by an application instance to the same service.
- A large number of requests made by an application instance to the same data store.
- A large number of I/O requests made to the same file.

## How to correct the problem

Reduce the number of I/O requests by packaging the data that your application reads or writes into larger, fewer requests.

In the web API example shown earlier, rather than exposing individual properties of *User* objects through the REST interface, provide a method that retrieves an entire *User* object within a single request.

**C# web API**

```C#
public class User
{
    public int UserID { get; set; }
    public string UserName { get; set; }
    public char? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
}
...
public class UserController : ApiController
{
    ...
    // GET: /Users/{id}
    [HttpGet]
    [Route("users/{id:int}")]
    public HttpResponseMessage GetUser(int id)
    {
        ...
    }
}
```

**C# client**

```C#
var client = new HttpClient();
client.BaseAddress = new Uri("...");
...
// Fetch the data for user 1
HttpResponseMessage response = await client.GetAsync("users/1");
response.EnsureSuccessStatusCode();
var user = await response.Content.ReadAsStringAsync();
...
```

In the Entity Framework example, you should save all the changes as a batch by using a single transaction. This approach prevents the application from creating and closing multiple connections and transmitting the data for each operation individually, reducing the volume of I/O required. As a side-benefit, this can also help to maintain consistency; if the `SalesOrderHeader` and `SalesOrderDetail` records are saved individually, there is a possibility that a concurrent process could query this order before all the details have been stored and be presented with incomplete information.

**C#**
```C#
var context = ...;

// Create the sales order header
var header = new SalesOrderHeader()
{
    OrderDate = DateTime.Now,
    CustomerID = 29825,
    ...
};
context.SalesOrderHeaders.Add(header);

// Create the first line item for this sales order
var details1 = new SalesOrderDetail()
{
    ProductID = 776,
    OrderQty = 3,
    ...
};

header.SalesOrderDetails.Add(details1);

// Create the second line item
var details2 = new SalesOrderDetail()
{
    ProductID = 778,
    OrderQty = 2,
    ...
};

header.SalesOrderDetails.Add(details2);

// Save the new sales order header and details to the database in a single transaction
await context.SaveChangesAsync();
```

In the file I/O example, you could buffer data in memory and provide a separate operation that writes the buffered data to the file as a single operation. This approach reduces the overhead associated with repeatedly opening and closing the file, and helps to reduce fragmentation of the file on disk.

**C#**
```C#
[Serializable]
public struct Customer
{
    public int Id;
    public string Name;
    ...
}
...

// In-memory buffer for customers
List<Customer> customers = new List<Customers>();
...

// Create a new customer and add it to the buffer
var cust = new Customer(...);
customers.Add(cust);

// Add further customers to the list as they are created
...

// Save the contents of the list, writing all customers in a single operation
await SaveCustomerListToFileAsync(customers);
...

// Save a list of customer objects to a file
private async Task SaveCustomerListToFileAsync(List<Customer> customers)
{
    using (Stream fileStream = new FileStream(CustomersFileName, FileMode.Append))
    {
        BinaryFormatter formatter = new BinaryFormatter();
        foreach (var cust in customers)
        {
            byte[] data = null;
            using (MemoryStream memStream = new MemoryStream())
            {
                formatter.Serialize(memStream, cust);
                data = memStream.ToArray();
            }
            await fileStream.WriteAsync(data, 0, data.Length);
        }
    }
}
```

You should consider the following points:

- When reading data, do not make your I/O requests too large. An application should only retrieve the information that it is likely to use. It may be necessary to partition the information for an object into two chunks; *frequently accessed data* that accounts for 90% of the requests and *less frequently accessed data* that is used only 10% of the time. When an application requests data, it should be retrieve  the *90%* chunk. The *10%* chunk should only be fetched if necessary. If the *10%* chunk constitues a large proportion of the information for an object this approach can save significant I/O overhead.
- When writing data, avoid locking resources for longer than necessary to reduce the chances of contention during a lengthy operation. If a write operation spans multiple data stores, files, or services then adopt an eventually consistent approach (see [eventual consistency][eventual-consistency] for details).
- *FURTHER NOTES AND BULLETS*


[Link to the related sample][fullDemonstrationOfSolution]


## How to validate the solution
The system should spend less time performing I/O, and contention for I/O resources should be decreased. This should manifest itself as an improvement in response time in an application.
*(NOTE: NEED TO ADD SOME QUANTIFIABLE GUIDANCE)*

## What problems will this uncover?
*TBD - Need more input from the developers*.


[fullDemonstrationOfProblem]: http://github.com/mspnp/performance-optimization/xyz
[fullDemonstrationOfSolution]: http://github.com/mspnp/performance-optimization/123
[eventual-consistency]: http://LINK_TO_CONSISTENCY_GUIDANCE
