#!/bin/sh
echo -ne '\033c\033]0;local-llm-npc (4.4)\a'
base_path="$(dirname "$(realpath "$0")")"
"$base_path/local-llm-npc (4.4).x86_64" "$@"
