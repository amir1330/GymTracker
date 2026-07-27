#!/bin/sh
cd /root/gym-tracker
docker-compose -f docker-compose.prod.yml pull gym-tracker db
# compose v1 won't recreate on image digest change alone, and its recreate
# path hits a KeyError bug with newer Docker — stop/rm first instead.
docker stop gym-tracker 2>/dev/null
docker rm gym-tracker 2>/dev/null
docker-compose -f docker-compose.prod.yml up -d gym-tracker db
docker image prune -f
