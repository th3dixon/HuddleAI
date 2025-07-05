#!/usr/bin/env python3
"""
Remove XML Returns Comments Script

This script removes XML comment lines that contain <returns></returns> tags
which are causing build warnings in the .NET solution.

Usage:
    python remove_returns_xml_comments.py --source-path src --project-path src/API/MeAndMyDog.API/MeAndMyDog.API.csproj
"""

import os
import re
import sys
import argparse
import subprocess
from pathlib import Path
from typing import List


class Colors:
    RED = '\033[31m'
    GREEN = '\033[32m'
    YELLOW = '\033[33m'
    BLUE = '\033[34m'
    CYAN = '\033[36m'
    RESET = '\033[0m'


def write_status(message: str, status: str) -> None:
    """Print status message with color coding."""
    color_map = {
        'PASS': Colors.GREEN,
        'FAIL': Colors.RED,
        'WARN': Colors.YELLOW,
        'INFO': Colors.BLUE,
        'PROCESS': Colors.CYAN
    }
    color = color_map.get(status, Colors.RESET)
    print(f"[{color}{status}{Colors.RESET}] {message}")


def test_build_stability(project_path: str) -> bool:
    """Test if the project builds successfully."""
    write_status("Verifying build stability...", "INFO")
    
    try:
        result = subprocess.run(
            ['dotnet', 'build', project_path, '--verbosity', 'quiet'],
            capture_output=True,
            text=True,
            timeout=120
        )
        
        if result.returncode != 0:
            write_status("Build failed after removing returns comments", "FAIL")
            print(result.stderr)
            return False
        
        # Check for warnings
        if 'warning' in result.stdout.lower():
            write_status("Build succeeded with warnings:", "WARN")
            warning_lines = [line for line in result.stdout.split('\n') if 'warning' in line.lower()]
            for line in warning_lines[:10]:  # Show first 10 warnings
                print(f"  {line}")
            if len(warning_lines) > 10:
                print(f"  ... and {len(warning_lines) - 10} more warnings")
        else:
            write_status("Build verification successful - no errors or warnings", "PASS")
        
        return True
        
    except subprocess.TimeoutExpired:
        write_status("Build verification timed out", "FAIL")
        return False
    except Exception as e:
        write_status(f"Build verification error: {e}", "FAIL")
        return False


def remove_returns_comments_from_file(file_path: str, dry_run: bool = False) -> int:
    """Remove XML returns comments from a C# file. Returns number of lines removed."""
    if not os.path.exists(file_path):
        write_status(f"File not found: {file_path}", "FAIL")
        return 0
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        new_lines = []
        removed_count = 0
        
        for line in lines:
            # Check if line contains XML returns comment
            if re.search(r'^\s*///.*<returns>.*</returns>', line.strip()):
                removed_count += 1
                if not dry_run:
                    write_status(f"Removing: {line.strip()}", "PROCESS")
                continue
            else:
                new_lines.append(line)
        
        if removed_count > 0:
            if dry_run:
                write_status(f"Would remove {removed_count} returns comment(s) from: {file_path}", "INFO")
            else:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write('\n'.join(new_lines))
                write_status(f"Removed {removed_count} returns comment(s) from: {file_path}", "PROCESS")
        
        return removed_count
            
    except Exception as e:
        write_status(f"Error processing {file_path}: {e}", "FAIL")
        return 0


def get_csharp_files(source_path: str) -> List[str]:
    """Get list of C# files to process."""
    path = Path(source_path)
    
    if path.is_file():
        if path.suffix == '.cs':
            return [str(path)]
        else:
            write_status(f"File is not a C# file: {source_path}", "WARN")
            return []
    
    elif path.is_dir():
        cs_files = []
        for cs_file in path.rglob('*.cs'):
            # Skip auto-generated and designer files
            if not any(pattern in cs_file.name for pattern in ['.g.cs', '.designer.cs', 'AssemblyInfo.cs', 'GlobalAssemblyInfo.cs']):
                # Skip obj and bin directories
                if not any(part in ['obj', 'bin', 'packages'] for part in cs_file.parts):
                    cs_files.append(str(cs_file))
        return cs_files
    
    else:
        write_status(f"Path not found: {source_path}", "FAIL")
        return []


def main():
    """Main execution function."""
    parser = argparse.ArgumentParser(description='Remove XML returns comments from C# files')
    parser.add_argument('--source-path', required=True, help='Path to source directory or C# file')
    parser.add_argument('--project-path', required=True, help='Path to .csproj file for build verification')
    parser.add_argument('--dry-run', action='store_true', help='Show what would be changed without modifying files')
    
    args = parser.parse_args()
    
    print(f"{Colors.BLUE}=== XML Returns Comments Remover ==={Colors.RESET}")
    print(f"Source Path: {args.source_path}")
    print(f"Project Path: {args.project_path}")
    print(f"Dry Run: {args.dry_run}")
    print()
    
    # Verify initial build state
    if not test_build_stability(args.project_path):
        write_status("Initial build verification failed. Fix build issues before removing comments.", "FAIL")
        return 1
    
    # Get all C# files to process
    files = get_csharp_files(args.source_path)
    if not files:
        write_status("No C# files found to process", "WARN")
        return 0
    
    write_status(f"Found {len(files)} C# files to process", "INFO")
    
    # Process files
    total_removed = 0
    files_modified = 0
    
    for file_path in files:
        relative_file = os.path.relpath(file_path)
        removed_count = remove_returns_comments_from_file(file_path, args.dry_run)
        
        if removed_count > 0:
            total_removed += removed_count
            files_modified += 1
    
    # Final verification
    if not args.dry_run and total_removed > 0:
        write_status("Running final build verification...", "INFO")
        if not test_build_stability(args.project_path):
            write_status("Final build verification failed", "FAIL")
            return 1
    
    # Summary
    print()
    print(f"{Colors.GREEN}=== Summary ==={Colors.RESET}")
    write_status(f"Files processed: {len(files)}", "INFO")
    write_status(f"Files modified: {files_modified}", "INFO")
    write_status(f"Total returns comments removed: {total_removed}", "INFO")
    
    if args.dry_run:
        write_status("Dry run completed - no files were actually modified", "INFO")
    else:
        write_status("XML returns comments removal completed successfully", "PASS")
    
    return 0


if __name__ == "__main__":
    sys.exit(main())