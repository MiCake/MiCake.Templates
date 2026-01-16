#!/bin/bash

# ============================================================
# MiCake AI Agent Setup Script (Bash)
# This script downloads and integrates AI Agent from GitHub
# ============================================================

set -e

REPO_URL="https://github.com/MiCake/MiCake.AI.Agent"
BRANCH="main"
TEMP_DIR=$(mktemp -d)
ZIP_URL="$REPO_URL/archive/refs/heads/$BRANCH.zip"
ZIP_FILE="$TEMP_DIR/repo.zip"

echo ''
echo '============================================================'
echo '  MiCake AI Agent Setup'
echo '============================================================'
echo ''
echo "Downloading AI Agent from: $REPO_URL"
echo ''

cleanup() {
    rm -rf "$TEMP_DIR" 2>/dev/null || true
}

trap cleanup EXIT

# Download ZIP
echo '[INFO] Downloading ZIP archive...'
if command -v curl &>/dev/null; then
    curl -L -o "$ZIP_FILE" "$ZIP_URL" 2>/dev/null || {
        echo '[ERROR] Failed to download with curl.'
        exit 1
    }
elif command -v wget &>/dev/null; then
    wget -O "$ZIP_FILE" "$ZIP_URL" 2>/dev/null || {
        echo '[ERROR] Failed to download with wget.'
        exit 1
    }
else
    echo '[ERROR] Neither curl nor wget is available.'
    echo 'Please manually download from: https://github.com/MiCake/MiCake.AI.Agent'
    exit 1
fi

# Extract ZIP
echo '[INFO] Extracting files...'
if command -v unzip &>/dev/null; then
    unzip -q "$ZIP_FILE" -d "$TEMP_DIR" || {
        echo '[ERROR] Failed to extract ZIP archive.'
        exit 1
    }
else
    echo '[ERROR] unzip is not available.'
    echo 'Please install unzip and try again.'
    exit 1
fi

# Find extracted directory
EXTRACTED_DIR=$(find "$TEMP_DIR" -maxdepth 1 -type d -name 'MiCake.AI.Agent-*' | head -1)

if [ -n "$EXTRACTED_DIR" ]; then
    # Copy files excluding README files
    echo '[INFO] Copying files to project directory...'
    find "$EXTRACTED_DIR" -maxdepth 1 ! -name 'README*' ! -path "$EXTRACTED_DIR" -exec cp -r {} . \;
fi

# Update preferences.yaml to link with project's copilot-instructions.md
PREFERENCES_FILE=".micake/agents/config/preferences.yaml"
if [ -f "$PREFERENCES_FILE" ]; then
    echo '[INFO] Configuring AI Agent preferences...'
    # Update custom_practices.file_path value using sed
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS sed requires different syntax
        sed -i '' 's|\(file_path: *\)""|\'1'".github/copilot-instructions.md"|' "$PREFERENCES_FILE"
    else
        # Linux sed
        sed -i 's|\(file_path: *\)""|\'1'".github/copilot-instructions.md"|' "$PREFERENCES_FILE"
    fi
    echo '[INFO] Linked AI Agent with .github/copilot-instructions.md'
fi

echo ''
echo '[SUCCESS] AI Agent has been successfully integrated!'
echo ''
