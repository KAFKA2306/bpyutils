#!/usr/bin/env python3
"""
Blender Weight Transfer for Unity
==================================

Transfers vertex weights from body mesh to clothing meshes using Blender,
specifically optimized for Unity game engine import.

This tool solves the common problem where clothing meshes imported to Unity 
have no skinning weights, making them unable to animate with character rigs.

Key Features:
- Automatic detection of body mesh (source) and clothing meshes (targets)
- Transfer weights using surface-to-surface mapping
- Unity-optimized FBX export settings
- Detailed logging for troubleshooting
- Batch processing support

Unity Workflow:
1. Export character model from 3D software (Maya, Blender, etc.) as FBX
2. Run this tool to transfer weights from body to clothing
3. Import the processed FBX into Unity - all meshes will be properly rigged

Usage:
    blender --background --python blender_weight_transfer_for_unity.py -- input.fbx output.fbx

Requirements:
- Blender 4.0.2+
- Input FBX with rigged body mesh and unrigged clothing meshes
- Linux/WSL2 environment

Author: Generated with Claude Code for Unity developers
Version: 1.0.0
License: MIT
"""

import bpy
import sys
import os
import logging
import argparse
from datetime import datetime
from pathlib import Path

# Global settings for Unity optimization
UNITY_FBX_SETTINGS = {
    'global_scale': 1.0,
    'apply_unit_scale': True,
    'use_space_transform': True,
    'primary_bone_axis': 'Y',
    'secondary_bone_axis': 'X',
    'add_leaf_bones': True,
    'armature_nodetype': 'NULL',
    'mesh_smooth_type': 'FACE'
}

def setup_detailed_logging(log_dir: str) -> logging.Logger:
    """Setup comprehensive logging system for debugging Unity import issues"""
    os.makedirs(log_dir, exist_ok=True)
    
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    log_file = os.path.join(log_dir, f"unity_weight_transfer_{timestamp}.log")
    
    # Create formatters
    detailed_formatter = logging.Formatter(
        '%(asctime)s - %(levelname)s - [%(funcName)s:%(lineno)d] - %(message)s'
    )
    simple_formatter = logging.Formatter('%(levelname)s: %(message)s')
    
    # File handler with detailed logs
    file_handler = logging.FileHandler(log_file)
    file_handler.setLevel(logging.DEBUG)
    file_handler.setFormatter(detailed_formatter)
    
    # Console handler with simple format
    console_handler = logging.StreamHandler()
    console_handler.setLevel(logging.INFO)
    console_handler.setFormatter(simple_formatter)
    
    # Setup logger
    logger = logging.getLogger('UnityWeightTransfer')
    logger.setLevel(logging.DEBUG)
    logger.addHandler(file_handler)
    logger.addHandler(console_handler)
    
    logger.info(f"Unity Weight Transfer Tool v1.0.0")
    logger.info(f"Detailed log: {log_file}")
    
    return logger

def clear_blender_scene(logger):
    """Remove all default objects to start clean"""
    logger.info("Clearing Blender scene...")
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    logger.info("✓ Scene cleared")

def import_fbx_for_unity_processing(filepath: str, logger) -> bool:
    """Import FBX with settings optimized for weight transfer processing"""
    logger.info(f"Importing FBX for Unity processing: {os.path.basename(filepath)}")
    
    if not os.path.exists(filepath):
        logger.error(f"✗ FBX file not found: {filepath}")
        return False
        
    try:
        # Import with specific settings for weight processing
        bpy.ops.import_scene.fbx(
            filepath=filepath,
            use_custom_normals=True,
            use_image_search=False,  # Skip textures for faster processing
            use_alpha_decals=False,
            decal_offset=0,
            use_anim=False,  # Skip animations for weight transfer
            use_custom_props=False,
            use_custom_props_enum_as_string=True,
            ignore_leaf_bones=False,
            force_connect_children=False,
            automatic_bone_orientation=False,
            primary_bone_axis='Y',
            secondary_bone_axis='X'
        )
        
        logger.info("✓ FBX imported successfully")
        return True
    except Exception as e:
        logger.error(f"✗ Failed to import FBX: {e}")
        return False

