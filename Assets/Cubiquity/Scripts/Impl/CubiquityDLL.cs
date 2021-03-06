using System;
using System.Runtime.InteropServices;
using System.Text;

using UnityEngine;

namespace Cubiquity
{
	namespace Impl
	{
		public class CubiquityDLL
		{
			private const string dllToImport = "CubiquityC";
			private static string logFilePath;
			
			const int CU_OK = 0;
			
			const uint requiredMajorVersion = 1;
			const uint requiredMinorVersion = 0;
			const uint requiredPatchVersion = 0;
				
			// This static constructor is supposed to make sure that the Cubiquity.dll is in the right place before the DllImport is done.
			// It doesn't seem to work, because in Standalone builds the message below is printed after the exception about the .dll not
			// being found. We need to look into this further.
			static CubiquityDLL()
			{				
				Installation.ValidateAndFix();
				
				uint majorVersion;
				uint minorVersion;
				uint patchVersion;
				cuGetVersionNumber(out majorVersion, out minorVersion, out patchVersion);
				
				if ((majorVersion != requiredMajorVersion) ||
					(minorVersion != requiredMinorVersion) ||
					(patchVersion != requiredPatchVersion))
				{
					throw new CubiquityException("Wrong version of Cubiquity native code library found! " +
						"Expected version " + requiredMajorVersion + "." + requiredMinorVersion + "." + requiredPatchVersion + 
						" but found version " + majorVersion + "." + minorVersion + "." + patchVersion + ".\n" +
						"If you are using the development version of Cubiquity (from the Git repository) then try a stable snapshot instead.\n");
				}
				
				logFilePath = GetLogFilePath();
			}
			
			private static void Validate(int returnCode)
			{
				if(returnCode != CU_OK)
				{
					throw new CubiquityException("An exception has occured inside the Cubiquity native code library.\n" +
						"Error code \'" + GetErrorCodeAsString(returnCode) + "\' with message \"" + GetLastErrorMessage() + "\".\n" +
						"Please see the log file '" + logFilePath + "' for more details.\n");
				}
			}
			
			////////////////////////////////////////////////////////////////////////////////
			// Version functions
			////////////////////////////////////////////////////////////////////////////////
			[DllImport (dllToImport)]
			private static extern int cuGetVersionNumber(out uint majorVersion, out uint minorVersion, out uint patchVersion);
			public static void GetVersionNumber(out uint majorVersion, out uint minorVersion, out uint patchVersion)
			{
				Validate(cuGetVersionNumber(out majorVersion, out minorVersion, out patchVersion));
			}
			
			////////////////////////////////////////////////////////////////////////////////
			// Logging functions
			////////////////////////////////////////////////////////////////////////////////
			[DllImport (dllToImport)]
			private static extern IntPtr cuGetLogFilePath();
			public static string GetLogFilePath()
			{
				IntPtr result = cuGetLogFilePath();
				string stringResult = Marshal.PtrToStringAnsi(result);
				return stringResult;
			}
			
			////////////////////////////////////////////////////////////////////////////////
			// Error handling functions
			////////////////////////////////////////////////////////////////////////////////
			[DllImport (dllToImport)]
			private static extern IntPtr cuGetErrorCodeAsString(int errorCode);
			public static string GetErrorCodeAsString(int errorCode)
			{
				IntPtr result = cuGetErrorCodeAsString(errorCode);
				string stringResult = Marshal.PtrToStringAnsi(result);
				return stringResult;
			}
			
			[DllImport (dllToImport)]
			private static extern IntPtr cuGetLastErrorMessage();
			public static string GetLastErrorMessage()
			{
				IntPtr result = cuGetLastErrorMessage();
				string stringResult = Marshal.PtrToStringAnsi(result);
				return stringResult;
			}
			
