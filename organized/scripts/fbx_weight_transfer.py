#!/usr/bin/env python3
"""
FBX Weight Transfer Tool for Unity
===================================

This script transfers vertex weights from a source mesh (typically a body mesh) 
to target garment meshes using Blender's data transfer functionality inspired by 
the Kiseru addon.

Features:
- Automatic weight transfer from body to clothing meshes
- Proper armature parenting and modifier setup
- Unity-optimized FBX export settings
- Detailed logging for troubleshooting
- Batch processing of multiple garment meshes

Usage:
    blender --background --python fbx_weight_transfer.py -- <input_fbx> <output_fbx>

Requirements:
- Blender 4.0.2 or later
- Input FBX with at least one rigged mesh (body mesh with vertex groups)
- Target meshes that need rigging (clothing, accessories, etc.)

Author: Generated with Claude Code
License: MIT
"""

import bpy
import sys
import os
import logging
import argparse
from datetime import datetime
from pathlib import Path

def setup_logging(log_dir: str) -> logging.Logger:
    """Setup detailed logging"""
    os.makedirs(log_dir, exist_ok=True)
    
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    log_file = os.path.join(log_dir, f"weight_transfer_{timestamp}.log")
    
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(levelname)s - %(message)s',
        handlers=[
            logging.FileHandler(log_file),
            logging.StreamHandler()
        ]
    )
    
    logger = logging.getLogger(__name__)
    logger.info(f"Log file: {log_file}")
    return logger

def clear_scene(logger):
    """Clear all objects from the scene"""
    logger.info("Clearing default scene...")
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    logger.info("Scene cleared")

def load_fbx(filepath: str, logger) -> bool:
    """Load FBX file into Blender"""
    logger.info(f"Loading FBX file: {filepath}")
    
    if not os.path.exists(filepath):
        logger.error(f"FBX file not found: {filepath}")
        return False
        
    try:
        bpy.ops.import_scene.fbx(filepath=filepath)
        logger.info("FBX file loaded successfully")
        return True
    except Exception as e:
        logger.error(f"Failed to load FBX: {e}")
        return False

def find_source_mesh(logger):
    """Find the source mesh with the most vertex groups (typically the body)"""
    best_mesh = None
    max_vertex_groups = 0
    
    for obj in bpy.data.objects:
        if obj.type == 'MESH' and len(obj.vertex_groups) > 0:
            vgroup_count = len(obj.vertex_groups)
            if vgroup_count > max_vertex_groups:
                max_vertex_groups = vgroup_count
                best_mesh = obj
    
    if best_mesh:
        logger.info(f"Found source mesh: '{best_mesh.name}' with {max_vertex_groups} vertex groups")
    else:
        logger.error("No mesh with vertex groups found for weight source!")
    
    return best_mesh

def find_target_meshes(source_mesh, logger):
    """Find all meshes that need weight transfer (meshes without vertex groups)"""
    target_meshes = []
    
    for obj in bpy.data.objects:
        if obj.type != 'MESH' or obj == source_mesh:
            continue
            
        # Find meshes with no vertex groups (need weights)
        if len(obj.vertex_groups) == 0:
            target_meshes.append(obj)
            logger.info(f"Found target mesh: '{obj.name}'")
    
    logger.info(f"Found {len(target_meshes)} target meshes needing weights")
    return target_meshes

def find_armature(logger):
    """Find the armature object"""
    for obj in bpy.data.objects:
        if obj.type == 'ARMATURE':
            logger.info(f"Found armature: '{obj.name}' with {len(obj.data.bones)} bones")
            return obj
    
    logger.error("No armature found!")
    return None

def transfer_weights(source_mesh, target_meshes, armature, logger) -> int:
    """Transfer weights from source mesh to target meshes"""
    logger.info("=== STARTING WEIGHT TRANSFER ===")
    logger.info(f"Source: {source_mesh.name}")
    logger.info(f"Armature: {armature.name}")
    logger.info(f"Targets: {len(target_meshes)} meshes")
    
    successful_transfers = 0
    
    for i, target_mesh in enumerate(target_meshes):
        logger.info(f"Processing mesh {i+1}/{len(target_meshes)}: {target_mesh.name}")
        
        try:
            # Clear any existing vertex groups
            target_mesh.vertex_groups.clear()
            
            # Clear selection
            bpy.ops.object.select_all(action='DESELECT')
            
            # Select source mesh first, then target
            source_mesh.select_set(True)
            bpy.context.view_layer.objects.active = source_mesh
            target_mesh.select_set(True)
            
            # Transfer vertex groups using data transfer
            logger.info(f"  Transferring weights...")
            bpy.ops.object.data_transfer(
                data_type='VGROUP_WEIGHTS',
                use_create=True,
                vert_mapping='POLYINTERP_NEAREST',
                layers_select_src='ALL',
                layers_select_dst='NAME',
                mix_mode='REPLACE'
            )
            
            # Parent to armature
            logger.info(f"  Setting up armature relationship...")
            bpy.ops.object.select_all(action='DESELECT')
            target_mesh.select_set(True)
            armature.select_set(True)
            bpy.context.view_layer.objects.active = armature
            bpy.ops.object.parent_set(type='ARMATURE')
            
            # Add armature modifier if needed
            bpy.context.view_layer.objects.active = target_mesh
            has_armature_mod = any(mod.type == 'ARMATURE' for mod in target_mesh.modifiers)
            if not has_armature_mod:
                armature_mod = target_mesh.modifiers.new(name="Armature", type='ARMATURE')
                armature_mod.object = armature
                armature_mod.use_vertex_groups = True
            
            # Verify transfer success
            vgroup_count = len(target_mesh.vertex_groups)
            logger.info(f"  ✓ Success: {vgroup_count} vertex groups transferred")
            
            if vgroup_count > 0:
                successful_transfers += 1
            else:
                logger.warning(f"  ✗ Warning: No vertex groups transferred to {target_mesh.name}")
                
        except Exception as e:
            logger.error(f"  ✗ Failed to transfer weights to {target_mesh.name}: {e}")
    
    logger.info(f"=== WEIGHT TRANSFER COMPLETE ===")
    logger.info(f"Successfully transferred weights to {successful_transfers}/{len(target_meshes)} meshes")
    
    return successful_transfers