def find_body_mesh_for_unity(logger):
    """Find the main body mesh that contains vertex weights for Unity rigging"""
    logger.info("Analyzing meshes to find Unity body mesh...")
    
    mesh_candidates = []
    
    for obj in bpy.data.objects:
        if obj.type != 'MESH':
            continue
            
        vgroup_count = len(obj.vertex_groups)
        vertex_count = len(obj.data.vertices)
        
        # Log mesh info for Unity debugging
        logger.debug(f"Mesh '{obj.name}': {vgroup_count} vertex groups, {vertex_count} vertices")
        
        if vgroup_count > 0:
            mesh_candidates.append((obj, vgroup_count, vertex_count))
    
    if not mesh_candidates:
        logger.error("✗ No rigged meshes found - Unity needs at least one mesh with vertex weights")
        return None
    
    # Sort by vertex group count (descending) then by vertex count
    mesh_candidates.sort(key=lambda x: (x[1], x[2]), reverse=True)
    best_mesh = mesh_candidates[0][0]
    
    logger.info(f"✓ Unity body mesh selected: '{best_mesh.name}' with {len(best_mesh.vertex_groups)} vertex groups")
    
    # Log all mesh candidates for debugging
    if len(mesh_candidates) > 1:
        logger.info("Other rigged meshes found:")
        for mesh, vg_count, v_count in mesh_candidates[1:]:
            logger.info(f"  - {mesh.name}: {vg_count} vertex groups")
    
    return best_mesh

def find_clothing_meshes_for_unity(body_mesh, logger):
    """Find all clothing/accessory meshes that need weights for Unity"""
    logger.info("Finding clothing meshes that need Unity rigging...")
    
    clothing_meshes = []
    rigged_meshes = []
    
    for obj in bpy.data.objects:
        if obj.type != 'MESH' or obj == body_mesh:
            continue
            
        vgroup_count = len(obj.vertex_groups)
        
        if vgroup_count == 0:
            clothing_meshes.append(obj)
            logger.debug(f"Clothing mesh: '{obj.name}' - needs rigging")
        else:
            rigged_meshes.append(obj)
            logger.debug(f"Already rigged: '{obj.name}' - {vgroup_count} groups")
    
    logger.info(f"✓ Found {len(clothing_meshes)} clothing meshes needing Unity weights")
    
    if rigged_meshes:
        logger.info(f"✓ Found {len(rigged_meshes)} already rigged meshes")
    
    if len(clothing_meshes) == 0:
        logger.warning("⚠ No clothing meshes found needing weights - all meshes may already be rigged")
    
    return clothing_meshes

def find_character_armature_for_unity(logger):
    """Find the character armature for Unity skinning"""
    armatures = [obj for obj in bpy.data.objects if obj.type == 'ARMATURE']
    
    if not armatures:
        logger.error("✗ No armature found - Unity needs bone structure for skinning")
        return None
    elif len(armatures) > 1:
        logger.warning(f"⚠ Multiple armatures found ({len(armatures)}), using first one")
        for arm in armatures:
            logger.info(f"  - {arm.name}: {len(arm.data.bones)} bones")
    
    armature = armatures[0]
    bone_count = len(armature.data.bones)
    
    logger.info(f"✓ Unity armature: '{armature.name}' with {bone_count} bones")
    
    return armature

