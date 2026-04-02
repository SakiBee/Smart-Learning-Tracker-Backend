# 📚 Smart Learning Tracker (SLT)

> **Transform passive reading into active knowledge management.**
> A full-stack AI-powered application that helps you capture, summarize, organize, and revisit everything you learn online — with spaced repetition flashcards, team collaboration, reading mode, and more.

---

## 🌐 Live Demo

| Service | URL |
|---|---|
| Frontend | `https://smart-learning-tracker-frontend-tbv.vercel.app/register` |
| Backend API | `https://smart-learning-tracker-backend-3.onrender.com/` |
| API Docs | `https://slt-backend.up.railway.app/scalar/v1` |

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Project Architecture](#-project-architecture)
- [Folder Structure](#-folder-structure)
- [Getting Started](#-getting-started)
- [Environment Variables](#-environment-variables)
- [Database Schema](#-database-schema)
- [API Reference](#-api-reference)
- [Feature Walkthroughs](#-feature-walkthroughs)
- [Browser Extension](#-browser-extension)
- [Deployment](#-deployment)
- [Roadmap](#-roadmap)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🧠 Overview

Most people consume enormous amounts of content daily — articles, LinkedIn posts, tutorials, threads — but retain very little of it. There's no structured system to capture, summarize, and revisit these learnings.

**Smart Learning Tracker** solves this by providing:

- **One-click URL saving** — paste any link and the app does the rest
- **AI-powered summarization** — automatically extracts content and generates summaries, key points, and tags
- **Spaced repetition flashcards** — review what you've learned using the SM-2 algorithm
- **Reading mode with quote highlighting** — distraction-free reading with text selection saving
- **Team collaboration** — share entries, comment, and learn together
- **Collections** — curate and organize learnings by topic
- **Progress tracking** — streaks, heatmaps, read rates, and weekly digest

---

## ✨ Features

### 🔐 Authentication
- Email & password registration/login with BCrypt password hashing
- Google OAuth 2.0 sign-in (one-click)
- JWT-based session management (7-day token expiry)
- Protected routes — unauthenticated users are redirected to login

### 🔗 Smart URL Capture
- Paste any URL from any website (articles, Medium, dev.to, Hashnode, LinkedIn, newsletters, YouTube, etc.)
- Uses **Jina AI Reader** (`r.jina.ai`) to extract clean text even from JavaScript-heavy pages like Medium
- Fallback to generic HTML scraping for simpler sites
- Extracts: title, author, description, thumbnail, main content text

### 🤖 AI-Powered Summarization (Groq + LLaMA)
- Sends extracted content to Groq API using LLaMA 3.3 70B model
- Auto-generates:
  - A 2–3 sentence concise **summary**
  - 3–5 **key points** as bullet points
  - 2–4 relevant **tags** (lowercase, auto-suggested)
  - Detected **content type** (Article, Tutorial, LinkedIn Post, Thread, Newsletter, Video, Other)
- If page extraction fails, AI still infers from the URL slug and title
- All AI-generated content is editable before saving

### 📋 Learning Entry Management
- Full **CRUD** on entries (Create, Read, Update, Delete)
- Fields: title, URL, author, thumbnail, summary, key points, personal notes, content type, priority, tags
- Toggle **Read**, **Favorite**, **Read Later** status from dashboard or detail page
- **Duplicate detection** — prevents saving the same URL twice
- Sort by date (newest first)

### 🔍 Search & Filtering
- Real-time **keyword search** across title, summary, and tags
- Filter by: **All, Favorites, Read Later, Unread**
- **Tag sidebar** (desktop) — click any tag to filter entries, with count badges
- Active filter indicator with one-click clear

### 📊 Dashboard
- Stats row: Total Saved, Read, Favorites, Read Later counts
- Responsive **3-column card grid**
- Skeleton loading states
- Empty state with call to action
- Quick actions on hover (favorite, mark read, delete)

### 🃏 Flashcard System (Spaced Repetition)
- **AI generates 5 flashcards** from any entry's summary and key points using Groq
- Implements the **SM-2 spaced repetition algorithm**:
  - 4 rating buttons: Again (< 10 min), Hard (1 day), Good (few days), Easy (longer)
  - Interval increases based on ease factor and rating
  - Cards are scheduled automatically for optimal review timing
- **Review session UI**:
  - Animated card flip (CSS 3D transform)
  - Progress bar across the session
  - Session completion screen with accuracy stats
- All cards list with accuracy rate, due status, and delete option
- Due cards counter with "review now" banner
- Manual flashcard creation supported

### 📁 Collections
- Create **named collections** with emoji icons and descriptions
- Add any entry to one or more collections
- **Public sharing** — toggle collection to public and share a unique link
- Public collections are accessible without login (`/shared/:slug`)
- Edit name, description, emoji
- Remove entries from collections without deleting them
- Entry count displayed on each collection card

### 💡 Smart Recommendations
- **Global recommendations** on dashboard sidebar — based on your top 3 most-used tags, surfaces unread high-priority entries
- **Per-entry recommendations** — on entry detail page, shows related entries by shared tag count with a match score
- Recommendations are tag-based and calculated entirely in the backend (no extra API calls)

### 📖 Reading Mode
- Distraction-free reading view with warm background (`amber-50`)
- **Adjustable font size** (12px–24px) via A- / A+ buttons in top bar
- **Reading time estimator** (based on word count at 200 WPM)
- **Text selection quote saving**:
  - Select any text → a popup appears
  - Choose a highlight color (yellow, green, blue, pink)
  - Add an optional note
  - Save with one click
- Quotes panel slides in from the right showing all saved quotes
- Mark entry as read directly from reading mode
- Link to original source article always visible

### ✨ Quotes
- All saved quotes viewable on dedicated `/quotes` page
- **Grouped by entry** with clickable entry titles
- **Color filter** — filter quotes by highlight color
- Copy quote text to clipboard
- Delete individual quotes
- Each quote shows color highlight, optional note, and timestamp

### 📈 Progress Tracking
- **Activity Heatmap** — last 30 days of saving activity, color-coded by intensity
- **Learning Streak**:
  - Current streak (consecutive days with at least one entry saved)
  - Longest streak ever
  - Animated flame indicators (up to 7 flames)
  - Motivational messages ("You're on fire! 🔥")
- **Read Progress Bar** — entries read vs total, with percentage
- **Weekly Goal Tracker** — target 10 entries/week
- **Content Type Breakdown** — horizontal bar chart of Article vs Tutorial vs etc.
- **Top Topics** — most-used tags ranked with count badges
- **Weekly Digest** — last 7 days of saved entries with mark-as-read toggle

### 🏢 Team Spaces
- Create team spaces with emoji and description
- **Invite code** — 8-character alphanumeric code (e.g. `ABC12345`)
- Join any team by entering the invite code
- **Team feed** — shared entries with the sharer's name, timestamp, and optional note
- **Comments** — add comments on shared entries, see all team discussion
- **Roles**: Owner, Admin, Member
- Owner can remove members, regenerate invite code, delete team
- Any member can leave the team
- Share any personal entry to one or more team spaces with an optional note

### 🌙 Dark Mode
- System preference detection on first load
- Manual toggle in navbar (sun/moon icon)
- Persists preference in `localStorage`
- Full dark mode across all pages, modals, cards, inputs, and toasts

### 🔔 Toast Notifications
- Replaces all browser `alert()` dialogs
- Success, error, and info variants
- Auto-dismiss after 3 seconds
- Themed for dark/light mode
- Bottom-right position, non-intrusive

### 🔌 Browser Extension
- Chrome extension (Manifest V3)
- **Login with SLT credentials** directly from the popup
- Shows current tab's URL and title pre-filled
- **⚡ Auto-fill with AI** button — calls the summarize endpoint and fills all fields
- Add tags, set priority, content type, read later flag
- **Save Learning** button — saves to backend and confirms
- **Open Dashboard** link
- Works with local backend — no deployment required for personal use

---

## 🛠 Tech Stack

### Backend
| Technology | Purpose |
|---|---|
| **.NET 10** | Web API framework |
| **ASP.NET Core** | HTTP pipeline, routing, middleware |
| **Entity Framework Core 9** | ORM and database migrations |
| **PostgreSQL** | Primary relational database |
| **Npgsql** | PostgreSQL EF Core provider |
| **BCrypt.Net** | Password hashing |
| **JWT Bearer** | Authentication tokens |
| **Google.Apis.Auth** | Google ID token verification |
| **HtmlAgilityPack** | HTML parsing for content extraction |
| **Scalar** | Modern API documentation UI |

### Frontend
| Technology | Purpose |
|---|---|
| **Next.js 15** | React framework with App Router |
| **TypeScript** | Type safety |
| **Tailwind CSS** | Utility-first styling |
| **Zustand** | Lightweight state management |
| **Axios** | HTTP client with interceptors |
| **NextAuth.js v4** | Google OAuth integration |
| **react-hot-toast** | Toast notifications |

### AI & External Services
| Service | Purpose |
|---|---|
| **Groq API** | LLaMA 3.3 70B for summarization and flashcard generation |
| **Jina AI Reader** | JavaScript-rendered page content extraction |
| **Google OAuth** | Social login |

### DevOps & Deployment
| Service | Purpose |
|---|---|
| **Railway** | Backend hosting + managed PostgreSQL |
| **Vercel** | Frontend hosting with CI/CD |
| **GitHub** | Source control |
| **Docker** | Container for backend deployment |

---

## 🏛 Project Architecture

SLT follows **Clean Architecture** on the backend, separating concerns into 4 distinct layers:

```
┌────────────────────────────────────────────────────────────┐
│                        SLT.API                             │
│          (Controllers, Middleware, Program.cs)             │
│                   HTTP Entry Point                         │
└──────────────────────┬─────────────────────────────────────┘
                       │ depends on
┌──────────────────────▼─────────────────────────────────────┐
│                   SLT.Application                          │
│             (DTOs, Interfaces, Features)                   │
│              Business Logic Contracts                      │
└──────────┬────────────────────────────┬────────────────────┘
           │ depends on                 │ depends on
┌──────────▼──────────┐    ┌────────────▼──────────────────  ┐
│      SLT.Core       │    │       SLT.Infrastructure         │
│  (Entities, Enums,  │    │   (DbContext, Repositories,      │
│    Interfaces)      │    │    Services, EF Migrations)      │
│  Domain Layer       │    │   Data Access Layer              │
└─────────────────────┘    └──────────────────────────────────┘
```

### Layer Responsibilities

**SLT.Core** — The heart of the application. Contains:
- Entity classes (`User`, `LearningEntry`, `Tag`, `Flashcard`, `Collection`, `Quote`, `TeamSpace`, etc.)
- Enums (`ContentType`, `PriorityLevel`, `TeamRole`)
- Repository interfaces (`ILearningEntryRepository`, `IFlashcardRepository`, etc.)
- Service interfaces (`IJwtTokenService`, `IAiSummaryService`, `IFlashcardGeneratorService`, etc.)

**SLT.Application** — Business logic contracts. Contains:
- DTOs for all request/response shapes
- No direct database or HTTP dependencies

**SLT.Infrastructure** — External concern implementations. Contains:
- `AppDbContext` (EF Core)
- All repository implementations
- `JwtTokenService`, `AiSummaryService`, `ContentExtractorService`, `FlashcardGeneratorService`
- EF Core migrations
- `DependencyInjection.cs` (service registration)

**SLT.API** — HTTP layer. Contains:
- Controllers (Auth, LearningEntries, Summarize, Flashcards, Collections, Stats, Quotes, Recommendations, TeamSpaces)
- `Program.cs` (middleware pipeline, DI setup, CORS, JWT config)
- Scalar API documentation

### Frontend Architecture

```
src/
├── app/                    → Next.js App Router pages
│   ├── (auth)/             → Auth group (no navbar)
│   │   ├── login/
│   │   └── register/
│   ├── dashboard/          → Protected pages
│   │   ├── page.tsx        → Main feed
│   │   ├── entries/[id]/   → Entry detail
│   │   │   └── read/       → Reading mode
│   │   ├── flashcards/     → Flashcard system
│   │   ├── collections/    → Collections
│   │   ├── quotes/         → Saved quotes
│   │   ├── progress/       → Stats & streaks
│   │   └── teams/          → Team spaces
│   └── api/auth/           → NextAuth route handler
├── components/             → Reusable UI components
├── store/                  → Zustand state (auth, theme)
├── lib/                    → API client, utils, toast helpers
└── types/                  → TypeScript interfaces
```

---

## 📂 Folder Structure

```
Smart LT/
├── slt.backend/
│   ├── SLT.sln
│   ├── SLT.API/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── LearningEntriesController.cs
│   │   │   ├── SummarizeController.cs
│   │   │   ├── FlashcardsController.cs
│   │   │   ├── CollectionsController.cs
│   │   │   ├── StatsController.cs
│   │   │   ├── QuotesController.cs
│   │   │   ├── RecommendationsController.cs
│   │   │   └── TeamSpacesController.cs
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Dockerfile
│   │   └── Program.cs
│   ├── SLT.Core/
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── User.cs
│   │   │   ├── LearningEntry.cs
│   │   │   ├── Tag.cs
│   │   │   ├── Flashcard.cs
│   │   │   ├── Collection.cs
│   │   │   ├── CollectionEntry.cs
│   │   │   ├── Quote.cs
│   │   │   ├── TeamSpace.cs
│   │   │   ├── TeamMember.cs
│   │   │   ├── TeamEntry.cs
│   │   │   └── EntryComment.cs
│   │   ├── Enums/
│   │   │   ├── ContentType.cs
│   │   │   ├── PriorityLevel.cs
│   │   │   └── TeamRole.cs
│   │   └── Interfaces/
│   │       ├── IRepository.cs
│   │       ├── ILearningEntryRepository.cs
│   │       ├── IUserRepository.cs
│   │       ├── IFlashcardRepository.cs
│   │       ├── ICollectionRepository.cs
│   │       ├── IQuoteRepository.cs
│   │       ├── IJwtTokenService.cs
│   │       ├── IAiSummaryService.cs
│   │       ├── IContentExtractorService.cs
│   │       └── IFlashcardGeneratorService.cs
│   ├── SLT.Application/
│   │   └── DTOs/
│   │       ├── LearningEntryDto.cs
│   │       ├── AuthResponseDto.cs
│   │       ├── FlashcardDto.cs
│   │       ├── CollectionDto.cs
│   │       ├── QuoteDto.cs
│   │       ├── StatsDto.cs
│   │       ├── TeamDto.cs
│   │       └── UrlSummaryDto.cs
│   └── SLT.Infrastructure/
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── Migrations/
│       ├── Repositories/
│       │   ├── LearningEntryRepository.cs
│       │   ├── UserRepository.cs
│       │   ├── FlashcardRepository.cs
│       │   ├── CollectionRepository.cs
│       │   └── QuoteRepository.cs
│       ├── Services/
│       │   ├── JwtTokenService.cs
│       │   ├── AiSummaryService.cs
│       │   ├── ContentExtractorService.cs
│       │   └── FlashcardGeneratorService.cs
│       └── DependencyInjection.cs
│
├── slt.frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── (auth)/
│   │   │   │   ├── login/page.tsx
│   │   │   │   └── register/page.tsx
│   │   │   ├── dashboard/
│   │   │   │   ├── page.tsx
│   │   │   │   ├── entries/[id]/
│   │   │   │   │   ├── page.tsx
│   │   │   │   │   └── read/page.tsx
│   │   │   │   ├── flashcards/page.tsx
│   │   │   │   ├── collections/
│   │   │   │   │   ├── page.tsx
│   │   │   │   │   └── [id]/page.tsx
│   │   │   │   ├── quotes/page.tsx
│   │   │   │   ├── progress/page.tsx
│   │   │   │   └── teams/
│   │   │   │       ├── page.tsx
│   │   │   │       └── [id]/page.tsx
│   │   │   ├── api/auth/[...nextauth]/route.ts
│   │   │   ├── layout.tsx
│   │   │   ├── page.tsx
│   │   │   └── globals.css
│   │   ├── components/
│   │   │   ├── layout/
│   │   │   │   ├── Navbar.tsx
│   │   │   │   └── SessionWrapper.tsx
│   │   │   ├── entries/
│   │   │   │   ├── EntryCard.tsx
│   │   │   │   └── AddEntryModal.tsx
│   │   │   ├── flashcards/
│   │   │   │   └── FlashcardReview.tsx
│   │   │   ├── stats/
│   │   │   │   ├── ActivityHeatmap.tsx
│   │   │   │   ├── ContentTypeChart.tsx
│   │   │   │   ├── StreakWidget.tsx
│   │   │   │   └── WeeklyDigest.tsx
│   │   │   ├── collections/
│   │   │   │   └── AddToCollectionButton.tsx
│   │   │   ├── recommendations/
│   │   │   │   ├── RecommendationsSidebar.tsx
│   │   │   │   └── RecommendationsForEntry.tsx
│   │   │   ├── teams/
│   │   │   │   └── ShareToTeamButton.tsx
│   │   │   └── ui/
│   │   │       └── GoogleButton.tsx
│   │   ├── store/
│   │   │   ├── authStore.ts
│   │   │   └── themeStore.ts
│   │   ├── lib/
│   │   │   ├── api.ts
│   │   │   ├── utils.ts
│   │   │   └── toast.ts
│   │   └── types/
│   │       ├── index.ts
│   │       └── next-auth.d.ts
│   ├── .env.local
│   ├── next.config.ts
│   └── package.json
│
└── slt-extension/
    ├── manifest.json
    ├── popup.html
    ├── popup.js
    └── icons/
        ├── icon16.png
        ├── icon32.png
        ├── icon48.png
        └── icon128.png
```

---

## 🚀 Getting Started

### Prerequisites

Make sure you have these installed:

| Tool | Version | Download |
|---|---|---|
| .NET SDK | 10.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Node.js | 18.0+ | [nodejs.org](https://nodejs.org) |
| PostgreSQL | 14.0+ | [postgresql.org](https://www.postgresql.org/download) |
| Git | Any | [git-scm.com](https://git-scm.com) |

### 1. Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/smart-learning-tracker.git
cd smart-learning-tracker
```

### 2. Setup the Database

Open PostgreSQL and create the database:

```sql
CREATE DATABASE slt_db;
```

### 3. Setup the Backend

```bash
cd slt.backend

# Restore packages
dotnet restore

# Apply database migrations
dotnet ef database update --project SLT.Infrastructure --startup-project SLT.API

# Run the API
cd SLT.API
dotnet run
```

The API will start at `http://localhost:5123`
API docs available at `http://localhost:5123/scalar/v1`

### 4. Setup the Frontend

```bash
cd slt.frontend

# Install dependencies
npm install

# Run the development server
npm run dev
```

The app will be available at `http://localhost:3000`

### 5. Setup the Browser Extension (Optional)

1. Open Chrome and navigate to `chrome://extensions`
2. Enable **Developer mode** (top-right toggle)
3. Click **Load unpacked**
4. Select the `slt-extension/` folder
5. The extension icon appears in your Chrome toolbar

---

## 🔑 Environment Variables

### Backend — `slt.backend/SLT.API/appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=slt_db;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Key": "your-super-secret-key-minimum-32-characters-long",
    "Issuer": "SLT.API",
    "Audience": "SLT.Client"
  },
  "Groq": {
    "ApiKey": "your-groq-api-key",
    "Model": "openai/gpt-oss-120b"
  },
  "Google": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com"
  },
  "AllowedOrigin": "http://localhost:3000"
}
```

### Frontend — `slt.frontend/.env.local`

```env
NEXT_PUBLIC_API_URL=http://localhost:5123/api

NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=any-random-string-minimum-32-characters

GOOGLE_CLIENT_ID=your-google-client-id.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=GOCSPX-your-google-client-secret
```

### Where to Get Each Key

| Key | Where to Get | Cost |
|---|---|---|
| `Groq:ApiKey` | [console.groq.com](https://console.groq.com) → API Keys | Free tier |
| `Google:ClientId` | [console.cloud.google.com](https://console.cloud.google.com) → Credentials | Free |
| `Google:ClientSecret` | Same as above | Free |
| `Jwt:Key` | Generate any random 32+ char string | — |
| `NEXTAUTH_SECRET` | Run `openssl rand -base64 32` | — |

---

## 🗄 Database Schema

```
Users
├── Id (PK, UUID)
├── Email (unique)
├── PasswordHash
├── FullName
├── CreatedAt
└── UpdatedAt

LearningEntries
├── Id (PK, UUID)
├── UserId (FK → Users)
├── Url
├── Title
├── Author
├── ThumbnailUrl
├── Summary
├── KeyPoints (JSON string)
├── PersonalNotes
├── ContentType (enum: 0–6)
├── Priority (enum: 0–2)
├── IsReadLater
├── IsFavorite
├── IsRead
├── CreatedAt
└── UpdatedAt

Tags
├── Id (PK, UUID)
├── Name
├── UserId (FK → Users)
└── (many-to-many with LearningEntries via LearningEntryTags)

Flashcards
├── Id (PK, UUID)
├── LearningEntryId (FK → LearningEntries)
├── UserId (FK → Users)
├── Question
├── Answer
├── TimesReviewed
├── TimesCorrect
├── EaseFactor (SM-2, x100)
├── Interval (days)
├── NextReviewAt
├── LastReviewedAt
├── CreatedAt
└── UpdatedAt

Collections
├── Id (PK, UUID)
├── UserId (FK → Users)
├── Name
├── Description
├── Emoji
├── IsPublic
├── ShareSlug (unique, nullable)
├── CreatedAt
└── UpdatedAt

CollectionEntries (join table)
├── Id (PK, UUID)
├── CollectionId (FK → Collections)
└── LearningEntryId (FK → LearningEntries)

Quotes
├── Id (PK, UUID)
├── UserId (FK → Users)
├── LearningEntryId (FK → LearningEntries)
├── Text
├── Note
├── Color (yellow/green/blue/pink)
├── CreatedAt
└── UpdatedAt

TeamSpaces
├── Id (PK, UUID)
├── OwnerId (FK → Users)
├── Name
├── Description
├── Emoji
├── InviteCode (unique, 8 chars)
├── CreatedAt
└── UpdatedAt

TeamMembers
├── Id (PK, UUID)
├── TeamSpaceId (FK → TeamSpaces)
├── UserId (FK → Users)
├── Role (enum: Member/Admin/Owner)
└── CreatedAt

TeamEntries
├── Id (PK, UUID)
├── TeamSpaceId (FK → TeamSpaces)
├── LearningEntryId (FK → LearningEntries)
├── SharedByUserId (FK → Users)
├── SharedNote
└── CreatedAt

EntryComments
├── Id (PK, UUID)
├── TeamEntryId (FK → TeamEntries)
├── UserId (FK → Users)
├── Text
├── CreatedAt
└── UpdatedAt
```

---

## 📡 API Reference

Base URL: `http://localhost:5123/api`

All endpoints except Auth require `Authorization: Bearer <token>` header.

### Auth
| Method | Endpoint | Description |
|---|---|---|
| POST | `/auth/register` | Register with email & password |
| POST | `/auth/login` | Login with email & password |
| POST | `/auth/google` | Login/register with Google ID token |

### Learning Entries
| Method | Endpoint | Description |
|---|---|---|
| GET | `/learningentries` | Get all entries for current user |
| GET | `/learningentries/:id` | Get single entry with tags |
| POST | `/learningentries` | Create new entry |
| PUT | `/learningentries/:id` | Update entry |
| DELETE | `/learningentries/:id` | Delete entry |

### AI Summarization
| Method | Endpoint | Description |
|---|---|---|
| POST | `/summarize` | Extract + summarize a URL |

### Flashcards
| Method | Endpoint | Description |
|---|---|---|
| GET | `/flashcards` | Get all flashcards |
| GET | `/flashcards/due` | Get cards due for review |
| POST | `/flashcards/generate` | AI-generate cards from entry |
| POST | `/flashcards` | Create manual flashcard |
| POST | `/flashcards/:id/review` | Submit review rating (0–3) |
| DELETE | `/flashcards/:id` | Delete flashcard |

### Collections
| Method | Endpoint | Description |
|---|---|---|
| GET | `/collections` | Get all collections |
| GET | `/collections/:id` | Get collection with entries |
| GET | `/collections/shared/:slug` | Get public collection (no auth) |
| POST | `/collections` | Create collection |
| PUT | `/collections/:id` | Update collection |
| DELETE | `/collections/:id` | Delete collection |
| POST | `/collections/:id/entries` | Add entry to collection |
| DELETE | `/collections/:id/entries/:entryId` | Remove entry |

### Quotes
| Method | Endpoint | Description |
|---|---|---|
| GET | `/quotes` | Get all quotes |
| GET | `/quotes/entry/:entryId` | Get quotes for an entry |
| POST | `/quotes` | Save a new quote |
| PUT | `/quotes/:id` | Update quote note/color |
| DELETE | `/quotes/:id` | Delete quote |

### Stats
| Method | Endpoint | Description |
|---|---|---|
| GET | `/stats` | Get all progress stats |

### Recommendations
| Method | Endpoint | Description |
|---|---|---|
| GET | `/recommendations` | Global recommendations |
| GET | `/recommendations/entry/:id` | Entry-specific recommendations |

### Team Spaces
| Method | Endpoint | Description |
|---|---|---|
| GET | `/teamspaces` | Get all teams user belongs to |
| GET | `/teamspaces/:id` | Get team with feed and members |
| POST | `/teamspaces` | Create team |
| POST | `/teamspaces/join` | Join team by invite code |
| POST | `/teamspaces/:id/entries` | Share entry to team |
| DELETE | `/teamspaces/:id/entries/:entryId` | Remove entry from team |
| POST | `/teamspaces/:id/entries/:entryId/comments` | Add comment |
| DELETE | `/teamspaces/:id/members/:userId` | Remove member |
| DELETE | `/teamspaces/:id` | Delete team (owner only) |

---

## 🎮 Feature Walkthroughs

### How to Save a Learning

1. Click **Add Learning** button on the dashboard
2. Paste any URL (article, blog post, LinkedIn post, YouTube video, etc.)
3. Click **⚡ Extract & Summarize**
4. Wait 3–5 seconds while the AI:
   - Fetches and parses the page content
   - Sends it to Groq (LLaMA 3.3 70B)
   - Returns summary, key points, tags, and content type
5. Review and edit any AI-generated fields
6. Set priority and optionally toggle Read Later
7. Click **Save Learning**

### How to Review Flashcards

1. Navigate to **Flashcards** in the navbar
2. If you have due cards, a blue banner shows "X cards ready for review"
3. Click **Start Review** or **Review X Due Cards**
4. For each card:
   - Read the **question** on the front (blue)
   - Click the card or **Show Answer** to flip it
   - Rate your recall: **Again** / **Hard** / **Good** / **Easy**
5. The SM-2 algorithm schedules the next review automatically
6. Session ends with an accuracy summary

### How to Use Reading Mode

1. Open any entry detail page
2. Click the **📖 Read** button in the header
3. In reading mode:
   - Use **A- / A+** to adjust font size
   - Select any text → popup appears → choose a color → optionally add a note → click **✨ Save Quote**
   - Click **✨ X Quotes** button to open the quotes panel
   - Click **✅ Done** when finished reading to mark it as read

### How to Create a Collection

1. Navigate to **Collections** in the navbar
2. Click **New Collection**
3. Pick an emoji, enter a name and description
4. Click **Create Collection**
5. Open any entry → click the **📦** icon in the header → select the collection
6. To make it public and shareable: open the collection → click **🔒 Private** to toggle to **🌐 Public** → **Copy Link**

### How to Create a Team Space

1. Navigate to **Teams** in the navbar
2. Click **Create Team**
3. Choose emoji, name, and description
4. Your 8-character invite code is shown (e.g. `ABC12345`)
5. Share the code with colleagues → they click **Join Team** and enter the code
6. Open any entry → click the **🔗 Share** icon → select a team → add optional note → **Share 🚀**
7. In the team feed, anyone can click **💬 X comments** to expand and add comments

### How to Track Progress

1. Navigate to **Progress** in the navbar
2. View your:
   - **Activity Heatmap** — hover over squares to see daily counts
   - **Streak** — current and longest consecutive days
   - **Read Rate** — % of saved entries you've actually read
   - **Weekly Goal** — progress toward 10 entries/week
   - **Content Type Breakdown** — what types of content you consume most
   - **Top Topics** — your most-used tags
   - **Weekly Digest** — everything saved in the last 7 days

---

## 🔌 Browser Extension

The Chrome extension lets you save any webpage to your learning tracker without leaving the tab.

### Installation

1. Download or clone the `slt-extension/` folder
2. Go to `chrome://extensions` in Chrome
3. Enable **Developer mode** (top-right)
4. Click **Load unpacked** → select `slt-extension/` folder
5. Pin the extension to your toolbar

### Usage

1. Browse to any article or post
2. Click the **📚 SLT** icon in Chrome toolbar
3. Sign in with your SLT email and password (one-time)
4. The current page URL and title are pre-filled
5. Click **⚡ Auto-fill with AI** to let AI generate the summary
6. Optionally add tags, set priority
7. Click **Save Learning** — it appears instantly in your dashboard

### How It Works

```
Chrome Extension
      ↓ (HTTP POST)
Local Backend (localhost:5123)
      ↓
Jina AI Reader + Groq API
      ↓
PostgreSQL Database
      ↓
Dashboard (localhost:3000)
```

> **Note:** The extension connects to your local backend. For it to work with a deployed backend, update the `API` constant in `popup.js` to your Railway URL.

---

## 🚢 Deployment

### Backend → Railway

1. Push `slt.backend/` to a GitHub repository
2. Go to [railway.app](https://railway.app) → **New Project** → **Deploy from GitHub**
3. Add a **PostgreSQL** database plugin
4. Set environment variables:

```
ASPNETCORE_ENVIRONMENT         = Production
ASPNETCORE_URLS                = http://+:8080
ConnectionStrings__DefaultConnection = <from Railway PostgreSQL>
Jwt__Key                       = <your 64-char secret>
Jwt__Issuer                    = SLT.API
Jwt__Audience                  = SLT.Client
Groq__ApiKey                   = <your key>
Groq__Model                    = openai/gpt-oss-120b
Google__ClientId               = <your client id>
AllowedOrigin                  = https://your-app.vercel.app
```

Migrations run automatically on startup via `db.Database.Migrate()` in `Program.cs`.

### Frontend → Vercel

1. Push `slt.frontend/` to a GitHub repository
2. Go to [vercel.com](https://vercel.com) → **Add New Project** → import repository
3. Set environment variables:

```
NEXT_PUBLIC_API_URL  = https://your-backend.up.railway.app/api
NEXTAUTH_URL         = https://your-app.vercel.app
NEXTAUTH_SECRET      = <your secret>
GOOGLE_CLIENT_ID     = <your client id>
GOOGLE_CLIENT_SECRET = <your secret>
```

4. Deploy. Vercel auto-deploys on every push to `main`.

### Update Google OAuth URIs

After deployment, add to your Google Cloud Console OAuth client:

- **Authorized JavaScript origins**: `https://your-app.vercel.app`
- **Authorized redirect URIs**: `https://your-app.vercel.app/api/auth/callback/google`

### Deployment Cost Summary

| Service | Plan | Cost |
|---|---|---|
| Railway (backend + DB) | Starter | ~$5/month (usage-based) |
| Vercel (frontend) | Hobby | Free |
| Groq API | Free tier | Free (14,400 req/day) |
| Google OAuth | Free | Free |

---

## 🗺 Roadmap

- [ ] **Phase 14** — React Native Mobile App (iOS + Android)
- [ ] **Phase 15** — Push Notifications (streaks, due flashcards)
- [ ] **Phase 16** — AI Learning Path Generator
- [ ] **Phase 17** — Podcast & Video transcription support
- [ ] **Phase 18** — Export to PDF / Notion / Obsidian
- [ ] **Phase 19** — Public Learning Profile (showcase what you're learning)
- [ ] **Phase 20** — Chrome Extension auto-save on page scroll

---

## 🤝 Contributing

Contributions are welcome! Here's how to get started:

1. **Fork** the repository
2. Create a feature branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. Make your changes and commit:
   ```bash
   git commit -m "feat: add your feature description"
   ```
4. Push and open a **Pull Request**

### Commit Message Convention

This project follows [Conventional Commits](https://www.conventionalcommits.org/):

```
feat:     New feature
fix:      Bug fix
docs:     Documentation changes
style:    Formatting changes
refactor: Code refactoring
test:     Adding tests
chore:    Build/config changes
```

### Development Guidelines

- Follow Clean Architecture principles — no business logic in controllers
- All new API endpoints must be authenticated with `[Authorize]`
- New entities must have a corresponding migration
- DTOs are required — never return Entity objects directly from controllers
- Use `showSuccess` / `showError` toasts — no `alert()` calls in frontend

---

## 🐛 Known Issues & Troubleshooting

### "relation does not exist" error
**Cause:** Migrations haven't been applied.
**Fix:**
```bash
dotnet ef database update --project SLT.Infrastructure --startup-project SLT.API
```

### AI summary shows generic response
**Cause:** The page (especially Medium) blocks server-side scraping.
**Fix:** The app uses Jina AI Reader as a bypass. If it still fails, the AI infers from the URL slug. Try again with a different article.

### Google login shows "This action with HTTP GET is not supported"
**Cause:** NextAuth route file is in the wrong location.
**Fix:** Ensure the file is at `src/app/api/auth/[...nextauth]/route.ts` (with square brackets in folder name).

### Groq API returns empty summary
**Cause:** API key is missing or invalid in `appsettings.Development.json`.
**Fix:** Verify your Groq API key at [console.groq.com](https://console.groq.com).

### Package version conflicts (.NET)
**Cause:** Npgsql and EF Core versions must match.
**Fix:** Use EF Core `9.0.x` and Npgsql `9.0.x` together.

---

## 📄 License

This project is licensed under the **MIT License**.

```
MIT License

Copyright (c) 2025 Smart Learning Tracker

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
```

---

## 👤 Author

**Your Name**
- GitHub: [@your-username](https://github.com/your-username)
- LinkedIn: [your-linkedin](https://linkedin.com/in/your-linkedin)
- Email: your@email.com

---

## 🙏 Acknowledgements

- [Groq](https://groq.com) — Ultra-fast LLaMA inference API
- [Jina AI](https://jina.ai) — Free web content reader
- [Scalar](https://scalar.com) — Beautiful API documentation
- [Tailwind CSS](https://tailwindcss.com) — Utility-first CSS framework
- [Zustand](https://zustand-demo.pmnd.rs) — Lightweight React state management
- [NextAuth.js](https://next-auth.js.org) — Authentication for Next.js

---

<div align="center">
  <p>Built with ❤️ to make learning stick.</p>
  <p>⭐ Star this repo if you found it useful!</p>
</div>
