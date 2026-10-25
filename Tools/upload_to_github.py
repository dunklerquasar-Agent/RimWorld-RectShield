#!/usr/bin/env python3
"""Pusht den kompletten Mod-Ordner in EINEM Commit via GitHub REST API.

Voraussetzungen
---------------
1. Auf https://github.com/new ein LEERES Repo anlegen (kein README, keine Lizenz).
   (Das Skript kann auch mit einem bestehenden nicht-leeren Repo umgehen.)
2. Fine-grained Personal Access Token erzeugen:
   Settings -> Developer settings -> Personal access tokens -> Fine-grained tokens
   - Repository access: "Only select repositories" -> dein neues Repo
   - Permissions: Contents = Read and Write
   Ein Classic-Token mit repo-Scope funktioniert ebenfalls.
3. Aufrufen:

   GITHUB_TOKEN=github_pat_xxx GITHUB_REPO=DEINNAME/RimWorld-RectShield \
       python3 Tools/upload_to_github.py

   Dry-Run (nur Dateiliste, kein Netzwerk):

   python3 Tools/upload_to_github.py --dry-run

Der Token wird NUR aus der Umgebung gelesen, nie geloggt und nie in eine
Datei geschrieben. Bitte nach dem Upload sofort widerrufen.

Hintergrund: Auf einem komplett leeren Repo lehnt GitHubs Git-Data-API das
Erstellen von Blobs mit HTTP 409 "Git Repository is empty" ab. Das Skript
erzeugt daher vorher per Contents-API einen winzigen Initial-Commit und
ersetzt dessen Ref anschliessend durch einen echten Root-Commit (force).
Sichtbar bleibt genau EIN Commit.
"""
from __future__ import annotations

import base64
import json
import os
import sys
import urllib.error
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
API = "https://api.github.com"
COMMIT_MESSAGE = "RectShield v0.1.1 - rectangular shields, ingame graphics and research tab"
SKIP_DIRS = {".git", "__pycache__", ".venv", "bin", "obj"}
SKIP_FILES = {".DS_Store", "Thumbs.db"}

TOKEN = os.environ.get("GITHUB_TOKEN", "").strip()
REPO = os.environ.get("GITHUB_REPO", "").strip().strip("/")
BRANCH = os.environ.get("GITHUB_BRANCH", "main").strip() or "main"
DRY_RUN = "--dry-run" in sys.argv


def raw_request(method: str, path: str, payload=None):
    """Gibt (status, body) zurueck, ohne bei HTTP-Fehlern zu werfen."""
    body = json.dumps(payload).encode("utf-8") if payload is not None else None
    req = urllib.request.Request(
        API + path,
        data=body,
        method=method,
        headers={
            "Authorization": "Bearer " + TOKEN,
            "Accept": "application/vnd.github+json",
            "X-GitHub-Api-Version": "2022-11-28",
            "Content-Type": "application/json",
            "User-Agent": "RectShield-Uploader/1.1",
        },
    )
    try:
        with urllib.request.urlopen(req, timeout=90) as response:
            raw = response.read()
            return response.status, (json.loads(raw) if raw else {})
    except urllib.error.HTTPError as exc:
        detail = exc.read().decode("utf-8", "replace")[:600]
        return exc.code, {"error": detail}


def request(method: str, path: str, payload=None):
    status, body = raw_request(method, path, payload)
    if status >= 400:
        raise RuntimeError(f"GitHub API {method} {path} -> HTTP {status}: "
                           f"{str(body)[:500]}")
    return body, status


def collect_files():
    result = []
    for path in sorted(ROOT.rglob("*")):
        if not path.is_file():
            continue
        rel = path.relative_to(ROOT)
        if any(part in SKIP_DIRS for part in rel.parts):
            continue
        if rel.name in SKIP_FILES or rel.suffix in {".zip", ".bundle"}:
            continue
        result.append((rel.as_posix(), path.read_bytes()))
    return result


