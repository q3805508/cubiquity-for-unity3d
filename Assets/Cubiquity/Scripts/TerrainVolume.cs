﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Cubiquity.Impl;

namespace Cubiquity
{	
	/// Allows the creation of dynamic terrains featuring caves and overhangs.
	/**
	 * The TerrainVolume behaves as a logical extention to Unity's built in terrains, but using a true volumetric (3D) represnetation of the world
	 * instead of a 2D heightmap. This allows the construction of features such as caves and overhangs which are not normally possible in Unity. It
	 * is also far more flexible in terms of the modifications which can be performed at runtime, allowing (for example) a tunnel to be dug into a
	 * hillside. This kind of modification is not possible using Unity's built in terrains because it would result in a structure which cannot be 
	 * represented by the heightmap.
	 * 
	 * Aside from these enhancements, Cubiquity's TerrainVolume provides similar functionality to the standard Unity terrain. Tools are provided to
	 * sculpt that shape of the terrain and also to paint on it with a number of materials. The TerrainVolume can also be modified from code by 
	 * getting the underlying TerrainVolumeData and manipulating it directly though the provided API. This allows you to write your own tools or
	 * actions as required by your specific gameplay mechanics.
	 * 
	 * // Picture
	 * 
	 * Each voxel of the TerrainVolume represent the material which exists at that location. More accuratly, a voxel can actually represent a *combination*
	 * of materials, such that it could be 50% rock, 40% soil, and 10% sand, for example. Please see the documentation on MaterialSet for a more
	 * comprehensive coverage of this.
	 * 
	 * The TerrainVolume class is used in conjunction with TerrainVolumeData, TerrainVolumeRenderer, and TerrainVolumeCollider. Each of these derives
	 * from a base class, and you should see the documentation and diagram accompanying the Volume class for an understanding of how they all fit
	 * together.
	 */
	[ExecuteInEditMode]
	public class TerrainVolume : Volume
	{
		/**
		 * \copydoc Volume::data
		 */
		public new TerrainVolumeData data
	    {
	        get { return (TerrainVolumeData)base.data; }
			set { base.data = value; }
	    }
		
		public static GameObject CreateGameObject(TerrainVolumeData data)
		{
			// Create our main game object representing the volume.
			GameObject terrainVolumeGameObject = new GameObject("Terrain Volume");
			
			//Add the requied components.
			TerrainVolume terrainVolume = terrainVolumeGameObject.GetOrAddComponent<TerrainVolume>();
			terrainVolumeGameObject.AddComponent<TerrainVolumeRenderer>();
			terrainVolumeGameObject.AddComponent<TerrainVolumeCollider>();
			
			// Set the provided data.
			terrainVolume.data = data;
			
			return terrainVolumeGameObject;
		}
		
		// It seems that we need to implement this function in order to make the volume pickable in the editor.
		// It's actually the gizmo which get's picked which is often bigger than than the volume (unless all
		// voxels are solid). So somtimes the volume will be selected by clicking on apparently empty space.
		// We shold try and fix this by using raycasting to check if a voxel is under the mouse cursor?
		void OnDrawGizmos()
		{
			// If there's no data then we don't need to (and can't?) draw the gizmos
			if(data != null)
			{
				// Compute the size of the volume.
				int width = (data.enclosingRegion.upperCorner.x - data.enclosingRegion.lowerCorner.x) + 1;
				int height = (data.enclosingRegion.upperCorner.y - data.enclosingRegion.lowerCorner.y) + 1;
				int depth = (data.enclosingRegion.upperCorner.z - data.enclosingRegion.lowerCorner.z) + 1;
				float offsetX = width / 2;
				float offsetY = height / 2;
				float offsetZ = depth / 2;
				
				// The origin is at the centre of a voxel, but we want this box to start at the corner of the voxel.
				Vector3 halfVoxelOffset = new Vector3(0.5f, 0.5f, 0.5f);
				
				// Draw an invisible box surrounding the volume. This is what actually gets picked.
		        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.0f);
				Gizmos.DrawCube (transform.position - halfVoxelOffset + new Vector3(offsetX, offsetY, offsetZ), new Vector3 (width, height, depth));
			}
	    }
		
		protected override void Synchronize()
		{
			base.Synchronize();
			
			// Syncronize the mesh data.
			if(data != null)
			{
				if(data.volumeHandle.HasValue)
				{
					CubiquityDLL.UpdateVolumeMC(data.volumeHandle.Value);
					
					if(CubiquityDLL.HasRootOctreeNodeMC(data.volumeHandle.Value) == 1)
					{		
						uint rootNodeHandle = CubiquityDLL.GetRootOctreeNodeMC(data.volumeHandle.Value);
						
						if(rootOctreeNodeGameObject == null)
						{
							rootOctreeNodeGameObject = OctreeNode.CreateOctreeNode(rootNodeHandle, gameObject);	
						}
						
						OctreeNode rootOctreeNode = rootOctreeNodeGameObject.GetComponent<OctreeNode>();
						int nodeSyncsPerformed = rootOctreeNode.syncNode(maxNodesPerSync, gameObject);
						
						// If no node were syncronized then the mesh data is up to
						// date and we can set the flag to convey this to the user.
						isMeshSyncronized = (nodeSyncsPerformed == 0);
					}
				}
			}
		}
	}
}