def transfer_weights_for_unity_skinning(body_mesh, clothing_meshes, armature, logger) -> int:
    """Transfer vertex weights optimized for Unity's skinning system"""
    logger.info("=== STARTING UNITY WEIGHT TRANSFER ===")
    logger.info(f"Body mesh (source): {body_mesh.name}")
    logger.info(f"Armature: {armature.name}")
    logger.info(f"Clothing meshes (targets): {len(clothing_meshes)}")
    
    successful_transfers = 0
    failed_transfers = []
    
    # Process each clothing mesh
    for i, clothing_mesh in enumerate(clothing_meshes, 1):
        logger.info(f"Processing clothing mesh {i}/{len(clothing_meshes)}: {clothing_mesh.name}")
        
        try:
            # Clear any existing vertex groups for clean transfer
            clothing_mesh.vertex_groups.clear()
            
            # Setup selection for data transfer (Blender requirement)
            bpy.ops.object.select_all(action='DESELECT')
            body_mesh.select_set(True)
            bpy.context.view_layer.objects.active = body_mesh
            clothing_mesh.select_set(True)
            
            # Transfer vertex weights using surface sampling
            logger.debug(f"  Transferring weights using surface interpolation...")
            bpy.ops.object.data_transfer(
                data_type='VGROUP_WEIGHTS',
                use_create=True,
                vert_mapping='POLYINTERP_NEAREST',  # Best for clothing
                layers_select_src='ALL',
                layers_select_dst='NAME',
                mix_mode='REPLACE'
            )
            
            # Setup Unity skinning relationship
            logger.debug(f"  Setting up Unity armature relationship...")
            bpy.ops.object.select_all(action='DESELECT')
            clothing_mesh.select_set(True)
            armature.select_set(True)
            bpy.context.view_layer.objects.active = armature
            bpy.ops.object.parent_set(type='ARMATURE')
            
            # Add armature modifier for Unity
            bpy.context.view_layer.objects.active = clothing_mesh
            has_armature_mod = any(mod.type == 'ARMATURE' for mod in clothing_mesh.modifiers)
            
            if not has_armature_mod:
                armature_mod = clothing_mesh.modifiers.new(name="Armature", type='ARMATURE')
                armature_mod.object = armature
                armature_mod.use_vertex_groups = True
                logger.debug(f"  Added armature modifier for Unity")
            
            # Verify Unity readiness
            final_vgroup_count = len(clothing_mesh.vertex_groups)
            
            if final_vgroup_count > 0:
                logger.info(f"  ✓ SUCCESS: {final_vgroup_count} vertex groups transferred")
                successful_transfers += 1
            else:
                logger.warning(f"  ⚠ WARNING: No weights transferred to {clothing_mesh.name}")
                failed_transfers.append(clothing_mesh.name)
                
        except Exception as e:
            logger.error(f"  ✗ FAILED: Error transferring to {clothing_mesh.name}: {e}")
            failed_transfers.append(clothing_mesh.name)
    
    # Summary for Unity developers
    logger.info("=== UNITY WEIGHT TRANSFER SUMMARY ===")
    logger.info(f"✓ Successful transfers: {successful_transfers}/{len(clothing_meshes)}")
    
    if failed_transfers:
        logger.warning(f"⚠ Failed transfers: {len(failed_transfers)}")
        for failed_name in failed_transfers:
            logger.warning(f"  - {failed_name}")
    
    return successful_transfers

def verify_unity_readiness(logger):
    """Verify all meshes are ready for Unity import"""
    logger.info("=== VERIFYING UNITY READINESS ===")
    
    unity_ready_count = 0
    total_meshes = 0
    unity_issues = []
    
    for obj in bpy.data.objects:
        if obj.type != 'MESH':
            continue
        
        total_meshes += 1
        mesh_name = obj.name
        vertex_group_count = len(obj.vertex_groups)
        has_armature_mod = any(mod.type == 'ARMATURE' for mod in obj.modifiers)
        has_armature_parent = obj.parent and obj.parent.type == 'ARMATURE'
        
        # Check Unity requirements
        unity_ready = vertex_group_count > 0 and has_armature_mod and has_armature_parent
        
        if unity_ready:
            unity_ready_count += 1
            logger.info(f"✓ UNITY READY: {mesh_name} ({vertex_group_count} vertex groups)")
        else:
            issues = []
            if vertex_group_count == 0:
                issues.append("no vertex groups")
            if not has_armature_mod:
                issues.append("no armature modifier")
            if not has_armature_parent:
                issues.append("not parented to armature")
            
            issue_text = ", ".join(issues)
            logger.warning(f"✗ UNITY ISSUE: {mesh_name} - {issue_text}")
            unity_issues.append(f"{mesh_name}: {issue_text}")
    
    # Final Unity readiness report
    logger.info(f"Unity readiness: {unity_ready_count}/{total_meshes} meshes ready")
    
    if unity_issues:
        logger.warning("Unity import issues detected:")
        for issue in unity_issues:
            logger.warning(f"  - {issue}")
    
    return unity_ready_count, total_meshes, unity_issues