def main() -> int:
    files = collect_files()
    total = sum(len(content) for _, content in files)
    print(f"Dateien: {len(files)} | Gesamtgroesse: {total / 1024:.1f} KiB")

    if DRY_RUN:
        for name, content in files:
            print(f"  {len(content):8d}  {name}")
        print("Dry-Run: keine API-Aufrufe.")
        return 0

    if not TOKEN or not REPO or "/" not in REPO:
        print("FEHLER: GITHUB_TOKEN und GITHUB_REPO=owner/name muessen gesetzt sein.",
              file=sys.stderr)
        return 2

    print(f"Ziel: https://github.com/{REPO} (Branch {BRANCH})")

    info, _ = request("GET", f"/repos/{REPO}")
    print(f"Repo gefunden: {info.get('full_name')} | private={info.get('private')}")

    # Branch-Ref pruefen. 404/409 = leeres Repo -> Git-Data-API braucht
    # zuerst irgendeinen Commit (Contents-API kann ihn anlegen).
    ref_status, ref_body = raw_request("GET", f"/repos/{REPO}/git/ref/heads/{BRANCH}")
    empty_repo = ref_status in (404, 409)

    if empty_repo:
        print("Leeres Repo: erzeuge Initial-Commit ueber die Contents-API...")
        request("PUT", f"/repos/{REPO}/contents/.gitkeep", {
            "message": "init",
            "content": base64.b64encode(b"").decode("ascii"),
            "branch": BRANCH,
        })
        parent_sha = None
        force = True          # Initial-Commit durch unseren Root-Commit ersetzen
    elif ref_status == 200:
        parent_sha = ref_body["object"]["sha"]
        force = False
        print(f"Bestehender Commit: {parent_sha[:8]}")
    else:
        raise RuntimeError(f"Ref-Abfrage fehlgeschlagen: HTTP {ref_status} {ref_body}")

    tree_entries = []
    for index, (name, content) in enumerate(files, start=1):
        blob, _ = request("POST", f"/repos/{REPO}/git/blobs", {
            "content": base64.b64encode(content).decode("ascii"),
            "encoding": "base64",
        })
        tree_entries.append({
            "path": name,
            "mode": "100755" if name.endswith(".sh") else "100644",
            "type": "blob",
            "sha": blob["sha"],
        })
        if index % 15 == 0 or index == len(files):
            print(f"  Blobs: {index}/{len(files)}")

    tree, _ = request("POST", f"/repos/{REPO}/git/trees", {"tree": tree_entries})
    print(f"Tree: {tree['sha'][:8]}")

    commit_payload = {"message": COMMIT_MESSAGE, "tree": tree["sha"]}
    if parent_sha:
        commit_payload["parents"] = [parent_sha]
    commit, _ = request("POST", f"/repos/{REPO}/git/commits", commit_payload)
    print(f"Commit: {commit['sha'][:8]} (Root-Commit={parent_sha is None})")

    if empty_repo and parent_sha is None:
        # Nach der Contents-API-Initialisierung existiert der Ref bereits.
        request("PATCH", f"/repos/{REPO}/git/refs/heads/{BRANCH}",
                {"sha": commit["sha"], "force": force})
    elif parent_sha:
        request("PATCH", f"/repos/{REPO}/git/refs/heads/{BRANCH}",
                {"sha": commit["sha"], "force": False})
    else:
        request("POST", f"/repos/{REPO}/git/refs",
                {"ref": f"refs/heads/{BRANCH}", "sha": commit["sha"]})

    print(f"FERTIG: https://github.com/{REPO}")
    print(f"Commit: {commit['sha']}")
    print("Bitte den Token jetzt widerrufen (GitHub Settings -> Developer settings).")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except RuntimeError as exc:
        print(f"FEHLER: {exc}", file=sys.stderr)
        sys.exit(1)
    except KeyboardInterrupt:
        sys.exit(130)
