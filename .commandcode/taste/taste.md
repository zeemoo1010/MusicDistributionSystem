# architecture
See [architecture/taste.md](architecture/taste.md)

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
- Use email as the primary registration method (not phone number). Confidence: 0.70

# logging
- Use Serilog for file-based logging to track application events and errors. Confidence: 0.65
- Use claims-based cookie authentication with ASP.NET Core CookieAuthenticationDefaults for role/identity management. Confidence: 0.65

# communication
- When user says "continue" in response to an established plan or task being complete, proceed autonomously with the next logical step rather than asking "what would you like next?" — the user expects forward momentum, not re-direction. Confidence: 0.85
- When making an architectural judgment call (e.g., deferring a consolidation because entities serve different purposes), document the reasoning clearly and move on — the user values thoughtful, pragmatic scope decisions over blind adherence to a task list. Confidence: 0.65

# documentation
- Produce structured, categorized professional audit/review reports organized by dimension (architecture, security, testing, performance) with severity ratings and phased improvement roadmaps. Confidence: 0.70

# testing
- Evaluate test coverage systematically — identify what is tested, what is missing, and what critical functionality lacks coverage. Confidence: 0.65
- Evaluate projects from multiple stakeholder perspectives (architect, engineer, QA, DevOps, product) for a well-rounded assessment. Confidence: 0.65

# workflow
- After completing a comprehensive analysis/audit, execute improvements in structured, prioritized phases — starting with critical structural fixes first — and autonomously progress through the tasks without requiring per-step approval. Confidence: 0.78
