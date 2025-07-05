#!/usr/bin/env python3
"""
Add Missing XML Comments Script

This script adds XML documentation comments to all undocumented properties and methods
in C# files, with automatic rollback capability if build issues occur.

Usage:
    python add_missing_xml_comments.py --source-path src --project-path src/API/MeAndMyDog.API/MeAndMyDog.API.csproj
"""

import os
import re
import sys
import argparse
import subprocess
import shutil
import tempfile
from pathlib import Path
from typing import List, Dict, Tuple
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
    backup_dir = tempfile.mkdtemp(prefix="xml_comments_backup_")
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


def generate_xml_comment_for_property(prop_name: str, prop_type: str, has_setter: bool = True) -> str:
    """Generate intelligent XML documentation for a property."""
    action = "Gets or sets" if has_setter else "Gets"
    
    # Convert PascalCase to readable format
    readable_name = re.sub(r'([A-Z])', r' \1', prop_name).strip().lower()
    
    # Handle common patterns
    if readable_name.endswith(' id'):
        readable_name = readable_name.replace(' id', ' identifier')
    elif readable_name.endswith(' url'):
        readable_name = readable_name.replace(' url', ' URL')
    elif readable_name.endswith(' api'):
        readable_name = readable_name.replace(' api', ' API')
    elif readable_name.endswith(' json'):
        readable_name = readable_name.replace(' json', ' JSON')
    elif readable_name.endswith(' xml'):
        readable_name = readable_name.replace(' xml', ' XML')
    elif readable_name.endswith(' html'):
        readable_name = readable_name.replace(' html', ' HTML')
    elif readable_name.startswith('is '):
        action = "Gets or sets a value indicating whether" if has_setter else "Gets a value indicating whether"
        readable_name = readable_name[3:]  # Remove 'is '
    elif readable_name.startswith('has '):
        action = "Gets or sets a value indicating whether" if has_setter else "Gets a value indicating whether"
        readable_name = "has " + readable_name[4:]  # Keep 'has '
    elif readable_name.startswith('can '):
        action = "Gets or sets a value indicating whether" if has_setter else "Gets a value indicating whether"
        readable_name = "can " + readable_name[4:]  # Keep 'can '
    
    return f"        /// <summary>\n        /// {action} the {readable_name}\n        /// </summary>"


def generate_xml_comment_for_method(method_name: str, parameters: List[str], return_type: str, is_constructor: bool = False) -> str:
    """Generate intelligent XML documentation for a method."""
    if is_constructor:
        summary = f"Initializes a new instance of the {method_name} class"
    else:
        method_lower = method_name.lower()
        if method_lower.startswith('get'):
            summary = f"Gets {method_name[3:]}"
        elif method_lower.startswith('set'):
            summary = f"Sets {method_name[3:]}"
        elif method_lower.startswith('create'):
            summary = f"Creates a new {method_name[6:]}"
        elif method_lower.startswith('update'):
            summary = f"Updates the {method_name[6:]}"
        elif method_lower.startswith('delete'):
            summary = f"Deletes the {method_name[6:]}"
        elif method_lower.startswith('validate'):
            summary = f"Validates the {method_name[8:]}"
        elif method_lower.startswith('process'):
            summary = f"Processes the {method_name[7:]}"
        elif method_lower.endswith('async'):
            summary = f"Asynchronously performs {method_name} operation"
        else:
            summary = f"Performs {method_name} operation"
    
    documentation = f"        /// <summary>\n        /// {summary}\n        /// </summary>"
    
    # Add parameter documentation
    for param in parameters:
        param = param.strip()
        if param and param != '':
            # Extract parameter name (last word, remove special characters)
            param_parts = param.split()
            if param_parts:
                param_name = re.sub(r'[^a-zA-Z0-9_]', '', param_parts[-1])
                if param_name:
                    documentation += f"\n        /// <param name=\"{param_name}\">The {param_name} parameter</param>"
    
    return documentation


