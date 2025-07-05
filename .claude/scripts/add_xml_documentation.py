#!/usr/bin/env python3
"""
XML Documentation Generator for .NET C# Projects

This script adds proper XML documentation comments to C# classes and properties 
following Microsoft standards while ensuring build stability through milestone verification.

Usage:
    python add_xml_documentation.py --source-path src/API --project-path src/API/MeAndMyDog.API.csproj
    python add_xml_documentation.py --source-path src/API/Services/PaymentService.cs --project-path src/API/MeAndMyDog.API.csproj --dry-run
"""

import os
import re
import sys
import argparse
import subprocess
from pathlib import Path
from typing import List, Tuple, Optional


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
            write_status("Build failed after XML documentation changes", "FAIL")
            print(result.stderr)
            return False
        
        # Check for warnings
        if 'warning' in result.stdout.lower():
            write_status("Build succeeded with warnings:", "WARN")
            for line in result.stdout.split('\n'):
                if 'warning' in line.lower():
                    print(f"  {line}")
        else:
            write_status("Build verification successful - no errors or warnings", "PASS")
        
        return True
        
    except subprocess.TimeoutExpired:
        write_status("Build verification timed out", "FAIL")
        return False
    except Exception as e:
        write_status(f"Build verification error: {e}", "FAIL")
        return False


def get_xml_documentation_for_class(class_name: str, class_type: str) -> str:
    """Generate XML documentation for a class."""
    summary_map = {
        'class': f"Represents a {class_name}",
        'interface': f"Defines the contract for {class_name} operations",
        'enum': f"Specifies the available {class_name} options",
        'struct': f"Represents a {class_name} structure"
    }
    
    summary = summary_map.get(class_type.lower(), f"Represents a {class_name}")
    
    return f"""    /// <summary>
    /// {summary}
    /// </summary>"""


def get_xml_documentation_for_property(prop_name: str, prop_type: str, has_setter: bool) -> str:
    """Generate XML documentation for a property."""
    action = "Gets or sets" if has_setter else "Gets"
    
    # Convert PascalCase to readable format
    readable_name = re.sub(r'([A-Z])', r' \1', prop_name).strip().lower()
    
    return f"""        /// <summary>
        /// {action} the {readable_name}
        /// </summary>"""


def get_xml_documentation_for_method(method_name: str, parameters: List[str], 
                                    return_type: str, is_constructor: bool) -> str:
    """Generate XML documentation for a method."""
    if is_constructor:
        summary = f"Initializes a new instance of the {method_name} class"
    else:
        # Generate intelligent summary based on method name
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
    
    documentation = f"""        /// <summary>
        /// {summary}
        /// </summary>"""
    
    # Add parameter documentation
    for param in parameters:
        param = param.strip()
        if param:
            # Extract parameter name (last word, remove special characters)
            param_parts = param.split()
            if param_parts:
                param_name = re.sub(r'[^\w]', '', param_parts[-1])
                if param_name:
                    documentation += f"""
        /// <param name="{param_name}">The {param_name} parameter</param>"""
    
    # Add return documentation
    if return_type and return_type != "void" and not is_constructor:
        if "Task" in return_type:
            return_desc = "A task representing the asynchronous operation"
        elif return_type == "bool":
            return_desc = "True if successful, false otherwise"
        elif return_type == "string":
            return_desc = "The result string"
        elif return_type in ["int", "long", "decimal", "double", "float"]:
            return_desc = "The numeric result"
        else:
            return_desc = f"The {return_type} result"
        
        documentation += f"""
        /// <returns>{return_desc}</returns>"""
    
    return documentation


