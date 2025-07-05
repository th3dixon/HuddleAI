#!/usr/bin/env python3
"""
Fix Duplicate XML Summary Comments Script

This script removes duplicate XML summary comments that appear before properties,
especially those that appear both before and after attributes like [JsonPropertyName].

Usage:
    python fix_duplicate_xml_summaries.py --source-path src --project-path src/API/MeAndMyDog.API/MeAndMyDog.API.csproj
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
            write_status("Build failed after fixing duplicate summaries", "FAIL")
            print(result.stderr)
            return False
        
        # Count warnings related to XML documentation
        xml_warnings = []
        if result.stdout:
            lines = result.stdout.split('\n')
            for line in lines:
                if 'warning' in line.lower() and ('xml' in line.lower() or 'cs1570' in line.lower() or 'cs1587' in line.lower()):
                    xml_warnings.append(line)
        
        if xml_warnings:
            write_status(f"Build succeeded with {len(xml_warnings)} XML warnings:", "WARN")
            for warning in xml_warnings[:5]:  # Show first 5 XML warnings
                print(f"  {warning}")
            if len(xml_warnings) > 5:
                print(f"  ... and {len(xml_warnings) - 5} more XML warnings")
        else:
            write_status("Build verification successful - no XML documentation warnings", "PASS")
        
        return True
        
    except subprocess.TimeoutExpired:
        write_status("Build verification timed out", "FAIL")
        return False
    except Exception as e:
        write_status(f"Build verification error: {e}", "FAIL")
        return False


def fix_duplicate_summaries_in_file(file_path: str, dry_run: bool = False) -> int:
    """Fix duplicate XML summary comments in a C# file. Returns number of duplicates removed."""
    if not os.path.exists(file_path):
        write_status(f"File not found: {file_path}", "FAIL")
        return 0
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        new_lines = []
        removed_count = 0
        i = 0
        
        while i < len(lines):
            line = lines[i]
            
            # Check for XML summary comment pattern
            if re.match(r'^\s*///\s*<summary>', line.strip()):
                # Look for potential duplicate summary ahead
                summary_lines = []
                j = i
                
                # Collect the full XML summary block
                while j < len(lines):
                    current_line = lines[j]
                    summary_lines.append(current_line)
                    
                    if re.match(r'^\s*///\s*</summary>', current_line.strip()):
                        j += 1
                        break
                    j += 1
                
                # Look ahead for attributes and potential duplicate summary
                k = j
                attribute_lines = []
                
                # Skip whitespace and collect attributes
                while k < len(lines):
                    ahead_line = lines[k]
                    if ahead_line.strip() == '':
                        attribute_lines.append(ahead_line)
                        k += 1
                    elif re.match(r'^\s*\[.*\]', ahead_line.strip()):
                        attribute_lines.append(ahead_line)
                        k += 1
                    else:
                        break
                
                # Check if there's a duplicate summary after attributes
                duplicate_found = False
                if k < len(lines) and re.match(r'^\s*///\s*<summary>', lines[k].strip()):
                    # Found potential duplicate, compare content
                    duplicate_summary_lines = []
                    m = k
                    
                    while m < len(lines):
                        dup_line = lines[m]
                        duplicate_summary_lines.append(dup_line)
                        
                        if re.match(r'^\s*///\s*</summary>', dup_line.strip()):
                            m += 1
                            break
                        m += 1
                    
                    # Compare the summary content (ignoring whitespace differences)
                    original_content = ' '.join([line.strip() for line in summary_lines]).lower()
                    duplicate_content = ' '.join([line.strip() for line in duplicate_summary_lines]).lower()
                    
                    # Remove common XML tags for comparison
                    original_clean = re.sub(r'///\s*</?summary>', '', original_content).strip()
                    duplicate_clean = re.sub(r'///\s*</?summary>', '', duplicate_content).strip()
                    
                    if original_clean == duplicate_clean:
                        duplicate_found = True
                        removed_count += 1
                        
                        if not dry_run:
                            write_status(f"Removing duplicate summary: {duplicate_clean[:50]}...", "PROCESS")
                        
                        # Add original summary
                        new_lines.extend(summary_lines)
                        # Add attributes
                        new_lines.extend(attribute_lines)
                        # Skip the duplicate summary
                        i = m
                        continue
                
                # No duplicate found, add normally
                new_lines.extend(summary_lines)
                i = j
            else:
                # Regular line, add it
                new_lines.append(line)
                i += 1
        
        if removed_count > 0:
            if dry_run:
                write_status(f"Would remove {removed_count} duplicate summaries from: {file_path}", "INFO")
            else:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write('\n'.join(new_lines))
                write_status(f"Removed {removed_count} duplicate summaries from: {file_path}", "PROCESS")
        
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
    parser = argparse.ArgumentParser(description='Fix duplicate XML summary comments in C# files')
    parser.add_argument('--source-path', required=True, help='Path to source directory or C# file')
    parser.add_argument('--project-path', required=True, help='Path to .csproj file for build verification')
    parser.add_argument('--dry-run', action='store_true', help='Show what would be changed without modifying files')
    
    args = parser.parse_args()
    
    print(f"{Colors.BLUE}=== XML Duplicate Summary Fixer ==={Colors.RESET}")
    print(f"Source Path: {args.source_path}")
    print(f"Project Path: {args.project_path}")
    print(f"Dry Run: {args.dry_run}")
    print()
    
    # Verify initial build state
    if not test_build_stability(args.project_path):
        write_status("Initial build verification failed. Fix build issues before removing duplicates.", "FAIL")
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
        removed_count = fix_duplicate_summaries_in_file(file_path, args.dry_run)
        
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
    write_status(f"Total duplicate summaries removed: {total_removed}", "INFO")
    
    if args.dry_run:
        write_status("Dry run completed - no files were actually modified", "INFO")
    else:
        write_status("Duplicate XML summary removal completed successfully", "PASS")
    
    return 0


if __name__ == "__main__":
    sys.exit(main())