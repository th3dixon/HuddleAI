#!/usr/bin/env python3
"""
Fix XML Comment Ordering Script

This script fixes XML documentation comments that appear after attributes
instead of before them, following Microsoft's XML documentation standards.

Usage:
    python fix_xml_comment_ordering.py --source-path src --project-path src/API/MeAndMyDog.API/MeAndMyDog.API.csproj
"""

import os
import re
import sys
import argparse
import subprocess
import shutil
import tempfile
from pathlib import Path
from typing import List, Tuple
import json


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


def test_build_stability(project_path: str) -> Tuple[bool, int]:
    """Test if the project builds successfully. Returns (success, warning_count)."""
    write_status("Verifying build stability...", "INFO")
    
    try:
        result = subprocess.run(
            ['dotnet', 'build', project_path, '--verbosity', 'quiet'],
            capture_output=True,
            text=True,
            timeout=180
        )
        
        if result.returncode != 0:
            write_status("Build failed", "FAIL")
            print(result.stderr)
            return False, 0
        
        # Count warnings
        warning_count = 0
        if result.stdout:
            warning_count = result.stdout.lower().count('warning')
        
        if warning_count > 0:
            write_status(f"Build succeeded with {warning_count} warnings", "WARN")
        else:
            write_status("Build verification successful - no warnings", "PASS")
        
        return True, warning_count
        
    except subprocess.TimeoutExpired:
        write_status("Build verification timed out", "FAIL")
        return False, 0
    except Exception as e:
        write_status(f"Build verification error: {e}", "FAIL")
        return False, 0


def create_backup(source_path: str) -> str:
    """Create a backup of the source directory."""
    backup_dir = tempfile.mkdtemp(prefix="xml_ordering_backup_")
    write_status(f"Creating backup at: {backup_dir}", "INFO")
    
    if os.path.isfile(source_path):
        shutil.copy2(source_path, backup_dir)
    else:
        shutil.copytree(source_path, os.path.join(backup_dir, "src"), dirs_exist_ok=True)
    
    return backup_dir


def restore_backup(backup_dir: str, target_path: str) -> bool:
    """Restore from backup."""
    try:
        write_status("Restoring from backup due to build issues...", "WARN")
        
        if os.path.isfile(target_path):
            # Single file restore
            backup_file = os.path.join(backup_dir, os.path.basename(target_path))
            if os.path.exists(backup_file):
                shutil.copy2(backup_file, target_path)
        else:
            # Directory restore
            backup_src = os.path.join(backup_dir, "src")
            if os.path.exists(backup_src):
                # Remove current and restore
                if os.path.exists(target_path):
                    shutil.rmtree(target_path)
                shutil.copytree(backup_src, target_path)
        
        write_status("Backup restored successfully", "PASS")
        return True
    except Exception as e:
        write_status(f"Failed to restore backup: {e}", "FAIL")
        return False


