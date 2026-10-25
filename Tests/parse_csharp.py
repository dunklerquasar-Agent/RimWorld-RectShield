#!/usr/bin/env python3
"""Syntaxcheck fuer alle C#-Dateien via tree-sitter.

Ersetzt KEINEN Compile gegen Assembly-CSharp.dll (dafuer docs/API_NOTES.md),
findet aber Syntaxfehler und vergessene Klammern zuverlaessig in CI.

    pip install tree-sitter tree-sitter-c-sharp
    python3 Tests/parse_csharp.py
"""
from __future__ import annotations

import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

try:
    import tree_sitter
    import tree_sitter_c_sharp
except ImportError:
    print("OK (uebersprungen): tree-sitter nicht installiert.", file=sys.stderr)
    sys.exit(0)

try:
    language = tree_sitter.Language(tree_sitter_c_sharp.language())
    parser = tree_sitter.Parser(language)
except TypeError:
    # Kompatibilitaet mit aelteren tree-sitter-Versionen
    parser = tree_sitter.Parser()
    parser.set_language(tree_sitter.Language(tree_sitter_c_sharp.language()))

files = sorted((ROOT / "Source").rglob("*.cs"))
errors: list[str] = []

for path in files:
    source = path.read_bytes()
    tree = parser.parse(source)
    if tree.root_node.has_error:
        def walk(node):
            if node.type == "ERROR" or node.is_missing:
                line = node.start_point[0] + 1
                snippet = source.splitlines()[line - 1].decode("utf-8", "replace").strip()
                errors.append(f"{path.relative_to(ROOT)}:{line}: {node.type} bei: {snippet[:100]}")
            for child in node.children:
                walk(child)

        walk(tree.root_node)

print(f"Geprueft: {len(files)} C#-Dateien.")

if errors:
    print("\nSYNTAXFEHLER:", file=sys.stderr)
    for err in errors[:40]:
        print(f"  - {err}", file=sys.stderr)
    sys.exit(1)

print("OK: keine C#-Syntaxfehler gefunden.")
