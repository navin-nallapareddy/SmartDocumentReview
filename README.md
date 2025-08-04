# Smart Document Review

An AI-assisted web tool to extract and highlight relevant sections in PDF documents based on user-supplied keywords.

## Features
- Upload PDF and enter one keyword per line (max 5 words per keyword)
- Highlights matched sections and keywords
- Shows results with section titles and links to PDF viewer
- Basic login (testuser / Test@123)
- Audit fields: CreatedBy, CreatedAt, etc.
- Built with Blazor Server + C# + PostgreSQL-ready
- Prefix a keyword with `*` to match inside larger words (e.g. `*bank` matches "bankruptcy")

## Getting Started

1. Clone or upload to GitHub
2. Run using `dotnet run`
3. Navigate to `/login` and use test credentials

## Deployment

- Designed to be hosted on Render or similar platforms.
- Replace hardcoded login with real Identity system for production.

## Default Credentials

- **Username**: testuser
- **Password**: Test@123