def verify_weights(logger):
    """Verify that all meshes have proper rigging"""
    logger.info("=== VERIFYING RIGGING ===")
    
    rigged_count = 0
    total_meshes = 0
    
    for obj in bpy.data.objects:
        if obj.type != 'MESH':
            continue
        
        total_meshes += 1
        vertex_group_count = len(obj.vertex_groups)
        has_armature_mod = any(mod.type == 'ARMATURE' for mod in obj.modifiers)
        
        if vertex_group_count > 0 and has_armature_mod:
            rigged_count += 1
            logger.info(f"✓ {obj.name}: {vertex_group_count} vertex groups + armature modifier")
        else:
            logger.warning(f"✗ {obj.name}: {vertex_group_count} vertex groups, armature modifier: {has_armature_mod}")
    
    logger.info(f"Verification complete: {rigged_count}/{total_meshes} meshes properly rigged")
    return rigged_count, total_meshes

def export_fbx(output_path: str, logger) -> bool:
    """Export FBX with Unity-optimized settings"""
    logger.info(f"=== EXPORTING FBX: {output_path} ===")
    
    try:
        bpy.ops.export_scene.fbx(
            filepath=output_path,
            use_selection=False,
            use_active_collection=False,
            global_scale=1.0,
            apply_unit_scale=True,
            apply_scale_options='FBX_SCALE_NONE',
            use_space_transform=True,
            bake_space_transform=False,
            object_types={'ARMATURE', 'MESH'},
            use_mesh_modifiers=True,
            use_mesh_modifiers_render=True,
            mesh_smooth_type='FACE',
            use_subsurf=False,
            use_mesh_edges=False,
            use_tspace=False,
            use_custom_props=False,
            add_leaf_bones=True,
            primary_bone_axis='Y',
            secondary_bone_axis='X',
            use_armature_deform_only=False,
            armature_nodetype='NULL',
            bake_anim=False,
            path_mode='AUTO',
            embed_textures=False,
            batch_mode='OFF'
        )
        
        logger.info("FBX export successful - Unity ready!")
        return True
    except Exception as e:
        logger.error(f"FBX export failed: {e}")
        return False

def main():
    """Main processing function"""
    # Parse command line arguments
    if "--" in sys.argv:
        custom_args = sys.argv[sys.argv.index("--") + 1:]
        if len(custom_args) >= 2:
            input_fbx = custom_args[0]
            output_fbx = custom_args[1]
        else:
            print("Usage: blender --background --python fbx_weight_transfer.py -- <input_fbx> <output_fbx>")
            return
    else:
        # Default paths for testing
        script_dir = Path(__file__).parent.parent
        input_fbx = str(script_dir / "workspace/input/model.fbx")
        output_fbx = str(script_dir / "workspace/output/model_with_weights.fbx")
    
    # Setup logging
    log_dir = str(Path(__file__).parent.parent / "workspace/logs")
    logger = setup_logging(log_dir)
    
    logger.info("=== FBX WEIGHT TRANSFER TOOL ===")
    logger.info(f"Input: {input_fbx}")
    logger.info(f"Output: {output_fbx}")
    
    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_fbx), exist_ok=True)
    
    try:
        # Clear scene and load FBX
        clear_scene(logger)
        
        if not load_fbx(input_fbx, logger):
            logger.error("Failed to load input FBX file")
            return
        
        # Find source mesh, targets, and armature
        source_mesh = find_source_mesh(logger)
        if not source_mesh:
            logger.error("No suitable source mesh found")
            return
            
        armature = find_armature(logger)
        if not armature:
            logger.error("No armature found")
            return
            
        target_meshes = find_target_meshes(source_mesh, logger)
        if not target_meshes:
            logger.warning("No target meshes found - all meshes already have weights")
        
        # Transfer weights
        if target_meshes:
            successful_transfers = transfer_weights(source_mesh, target_meshes, armature, logger)
            
            if successful_transfers == 0:
                logger.error("Weight transfer failed completely")
                return
        
        # Verify results
        rigged_count, total_count = verify_weights(logger)
        
        # Export FBX
        if export_fbx(output_fbx, logger):
            logger.info("=== PROCESSING COMPLETED SUCCESSFULLY ===")
            logger.info(f"Output file ready for Unity: {output_fbx}")
            logger.info(f"Meshes with proper rigging: {rigged_count}/{total_count}")
        else:
            logger.error("Export failed")
            
    except Exception as e:
        logger.error(f"Unexpected error: {e}")
        import traceback
        logger.error(traceback.format_exc())

if __name__ == "__main__":
    main()