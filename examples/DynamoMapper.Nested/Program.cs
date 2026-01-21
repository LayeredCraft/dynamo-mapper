using Amazon.DynamoDBv2.Model;
using DynamoMapper.Nested;

Console.WriteLine("=== DynamoMapper Nested Object Examples ===\n");

// Example 1: Simple inline nested object (Order with Address)
Console.WriteLine("--- Example 1: Simple Inline Nested Object ---");

var order =
    new Order
    {
        Id = "ORD-123",
        ShippingAddress =
            new Address { Line1 = "123 Main St", City = "Seattle", PostalCode = "98101", },
    };

Console.WriteLine($"Original Order: {order}");

// Serialize to DynamoDB
var orderItem = OrderMapper.ToItem(order);
Console.WriteLine($"Serialized to DynamoDB: {FormatAttributeValue(orderItem)}");

// Deserialize back
var orderFromDb = OrderMapper.FromItem(orderItem);
Console.WriteLine($"Deserialized Order: {orderFromDb}");
Console.WriteLine();

// Example: Create from raw DynamoDB item
Console.WriteLine("--- Example: FromItem with raw DynamoDB data ---");

var rawItem =
    new Dictionary<string, AttributeValue>
    {
        ["id"] = new() { S = "ORD-456" },
        ["shippingAddress"] =
            new()
            {
                M =
                    new Dictionary<string, AttributeValue>
                    {
                        ["line1"] = new() { S = "456 Oak Ave" },
                        ["city"] = new() { S = "Portland" },
                        ["postalCode"] = new() { S = "97201" },
                    },
            },
    };

var orderFromRaw = OrderMapper.FromItem(rawItem);
Console.WriteLine($"Order from raw DynamoDB data: {orderFromRaw}");
Console.WriteLine();

// Example 2: Nullable nested object
Console.WriteLine("--- Example 2: Nullable Nested Object ---");

var productWithWarranty =
    new Product
    {
        Sku = "LAPTOP-001",
        Name = "Gaming Laptop",
        Warranty = new Warranty { DurationMonths = 24, Provider = "TechCare Inc.", },
    };

var productWithoutWarranty =
    new Product
    {
        Sku = "CABLE-001", Name = "USB Cable", Warranty = null, // No warranty
    };

Console.WriteLine($"Product with warranty: {productWithWarranty}");
var productItem1 = ProductMapper.ToItem(productWithWarranty);
Console.WriteLine($"Serialized: {FormatAttributeValue(productItem1)}");

Console.WriteLine($"\nProduct without warranty: {productWithoutWarranty}");
var productItem2 = ProductMapper.ToItem(productWithoutWarranty);
Console.WriteLine($"Serialized: {FormatAttributeValue(productItem2)}");

var productBack = ProductMapper.FromItem(productItem1);
Console.WriteLine($"Deserialized: {productBack}");
Console.WriteLine();

// Example 3: Multi-level nested objects
Console.WriteLine("--- Example 3: Multi-Level Nested Objects ---");

var company =
    new Company
    {
        Id = "ACME-001",
        Name = "Acme Corporation",
        HeadOffice =
            new Department
            {
                Code = "HQ",
                Name = "Headquarters",
                Lead = new Manager { EmployeeId = "EMP-100", FullName = "Jane Smith", },
            },
    };

Console.WriteLine($"Company: {company}");
var companyItem = CompanyMapper.ToItem(company);
Console.WriteLine($"Serialized: {FormatAttributeValue(companyItem)}");

var companyBack = CompanyMapper.FromItem(companyItem);
Console.WriteLine($"Deserialized: {companyBack}");
Console.WriteLine();

// Example 4: Nested object with various scalar types
Console.WriteLine("--- Example 4: Nested Object with Scalar Types ---");

var invoice =
    new Invoice
    {
        InvoiceNumber = "INV-2024-001",
        Payment =
            new PaymentInfo
            {
                Amount = 1299.99m,
                PaidAt = new DateTime(2024, 1, 15, 10, 30, 0),
                IsRefundable = true,
                InstallmentCount = 3,
            },
    };

Console.WriteLine($"Invoice: {invoice}");
var invoiceItem = InvoiceMapper.ToItem(invoice);
Console.WriteLine($"Serialized: {FormatAttributeValue(invoiceItem)}");

