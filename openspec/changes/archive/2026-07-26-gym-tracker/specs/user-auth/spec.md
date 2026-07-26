## ADDED Requirements

### Requirement: User registration
The system SHALL allow users to create an account with username, email, and password.

#### Scenario: Successful registration
- **WHEN** user submits valid username, email, and password
- **THEN** system creates account and returns JWT token

#### Scenario: Duplicate username
- **WHEN** user submits username that already exists
- **THEN** system returns error "Username already taken"

#### Scenario: Invalid email
- **WHEN** user submits invalid email format
- **THEN** system returns validation error

### Requirement: User login
The system SHALL authenticate users with email and password.

#### Scenario: Successful login
- **WHEN** user submits valid email and password
- **THEN** system returns JWT token

#### Scenario: Invalid credentials
- **WHEN** user submits wrong email or password
- **THEN** system returns error "Invalid credentials"

### Requirement: Get user profile
The system SHALL return the authenticated user's profile.

#### Scenario: Retrieve profile
- **WHEN** authenticated user requests profile
- **THEN** system returns user data including weight and height

### Requirement: Update user profile
The system SHALL allow users to update their weight and height.

#### Scenario: Update profile
- **WHEN** user updates weight to 72 and height to 180
- **THEN** system saves changes and returns updated profile
