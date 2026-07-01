# architecture
- Use Clean/Onion Architecture with layers: Domain, Application, Infrastructure, Persistence, and Web. Confidence: 0.85
- Keep Controllers thin; move business logic, data mapping, and validation into Services. Confidence: 0.85
- Do NOT rebuild the project from scratch; always preserve and build upon the existing codebase. Confidence: 0.85
- Do NOT add unnecessary features or overengineer; focus only on needed fixes and improvements. Confidence: 0.75
- Include standard CRUD actions (Index, Details, Create, Edit/Update, Delete) in Controllers. Confidence: 0.60

# code-style
- Use Guid instead of int for entity IDs for security, uniqueness, and scalability. Confidence: 0.85
- Use LINQ and Entity Framework ORM instead of raw SQL queries for maintainability. Confidence: 0.65
- Place enum classes in a dedicated Enums folder, not inside the Models folder. Confidence: 0.70

# payments
- Use Paystack for payment integration. Confidence: 0.50

# project-structure
- Use proper separation of concerns: Entity/Domain Layer, DTO Layer, Repository Layer, Service Layer, Controller Layer. Confidence: 0.75
- Ensure Application depends only on Domain and Application.Contracts interfaces. Confidence: 0.70
- Infrastructure/Persistence should implement interfaces defined in Application.Contracts. Confidence: 0.70

# auth
- Send OTP verification codes directly to the user's email, not to a log file. Confidence: 0.70

# authentication
- Use claims-based authentication with Cookie Authentication (create claims for UserId, Email, Username, Role on successful login). Confidence: 0.70

# authentication
- Use email as the primary registration method (not phone number). Confidence: 0.70

# logging
- Use Serilog for file-based logging to track application events and errors. Confidence: 0.65
- Use claims-based cookie authentication with ASP.NET Core CookieAuthenticationDefaults for role/identity management. Confidence: 0.65