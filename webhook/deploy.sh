#!/bin/sh
set -e
cd /root/gym-tracker

echo "Pulling latest images..."
docker compose -f docker-compose.prod.yml pull gym-tracker db

echo "Recreating gym-tracker..."
# compose v1 won't recreate on image digest change alone, and its recreate
# path hits a KeyError bug with newer Docker — stop/rm first instead.
docker stop gym-tracker 2>/dev/null || true
docker rm gym-tracker 2>/dev/null || true

docker compose -f docker-compose.prod.yml up -d gym-tracker db webhook

echo "Pruning old images..."
docker image prune -f

echo "Deploy complete."