def export_fbx_for_unity(output_path: str, logger) -> bool:
    """Export FBX with Unity-specific optimized settings"""
    logger.info(f"=== EXPORTING FOR UNITY ===")
    logger.info(f"Output file: {os.path.basename(output_path)}")
    
    try:
        # Unity-optimized FBX export settings
        bpy.ops.export_scene.fbx(
            filepath=output_path,
            
            # Scene settings
            use_selection=False,
            use_active_collection=False,
            
            # Scale and orientation for Unity
            global_scale=UNITY_FBX_SETTINGS['global_scale'],
            apply_unit_scale=UNITY_FBX_SETTINGS['apply_unit_scale'],
            apply_scale_options='FBX_SCALE_NONE',
            use_space_transform=UNITY_FBX_SETTINGS['use_space_transform'],
            bake_space_transform=False,
            
            # Object types for Unity
            object_types={'ARMATURE', 'MESH'},  # Only export what Unity needs
            
            # Mesh settings for Unity
            use_mesh_modifiers=True,
            use_mesh_modifiers_render=True,
            mesh_smooth_type=UNITY_FBX_SETTINGS['mesh_smooth_type'],
            use_subsurf=False,
            use_mesh_edges=False,
            use_tspace=False,
            
            # Unity doesn't need custom properties
            use_custom_props=False,
            
            # Armature settings for Unity
            add_leaf_bones=UNITY_FBX_SETTINGS['add_leaf_bones'],
            primary_bone_axis=UNITY_FBX_SETTINGS['primary_bone_axis'],
            secondary_bone_axis=UNITY_FBX_SETTINGS['secondary_bone_axis'],
            use_armature_deform_only=False,
            armature_nodetype=UNITY_FBX_SETTINGS['armature_nodetype'],
            
            # Animation settings (disable for static models)
            bake_anim=False,
            
            # File settings
            path_mode='AUTO',
            embed_textures=False,
            batch_mode='OFF'
        )
        
        # Verify file was created
        if os.path.exists(output_path):
            file_size = os.path.getsize(output_path)
            file_size_mb = file_size / (1024 * 1024)
            logger.info(f"✓ Unity FBX exported successfully")
            logger.info(f"  File size: {file_size_mb:.1f} MB")
            logger.info(f"  Ready for Unity import!")
        else:
            logger.error("✗ Export failed - output file not created")
            return False
        
        return True
        
    except Exception as e:
        logger.error(f"✗ Unity FBX export failed: {e}")
        return False

def main():
    """Main function for Unity weight transfer workflow"""
    # Parse arguments
    if "--" in sys.argv:
        custom_args = sys.argv[sys.argv.index("--") + 1:]
        if len(custom_args) >= 2:
            input_fbx = custom_args[0]
            output_fbx = custom_args[1]
        else:
            print("Usage: blender --background --python blender_weight_transfer_for_unity.py -- input.fbx output.fbx")
            return 1
    else:
        # Default paths for development
        script_dir = Path(__file__).parent.parent
        input_fbx = str(script_dir / "project-files/input-fbx/character.fbx")
        output_fbx = str(script_dir / "project-files/output-fbx/character_unity_ready.fbx")
    
    # Setup logging
    log_dir = str(Path(__file__).parent.parent / "project-files/process-logs")
    logger = setup_detailed_logging(log_dir)
    
    logger.info("=== BLENDER WEIGHT TRANSFER FOR UNITY ===")
    logger.info(f"Input FBX: {input_fbx}")
    logger.info(f"Output FBX: {output_fbx}")
    
    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_fbx), exist_ok=True)
    
    try:
        # Step 1: Clean scene and import FBX
        clear_blender_scene(logger)
        
        if not import_fbx_for_unity_processing(input_fbx, logger):
            logger.error("Failed to import FBX - aborting Unity processing")
            return 1
        
        # Step 2: Find body mesh for Unity
        body_mesh = find_body_mesh_for_unity(logger)
        if not body_mesh:
            logger.error("No suitable body mesh found - Unity needs rigged body mesh")
            return 1
        
        # Step 3: Find armature for Unity
        armature = find_character_armature_for_unity(logger)
        if not armature:
            logger.error("No armature found - Unity needs bone structure")
            return 1
        
        # Step 4: Find clothing meshes needing weights
        clothing_meshes = find_clothing_meshes_for_unity(body_mesh, logger)
        
        # Step 5: Transfer weights for Unity
        if clothing_meshes:
            successful_transfers = transfer_weights_for_unity_skinning(
                body_mesh, clothing_meshes, armature, logger
            )
            
            if successful_transfers == 0:
                logger.error("All weight transfers failed - Unity model will not animate properly")
                return 1
        else:
            logger.info("All meshes already have weights - proceeding to Unity export")
        
        # Step 6: Verify Unity readiness
        unity_ready_count, total_meshes, unity_issues = verify_unity_readiness(logger)
        
        # Step 7: Export for Unity
        if export_fbx_for_unity(output_fbx, logger):
            logger.info("=== UNITY PROCESSING COMPLETED SUCCESSFULLY ===")
            logger.info(f"Unity-ready meshes: {unity_ready_count}/{total_meshes}")
            logger.info(f"Output file: {output_fbx}")
            logger.info("Import this FBX into Unity - all meshes should animate with rig!")
            
            if unity_issues:
                logger.warning(f"Note: {len(unity_issues)} meshes may have Unity import issues (see log)")
            
            return 0
        else:
            logger.error("Unity export failed - check log for details")
            return 1
            
    except Exception as e:
        logger.error(f"Unexpected error in Unity processing: {e}")
        import traceback
        logger.error(traceback.format_exc())
        return 1

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)