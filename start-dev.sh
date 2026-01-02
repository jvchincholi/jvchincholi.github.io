#!/bin/bash

# Define the root path (the directory where this script sits)
ROOT_DIR=$(pwd)

echo "🚀 Starting the MyModernAPI Ecosystem..."

# 1. Start Azurite in a new Background Process
# We use & to run it in the background of this script
echo "📦 Starting Azurite (Storage)..."
azurite --silent &
AZURITE_PID=$!

# 2. Start Azure Functions in a new Terminal Tab
echo "⚡ Starting Background Tasks (Functions)..."
osascript -e "tell application \"Terminal\" to do script \"cd '$ROOT_DIR/BackgroundTasks' && func start\""

# 3. Start the Web API in the current window
echo "🌐 Starting Web API..."
echo "Wait 5 seconds for services to warm up..."
sleep 5

cd "$ROOT_DIR"
dotnet run