def add_xml_documentation_to_file(file_path: str, dry_run: bool = False) -> bool:
    """Add XML documentation to a C# file."""
    if not os.path.exists(file_path):
        write_status(f"File not found: {file_path}", "FAIL")
        return False
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        new_lines = []
        modified = False
        
        i = 0
        while i < len(lines):
            line = lines[i]
            next_line = lines[i + 1] if i + 1 < len(lines) else ""
            
            # Skip if current line is already XML documentation
            if line.strip().startswith('///'):
                new_lines.append(line)
                i += 1
                continue
            
            doc_to_add = ""
            needs_doc = False
            
            # Class/Interface/Enum/Struct declaration
            class_match = re.match(r'^\s*(public|internal|private|protected)?\s*(static|abstract|sealed|partial)?\s*(class|interface|enum|struct)\s+(\w+)', next_line)
            if class_match and not line.strip().startswith('///'):
                class_type = class_match.group(3)
                class_name = class_match.group(4)
                doc_to_add = get_xml_documentation_for_class(class_name, class_type)
                needs_doc = True
            
            # Property declaration
            prop_match = re.match(r'^\s*(public|internal|private|protected)\s+(?:static\s+)?(?:readonly\s+)?(\w+(?:<[^>]+>)?(?:\[\])?)\s+(\w+)\s*\{', next_line)
            if prop_match and not line.strip().startswith('///'):
                prop_type = prop_match.group(2)
                prop_name = prop_match.group(3)
                has_setter = 'set' in next_line
                doc_to_add = get_xml_documentation_for_property(prop_name, prop_type, has_setter)
                needs_doc = True
            
            # Method/Constructor declaration
            method_match = re.match(r'^\s*(public|internal|private|protected)\s+(?:static\s+)?(?:async\s+)?(?:virtual\s+)?(?:override\s+)?(\w+(?:<[^>]+>)?(?:\?)?)\s+(\w+)\s*\(([^)]*)\)', next_line)
            if method_match and not line.strip().startswith('///'):
                return_type = method_match.group(2)
                method_name = method_match.group(3)
                parameters = [p.strip() for p in method_match.group(4).split(',') if p.strip()]
                is_constructor = return_type == method_name
                doc_to_add = get_xml_documentation_for_method(method_name, parameters, return_type, is_constructor)
                needs_doc = True
            
            # Add current line
            new_lines.append(line)
            
            # Add documentation if needed
            if needs_doc and doc_to_add:
                # Get indentation from next line
                indent_match = re.match(r'^(\s*)', next_line)
                indentation = indent_match.group(1) if indent_match else ""
                
                # Add the documentation with proper indentation
                doc_lines = doc_to_add.split('\n')
                for doc_line in doc_lines:
                    if doc_line.strip():
                        new_lines.append(f"{indentation}{doc_line.lstrip()}")
                    else:
                        new_lines.append("")
                
                modified = True
            
            i += 1
        
        if modified:
            if dry_run:
                write_status(f"Would add XML documentation to: {file_path}", "INFO")
            else:
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write('\n'.join(new_lines))
                write_status(f"Added XML documentation to: {file_path}", "PROCESS")
            return True
        else:
            write_status(f"No documentation needed for: {file_path}", "INFO")
            return False
            
    except Exception as e:
        write_status(f"Error processing {file_path}: {e}", "FAIL")
        return False


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
    parser = argparse.ArgumentParser(description='Add XML documentation to C# files')
    parser.add_argument('--source-path', required=True, help='Path to source directory or C# file')
    parser.add_argument('--project-path', required=True, help='Path to .csproj file')
    parser.add_argument('--batch-size', type=int, default=5, help='Files to process before build verification')
    parser.add_argument('--dry-run', action='store_true', help='Show what would be changed without modifying files')
    
    args = parser.parse_args()
    
    print(f"{Colors.BLUE}=== .NET XML Documentation Generator ==={Colors.RESET}")
    print(f"Source Path: {args.source_path}")
    print(f"Project Path: {args.project_path}")
    print(f"Batch Size: {args.batch_size}")
    print(f"Dry Run: {args.dry_run}")
    print()
    
    # Verify initial build state
    if not test_build_stability(args.project_path):
        write_status("Initial build verification failed. Fix build issues before adding documentation.", "FAIL")
        return 1
    
    # Get all C# files to process
    files = get_csharp_files(args.source_path)
    if not files:
        write_status("No C# files found to process", "WARN")
        return 0
    
    write_status(f"Found {len(files)} C# files to process", "INFO")
    
    # Process files in batches
    processed_count = 0
    modified_count = 0
    batch_count = 0
    
    for file_path in files:
        relative_file = os.path.relpath(file_path)
        write_status(f"Processing: {relative_file}", "PROCESS")
        
        if add_xml_documentation_to_file(file_path, args.dry_run):
            modified_count += 1
        
        processed_count += 1
        
        # Check build stability at batch intervals
        if processed_count % args.batch_size == 0 and not args.dry_run:
            batch_count += 1
            write_status(f"Completed batch {batch_count} ({processed_count}/{len(files)} files)", "INFO")
            
            if not test_build_stability(args.project_path):
                write_status(f"Build verification failed after batch {batch_count}", "FAIL")
                return 1
    
    # Final verification
    if not args.dry_run and modified_count > 0:
        write_status("Running final build verification...", "INFO")
        if not test_build_stability(args.project_path):
            write_status("Final build verification failed", "FAIL")
            return 1
    
    # Summary
    print()
    print(f"{Colors.GREEN}=== Summary ==={Colors.RESET}")
    write_status(f"Files processed: {processed_count}", "INFO")
    write_status(f"Files modified: {modified_count}", "INFO")
    
    if args.dry_run:
        write_status("Dry run completed - no files were actually modified", "INFO")
    else:
        write_status("XML documentation generation completed successfully", "PASS")
    
    return 0


if __name__ == "__main__":
    sys.exit(main())