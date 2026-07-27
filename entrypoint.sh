#!/bin/bash
nginx &
exec dotnet /app/GymTracker.dll
