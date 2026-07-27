#!/bin/sh
cd /root/gym-tracker
docker-compose -f docker-compose.prod.yml pull gym-tracker db
docker-compose -f docker-compose.prod.yml up -d gym-tracker db
docker image prune -f
