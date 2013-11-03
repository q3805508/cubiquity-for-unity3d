﻿using UnityEngine;
using System.Collections;

namespace Cubiquity
{	
	public struct QuantizedColor
	{
		private static int MaxInOutValue = byte.MaxValue;
		
		private static int RedMSB = 15;
		private static int RedLSB = 12;		
		private static int GreenMSB = 11;
		private static int GreenLSB = 8;
		private static int BlueMSB = 7;
		private static int BlueLSB = 4;
		private static int AlphaMSB = 3;
		private static int AlphaLSB = 0;
		
		private static int NoOfRedBits = RedMSB - RedLSB + 1;
		private static int NoOfGreenBits = GreenMSB - GreenLSB + 1;
		private static int NoOfBlueBits = BlueMSB - BlueLSB + 1;
		private static int NoOfAlphaBits = AlphaMSB - AlphaLSB + 1;
		
		private static int RedScaleFactor = MaxInOutValue / ((0x01 << NoOfRedBits) - 1);
		private static int GreenScaleFactor = MaxInOutValue / ((0x01 << NoOfGreenBits) - 1);
		private static int BlueScaleFactor = MaxInOutValue / ((0x01 << NoOfBlueBits) - 1);
		private static int AlphaScaleFactor = MaxInOutValue / ((0x01 << NoOfAlphaBits) - 1);
		
	    public uint color;
		
		public QuantizedColor(byte red, byte green, byte blue, byte alpha)
		{
			this.red = red;
			this.green = green;
			this.blue = blue;
			this.alpha = alpha;
		}
		
		public static explicit operator QuantizedColor(Color color)
		{
			QuantizedColor quantizedColor = new QuantizedColor();
			quantizedColor.red = (byte)(color.r * 255.0f);
			quantizedColor.green = (byte)(color.g * 255.0f);
			quantizedColor.blue = (byte)(color.b * 255.0f);
			quantizedColor.alpha = (byte)(color.a * 255.0f);
			return quantizedColor;
		}
		
		public static explicit operator QuantizedColor(Color32 color32)
		{
			QuantizedColor quantizedColor = new QuantizedColor();
			quantizedColor.red = color32.r;
			quantizedColor.green = color32.g;
			quantizedColor.blue = color32.b;
			quantizedColor.alpha = color32.a;
			return quantizedColor;
		}
	
	    public byte red
	    {
	        get
			{
				return (byte)(getBits(RedMSB, RedLSB) * RedScaleFactor);
			}
			set
			{
				setBits(RedMSB, RedLSB, (byte)(value / RedScaleFactor));
			}
	    }
		
		public byte green
	    {
	        get
			{
				return (byte)(getBits(GreenMSB, GreenLSB) * GreenScaleFactor);
			}
			set
			{
				setBits(GreenMSB, GreenLSB, (byte)(value / GreenScaleFactor));
			}
	    }
		
		public byte blue
	    {
	        get
			{
				return (byte)(getBits(BlueMSB, BlueLSB) * BlueScaleFactor);
			}
			set
			{
				setBits(BlueMSB, BlueLSB, (byte)(value / BlueScaleFactor));
			}
	    }
		
		public byte alpha
	    {
	        get
			{
				return (byte)(getBits(AlphaMSB, AlphaLSB) * AlphaScaleFactor);
			}
			set
			{
				setBits(AlphaMSB, AlphaLSB, (byte)(value / AlphaScaleFactor));
			}
	    }
		
		uint getBits(int MSB, int LSB)
		{
			int noOfBitsToGet = (MSB - LSB) + 1;

			// Build a mask containing all '0's except for the least significant bits (which are '1's).
			uint mask = uint.MaxValue; //Set to all '1's
			mask = mask << noOfBitsToGet; // Insert the required number of '0's for the lower bits
			mask = ~mask; // And invert

			// Move the desired bits into the LSBs and mask them off
			uint result = (color >> LSB) & mask;

			return result;
		}
		
		void setBits(int MSB, int LSB, uint bitsToSet)
		{
			int noOfBitsToSet = (MSB - LSB) + 1;

			uint mask = uint.MaxValue; //Set to all '1's
			mask = mask << noOfBitsToSet; // Insert the required number of '0's for the lower bits
			mask = ~mask; // And invert
			mask = mask << LSB;

			bitsToSet = (bitsToSet << LSB) & mask;

			color = (color & ~mask) | bitsToSet;
		}
	}
}
