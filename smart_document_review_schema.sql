-- Table: Documents
CREATE TABLE Documents (
    Id SERIAL PRIMARY KEY,
    FileName TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    CreatedBy TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedBy TEXT,
    UpdatedAt TIMESTAMP
);

-- Table: Keywords
CREATE TABLE Keywords (
    Id SERIAL PRIMARY KEY,
    DocumentId INT REFERENCES Documents(Id) ON DELETE CASCADE,
    Keyword TEXT NOT NULL,
    CreatedBy TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedBy TEXT,
    UpdatedAt TIMESTAMP
);

-- Table: TagMatches
CREATE TABLE TagMatches (
    Id SERIAL PRIMARY KEY,
    DocumentId INT REFERENCES Documents(Id) ON DELETE CASCADE,
    Keyword TEXT NOT NULL,
    SectionTitle TEXT,
    MatchedText TEXT,
    PageNumber INT,
    PageX REAL,
    PageY REAL,
    Width REAL,
    CreatedBy TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedBy TEXT,
    UpdatedAt TIMESTAMP
);