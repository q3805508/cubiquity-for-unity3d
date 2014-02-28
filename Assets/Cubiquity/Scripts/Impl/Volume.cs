﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Cubiquity.Impl;

namespace Cubiquity
{
	public class Volume : MonoBehaviour
	{
		/// Sets an upper limit on the rate at which the mesh representation is updated to match the volume data.
		/**
		 * %Cubiquity continuously checks whether the the mesh representation (used for rendering and physics) is synchronized with the underlying
		 * volume data. Such synchronization can be lost whenever the volume data is modified, and %Cubiquity will then regenerate the mesh. This
		 * regeneration process can take some time, and so typically you want to spread the regeneration over a number of frames.
		 *
		 * Internally %Cubiquity breaks down the volume into a number regions each corresponding to an octree node, and these can be resynchronized
		 * individually. Therefore this property controls how many of the octree nodes will be resynchronized each frame. A small value will result
		 * in a better frame rate when modifications are being performed, but at the possible expense of the rendered mesh noticeably lagging behind 
		 * the modifications which are being performed.
		 */
		public int maxNodesPerSync = 4;
		
		// Indicates whether the mesh representation is currently up to date with the volume data. Note that this property may
		// fluctuate rapidly during real-time editing as the system tries to keep up with the users modifications, and also that
		// it may lag a few frames behind the true syncronization state.
		public bool isMeshSyncronized
		{
			get { return mIsMeshSyncronized; }
			protected set
			{
				// Check if the state of the mesh sync variable has actually changed.
				if(mIsMeshSyncronized != value)
				{
					// If so update it.
					mIsMeshSyncronized = value;
					
					// And fire the appropriate event.
					if(mIsMeshSyncronized)
					{
						if(OnMeshSyncComplete != null) { OnMeshSyncComplete(); }
					}
					else
					{
						if(OnMeshSyncLost != null) { OnMeshSyncLost(); }
					}
				}
			}
		} private bool mIsMeshSyncronized = false;
		
		public delegate void MeshSyncAction();
		public event MeshSyncAction OnMeshSyncComplete;
		public event MeshSyncAction OnMeshSyncLost;
		
		protected GameObject rootOctreeNodeGameObject;
		
		private bool flushRequested;
		
		private int previousLayer = -1;
		
		// We only keep a list of enabled volumes (rather than all volumes) because OnEnable()/OnDisable() are called after
		// script recompilation, whereas Awake(), Start(), etc are not. For updating purposes we only need enabled ones anyway.
		// I don't think user code should need this, so we should leave it out of the API docs.
		public static List<Volume> allEnabledVolumes = new List<Volume>();
		
		protected void Awake()
		{
			if(rootOctreeNodeGameObject != null)
			{
				// This should not happen because the rootOctreeNodeGameObject should have been set to null before being serialized.
				Debug.LogWarning("Root octree node is already set. This is probably a bug in Cubiquity for Unity3D, but it is not serious.");
				FlushInternalData();
			}
			
			StartCoroutine(Synchronization());
		}
		
		void OnEnable()
		{
			// When switching to MonoDevelop, editing code, and then switching back to Unity, some kind of scene reload is performed.
			// It's actually a bit unclear, but it results in a new octree being built without the old one being destroyed first. It
			// seems Awake/OnDestroy() are not called as part of this process, and we are not allowed to modify the scene graph from
			//OnEnable()/OnDisable(). Therefore we just set a flag to say that the root node shot be deleted at the next update cycle.
			//
			// We set the flag here (rather than OnDisable() where it might make more sense) because the flag doesn't survive the
			// script reload, and we don't really wnt to serialize it.
			RequestFlushInternalData();
			
			allEnabledVolumes.Add(this);
		}
		
		void OnDisable()
		{
			allEnabledVolumes.Remove(this);
		}
		
		public void RequestFlushInternalData()
		{
			flushRequested = true;
		}
		
		// We do not serialize the root octree node but in practice we have still seen some issues. It seems that Unity does
		// still serialize other data (meshes, etc) in the scene even though the root game object which they are a child of
		// is not serialize. Actually this needs more investigation. Problematic scenarios include when saving the scene, 
		// switching from edit mode to play mode (which includes implicit serialzation), or when changing and recompiling scripts.
		//
		// To handle thee scenarios we need the ability to explititly destroy the root node, rather than just not serializing it.
		public void FlushInternalData()
		{
			DestroyImmediate(rootOctreeNodeGameObject);
			rootOctreeNodeGameObject = null;
		}
		
		IEnumerator Synchronization()
		{
			while(true)
			{
				Synchronize();
				yield return null;
			}
		}
		
		public virtual void Synchronize()
		{
			if(flushRequested)
			{
				FlushInternalData();
				flushRequested = false;
			}
			
			// Check whether the gameObject has been moved to a new layer.
			if(gameObject.layer != previousLayer)
			{
				// If so we update the children to match and then clear the flag.
				gameObject.SetLayerRecursively(gameObject.layer);
				previousLayer = gameObject.layer;
			}
			
			// NOTE - The following line passes transform.worldToLocalMatrix as a shader parameter. This is explicitly
			// forbidden by the Unity docs which say:
			//
			//   IMPORTANT: If you're setting shader parameters you MUST use Renderer.worldToLocalMatrix instead.
			//
			// However, we don't have a renderer on this game object as the rendering is handled by the child OctreeNodes.
			// The Unity doc's do not say why this is the case, but my best guess is that it is related to secret scaling 
			// which Unity may perform before sending data to the GPU (probably to avoid precision problems). See here:
			//
			//   http://forum.unity3d.com/threads/153328-How-to-reproduce-_Object2World
			//
			// It seems to work in our case, even with non-uniform scaling applied to the volume. Perhaps we are just geting
			// lucky, pehaps it just works on our platform, or perhaps it is actually valid for some other reason. Just be aware.
			VolumeRenderer volumeRenderer = gameObject.GetComponent<VolumeRenderer>();
			if(volumeRenderer != null)
			{
				if(volumeRenderer.material != null)
				{
					volumeRenderer.material.SetMatrix("_World2Volume", transform.worldToLocalMatrix);
				}
			}
		}
	}
}