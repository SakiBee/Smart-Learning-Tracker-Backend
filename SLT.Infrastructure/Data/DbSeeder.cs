using Microsoft.EntityFrameworkCore;
using SLT.Core.Entities;
using SLT.Core.Enums;

namespace SLT.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Run migrations first
        await context.Database.MigrateAsync();

        // Skip if already seeded
        if (await context.Users.AnyAsync()) return;

        Console.WriteLine("🌱 Seeding database...");

        // ── 1. USERS ─────────────────────────────────────────────────────────
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

        var alex = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            FullName = "Alex Rahman",
            Email = "alex@slt.dev",
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow.AddDays(-45),
            UpdatedAt = DateTime.UtcNow.AddDays(-45),
        };

        var sara = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            FullName = "Sara Hossain",
            Email = "sara@slt.dev",
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-30),
        };

        var nabil = new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            FullName = "Nabil Khan",
            Email = "nabil@slt.dev",
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            UpdatedAt = DateTime.UtcNow.AddDays(-20),
        };

        await context.Users.AddRangeAsync(alex, sara, nabil);
        await context.SaveChangesAsync();
        Console.WriteLine("Users seeded");

        // ── 2. TAGS ───────────────────────────────────────────────────────────
        var tags = new Dictionary<string, Tag>();

        var tagNames = new[]
        {
            // Alex's tags
            ("dotnet",         alex.Id),
            ("csharp",         alex.Id),
            ("architecture",   alex.Id),
            ("postgresql",     alex.Id),
            ("react",          alex.Id),
            ("nextjs",         alex.Id),
            ("javascript",     alex.Id),
            ("typescript",     alex.Id),
            ("career",         alex.Id),
            ("productivity",   alex.Id),
            ("ai",             alex.Id),
            ("machinelearning",alex.Id),
            ("docker",         alex.Id),
            ("devops",         alex.Id),
            ("api",            alex.Id),

            // Sara's tags
            ("ux",             sara.Id),
            ("design",         sara.Id),
            ("figma",          sara.Id),
            ("frontend",       sara.Id),
            ("css",            sara.Id),
            ("accessibility",  sara.Id),
            ("python",         sara.Id),
            ("dataanalysis",   sara.Id),
            ("career",         sara.Id), // will be differentiated by userId
            ("leadership",     sara.Id),

            // Nabil's tags
            ("startup",        nabil.Id),
            ("entrepreneurship",nabil.Id),
            ("marketing",      nabil.Id),
            ("growth",         nabil.Id),
            ("finance",        nabil.Id),
        };

        foreach (var (name, userId) in tagNames)
        {
            var key = $"{name}_{userId}";
            var tag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = name,
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddDays(-40),
                UpdatedAt = DateTime.UtcNow.AddDays(-40),
            };
            tags[key] = tag;
            await context.Tags.AddAsync(tag);
        }

        await context.SaveChangesAsync();
        Console.WriteLine("Tags seeded");

        // ── 3. LEARNING ENTRIES (Alex) ────────────────────────────────────────
        Tag T(string name, Guid userId) => tags[$"{name}_{userId}"];

        var alexEntries = new List<LearningEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = alex.Id,
                Url = "https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures",
                Title = "Common Web Application Architectures - Microsoft Docs",
                Author = "Microsoft",
                Summary = "This article explores common architectural patterns for modern web applications including Clean Architecture, traditional N-tier, and microservices. Clean Architecture keeps business logic independent from infrastructure concerns, making it easier to test and maintain over time.",
                KeyPoints = """["Clean Architecture separates code into concentric layers with dependencies pointing inward","Domain entities and business rules live in the Core layer with no external dependencies","Infrastructure concerns like databases and APIs are kept in outer layers","This pattern makes unit testing business logic straightforward without mocking databases","The pattern scales well as applications grow in complexity"]""",
                PersonalNotes = "This is exactly what I implemented in SLT. The key insight is that Core should never reference Infrastructure — only the other way around.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.High,
                IsRead = true,
                IsFavorite = true,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-40),
                UpdatedAt = DateTime.UtcNow.AddDays(-38),
                Tags = new List<Tag> { T("dotnet", alex.Id), T("architecture", alex.Id), T("csharp", alex.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = alex.Id,
                Url = "https://www.postgresql.org/docs/current/indexes.html",
                Title = "PostgreSQL Index Types and When to Use Them",
                Author = "PostgreSQL Global Development Group",
                Summary = "PostgreSQL supports several index types including B-tree, Hash, GiST, and GIN. B-tree indexes are the default and work for most use cases including equality and range queries. GIN indexes are optimized for full-text search and array operations.",
                KeyPoints = """["B-tree indexes work for equality, range, and sorting operations","Hash indexes are only useful for simple equality comparisons","GIN indexes excel at full-text search and jsonb queries","Partial indexes can dramatically reduce index size for filtered queries","Too many indexes slow down writes — only index what you query"]""",
                PersonalNotes = "I should add indexes on LearningEntries.UserId and CreatedAt since those are queried on every dashboard load.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.High,
                IsRead = true,
                IsFavorite = false,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-35),
                UpdatedAt = DateTime.UtcNow.AddDays(-34),
                Tags = new List<Tag> { T("postgresql", alex.Id), T("api", alex.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = alex.Id,
                Url = "https://jwt.io/introduction",
                Title = "Introduction to JSON Web Tokens",
                Author = "Auth0",
                Summary = "JWT is an open standard for securely transmitting information as a JSON object. Tokens are signed using a secret key or public/private key pair. JWTs are stateless — the server does not store session information, making them ideal for distributed systems and horizontal scaling.",
                KeyPoints = """["JWTs consist of three parts: Header, Payload, and Signature","The payload contains claims — statements about the user","Tokens are signed but not encrypted by default — never store sensitive data in payload","Stateless nature enables horizontal scaling without shared session storage","Short expiry times (minutes/hours) reduce risk if a token is compromised"]""",
                PersonalNotes = "I use 7-day expiry in SLT. For production apps with sensitive data, access tokens should be 15-30 minutes with refresh tokens.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.High,
                IsRead = true,
                IsFavorite = true,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-33),
                UpdatedAt = DateTime.UtcNow.AddDays(-32),
                Tags = new List<Tag> { T("dotnet", alex.Id), T("api", alex.Id), T("architecture", alex.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = alex.Id,
                Url = "https://nextjs.org/docs/app/building-your-application/routing",
                Title = "Next.js App Router — File-based Routing Deep Dive",
                Author = "Vercel",
                Summary = "The Next.js App Router introduces a new paradigm using React Server Components by default. Routing is file-system based — folders define routes and special files like page.tsx, layout.tsx, and loading.tsx define UI. Route groups using parentheses allow organizing routes without affecting the URL.",
                KeyPoints = """["The app/ directory uses React Server Components by default for better performance","layout.tsx wraps child routes and persists across navigation","loading.tsx automatically shows a loading UI using React Suspense","Route groups like (auth) organize routes without changing the URL path","Dynamic routes use square brackets like [id] in folder names"]""",
                PersonalNotes = "Route groups are what I used to separate the (auth) pages from the dashboard — elegant solution!",
                ContentType = ContentType.Tutorial,
                Priority = PriorityLevel.Medium,
                IsRead = true,
                IsFavorite = false,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-28),
                UpdatedAt = DateTime.UtcNow.AddDays(-27),
                Tags = new List<Tag> { T("nextjs", alex.Id), T("react", alex.Id), T("javascript", alex.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = alex.Id,
                Url = "https://www.docker.com/blog/how-to-dockerize-dotnet-application/",
                Title = "How to Dockerize a .NET Application",
                Author = "Docker Team",
                Summary = "Dockerizing .NET applications involves creating a Dockerfile using multi-stage builds to keep the final image small. The SDK image is used for building and the runtime-only aspnet image for the final container, reducing image size by 60-70% compared to single-stage builds.",
                KeyPoints = """["Use multi-stage builds to separate build environment from runtime","The aspnet:10.0 image is much smaller than sdk:10.0 — use it for production","EXPOSE declares which port the container listens on","Use COPY --from=build to copy only published artifacts",".dockerignore prevents unnecessary files from being included in the image"]""",
                PersonalNotes = "The Dockerfile I wrote for SLT follows exactly this multi-stage pattern. Railway auto-detects it.",
                ContentType = ContentType.Tutorial,
                Priority = PriorityLevel.Medium,
                IsRead = true,
                IsFavorite = false,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-24),
                Tags = new List<Tag> { T("docker", alex.Id), T("devops", alex.Id), T("dotnet", alex.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = alex.Id,
                Url = "https://groq.com/blog/llama-3-3-70b",
                Title = "LLaMA 3.3 70B — Groq's Fastest Open Source Model",
                Author = "Groq",
                Summary = "LLaMA 3.3 70B is Meta's latest open-source language model offering performance comparable to GPT-4 Turbo on reasoning and coding tasks. Groq's LPU inference hardware achieves over 1000 tokens/second — roughly 10x faster than GPU-based inference, making real-time AI features in applications practical.",
                KeyPoints = """["LLaMA 3.3 70B matches GPT-4 Turbo on many benchmarks at zero cost","Groq's LPU (Language Processing Unit) is purpose-built for transformer inference","14,400 free API requests per day on Groq's free tier","JSON mode ensures structured outputs suitable for application integration","Open weights mean the model can be self-hosted if needed"]""",
                PersonalNotes = "This is the AI backbone of SLT's summarization and flashcard generation. The speed is impressive — responses in under 2 seconds.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.High,
                IsRead = true,
                IsFavorite = true,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-22),
                UpdatedAt = DateTime.UtcNow.AddDays(-21),
                Tags = new List<Tag> { T("ai", alex.Id), T("machinelearning", alex.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = alex.Id,
                Url = "https://zustand-demo.pmnd.rs/",
                Title = "Zustand — Bear Necessities for State Management",
                Author = "pmndrs",
                Summary = "Zustand is a small, fast state management library for React that uses hooks. Unlike Redux, it requires no boilerplate — you define a store as a function with state and actions combined. It works outside React components and integrates naturally with TypeScript.",
                KeyPoints = """["Zustand stores are plain JavaScript objects with actions and state combined","No Provider wrapper needed — stores work anywhere in the component tree","Supports middleware like persist for localStorage integration","Much smaller bundle size than Redux (1KB vs 16KB)","Subscriptions are selector-based — components only re-render when their slice changes"]""",
                PersonalNotes = "I use Zustand for authStore and themeStore in SLT. The persist middleware would be useful to remove the manual localStorage calls I'm doing.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.Medium,
                IsRead = true,
                IsFavorite = false,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-18),
                UpdatedAt = DateTime.UtcNow.AddDays(-17),
                Tags = new List<Tag> { T("react", alex.Id), T("javascript", alex.Id), T("nextjs", alex.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = alex.Id,
                Url = "https://www.supermemo.com/en/blog/application-of-a-computer-to-improve-the-results-obtained-in-working-with-the-supermemo-method",
                Title = "The SM-2 Spaced Repetition Algorithm Explained",
                Author = "Piotr Woźniak",
                Summary = "The SM-2 algorithm schedules flashcard reviews based on recall quality. After each review, the card's ease factor and interval are adjusted. Cards answered correctly get longer intervals; cards answered incorrectly are reset. This mimics the brain's natural forgetting curve and dramatically improves long-term retention.",
                KeyPoints = """["The ease factor starts at 2.5 and adjusts based on recall quality (0-5 rating)","Interval grows exponentially for well-remembered cards","Poor recall resets the card to a 1-day interval","The algorithm is designed to review cards just before you forget them","Studies show spaced repetition improves retention by 200-500% vs mass study"]""",
                PersonalNotes = "I simplified the 0-5 rating to 4 buttons (Again/Hard/Good/Easy) for better UX. The core algorithm is the same.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.High,
                IsRead = true,
                IsFavorite = true,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-14),
                Tags = new List<Tag> { T("ai", alex.Id), T("productivity", alex.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = alex.Id,
                Url = "https://roadmap.sh/backend",
                Title = "Backend Developer Roadmap 2024",
                Author = "roadmap.sh",
                Summary = "A comprehensive visual guide to becoming a backend developer covering internet fundamentals, OS concepts, databases, APIs, caching, security, testing, and DevOps. The roadmap is opinionated but highlights essential skills that most backend roles require regardless of technology stack.",
                KeyPoints = """["Learn HTTP, DNS, and how browsers work before diving into frameworks","Master at least one relational database deeply before exploring NoSQL","Understand RESTful API design principles and status codes","Caching with Redis can reduce database load by 90% for read-heavy applications","CI/CD pipelines and container knowledge are now expected in most backend roles"]""",
                PersonalNotes = "Good reference to check what skills I still need to strengthen. Need to practice Redis and message queues.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.Medium,
                IsRead = false,
                IsFavorite = false,
                IsReadLater = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10),
                Tags = new List<Tag> { T("career", alex.Id), T("devops", alex.Id), T("api", alex.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = alex.Id,
                Url = "https://www.typescriptlang.org/docs/handbook/2/types-from-types.html",
                Title = "TypeScript Advanced Types — Utility Types and Mapped Types",
                Author = "Microsoft",
                Summary = "TypeScript's advanced type system allows creating new types from existing ones using utility types like Partial, Required, Pick, and Omit. Mapped types iterate over object properties to transform them. Template literal types enable string manipulation at the type level.",
                KeyPoints = """["Partial<T> makes all properties of T optional — useful for update DTOs","Pick<T, K> creates a type with only the specified keys","Record<K, V> creates an object type with keys K and values V","Mapped types use the 'in keyof' syntax to transform each property","Conditional types allow if-else logic at the type level"]""",
                PersonalNotes = "I should update my frontend types to use these utilities — especially Partial for update DTOs instead of having separate interfaces.",
                ContentType = ContentType.Tutorial,
                Priority = PriorityLevel.Low,
                IsRead = false,
                IsFavorite = false,
                IsReadLater = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                Tags = new List<Tag> { T("typescript", alex.Id), T("javascript", alex.Id) }
            },
        };

        await context.LearningEntries.AddRangeAsync(alexEntries);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Alex's entries seeded");

        // ── 4. LEARNING ENTRIES (Sara) ────────────────────────────────────────
        var saraEntries = new List<LearningEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = sara.Id,
                Url = "https://www.nngroup.com/articles/ten-usability-heuristics/",
                Title = "10 Usability Heuristics for User Interface Design",
                Author = "Jakob Nielsen",
                Summary = "Nielsen's 10 heuristics are general principles for interaction design. They include visibility of system status, match between system and real world, user control, consistency, error prevention, recognition over recall, flexibility, aesthetic design, help users recover from errors, and help documentation.",
                KeyPoints = """["Visibility of system status: always keep users informed about what's happening","Error prevention is more important than error recovery","Recognition is easier than recall — minimize what users need to remember","Consistency means following platform conventions users already know","Aesthetic design should remove irrelevant information, not just look pretty"]""",
                PersonalNotes = "The 'recognition over recall' principle is why I prefer visible navigation over hamburger menus in my designs.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.High,
                IsRead = true,
                IsFavorite = true,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-28),
                UpdatedAt = DateTime.UtcNow.AddDays(-27),
                Tags = new List<Tag> { T("ux", sara.Id), T("design", sara.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = sara.Id,
                Url = "https://web.dev/accessibility",
                Title = "Web Accessibility Fundamentals — web.dev",
                Author = "Google",
                Summary = "Web accessibility ensures people with disabilities can perceive, understand, navigate, and interact with websites. The WCAG 2.1 guidelines provide four principles: Perceivable, Operable, Understandable, and Robust (POUR). Proper semantic HTML, ARIA labels, color contrast ratios, and keyboard navigation are foundational.",
                KeyPoints = """["Use semantic HTML elements like nav, main, article, button instead of divs","Color contrast ratio must be at least 4.5:1 for normal text (WCAG AA)","All interactive elements must be keyboard accessible","Images need descriptive alt text unless decorative","ARIA labels supplement HTML semantics but should not replace them"]""",
                PersonalNotes = "I need to audit my portfolio for color contrast issues. WebAIM Contrast Checker is a good free tool.",
                ContentType = ContentType.Tutorial,
                Priority = PriorityLevel.High,
                IsRead = true,
                IsFavorite = true,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-24),
                Tags = new List<Tag> { T("accessibility", sara.Id), T("frontend", sara.Id), T("css", sara.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = sara.Id,
                Url = "https://www.figma.com/blog/design-tokens/",
                Title = "Design Tokens — The Bridge Between Design and Development",
                Author = "Figma",
                Summary = "Design tokens are named variables that store design decisions like colors, spacing, and typography. They create a shared language between designers and developers. When a brand color changes, updating one token propagates the change everywhere automatically.",
                KeyPoints = """["Tokens store decisions like primary-color or spacing-md as named variables","They bridge the gap between Figma designs and code implementation","Tools like Style Dictionary can transform tokens into CSS variables, SCSS, or JS","Semantic token names (color-action) are better than literal names (blue-500)","Component-level tokens reference global tokens for a two-tier system"]""",
                PersonalNotes = "Tailwind CSS is essentially a design token system. The config file is the token source of truth.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.Medium,
                IsRead = true,
                IsFavorite = false,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-19),
                Tags = new List<Tag> { T("design", sara.Id), T("figma", sara.Id), T("frontend", sara.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = sara.Id,
                Url = "https://pandas.pydata.org/docs/getting_started/intro_tutorials/",
                Title = "Pandas Getting Started — Data Analysis with Python",
                Author = "Pandas Development Team",
                Summary = "Pandas is the foundational data analysis library for Python. DataFrames are the core data structure — a 2D labeled table similar to an Excel spreadsheet. Pandas excels at reading data from CSV/Excel/SQL, cleaning missing values, filtering rows, grouping data, and computing aggregations.",
                KeyPoints = """["DataFrame is the primary data structure — think of it as a smart spreadsheet","read_csv() and read_excel() load data in one line","groupby() splits data into groups for aggregation operations","fillna() and dropna() handle missing values","merge() and join() combine DataFrames similar to SQL joins"]""",
                PersonalNotes = "Going through this for my data analysis side project. The groupby documentation is especially useful.",
                ContentType = ContentType.Tutorial,
                Priority = PriorityLevel.Medium,
                IsRead = false,
                IsFavorite = false,
                IsReadLater = true,
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                UpdatedAt = DateTime.UtcNow.AddDays(-12),
                Tags = new List<Tag> { T("python", sara.Id), T("dataanalysis", sara.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = sara.Id,
                Url = "https://hbr.org/2018/01/what-makes-a-good-manager-great",
                Title = "What Makes a Good Manager Great — HBR",
                Author = "Harvard Business Review",
                Summary = "Great managers distinguish themselves not through technical expertise alone but through their ability to develop others. The best managers show genuine care for their team, give specific actionable feedback, create psychological safety, and connect individual work to a larger purpose.",
                KeyPoints = """["Great managers know each team member's strengths and growth areas individually","Psychological safety — where people can speak up without fear — drives team performance","Regular 1:1s focused on the person, not just the work, build trust","Specific praise is more motivating than general positive feedback","Delegating meaningfully shows trust and develops team members faster"]""",
                PersonalNotes = "Useful now that I'm mentoring junior designers. The specific feedback point is something I want to practice more.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.Low,
                IsRead = true,
                IsFavorite = true,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-7),
                Tags = new List<Tag> { T("leadership", sara.Id), T("career", sara.Id) }
            },
        };

        await context.LearningEntries.AddRangeAsync(saraEntries);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Sara's entries seeded");

        // ── 5. LEARNING ENTRIES (Nabil) ───────────────────────────────────────
        var nabilEntries = new List<LearningEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = nabil.Id,
                Url = "https://www.ycombinator.com/library/4D-a-fundraising-survival-guide",
                Title = "A Fundraising Survival Guide — Paul Graham",
                Author = "Paul Graham",
                Summary = "Fundraising is one of the most distracting and demoralizing parts of starting a company. The key is to spend as little time on it as possible. Get to the point where you can raise money quickly by having a working product and initial traction. Investors fund people as much as ideas.",
                KeyPoints = """["Raise money when you have momentum — not when you need it desperately","The best pitches tell a compelling story about why now and why you","Talk to many investors in parallel — it creates urgency and better terms","Most meetings end in 'no' and that's normal — rejection is part of the process","Your credibility comes from what you've built, not what you plan to build"]""",
                PersonalNotes = "Reading this before our seed round. The advice about raising in parallel is counter-intuitive but makes sense.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.High,
                IsRead = true,
                IsFavorite = true,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-18),
                UpdatedAt = DateTime.UtcNow.AddDays(-17),
                Tags = new List<Tag> { T("startup", nabil.Id), T("entrepreneurship", nabil.Id), T("finance", nabil.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = nabil.Id,
                Url = "https://andrewchen.com/how-to-build-a-growth-team/",
                Title = "How to Build a Growth Team — Andrew Chen",
                Author = "Andrew Chen",
                Summary = "A growth team's job is to systematically increase key metrics through rapid experimentation. Unlike product teams focused on features, growth teams focus on acquisition, activation, retention, and revenue. The best growth teams run hundreds of small experiments per month rather than betting on big launches.",
                KeyPoints = """["Growth hacking is a mindset of rapid testing — fail fast, double down on winners","The AARRR funnel (Acquisition, Activation, Retention, Revenue, Referral) is the framework","Growth teams work best when they own a specific metric, not a feature","North Star Metric aligns the whole company on what actually matters","Most growth comes from retention, not acquisition — fix the leaky bucket first"]""",
                PersonalNotes = "We need a growth framework before hiring. Starting with retention is the right advice for SaaS.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.High,
                IsRead = true,
                IsFavorite = true,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-14),
                UpdatedAt = DateTime.UtcNow.AddDays(-13),
                Tags = new List<Tag> { T("growth", nabil.Id), T("marketing", nabil.Id), T("startup", nabil.Id) }
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = nabil.Id,
                Url = "https://www.investopedia.com/terms/u/unit-economics.asp",
                Title = "Unit Economics — Understanding CAC, LTV, and Payback Period",
                Author = "Investopedia",
                Summary = "Unit economics measure the direct revenues and costs associated with a particular business model on a per-unit basis. For SaaS, the key metrics are Customer Acquisition Cost (CAC) and Lifetime Value (LTV). A healthy SaaS business has an LTV:CAC ratio of at least 3:1.",
                KeyPoints = """["CAC is the total cost to acquire one paying customer including sales and marketing","LTV is the total revenue expected from a customer over their lifetime","LTV:CAC ratio above 3:1 is healthy; below 1:1 means you're losing money on each customer","Payback period should be under 12 months for early-stage startups","Churn rate is the silent killer — even small churn compounds to devastating LTV loss"]""",
                PersonalNotes = "Our CAC through content marketing is low but LTV calculation needs work. Need to track monthly churn properly.",
                ContentType = ContentType.Article,
                Priority = PriorityLevel.High,
                IsRead = true,
                IsFavorite = false,
                IsReadLater = false,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-9),
                Tags = new List<Tag> { T("finance", nabil.Id), T("startup", nabil.Id) }
            },
        };

        await context.LearningEntries.AddRangeAsync(nabilEntries);
        await context.SaveChangesAsync();
        Console.WriteLine("Nabil's entries seeded");

        // ── 6. FLASHCARDS ─────────────────────────────────────────────────────
        var flashcards = new List<Flashcard>
        {
            new()
            {
                Question = "What are the four layers in Clean Architecture and what does each do?",
                Answer = "Core (entities + interfaces), Application (DTOs + use cases), Infrastructure (DB + external services), and API (controllers + HTTP). Dependencies only point inward — outer layers know about inner layers, never the reverse.",
                LearningEntryId = alexEntries[0].Id,
                UserId = alex.Id,
                TimesReviewed = 5,
                TimesCorrect = 4,
                EaseFactor = 260,
                Interval = 12,
                NextReviewAt = DateTime.UtcNow.AddDays(8),
                LastReviewedAt = DateTime.UtcNow.AddDays(-4),
                CreatedAt = DateTime.UtcNow.AddDays(-38),
            },
            new()
            {
                Question = "What is the main advantage of Clean Architecture over traditional N-tier/MVC?",
                Answer = "Business logic and domain entities are completely independent of frameworks, databases, and UI. You can swap the database, change the API framework, or test business rules without touching infrastructure code.",
                LearningEntryId = alexEntries[0].Id,
                UserId = alex.Id,
                TimesReviewed = 4,
                TimesCorrect = 4,
                EaseFactor = 280,
                Interval = 21,
                NextReviewAt = DateTime.UtcNow.AddDays(14),
                LastReviewedAt = DateTime.UtcNow.AddDays(-7),
                CreatedAt = DateTime.UtcNow.AddDays(-38),
            },
            new()
            {
                Question = "What is the difference between a B-tree and a GIN index in PostgreSQL?",
                Answer = "B-tree indexes work for equality, range queries, and sorting — they're the default and cover most use cases. GIN (Generalized Inverted Index) indexes are optimized for searching within composite values like arrays, JSONB, and full-text search.",
                LearningEntryId = alexEntries[1].Id,
                UserId = alex.Id,
                TimesReviewed = 3,
                TimesCorrect = 2,
                EaseFactor = 230,
                Interval = 3,
                NextReviewAt = DateTime.UtcNow.AddDays(1),
                LastReviewedAt = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-33),
            },
            new()
            {
                Question = "Why are JWTs well-suited for horizontally scaled applications?",
                Answer = "JWTs are stateless — the token contains all information needed to authenticate the user (encoded in the payload), so any server instance can verify the token without consulting a shared session store.",
                LearningEntryId = alexEntries[2].Id,
                UserId = alex.Id,
                TimesReviewed = 6,
                TimesCorrect = 6,
                EaseFactor = 300,
                Interval = 30,
                NextReviewAt = DateTime.UtcNow.AddDays(22),
                LastReviewedAt = DateTime.UtcNow.AddDays(-8),
                CreatedAt = DateTime.UtcNow.AddDays(-31),
            },
            new()
            {
                Question = "What does the SM-2 algorithm adjust after each flashcard review?",
                Answer = "It adjusts the ease factor (how easy the card is, starts at 2.5) and the interval (days until next review). Good/Easy answers increase both; Again/Hard answers reset the interval to 1 day and slightly decrease the ease factor.",
                LearningEntryId = alexEntries[7].Id,
                UserId = alex.Id,
                TimesReviewed = 2,
                TimesCorrect = 2,
                EaseFactor = 250,
                Interval = 6,
                NextReviewAt = DateTime.UtcNow.AddDays(3),
                LastReviewedAt = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-14),
            },
            new()
            {
                Question = "What are Nielsen's most important usability heuristics?",
                Answer = "Visibility of system status (show users what's happening), error prevention (better than recovery), recognition over recall (minimize memory load), and consistency with platform conventions.",
                LearningEntryId = saraEntries[0].Id,
                UserId = sara.Id,
                TimesReviewed = 3,
                TimesCorrect = 3,
                EaseFactor = 270,
                Interval = 15,
                NextReviewAt = DateTime.UtcNow.AddDays(10),
                LastReviewedAt = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-26),
            },
            new()
            {
                Question = "What is the minimum color contrast ratio required by WCAG 2.1 AA for normal text?",
                Answer = "4.5:1 contrast ratio between the text color and background color. For large text (18pt+ or 14pt bold), the minimum is 3:1.",
                LearningEntryId = saraEntries[1].Id,
                UserId = sara.Id,
                TimesReviewed = 4,
                TimesCorrect = 3,
                EaseFactor = 240,
                Interval = 5,
                NextReviewAt = DateTime.UtcNow.AddDays(2),
                LastReviewedAt = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-23),
            },
            new()
            {
                Question = "What is the LTV:CAC ratio and what is considered healthy for SaaS?",
                Answer = "LTV:CAC is the ratio of Customer Lifetime Value to Customer Acquisition Cost. A ratio of 3:1 or higher is considered healthy — meaning you earn $3 for every $1 spent acquiring a customer.",
                LearningEntryId = nabilEntries[2].Id,
                UserId = nabil.Id,
                TimesReviewed = 2,
                TimesCorrect = 1,
                EaseFactor = 220,
                Interval = 1,
                NextReviewAt = DateTime.UtcNow.AddHours(2),
                LastReviewedAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-9),
            },
        };

        await context.Flashcards.AddRangeAsync(flashcards);
        await context.SaveChangesAsync();
        Console.WriteLine("Flashcards seeded");

        // ── 7. COLLECTIONS ────────────────────────────────────────────────────
        var alexBackendCollection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Backend Mastery",
            Description = "Everything I'm learning about .NET, databases, and API design",
            Emoji = "⚙️",
            IsPublic = true,
            ShareSlug = "backend-mastery-alex42",
            UserId = alex.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-35),
            UpdatedAt = DateTime.UtcNow.AddDays(-5),
        };

        var alexAiCollection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "AI & Machine Learning",
            Description = "AI tools, models, and learning resources",
            Emoji = "🤖",
            IsPublic = false,
            UserId = alex.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            UpdatedAt = DateTime.UtcNow.AddDays(-20),
        };

        var saraDesignCollection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "UX Design Principles",
            Description = "Heuristics, accessibility, and design systems resources",
            Emoji = "🎨",
            IsPublic = true,
            ShareSlug = "ux-design-principles-sara",
            UserId = sara.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-24),
            UpdatedAt = DateTime.UtcNow.AddDays(-10),
        };

        var nabilStartupCollection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Startup Playbook",
            Description = "Fundraising, growth, and unit economics resources",
            Emoji = "🚀",
            IsPublic = false,
            UserId = nabil.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-16),
            UpdatedAt = DateTime.UtcNow.AddDays(-16),
        };

        await context.Collections.AddRangeAsync(
            alexBackendCollection, alexAiCollection,
            saraDesignCollection, nabilStartupCollection);
        await context.SaveChangesAsync();

        // Collection entries
        var collectionEntries = new List<CollectionEntry>
        {
            new() { CollectionId = alexBackendCollection.Id, LearningEntryId = alexEntries[0].Id },
            new() { CollectionId = alexBackendCollection.Id, LearningEntryId = alexEntries[1].Id },
            new() { CollectionId = alexBackendCollection.Id, LearningEntryId = alexEntries[2].Id },
            new() { CollectionId = alexBackendCollection.Id, LearningEntryId = alexEntries[4].Id },
            new() { CollectionId = alexAiCollection.Id,      LearningEntryId = alexEntries[5].Id },
            new() { CollectionId = alexAiCollection.Id,      LearningEntryId = alexEntries[7].Id },
            new() { CollectionId = saraDesignCollection.Id,  LearningEntryId = saraEntries[0].Id },
            new() { CollectionId = saraDesignCollection.Id,  LearningEntryId = saraEntries[1].Id },
            new() { CollectionId = saraDesignCollection.Id,  LearningEntryId = saraEntries[2].Id },
            new() { CollectionId = nabilStartupCollection.Id, LearningEntryId = nabilEntries[0].Id },
            new() { CollectionId = nabilStartupCollection.Id, LearningEntryId = nabilEntries[1].Id },
            new() { CollectionId = nabilStartupCollection.Id, LearningEntryId = nabilEntries[2].Id },
        };

        await context.CollectionEntries.AddRangeAsync(collectionEntries);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Collections seeded");

        // ── 8. QUOTES ─────────────────────────────────────────────────────────
        var quotes = new List<Quote>
        {
            new()
            {
                Text = "Clean Architecture keeps business logic independent from infrastructure concerns, making it easier to test and maintain over time.",
                Note = "This is the core principle I followed when designing SLT's backend",
                Color = "yellow",
                LearningEntryId = alexEntries[0].Id,
                UserId = alex.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-38),
            },
            new()
            {
                Text = "Too many indexes slow down writes — only index what you query.",
                Note = "Important tradeoff to remember — indexes have a cost on INSERT/UPDATE",
                Color = "blue",
                LearningEntryId = alexEntries[1].Id,
                UserId = alex.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-34),
            },
            new()
            {
                Text = "JWTs are stateless — the server does not store session information, making them ideal for distributed systems and horizontal scaling.",
                Note = "Key architectural advantage over cookie-based sessions",
                Color = "green",
                LearningEntryId = alexEntries[2].Id,
                UserId = alex.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-32),
            },
            new()
            {
                Text = "Studies show spaced repetition improves retention by 200-500% vs mass study.",
                Note = "The science behind why I built the flashcard system",
                Color = "yellow",
                LearningEntryId = alexEntries[7].Id,
                UserId = alex.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-14),
            },
            new()
            {
                Text = "Error prevention is more important than error recovery.",
                Note = "Design principle to always keep in mind — validate inputs upfront",
                Color = "pink",
                LearningEntryId = saraEntries[0].Id,
                UserId = sara.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-27),
            },
            new()
            {
                Text = "Color contrast ratio must be at least 4.5:1 for normal text (WCAG AA).",
                Note = "Bookmark this — use WebAIM Contrast Checker to verify designs",
                Color = "yellow",
                LearningEntryId = saraEntries[1].Id,
                UserId = sara.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-24),
            },
            new()
            {
                Text = "Most growth comes from retention, not acquisition — fix the leaky bucket first.",
                Note = "Counter-intuitive but backed by data. Fix churn before spending on ads.",
                Color = "green",
                LearningEntryId = nabilEntries[1].Id,
                UserId = nabil.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-13),
            },
            new()
            {
                Text = "Churn rate is the silent killer — even small churn compounds to devastating LTV loss.",
                Note = "5% monthly churn = 46% annual retention. That's terrible.",
                Color = "pink",
                LearningEntryId = nabilEntries[2].Id,
                UserId = nabil.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-9),
            },
        };

        await context.Quotes.AddRangeAsync(quotes);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Quotes seeded");

        // ── 9. TEAM SPACES ────────────────────────────────────────────────────
        var devTeam = new TeamSpace
        {
            Id = Guid.NewGuid(),
            Name = "Full Stack Dev Team",
            Description = "Sharing resources across our full stack engineering team",
            Emoji = "💻",
            InviteCode = "DEVTEAM1",
            OwnerId = alex.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
        };

        var productTeam = new TeamSpace
        {
            Id = Guid.NewGuid(),
            Name = "Product & Design Sync",
            Description = "Design principles, UX research, and product thinking",
            Emoji = "🎯",
            InviteCode = "PRODUCT2",
            OwnerId = sara.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            UpdatedAt = DateTime.UtcNow.AddDays(-2),
        };

        await context.TeamSpaces.AddRangeAsync(devTeam, productTeam);
        await context.SaveChangesAsync();

        // Team members
        var teamMembers = new List<TeamMember>
        {
            new() { TeamSpaceId = devTeam.Id, UserId = alex.Id,  Role = TeamRole.Owner,  CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new() { TeamSpaceId = devTeam.Id, UserId = sara.Id,  Role = TeamRole.Member, CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new() { TeamSpaceId = devTeam.Id, UserId = nabil.Id, Role = TeamRole.Member, CreatedAt = DateTime.UtcNow.AddDays(-17) },
            new() { TeamSpaceId = productTeam.Id, UserId = sara.Id,  Role = TeamRole.Owner,  CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new() { TeamSpaceId = productTeam.Id, UserId = alex.Id,  Role = TeamRole.Member, CreatedAt = DateTime.UtcNow.AddDays(-14) },
            new() { TeamSpaceId = productTeam.Id, UserId = nabil.Id, Role = TeamRole.Member, CreatedAt = DateTime.UtcNow.AddDays(-13) },
        };

        await context.TeamMembers.AddRangeAsync(teamMembers);
        await context.SaveChangesAsync();

        // Shared team entries
        var teamEntry1 = new TeamEntry
        {
            Id = Guid.NewGuid(),
            TeamSpaceId = devTeam.Id,
            LearningEntryId = alexEntries[0].Id,
            SharedByUserId = alex.Id,
            SharedNote = "Everyone should read this before our architecture review meeting next week. We're following this pattern in the new service.",
            CreatedAt = DateTime.UtcNow.AddDays(-18),
        };

        var teamEntry2 = new TeamEntry
        {
            Id = Guid.NewGuid(),
            TeamSpaceId = devTeam.Id,
            LearningEntryId = alexEntries[2].Id,
            SharedByUserId = alex.Id,
            SharedNote = "Sharing context on why we chose JWT over sessions for the auth system.",
            CreatedAt = DateTime.UtcNow.AddDays(-16),
        };

        var teamEntry3 = new TeamEntry
        {
            Id = Guid.NewGuid(),
            TeamSpaceId = devTeam.Id,
            LearningEntryId = alexEntries[4].Id,
            SharedByUserId = alex.Id,
            SharedNote = "Our Dockerfile follows exactly this multi-stage pattern. Good reference.",
            CreatedAt = DateTime.UtcNow.AddDays(-14),
        };

        var teamEntry4 = new TeamEntry
        {
            Id = Guid.NewGuid(),
            TeamSpaceId = productTeam.Id,
            LearningEntryId = saraEntries[0].Id,
            SharedByUserId = sara.Id,
            SharedNote = "Core reading for our UX audit next sprint. Please review heuristics 1, 5, and 7.",
            CreatedAt = DateTime.UtcNow.AddDays(-13),
        };

        var teamEntry5 = new TeamEntry
        {
            Id = Guid.NewGuid(),
            TeamSpaceId = productTeam.Id,
            LearningEntryId = saraEntries[1].Id,
            SharedByUserId = sara.Id,
            SharedNote = "Accessibility compliance is non-negotiable for our next release. Sharing this for the team.",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
        };

        await context.TeamEntries.AddRangeAsync(
            teamEntry1, teamEntry2, teamEntry3,
            teamEntry4, teamEntry5);
        await context.SaveChangesAsync();

        // Comments on team entries
        var comments = new List<EntryComment>
        {
            new()
            {
                Text = "Great article! I didn't realize how much the repository pattern simplifies testing. Going to refactor my current project to follow this.",
                TeamEntryId = teamEntry1.Id,
                UserId = sara.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-17),
            },
            new()
            {
                Text = "This is the architecture we should standardize on across all our services. Let's discuss in the team meeting.",
                TeamEntryId = teamEntry1.Id,
                UserId = nabil.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-16),
            },
            new()
            {
                Text = "Agreed. I'll create a template repo following this structure so the team can start new services consistently.",
                TeamEntryId = teamEntry1.Id,
                UserId = alex.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-16),
            },
            new()
            {
                Text = "One thing to note — JWT revocation is tricky. We should implement a token blacklist in Redis for logout functionality.",
                TeamEntryId = teamEntry2.Id,
                UserId = sara.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
            },
            new()
            {
                Text = "Good call Sara. I'll add a Redis-based token denylist to our backlog.",
                TeamEntryId = teamEntry2.Id,
                UserId = alex.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-14),
            },
            new()
            {
                Text = "Can we also use this Dockerfile pattern for our Python services? Or does it only apply to .NET?",
                TeamEntryId = teamEntry3.Id,
                UserId = nabil.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-13),
            },
            new()
            {
                Text = "The pattern applies to any language. For Python, you'd use python:3.12-slim as the base image instead of aspnet.",
                TeamEntryId = teamEntry3.Id,
                UserId = alex.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-13),
            },
            new()
            {
                Text = "Heuristic #7 (flexibility and efficiency) is something our app really lacks for power users. We should add keyboard shortcuts.",
                TeamEntryId = teamEntry4.Id,
                UserId = alex.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-12),
            },
            new()
            {
                Text = "I've been thinking about this too. Let me schedule a quick workshop on applying these heuristics to our current product.",
                TeamEntryId = teamEntry4.Id,
                UserId = sara.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-11),
            },
            new()
            {
                Text = "Ran our current UI through a contrast checker — 3 components fail WCAG AA. I'll create tickets for each fix.",
                TeamEntryId = teamEntry5.Id,
                UserId = sara.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-9),
            },
            new()
            {
                Text = "Thanks for flagging this Sara. Let's target full WCAG AA compliance before launch.",
                TeamEntryId = teamEntry5.Id,
                UserId = nabil.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
            },
        };

        await context.EntryComments.AddRangeAsync(comments);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Team spaces seeded");

        Console.WriteLine("");
        Console.WriteLine("🎉 Database seeding completed successfully!");
        Console.WriteLine("");
        Console.WriteLine("👤 Test Accounts (all use password: Password123!)");
        Console.WriteLine("   alex@slt.dev  — Full Stack Developer, 10 entries, 5 flashcards");
        Console.WriteLine("   sara@slt.dev  — UX Designer, 5 entries, 2 flashcards");
        Console.WriteLine("   nabil@slt.dev — Startup Founder, 3 entries, 1 flashcard");
        Console.WriteLine("");
    }
}