-- GymTracker 001 — plain PG from EF Core InitialCreate
CREATE TABLE IF NOT EXISTS "Users" (id SERIAL PRIMARY KEY, email VARCHAR(320) UNIQUE NOT NULL, password_hash TEXT NOT NULL, created_at TIMESTAMPTZ DEFAULT now());
CREATE TABLE IF NOT EXISTS exercises (id SERIAL PRIMARY KEY, user_id INTEGER REFERENCES "Users"(id), name VARCHAR(200) NOT NULL, created_at TIMESTAMPTZ DEFAULT now());
CREATE TABLE IF NOT EXISTS workouts (id SERIAL PRIMARY KEY, user_id INTEGER REFERENCES "Users"(id), date DATE NOT NULL, created_at TIMESTAMPTZ DEFAULT now());
CREATE TABLE IF NOT EXISTS workout_exercises (id SERIAL PRIMARY KEY, workout_id INTEGER REFERENCES workouts(id), exercise_id INTEGER REFERENCES exercises(id), reps INTEGER, weight DOUBLE PRECISION, duration INTEGER);
CREATE INDEX IF NOT EXISTS ix_exercises_user_id ON exercises(user_id);
CREATE INDEX IF NOT EXISTS ix_workouts_user_id ON workouts(user_id);
CREATE INDEX IF NOT EXISTS ix_workouts_date ON workouts(date DESC);
