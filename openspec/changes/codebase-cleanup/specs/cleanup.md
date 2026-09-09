## Overview

This change is a pure cleanup — no new or modified capabilities. Dead code, unused endpoints, unused model properties, and duplicated utilities are removed. All existing behavior is preserved identically.

## Scope

- Backend: Remove 7 dead endpoints, gut StatsService, delete dead DTOs/AutoMapper profiles, clean model properties
- Frontend: Remove dead interfaces/methods, extract shared error utility, clean unused imports
