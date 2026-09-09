# GymTracker

Web app to track gym progress. Stack: **Rust Axum** backend + **SvelteKit** frontend + **PostgreSQL** (+PgBouncer) + **Caddy**. Alpine/distroless images, monolith, no Redis.

Live: [gym.abuyunus.cc](https://gym.abuyunus.cc) · [Actions](https://github.com/amir1330/GymTracker/actions)

## Deployment (Docker + CI/CD)

Same hardened pattern as **sanaq** / **telegram-queue-bot**:

1. GitHub Actions builds/pushes `ghcr.io/amir1330/gymtracker` (`:latest` and `:<sha>`).
2. CI SSHs as non-root user **`gymtracker`** with a **forced-command** key.
3. Remote `deploy/ci-entry.sh` only accepts `sync-deploy` / `deploy`.
4. Host key is pinned in `.github/known_hosts`.

No webhook. No root SSH from CI. Postgres volume `gym-tracker_postgres_data` is not recreated.

### One-time VPS + secrets setup

From your laptop (needs `gh` auth + SSH as root once):

```bash
chmod +x deploy/provision-ci-key.sh
./deploy/provision-ci-key.sh
```

This creates `gymtracker` (in `docker` group), installs the restricted key, and sets
repo secrets `HOST`, `USERNAME`, `SSH_KEY`.

### Server layout

| Path | Purpose |
|---|---|
| `/home/gymtracker/gym-tracker/` | compose + `.env` (`POSTGRES_PASSWORD`, `JWT_SECRET`) |
| `/home/gymtracker/bin/ci-entry.sh` | forced-command entrypoint |

Compose project name stays **`gym-tracker`** so the existing Postgres volume is reused.

### GitHub secrets

| Secret | Value |
|---|---|
| `HOST` | VPS IP |
| `USERNAME` | `gymtracker` |
| `SSH_KEY` | private key (forced-command) |

**Never commit** database or JWT secrets. They stay in the server `.env` only.