			////////////////////////////////////////////////////////////////////////////////
			// Volume functions
			////////////////////////////////////////////////////////////////////////////////
			[DllImport (dllToImport)]
			private static extern int cuNewEmptyColoredCubesVolume(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ, StringBuilder datasetName, uint baseNodeSize, out uint result);
			public static uint NewEmptyColoredCubesVolume(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ, string datasetName, uint baseNodeSize)
			{
				uint result;
				Validate(cuNewEmptyColoredCubesVolume(lowerX, lowerY, lowerZ, upperX, upperY, upperZ, new StringBuilder(datasetName), baseNodeSize, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuNewColoredCubesVolumeFromVDB(StringBuilder datasetName, uint baseNodeSize, out uint result);
			public static uint NewColoredCubesVolumeFromVDB(string datasetName, uint baseNodeSize)
			{
				uint result;
				Validate(cuNewColoredCubesVolumeFromVDB(new StringBuilder(datasetName), baseNodeSize, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuNewColoredCubesVolumeFromVolDat(StringBuilder voldatFolder, StringBuilder datasetName, uint baseNodeSize, out uint result);	
			public static uint NewColoredCubesVolumeFromVolDat(string voldatFolder, string datasetName, uint baseNodeSize)
			{
				uint result;
				Validate(cuNewColoredCubesVolumeFromVolDat(new StringBuilder(voldatFolder), new StringBuilder(datasetName), baseNodeSize, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuNewColoredCubesVolumeFromHeightmap(StringBuilder heightmapFileName, StringBuilder colormapFileName, StringBuilder datasetName, uint baseNodeSize, out uint result);	
			public static uint NewColoredCubesVolumeFromHeightmap(string heightmapFileName, string colormapFileName, string datasetName, uint baseNodeSize)
			{
				uint result;
				Validate(cuNewColoredCubesVolumeFromHeightmap(new StringBuilder(heightmapFileName), new StringBuilder(colormapFileName), new StringBuilder(datasetName), baseNodeSize, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuUpdateVolume(uint volumeHandle);
			public static void UpdateVolume(uint volumeHandle)
			{
				Validate(cuUpdateVolume(volumeHandle));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuGetEnclosingRegion(uint volumeHandle, out int lowerX, out int lowerY, out int lowerZ, out int upperX, out int upperY, out int upperZ);	
			public static void GetEnclosingRegion(uint volumeHandle, out int lowerX, out int lowerY, out int lowerZ, out int upperX, out int upperY, out int upperZ)
			{		
				Validate(cuGetEnclosingRegion(volumeHandle, out lowerX, out lowerY, out lowerZ, out upperX, out upperY, out upperZ));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuSetVoxel(uint volumeHandle, int x, int y, int z, QuantizedColor color);
			public static void SetVoxel(uint volumeHandle, int x, int y, int z, QuantizedColor color)
			{
				Validate(cuSetVoxel(volumeHandle, x, y, z, color));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuGetVoxel(uint volumeHandle, int x, int y, int z, out QuantizedColor color);	
			public static void GetVoxel(uint volumeHandle, int x, int y, int z, out QuantizedColor color)
			{		
				Validate(cuGetVoxel(volumeHandle, x, y, z, out color));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuDeleteColoredCubesVolume(uint volumeHandle);
			public static void DeleteColoredCubesVolume(uint volumeHandle)
			{
				Validate(cuDeleteColoredCubesVolume(volumeHandle));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuAcceptOverrideBlocks(uint volumeHandle);
			public static void AcceptOverrideBlocks(uint volumeHandle)
			{
				Validate(cuAcceptOverrideBlocks(volumeHandle));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuDiscardOverrideBlocks(uint volumeHandle);
			public static void DiscardOverrideBlocks(uint volumeHandle)
			{
				Validate(cuDiscardOverrideBlocks(volumeHandle));
			}
			
			//--------------------------------------------------------------------------------
			
			[DllImport (dllToImport)]
			private static extern int cuNewEmptyTerrainVolume(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ, StringBuilder datasetName, uint baseNodeSize, out uint result);
			public static uint NewEmptyTerrainVolume(int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ, string datasetName, uint baseNodeSize)
			{
				uint result;
				Validate(cuNewEmptyTerrainVolume(lowerX, lowerY, lowerZ, upperX, upperY, upperZ, new StringBuilder(datasetName), baseNodeSize, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuNewTerrainVolumeFromVDB(StringBuilder datasetName, uint baseNodeSize, out uint result);
			public static uint NewTerrainVolumeFromVDB(string datasetName, uint baseNodeSize)
			{
				uint result;
				Validate(cuNewTerrainVolumeFromVDB(new StringBuilder(datasetName), baseNodeSize, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuUpdateVolumeMC(uint volumeHandle);
			public static void UpdateVolumeMC(uint volumeHandle)
			{
				Validate(cuUpdateVolumeMC(volumeHandle));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuGetVoxelMC(uint volumeHandle, int x, int y, int z, out MaterialSet materialSet);	
			public static void GetVoxelMC(uint volumeHandle, int x, int y, int z, out MaterialSet materialSet)
			{		
				Validate(cuGetVoxelMC(volumeHandle, x, y, z, out materialSet));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuSetVoxelMC(uint volumeHandle, int x, int y, int z, MaterialSet materialSet);
			public static void SetVoxelMC(uint volumeHandle, int x, int y, int z, MaterialSet materialSet)
			{
				Validate(cuSetVoxelMC(volumeHandle, x, y, z, materialSet));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuDeleteTerrainVolume(uint volumeHandle);
			public static void DeleteTerrainVolume(uint volumeHandle)
			{
				Validate(cuDeleteTerrainVolume(volumeHandle));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuAcceptOverrideBlocksMC(uint volumeHandle);
			public static void AcceptOverrideBlocksMC(uint volumeHandle)
			{
				Validate(cuAcceptOverrideBlocksMC(volumeHandle));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuDiscardOverrideBlocksMC(uint volumeHandle);
			public static void DiscardOverrideBlocksMC(uint volumeHandle)
			{
				Validate(cuDiscardOverrideBlocksMC(volumeHandle));
			}
			
			////////////////////////////////////////////////////////////////////////////////
			// Octree functions
			////////////////////////////////////////////////////////////////////////////////
			[DllImport (dllToImport)]
			private static extern int cuHasRootOctreeNode(uint volumeHandle, out uint result);
			public static uint HasRootOctreeNode(uint volumeHandle)
			{
				uint result;
				Validate(cuHasRootOctreeNode(volumeHandle, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuGetRootOctreeNode(uint volumeHandle, out uint result);
			public static uint GetRootOctreeNode(uint volumeHandle)
			{
				uint result;
				Validate(cuGetRootOctreeNode(volumeHandle, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuHasChildNode(uint nodeHandle, uint childX, uint childY, uint childZ, out uint result);
			public static uint HasChildNode(uint nodeHandle, uint childX, uint childY, uint childZ)
			{
				uint result;
				Validate(cuHasChildNode(nodeHandle, childX, childY, childZ, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuGetChildNode(uint nodeHandle, uint childX, uint childY, uint childZ, out uint result);
			public static uint GetChildNode(uint nodeHandle, uint childX, uint childY, uint childZ)
			{
				uint result;
				Validate(cuGetChildNode(nodeHandle, childX, childY, childZ, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuNodeHasMesh(uint nodeHandle, out uint result);
			public static uint NodeHasMesh(uint nodeHandle)
			{
				uint result;
				Validate(cuNodeHasMesh(nodeHandle, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuGetNodePosition(uint nodeHandle, out int x, out int y, out int z);
			public static void GetNodePosition(uint nodeHandle, out int x, out int y, out int z)
			{
				Validate(cuGetNodePosition(nodeHandle, out x, out y, out z));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuGetMeshLastUpdated(uint nodeHandle, out uint result);
			public static uint GetMeshLastUpdated(uint nodeHandle)
			{
				uint result;
				Validate(cuGetMeshLastUpdated(nodeHandle, out result));
				return result;
			}
			
			//----------------------------------------------------------------------
			
			[DllImport (dllToImport)]
			private static extern int cuHasRootOctreeNodeMC(uint volumeHandle, out uint result);
			public static uint HasRootOctreeNodeMC(uint volumeHandle)
			{
				uint result;
				Validate(cuHasRootOctreeNodeMC(volumeHandle, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuGetRootOctreeNodeMC(uint volumeHandle, out uint result);
			public static uint GetRootOctreeNodeMC(uint volumeHandle)
			{
				uint result;
				Validate(cuGetRootOctreeNodeMC(volumeHandle, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuHasChildNodeMC(uint nodeHandle, uint childX, uint childY, uint childZ, out uint result);
			public static uint HasChildNodeMC(uint nodeHandle, uint childX, uint childY, uint childZ)
			{
				uint result;
				Validate(cuHasChildNodeMC(nodeHandle, childX, childY, childZ, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuGetChildNodeMC(uint nodeHandle, uint childX, uint childY, uint childZ, out uint result);
			public static uint GetChildNodeMC(uint nodeHandle, uint childX, uint childY, uint childZ)
			{
				uint result;
				Validate(cuGetChildNodeMC(nodeHandle, childX, childY, childZ, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuNodeHasMeshMC(uint nodeHandle, out uint result);
			public static uint NodeHasMeshMC(uint nodeHandle)
			{
				uint result;
				Validate(cuNodeHasMeshMC(nodeHandle, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuGetNodePositionMC(uint nodeHandle, out int x, out int y, out int z);
			public static void GetNodePositionMC(uint nodeHandle, out int x, out int y, out int z)
			{
				Validate(cuGetNodePositionMC(nodeHandle, out x, out y, out z));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuGetMeshLastUpdatedMC(uint nodeHandle, out uint result);
			public static uint GetMeshLastUpdatedMC(uint nodeHandle)
			{
				uint result;
				Validate(cuGetMeshLastUpdatedMC(nodeHandle, out result));
				return result;
			}
			
			////////////////////////////////////////////////////////////////////////////////
			// Mesh functions
			////////////////////////////////////////////////////////////////////////////////
			[DllImport (dllToImport)]
			private static extern int cuGetNoOfIndices(uint octreeNodeHandle, out uint result);
			[DllImport (dllToImport)]
			private static extern int cuGetIndices(uint octreeNodeHandle, out int[] result);
			public static int[] GetIndices(uint octreeNodeHandle)
			{
				uint noOfIndices;
				Validate(cuGetNoOfIndices(octreeNodeHandle, out noOfIndices));
				
				int[] result = new int[noOfIndices];
				Validate(cuGetIndices(octreeNodeHandle, out result));
				
				return result;
			}
				
			[DllImport (dllToImport)]
			private static extern int cuGetNoOfVertices(uint octreeNodeHandle, out uint result);
			[DllImport (dllToImport)]
			private static extern int cuGetVertices(uint octreeNodeHandle, out ColoredCubesVertex[] result);
			public static ColoredCubesVertex[] GetVertices(uint octreeNodeHandle)
			{
				// Based on http://stackoverflow.com/a/1318929
				uint noOfVertices;
				Validate(cuGetNoOfVertices(octreeNodeHandle, out noOfVertices));
				
				ColoredCubesVertex[] result = new ColoredCubesVertex[noOfVertices];
				Validate(cuGetVertices(octreeNodeHandle, out result));
				
				return result;
			}
			
			//--------------------------------------------------------------------------------
			
			[DllImport (dllToImport)]
			private static extern int cuGetNoOfIndicesMC(uint octreeNodeHandle, out uint result);
			[DllImport (dllToImport)]
			private static extern int cuGetIndicesMC(uint octreeNodeHandle, out int[] result);
			public static int[] GetIndicesMC(uint octreeNodeHandle)
			{
				uint noOfIndices;
				Validate(cuGetNoOfIndicesMC(octreeNodeHandle, out noOfIndices));
				
				int[] result = new int[noOfIndices];
				Validate(cuGetIndicesMC(octreeNodeHandle, out result));
				
				return result;
			}
				
			[DllImport (dllToImport)]
			private static extern int cuGetNoOfVerticesMC(uint octreeNodeHandle, out uint result);
			[DllImport (dllToImport)]
			private static extern int cuGetVerticesMC(uint octreeNodeHandle, out TerrainVertex[] result);
			public static TerrainVertex[] GetVerticesMC(uint octreeNodeHandle)
			{
				// Based on http://stackoverflow.com/a/1318929
				uint noOfVertices;
				Validate(cuGetNoOfVerticesMC(octreeNodeHandle, out noOfVertices));
				
				TerrainVertex[] result = new TerrainVertex[noOfVertices];
				Validate(cuGetVerticesMC(octreeNodeHandle, out result));
				
				return result;
			}
			
			////////////////////////////////////////////////////////////////////////////////
			// Clock functions
			////////////////////////////////////////////////////////////////////////////////
			[DllImport (dllToImport)]
			private static extern int cuGetCurrentTime(out uint result);
			public static uint GetCurrentTime()
			{
				uint result;
				Validate(cuGetCurrentTime(out result));
				return result;
			}
			
			////////////////////////////////////////////////////////////////////////////////
			// Raycasting functions
			////////////////////////////////////////////////////////////////////////////////
			[DllImport (dllToImport)]
			private static extern int cuPickFirstSolidVoxel(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out int resultX, out int resultY, out int resultZ, out uint result);
			public static uint PickFirstSolidVoxel(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out int resultX, out int resultY, out int resultZ)
			{
				uint result;
				Validate(cuPickFirstSolidVoxel(volumeHandle, rayStartX, rayStartY, rayStartZ, rayDirX, rayDirY, rayDirZ, out resultX, out resultY, out resultZ, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuPickLastEmptyVoxel(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out int resultX, out int resultY, out int resultZ, out uint result);
			public static uint PickLastEmptyVoxel(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out int resultX, out int resultY, out int resultZ)
			{
				uint result;
				Validate(cuPickLastEmptyVoxel(volumeHandle, rayStartX, rayStartY, rayStartZ, rayDirX, rayDirY, rayDirZ, out resultX, out resultY, out resultZ, out result));
				return result;
			}
			
			[DllImport (dllToImport)]
			private static extern int cuPickTerrainSurface(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out float resultX, out float resultY, out float resultZ, out uint result);
			public static uint PickTerrainSurface(uint volumeHandle, float rayStartX, float rayStartY, float rayStartZ, float rayDirX, float rayDirY, float rayDirZ, out float resultX, out float resultY, out float resultZ)
			{
				uint result;
				Validate(cuPickTerrainSurface(volumeHandle, rayStartX, rayStartY, rayStartZ, rayDirX, rayDirY, rayDirZ, out resultX, out resultY, out resultZ, out result));
				return result;
			}
			
			////////////////////////////////////////////////////////////////////////////////
			// Editing functions
			////////////////////////////////////////////////////////////////////////////////
			
			[DllImport (dllToImport)]
			private static extern int cuSculptTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount);
			public static void SculptTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount)
			{
				Validate(cuSculptTerrainVolume(volumeHandle, centerX, centerY, centerZ, brushInnerRadius, brushOuterRadius, amount));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuBlurTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount);
			public static void BlurTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount)
			{
				Validate(cuBlurTerrainVolume(volumeHandle, centerX, centerY, centerZ, brushInnerRadius, brushOuterRadius, amount));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuBlurTerrainVolumeRegion(uint volumeHandle, int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ);
			public static void BlurTerrainVolumeRegion(uint volumeHandle, int lowerX, int lowerY, int lowerZ, int upperX, int upperY, int upperZ)
			{
				Validate(cuBlurTerrainVolumeRegion(volumeHandle, lowerX, lowerY, lowerZ, upperX, upperY, upperZ));
			}
			
			[DllImport (dllToImport)]
			private static extern int cuPaintTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount, uint materialIndex);
			public static void PaintTerrainVolume(uint volumeHandle, float centerX, float centerY, float centerZ, float brushInnerRadius, float brushOuterRadius, float amount, uint materialIndex)
			{
				Validate(cuPaintTerrainVolume(volumeHandle, centerX, centerY, centerZ, brushInnerRadius, brushOuterRadius, amount, materialIndex));
			}
			
			////////////////////////////////////////////////////////////////////////////////
			// Volume generation functions
			////////////////////////////////////////////////////////////////////////////////
			[DllImport (dllToImport)]
			private static extern int cuGenerateFloor(uint volumeHandle, int lowerLayerHeight, uint lowerLayerMaterial, int upperLayerHeight, uint upperLayerMaterial);
			public static void GenerateFloor(uint volumeHandle, int lowerLayerHeight, uint lowerLayerMaterial, int upperLayerHeight, uint upperLayerMaterial)
			{
				Validate(cuGenerateFloor(volumeHandle, lowerLayerHeight, lowerLayerMaterial, upperLayerHeight, upperLayerMaterial));
			}
		}
	}
}