var invoiceBack = InvoiceMapper.FromItem(invoiceItem);
Console.WriteLine($"Deserialized: {invoiceBack}");
Console.WriteLine();

// Example 5: Mapper-based nested object
Console.WriteLine("--- Example 5: Mapper-Based Nested Object ---");

var blogPost =
    new BlogPost
    {
        Slug = "hello-world",
        Title = "Hello World!",
        Writer =
            new Author
            {
                Handle = "jdoe",
                DisplayName = "John Doe",
                Bio = "Software developer and blogger",
            },
    };

Console.WriteLine($"BlogPost: {blogPost}");
var blogPostItem = BlogPostMapper.ToItem(blogPost);
Console.WriteLine($"Serialized: {FormatAttributeValue(blogPostItem)}");

var blogPostBack = BlogPostMapper.FromItem(blogPostItem);
Console.WriteLine($"Deserialized: {blogPostBack}");
Console.WriteLine();

// Example 6: List of nested objects
Console.WriteLine("--- Example 6: List of Nested Objects ---");

var shoppingCart =
    new ShoppingCart
    {
        CartId = "CART-001",
        Items =
        [
            new CartItem
            {
                ProductId = "PROD-001",
                ProductName = "Wireless Mouse",
                Quantity = 2,
                UnitPrice = 29.99m,
            },
            new CartItem
            {
                ProductId = "PROD-002",
                ProductName = "USB-C Hub",
                Quantity = 1,
                UnitPrice = 49.99m,
            },
        ],
    };

Console.WriteLine($"Shopping Cart: {shoppingCart}");
var cartItem = ShoppingCartMapper.ToItem(shoppingCart);
Console.WriteLine($"Serialized: {FormatAttributeValue(cartItem)}");

var cartBack = ShoppingCartMapper.FromItem(cartItem);
Console.WriteLine($"Deserialized: {cartBack}");
Console.WriteLine();

// Example 7: Dictionary of nested objects
Console.WriteLine("--- Example 7: Dictionary of Nested Objects ---");

var directory =
    new EmployeeDirectory
    {
        DepartmentId = "DEPT-001",
        DepartmentName = "Engineering",
        Employees =
            new Dictionary<string, Employee>
            {
                ["emp001"] =
                    new Employee
                    {
                        Name = "Alice Johnson",
                        Title = "Senior Engineer",
                        Salary = 120000m,
                    },
                ["emp002"] =
                    new Employee
                    {
                        Name = "Bob Smith", Title = "Junior Engineer", Salary = 75000m,
                    },
            },
    };

Console.WriteLine($"Employee Directory: {directory}");
var directoryItem = EmployeeDirectoryMapper.ToItem(directory);
Console.WriteLine($"Serialized: {FormatAttributeValue(directoryItem)}");

var directoryBack = EmployeeDirectoryMapper.FromItem(directoryItem);
Console.WriteLine($"Deserialized: {directoryBack}");
Console.WriteLine();

// Helper method to format AttributeValue for display
static string FormatAttributeValue(Dictionary<string, AttributeValue> item, int indent = 0)
{
    var prefix = new string(' ', indent * 2);
    var lines = new List<string> { "{" };
    lines.AddRange(
        from kvp in item
        let value = FormatSingleAttributeValue(kvp.Value, indent + 1)
        select $"{prefix}  \"{kvp.Key}\": {value},"
    );

    lines.Add($"{prefix}}}");
    return string.Join("\n", lines);
}

static string FormatSingleAttributeValue(AttributeValue av, int indent)
{
    return av switch
    {
        { S: not null } => $"\"{av.S}\"",
        { N: not null } => av.N,
        { IsBOOLSet: true, BOOL: var b } => b.ToString()!.ToLower(),
        { NULL: true } => "null",
        { M: not null } => FormatAttributeValue(av.M, indent),
        { L: not null } => FormatList(av.L, indent),
        _ => "<?>",
    };
}

static string FormatList(List<AttributeValue> list, int indent)
{
    if (list.Count == 0) return "[]";

    var prefix = new string(' ', indent * 2);
    var lines = new List<string> { "[" };
    lines.AddRange(
        list.Select(item => FormatSingleAttributeValue(item, indent + 1))
            .Select(value => $"{prefix}  {value},")
    );

    lines.Add($"{prefix}]");
    return string.Join("\n", lines);
}
