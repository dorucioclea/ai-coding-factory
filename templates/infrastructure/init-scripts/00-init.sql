-- Database initialization script
-- This runs automatically when PostgreSQL container starts with empty volume

-- Enable useful extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create additional schemas if needed
-- CREATE SCHEMA IF NOT EXISTS audit;

-- Grant permissions (adjust as needed)
-- GRANT ALL PRIVILEGES ON SCHEMA public TO postgres;

-- Log initialization
DO $$
BEGIN
    RAISE NOTICE 'Database initialized at %', NOW();
END $$;