def needs_xml_documentation(line: str, next_lines: List[str], line_index: int) -> Tuple[bool, str, str]:
    """Check if a line needs XML documentation and return the type and content."""
    # Check if there's already XML documentation before this line
    # Look backwards for XML comments
    for i in range(line_index - 1, max(line_index - 10, -1), -1):
        if i >= 0 and i < len(next_lines):
            prev_line = next_lines[i].strip()
            if prev_line.startswith("/// <summary>") or prev_line.endswith("</summary>"):
                return False, "", ""
            if prev_line and not prev_line.startswith("///") and not prev_line.startswith("[") and prev_line != "":
                break
    
    # Check for property declarations
    prop_match = re.match(r'^\s*(public|internal|protected)\s+(?:static\s+)?(?:readonly\s+)?(?:virtual\s+)?(?:override\s+)?([a-zA-Z_][a-zA-Z0-9_<>\[\]?,\s]*)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\{', line)
    if prop_match:
        visibility = prop_match.group(1)
        prop_type = prop_match.group(2).strip()
        prop_name = prop_match.group(3).strip()
        has_setter = 'set' in line or (line_index + 1 < len(next_lines) and 'set' in next_lines[line_index + 1])
        
        if visibility == 'public':  # Only document public properties
            return True, "property", generate_xml_comment_for_property(prop_name, prop_type, has_setter)
    
    # Check for method declarations (public only)
    method_match = re.match(r'^\s*public\s+(?:static\s+)?(?:async\s+)?(?:virtual\s+)?(?:override\s+)?([a-zA-Z_][a-zA-Z0-9_<>\[\]?,\s]*)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\(([^)]*)\)', line)
    if method_match:
        return_type = method_match.group(1).strip()
        method_name = method_match.group(2).strip()
        parameters_str = method_match.group(3).strip()
        
        # Skip if this is a property getter/setter
        if method_name in ['get', 'set'] or line.strip().endswith(';'):
            return False, "", ""
        
        parameters = [p.strip() for p in parameters_str.split(',') if p.strip()]
        is_constructor = return_type == method_name
        
        return True, "method", generate_xml_comment_for_method(method_name, parameters, return_type, is_constructor)
    
    return False, "", ""


def add_xml_comments_to_file(file_path: str, dry_run: bool = False) -> int:
    """Add XML comments to undocumented properties and methods. Returns number of additions."""
    if not os.path.exists(file_path):
        write_status(f"File not found: {file_path}", "FAIL")
        return 0
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        new_lines = []
        additions_count = 0
        
        i = 0
        while i < len(lines):
            line = lines[i]
            
            # Check if this line needs documentation
            needs_doc, doc_type, doc_content = needs_xml_documentation(line, lines, i)
            
            if needs_doc and doc_content:
                # Get indentation from current line
                indent_match = re.match(r'^(\s*)', line)
                indentation = indent_match.group(1) if indent_match else ""
                
                # Add the documentation with proper indentation
                doc_lines = doc_content.split('\n')
                for doc_line in doc_lines:
                    if doc_line.strip():
                        new_lines.append(f"{indentation}{doc_line.strip()}")
                    else:
                        new_lines.append("")
                
                additions_count += 1
                
                if not dry_run:
                    write_status(f"Adding {doc_type} documentation: {line.strip()[:60]}...", "PROCESS")
            
            # Add the original line
            new_lines.append(line)
            i += 1
        
        if additions_count > 0:
            if dry_run:
                write_status(f"Would add {additions_count} XML comments to: {file_path}", "INFO")
            else:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write('\n'.join(new_lines))
                write_status(f"Added {additions_count} XML comments to: {file_path}", "PROCESS")
        
        return additions_count
            
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
    parser = argparse.ArgumentParser(description='Add missing XML comments to C# files with automatic rollback')
    parser.add_argument('--source-path', required=True, help='Path to source directory or C# file')
    parser.add_argument('--project-path', required=True, help='Path to .csproj file for build verification')
    parser.add_argument('--batch-size', type=int, default=10, help='Files to process before build verification')
    parser.add_argument('--dry-run', action='store_true', help='Show what would be changed without modifying files')
    
    args = parser.parse_args()
    
    print(f"{Colors.BLUE}=== Missing XML Comments Adder ==={Colors.RESET}")
    print(f"Source Path: {args.source_path}")
    print(f"Project Path: {args.project_path}")
    print(f"Batch Size: {args.batch_size}")
    print(f"Dry Run: {args.dry_run}")
    print()
    
    # Verify initial build state
    initial_success, initial_warnings = test_build_stability(args.project_path)
    if not initial_success:
        write_status("Initial build verification failed. Fix build issues before adding documentation.", "FAIL")
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
        total_additions = 0
        files_modified = 0
        processed_count = 0
        
        for file_path in files:
            relative_file = os.path.relpath(file_path)
            additions = add_xml_comments_to_file(file_path, args.dry_run)
            
            if additions > 0:
                total_additions += additions
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
        if not args.dry_run and total_additions > 0:
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
                write_status("You may want to review the added documentation", "INFO")
        
        # Summary
        print()
        print(f"{Colors.GREEN}=== Summary ==={Colors.RESET}")
        write_status(f"Files processed: {len(files)}", "INFO")
        write_status(f"Files modified: {files_modified}", "INFO")
        write_status(f"Total XML comments added: {total_additions}", "INFO")
        
        if args.dry_run:
            write_status("Dry run completed - no files were actually modified", "INFO")
        else:
            write_status("XML comment addition completed successfully", "PASS")
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