def fix_xml_comment_ordering_in_file(file_path: str, dry_run: bool = False) -> int:
    """Fix XML comment ordering in a single file. Returns number of fixes."""
    if not os.path.exists(file_path):
        write_status(f"File not found: {file_path}", "FAIL")
        return 0
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        new_lines = []
        fixes_count = 0
        
        i = 0
        while i < len(lines):
            line = lines[i]
            
            # Look for pattern: attributes followed by XML comments followed by declaration
            if line.strip().startswith('['):
                
                # Collect all attribute lines (including multi-line attributes)
                attribute_lines = []
                attr_start = i
                
                while i < len(lines) and lines[i].strip().startswith('['):
                    attribute_lines.append(lines[i])
                    i += 1
                
                # Check if next lines are XML comments
                xml_comment_lines = []
                
                while (i < len(lines) and 
                       lines[i].strip().startswith('///')):
                    xml_comment_lines.append(lines[i])
                    i += 1
                
                # If we found both attributes and XML comments, and there's a declaration after
                if (attribute_lines and xml_comment_lines and 
                    i < len(lines) and 
                    (lines[i].strip().startswith('public') or 
                     lines[i].strip().startswith('private') or 
                     lines[i].strip().startswith('protected') or 
                     lines[i].strip().startswith('internal') or
                     lines[i].strip().startswith('static') or
                     lines[i].strip().startswith('abstract') or
                     lines[i].strip().startswith('virtual') or
                     lines[i].strip().startswith('override'))):
                    
                    # Found a case that needs fixing - XML comments should come before attributes
                    fixes_count += 1
                    
                    if not dry_run:
                        write_status(f"Fixing XML comment ordering: {lines[i].strip()[:60]}...", "PROCESS")
                    else:
                        write_status(f"Would fix XML comment ordering: {lines[i].strip()[:60]}...", "INFO")
                    
                    # Add XML comments first, then attributes
                    new_lines.extend(xml_comment_lines)
                    new_lines.extend(attribute_lines)
                    
                    # Continue from the declaration line (don't increment i)
                    continue
                else:
                    # No XML comments after attributes, add the attribute lines as they were
                    new_lines.extend(attribute_lines)
                    new_lines.extend(xml_comment_lines)
                    # Continue from current position (don't increment i)
                    continue
            
            # Regular line, just add it
            new_lines.append(line)
            i += 1
        
        if fixes_count > 0:
            if dry_run:
                write_status(f"Would fix {fixes_count} XML comment ordering issues in: {file_path}", "INFO")
            else:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write('\n'.join(new_lines))
                write_status(f"Fixed {fixes_count} XML comment ordering issues in: {file_path}", "PROCESS")
        
        return fixes_count
            
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
    parser = argparse.ArgumentParser(description='Fix XML comment ordering in C# files')
    parser.add_argument('--source-path', required=True, help='Path to source directory or C# file')
    parser.add_argument('--project-path', required=True, help='Path to .csproj file for build verification')
    parser.add_argument('--batch-size', type=int, default=10, help='Files to process before build verification')
    parser.add_argument('--dry-run', action='store_true', help='Show what would be changed without modifying files')
    
    args = parser.parse_args()
    
    print(f"{Colors.BLUE}=== XML Comment Ordering Fixer ==={Colors.RESET}")
    print(f"Source Path: {args.source_path}")
    print(f"Project Path: {args.project_path}")
    print(f"Batch Size: {args.batch_size}")
    print(f"Dry Run: {args.dry_run}")
    print()
    
    # Verify initial build state
    initial_success, initial_warnings = test_build_stability(args.project_path)
    if not initial_success:
        write_status("Initial build verification failed. Fix build issues before proceeding.", "FAIL")
        return 1
    
    # Create backup if not dry run
    backup_dir = None
    if not args.dry_run:
        backup_dir = create_backup(args.source_path)
    
    try:
        # Get all C# files to process
        files = get_csharp_files(args.source_path)
        if not files:
            write_status("No C# files found to process", "WARN")
            return 0
        
        write_status(f"Found {len(files)} C# files to process", "INFO")
        
        # Process files in batches
        total_fixes = 0
        files_modified = 0
        processed_count = 0
        
        for file_path in files:
            relative_file = os.path.relpath(file_path)
            fixes = fix_xml_comment_ordering_in_file(file_path, args.dry_run)
            
            if fixes > 0:
                total_fixes += fixes
                files_modified += 1
            
            processed_count += 1
            
            # Check build stability at batch intervals
            if processed_count % args.batch_size == 0 and not args.dry_run:
                write_status(f"Completed batch ({processed_count}/{len(files)} files)", "INFO")
                
                success, warnings = test_build_stability(args.project_path)
                if not success:
                    write_status(f"Build verification failed after processing {processed_count} files", "FAIL")
                    if backup_dir:
                        restore_backup(backup_dir, args.source_path)
                        write_status("Changes reverted due to build failure", "WARN")
                    return 1
                
                # Check if warnings increased significantly (more than 50% increase)
                if warnings > initial_warnings * 1.5:
                    write_status(f"Warning count increased significantly ({initial_warnings} -> {warnings})", "WARN")
                    write_status("Consider reviewing the changes", "INFO")
        
        # Final verification
        if not args.dry_run and total_fixes > 0:
            write_status("Running final build verification...", "INFO")
            success, final_warnings = test_build_stability(args.project_path)
            
            if not success:
                write_status("Final build verification failed", "FAIL")
                if backup_dir:
                    restore_backup(backup_dir, args.source_path)
                    write_status("All changes reverted due to build failure", "WARN")
                return 1
            
            if final_warnings > initial_warnings * 2:
                write_status(f"Warning count doubled ({initial_warnings} -> {final_warnings})", "WARN")
                write_status("You may want to review the changes", "INFO")
        
        # Summary
        print()
        print(f"{Colors.GREEN}=== Summary ==={Colors.RESET}")
        write_status(f"Files processed: {len(files)}", "INFO")
        write_status(f"Files modified: {files_modified}", "INFO")
        write_status(f"Total XML comment ordering fixes: {total_fixes}", "INFO")
        
        if args.dry_run:
            write_status("Dry run completed - no files were actually modified", "INFO")
        else:
            write_status("XML comment ordering fix completed successfully", "PASS")
            if backup_dir:
                write_status(f"Backup available at: {backup_dir}", "INFO")
        
        return 0
        
    except KeyboardInterrupt:
        write_status("Operation interrupted by user", "WARN")
        if backup_dir and not args.dry_run:
            restore_backup(backup_dir, args.source_path)
            write_status("Changes reverted due to interruption", "WARN")
        return 1
    except Exception as e:
        write_status(f"Unexpected error: {e}", "FAIL")
        if backup_dir and not args.dry_run:
            restore_backup(backup_dir, args.source_path)
            write_status("Changes reverted due to error", "WARN")
        return 1


if __name__ == "__main__":
    sys.exit(main())